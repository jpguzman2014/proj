/**
 * Room client.
 *
 * WebSockets first, as the brief asks: Bluetooth is not usable on iOS Safari
 * and WebRTC needs signalling anyway. Nothing here assumes a particular host,
 * so the server can move to Durable Objects or Supabase Realtime later without
 * touching the UI.
 */
export interface RoomPlayer {
  id: string;
  name: string;
  score: number;
  connected: boolean;
  isHost: boolean;
}

export interface RoomQuestion {
  index: number;
  total: number;
  prompt: string;
  options: string[];
  typeLabel: string;
  difficulty: string;
  artEntityId: string | null;
  clues: Array<{ label: string; text: string }>;
  endsAt: number;
  seconds: number;
}

export interface RoundResult {
  id: string;
  name: string;
  delta: number;
  correct: boolean;
  answered: boolean;
  ms: number | null;
}

export type ServerMessage =
  | { t: 'welcome'; code: string; playerId: string; isHost: boolean }
  | {
      t: 'room';
      code: string;
      phase: 'lobby' | 'question' | 'reveal' | 'over';
      hostId: string;
      players: RoomPlayer[];
      questionCount: number;
      index: number;
    }
  | ({ t: 'question' } & RoomQuestion)
  | {
      t: 'reveal';
      index: number;
      answerIndex: number;
      explanation: string;
      round: RoundResult[];
      standings: RoomPlayer[];
      last: boolean;
    }
  | { t: 'answered'; playerId: string; answered: number }
  | { t: 'over'; standings: RoomPlayer[] }
  | { t: 'error'; message: string }
  | { t: 'pong' };

export function roomServerUrl(): string {
  const configured = import.meta.env.VITE_ROOM_SERVER as string | undefined;
  if (configured) return configured;
  const secure = window.location.protocol === 'https:';
  return `${secure ? 'wss' : 'ws'}://${window.location.hostname}:8787`;
}

export function joinUrl(code: string): string {
  const { origin, pathname } = window.location;
  return `${origin}${pathname}#/join/${code}`;
}

export type Status = 'connecting' | 'open' | 'closed' | 'error';

export class RoomConnection {
  private socket: WebSocket | null = null;
  private queue: unknown[] = [];

  constructor(
    private readonly onMessage: (message: ServerMessage) => void,
    private readonly onStatus: (status: Status) => void,
  ) {}

  connect(): void {
    this.onStatus('connecting');
    let socket: WebSocket;
    try {
      socket = new WebSocket(roomServerUrl());
    } catch {
      this.onStatus('error');
      return;
    }
    this.socket = socket;

    socket.onopen = () => {
      this.onStatus('open');
      for (const message of this.queue) socket.send(JSON.stringify(message));
      this.queue = [];
    };
    socket.onmessage = (event) => {
      try {
        this.onMessage(JSON.parse(String(event.data)) as ServerMessage);
      } catch {
        /* ignore anything that is not our protocol */
      }
    };
    socket.onclose = () => this.onStatus('closed');
    socket.onerror = () => this.onStatus('error');
  }

  send(message: unknown): void {
    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(message));
    } else {
      this.queue.push(message);
    }
  }

  close(): void {
    this.socket?.close();
    this.socket = null;
  }
}
