/**
 * Gaming Guess room server.
 *
 * Deliberately small: rooms live in memory, there is no database, no account
 * and nothing persisted. The host's browser builds the questions from its own
 * copy of the database and hands them over; the server keeps the correct answer
 * to itself, times each round and does the scoring, so a player cannot read the
 * answer out of their own client.
 */
import { WebSocketServer } from 'ws';
import { randomUUID } from 'node:crypto';

const PORT = Number(process.env.PORT ?? 8787);
const CODE_ALPHABET = 'ABCDEFGHJKMNPQRSTUVWXYZ23456789'; // no O/0, I/1, L
const CODE_LENGTH = 5;
const MAX_PLAYERS = 20;
const MAX_NICK = 16;
const ROOM_IDLE_MS = 45 * 60 * 1000;
const HOST_GRACE_MS = 90 * 1000;

/** @type {Map<string, Room>} */
const rooms = new Map();

function makeCode() {
  let code = '';
  do {
    code = Array.from(
      { length: CODE_LENGTH },
      () => CODE_ALPHABET[Math.floor(Math.random() * CODE_ALPHABET.length)],
    ).join('');
  } while (rooms.has(code));
  return code;
}

/** Nicknames are shown to strangers, so strip markup and control characters. */
function cleanNick(raw, fallback) {
  const trimmed = String(raw ?? '')
    .replace(/[\u0000-\u001f\u007f<>]/g, '')
    .trim();
  return (trimmed || fallback).slice(0, MAX_NICK);
}

class Room {
  constructor(code) {
    this.code = code;
    this.players = new Map(); // id -> { id, name, score, socket, connected, isHost }
    this.hostId = null;
    this.phase = 'lobby'; // lobby | question | reveal | over
    this.questions = [];
    this.index = -1;
    this.perQuestionSeconds = 20;
    this.deadline = 0;
    this.answers = new Map(); // playerId -> { optionIndex, ms }
    this.timer = null;
    this.touched = Date.now();
    this.hostLeftAt = null;
  }

  touch() {
    this.touched = Date.now();
  }

  send(socket, payload) {
    if (socket && socket.readyState === socket.OPEN) socket.send(JSON.stringify(payload));
  }

  broadcast(payload) {
    for (const player of this.players.values()) this.send(player.socket, payload);
  }

  roster() {
    return [...this.players.values()]
      .map((p) => ({
        id: p.id,
        name: p.name,
        score: p.score,
        connected: p.connected,
        isHost: p.isHost,
      }))
      .sort((a, b) => b.score - a.score || a.name.localeCompare(b.name));
  }

  broadcastRoom() {
    this.broadcast({
      t: 'room',
      code: this.code,
      phase: this.phase,
      hostId: this.hostId,
      players: this.roster(),
      questionCount: this.questions.length,
      index: this.index,
    });
  }

  questionPayload() {
    const question = this.questions[this.index];
    return {
      t: 'question',
      index: this.index,
      total: this.questions.length,
      prompt: question.prompt,
      options: question.options,
      typeLabel: question.typeLabel,
      difficulty: question.difficulty,
      artEntityId: question.artEntityId ?? null,
      clues: question.clues ?? [],
      endsAt: this.deadline,
      seconds: this.perQuestionSeconds,
    };
  }

  start(questions, perQuestionSeconds) {
    this.questions = questions;
    this.perQuestionSeconds = Math.min(60, Math.max(5, Number(perQuestionSeconds) || 20));
    this.index = -1;
    for (const player of this.players.values()) player.score = 0;
    this.nextQuestion();
  }

  nextQuestion() {
    clearTimeout(this.timer);
    this.index += 1;
    if (this.index >= this.questions.length) return this.finish();

    this.phase = 'question';
    this.answers = new Map();
    this.deadline = Date.now() + this.perQuestionSeconds * 1000;

    this.broadcast(this.questionPayload());
    this.broadcastRoom();

    this.timer = setTimeout(() => this.reveal(), this.perQuestionSeconds * 1000 + 250);
    this.touch();
  }

  reveal() {
    clearTimeout(this.timer);
    if (this.phase !== 'question') return;
    const question = this.questions[this.index];
    this.phase = 'reveal';

    const round = [];
    for (const player of this.players.values()) {
      if (player.isHost) continue;
      const submitted = this.answers.get(player.id);
      const correct = Boolean(submitted) && submitted.optionIndex === question.answerIndex;
      // Fastest correct answer scores most; any correct answer still scores.
      const speedShare = submitted
        ? Math.max(0, 1 - submitted.ms / (this.perQuestionSeconds * 1000))
        : 0;
      const delta = correct ? 100 + Math.round(900 * speedShare) : 0;
      player.score += delta;
      round.push({
        id: player.id,
        name: player.name,
        delta,
        correct,
        answered: Boolean(submitted),
        ms: submitted ? submitted.ms : null,
      });
    }
    round.sort((a, b) => b.delta - a.delta);

    this.broadcast({
      t: 'reveal',
      index: this.index,
      answerIndex: question.answerIndex,
      explanation: question.explanation ?? '',
      round,
      standings: this.roster(),
      last: this.index >= this.questions.length - 1,
    });
    this.touch();
  }

  finish() {
    clearTimeout(this.timer);
    this.phase = 'over';
    this.broadcast({ t: 'over', standings: this.roster() });
    this.broadcastRoom();
    this.touch();
  }

  submit(playerId, optionIndex) {
    if (this.phase !== 'question') return;
    if (this.answers.has(playerId)) return; // the first answer is the one that counts
    const ms = Math.max(0, this.perQuestionSeconds * 1000 - (this.deadline - Date.now()));
    this.answers.set(playerId, { optionIndex, ms });
    this.broadcast({ t: 'answered', playerId, answered: this.answers.size });
    // Everyone is in: reveal early rather than making the room wait out the clock.
    const active = [...this.players.values()].filter((p) => p.connected && !p.isHost).length;
    if (this.answers.size >= active) this.reveal();
    this.touch();
  }

  remove(playerId) {
    const player = this.players.get(playerId);
    if (!player) return;
    player.connected = false;
    if (player.isHost) this.hostLeftAt = Date.now();
    this.broadcastRoom();
    this.touch();
  }
}

const wss = new WebSocketServer({ port: PORT });

wss.on('connection', (socket) => {
  let room = null;
  let playerId = null;

  const fail = (message) => socket.send(JSON.stringify({ t: 'error', message }));

  socket.on('message', (raw) => {
    let msg;
    try {
      msg = JSON.parse(String(raw));
    } catch {
      return fail('Malformed message.');
    }

    switch (msg.t) {
      case 'create': {
        room = new Room(makeCode());
        rooms.set(room.code, room);
        playerId = randomUUID();
        room.hostId = playerId;
        room.players.set(playerId, {
          id: playerId,
          name: cleanNick(msg.nickname, 'Host'),
          score: 0,
          socket,
          connected: true,
          isHost: true,
        });
        socket.send(JSON.stringify({ t: 'welcome', code: room.code, playerId, isHost: true }));
        room.broadcastRoom();
        break;
      }

      case 'join': {
        const code = String(msg.code ?? '')
          .toUpperCase()
          .trim();
        const target = rooms.get(code);
        if (!target) return fail('No room with that code. Check it and try again.');
        if (target.players.size >= MAX_PLAYERS) return fail('That room is full.');
        room = target;
        playerId = randomUUID();
        room.players.set(playerId, {
          id: playerId,
          name: cleanNick(msg.nickname, `Player ${room.players.size + 1}`),
          score: 0,
          socket,
          connected: true,
          isHost: false,
        });
        socket.send(JSON.stringify({ t: 'welcome', code: room.code, playerId, isHost: false }));
        room.broadcastRoom();
        if (room.phase === 'question') room.send(socket, room.questionPayload());
        break;
      }

      case 'start': {
        if (!room || playerId !== room.hostId) return fail('Only the host can start the game.');
        if (!Array.isArray(msg.questions) || !msg.questions.length) {
          return fail('No questions were supplied.');
        }
        room.start(msg.questions.slice(0, 100), msg.perQuestionSeconds);
        break;
      }

      case 'next': {
        if (!room || playerId !== room.hostId) return fail('Only the host can advance.');
        if (room.phase === 'reveal') room.nextQuestion();
        break;
      }

      case 'reveal': {
        if (!room || playerId !== room.hostId) return fail('Only the host can reveal.');
        room.reveal();
        break;
      }

      case 'answer': {
        if (!room || playerId === null) return;
        const index = Number(msg.optionIndex);
        if (!Number.isInteger(index) || index < 0 || index > 9) return;
        room.submit(playerId, index);
        break;
      }

      case 'ping':
        socket.send(JSON.stringify({ t: 'pong' }));
        break;

      default:
        fail('Unknown message.');
    }
  });

  socket.on('close', () => {
    if (room && playerId) room.remove(playerId);
  });
});

// Rooms are transient. Drop them when the host is gone or the room goes quiet.
const sweeper = setInterval(() => {
  const now = Date.now();
  for (const [code, room] of rooms) {
    const hostGone = room.hostLeftAt && now - room.hostLeftAt > HOST_GRACE_MS;
    const idle = now - room.touched > ROOM_IDLE_MS;
    const empty = [...room.players.values()].every((p) => !p.connected);
    if (hostGone || idle || (empty && now - room.touched > 60_000)) {
      clearTimeout(room.timer);
      room.broadcast({ t: 'error', message: 'This room has closed.' });
      rooms.delete(code);
    }
  }
}, 15_000);
sweeper.unref();

console.log(`Gaming Guess room server listening on ws://localhost:${PORT}`);
