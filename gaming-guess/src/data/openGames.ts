import type { Difficulty, Game, GenreId } from './types';

/**
 * The open-source catalogue.
 *
 * These are the games the app can legally show pictures of. Their communities
 * publish screenshots under free licences, so a real screenshot mode is possible
 * here and nowhere else in the database. Everything is dated by first public
 * release, and `softwareLicense` is the licence of the game itself, which is a
 * question worth asking of this audience.
 */
function og(
  id: string,
  name: string,
  developer: string,
  releaseYear: number,
  genres: GenreId[],
  softwareLicense: string,
  difficulty: Difficulty,
  extra: Partial<Game> = {},
): Game {
  return {
    id,
    kind: 'game',
    corpus: 'open-source',
    name,
    franchise: name,
    developer,
    publisher: developer,
    releaseYear,
    platforms: ['pc'],
    genres,
    softwareLicense,
    difficulty,
    tags: ['indie', 'single-player', 'open-source', ...(extra.tags ?? [])],
    ...extra,
  } as Game;
}

export const OPEN_GAMES: Game[] = [
  og('nethack', 'NetHack', 'The NetHack DevTeam', 1987, ['roguelike', 'rpg'], 'NetHack General Public License', 'medium', {
    tags: ['indie', 'single-player', 'open-source', 'retro', 'fantasy'],
    art: { palette: ['#1a1a1a', '#c8c8c8', '#7a5c2e'], motif: 'blocks' },
    facts: ['Played on a grid of letters and symbols rather than sprites.'],
  }),
  og('freedoom', 'Freedoom', 'The Freedoom Project', 2001, ['fps', 'shooter', 'action'], 'BSD 3-Clause', 'medium', {
    tags: ['indie', 'single-player', 'open-source', 'sci-fi', 'first-person'],
    perspective: 'first-person',
    art: { palette: ['#7a1c1c', '#c4a35a', '#191919'], motif: 'maze' },
    facts: ['A free replacement for the data files of a famous 1993 shooter.'],
  }),
  og('flightgear', 'FlightGear', 'The FlightGear Project', 1997, ['simulation'], 'GPL-2.0', 'medium', {
    art: { palette: ['#5a8fc0', '#d8e4ef', '#3b5a2b'], motif: 'orbit' },
    facts: ['Models the whole planet, not a selection of regions.'],
  }),
  og('torcs', 'TORCS', 'Eric Espié and Christophe Guionneau', 1997, ['racing', 'simulation'], 'GPL-2.0', 'hard', {
    art: { palette: ['#2c2c2c', '#d92b2b', '#9fb0bd'], motif: 'loop' },
    facts: ['Widely used as a research platform for driving agents.'],
  }),
  og('simutrans', 'Simutrans', 'Hansjörg Malthaner', 1997, ['simulation', 'strategy'], 'Artistic License', 'hard', {
    perspective: 'isometric',
    art: { palette: ['#6f9c4a', '#b5772e', '#4a6b8a'], motif: 'blocks' },
  }),
  og('pingus', 'Pingus', 'Ingo Ruhnke', 1998, ['puzzle', 'strategy'], 'GPL-3.0', 'hard', {
    perspective: 'side-on',
    art: { palette: ['#2e6fb0', '#f2f2f2', '#e8a33d'], motif: 'platform' },
    facts: ['Small creatures walk until you give them a job.'],
  }),
  og('warzone-2100', 'Warzone 2100', 'Pumpkin Studios, then the Warzone 2100 Project', 1999, ['strategy', 'action'], 'GPL-2.0', 'medium', {
    tags: ['indie', 'single-player', 'multiplayer', 'open-source', 'sci-fi', 'post-apocalyptic'],
    art: { palette: ['#7d8a5c', '#c8792a', '#22262b'], motif: 'radar' },
    facts: ['Released commercially first, then opened up by its publisher in 2004.'],
  }),
  og('armagetron', 'Armagetron Advanced', 'The Armagetron Advanced team', 2001, ['action', 'racing'], 'GPL-2.0', 'hard', {
    tags: ['indie', 'multiplayer', 'open-source', 'sci-fi', 'retro'],
    art: { palette: ['#00e5ff', '#ff2e88', '#05060f'], motif: 'neon' },
    facts: ['Light cycles leaving walls behind them on a dark grid.'],
  }),
  og('widelands', 'Widelands', 'The Widelands Development Team', 2001, ['strategy', 'simulation'], 'GPL-2.0', 'hard', {
    art: { palette: ['#6b8f3a', '#a8763c', '#3f5d7a' ], motif: 'farm' },
  }),
  og('frozen-bubble', 'Frozen Bubble', 'Guillaume Cottenceau and others', 2002, ['puzzle'], 'GPL-2.0', 'medium', {
    art: { palette: ['#2b6cb0', '#ffffff', '#e05b7a'], motif: 'orb' },
    facts: ['Penguins fire coloured bubbles at a ceiling of more bubbles.'],
  }),
  og('battle-for-wesnoth', 'The Battle for Wesnoth', 'David White and the Wesnoth team', 2003, ['strategy', 'rpg'], 'GPL-2.0', 'medium', {
    tags: ['indie', 'single-player', 'multiplayer', 'open-source', 'fantasy'],
    art: { palette: ['#3d6b3f', '#c2a35c', '#2b2f3a'], motif: 'castle' },
    facts: ['Turn-based fantasy battles on a hex grid.'],
  }),
  og('supertux', 'SuperTux', 'The SuperTux team', 2003, ['platformer', 'action'], 'GPL-3.0', 'medium', {
    perspective: 'side-on',
    art: { palette: ['#4aa3df', '#f2c14e', '#2e3b45'], motif: 'platform' },
    facts: ['A penguin runs and jumps through snowy levels to rescue a friend.'],
  }),
  og('neverball', 'Neverball', 'Robert Kooima', 2003, ['puzzle', 'action'], 'GPL-2.0', 'hard', {
    art: { palette: ['#e8e8e8', '#3d7fd4', '#c0392b'], motif: 'orb' },
    facts: ['You tilt the floor rather than moving the ball.'],
  }),
  og('supertuxkart', 'SuperTuxKart', 'The SuperTuxKart team', 2004, ['racing', 'action'], 'GPL-3.0', 'easy', {
    tags: ['indie', 'single-player', 'multiplayer', 'open-source'],
    art: { palette: ['#3aa0dc', '#f5c542', '#2f6b3a'], motif: 'loop' },
    facts: ['Kart racing with free-software mascots as drivers.'],
  }),
  og('openttd', 'OpenTTD', 'The OpenTTD team', 2004, ['simulation', 'strategy'], 'GPL-2.0', 'easy', {
    perspective: 'isometric',
    tags: ['indie', 'single-player', 'multiplayer', 'open-source'],
    art: { palette: ['#4a7c2f', '#b8452f', '#c9bd8a'], motif: 'blocks' },
    facts: ['A reimplementation of a 1994 transport management game.'],
  }),
  og('sauerbraten', 'Sauerbraten', 'Wouter van Oortmerssen and others', 2004, ['fps', 'shooter'], 'zlib', 'hard', {
    perspective: 'first-person',
    tags: ['indie', 'multiplayer', 'open-source', 'first-person'],
    art: { palette: ['#8a7b5c', '#c0392b', '#2b2b2b'], motif: 'maze' },
    facts: ['Its engine lets players edit the map while playing on it.'],
  }),
  og('hedgewars', 'Hedgewars', 'The Hedgewars team', 2004, ['strategy', 'action'], 'GPL-2.0', 'medium', {
    perspective: 'side-on',
    tags: ['indie', 'multiplayer', 'open-source'],
    art: { palette: ['#6fbf4a', '#f2b134', '#2c4a6b'], motif: 'platform' },
    facts: ['Turn-based artillery between teams of hedgehogs.'],
  }),
  og('openarena', 'OpenArena', 'The OpenArena team', 2005, ['fps', 'shooter', 'action'], 'GPL-2.0', 'hard', {
    perspective: 'first-person',
    tags: ['indie', 'multiplayer', 'open-source', 'sci-fi', 'first-person'],
    art: { palette: ['#8c3b2f', '#d9c07a', '#1f1f22'], motif: 'maze' },
  }),
  og('assaultcube', 'AssaultCube', 'Rabid Viper Productions', 2006, ['fps', 'shooter'], 'Custom, source available', 'expert', {
    perspective: 'first-person',
    tags: ['indie', 'multiplayer', 'open-source', 'first-person'],
    art: { palette: ['#6b6f4a', '#c2a15a', '#26282b'], motif: 'maze' },
  }),
  og('dcss', 'Dungeon Crawl Stone Soup', 'The DCSS development team', 2006, ['roguelike', 'rpg'], 'GPL-2.0', 'medium', {
    tags: ['indie', 'single-player', 'open-source', 'fantasy'],
    art: { palette: ['#241f1a', '#b08a3e', '#7a2f2f'], motif: 'cave' },
  }),
  og('ufo-ai', 'UFO: Alien Invasion', 'The UFO:AI team', 2006, ['strategy', 'tps'], 'GPL-2.0', 'hard', {
    tags: ['indie', 'single-player', 'open-source', 'sci-fi'],
    art: { palette: ['#2b3d4f', '#4ad9a5', '#c0392b'], motif: 'orbit' },
  }),
  og('teeworlds', 'Teeworlds', 'Magnus Auvinen', 2007, ['shooter', 'platformer', 'action'], 'zlib-style', 'hard', {
    perspective: 'side-on',
    tags: ['indie', 'multiplayer', 'open-source'],
    art: { palette: ['#f2b134', '#4a90d9', '#2e2e2e'], motif: 'platform' },
    facts: ['Round-headed characters fight with a grappling hook.'],
  }),
  og('extreme-tux-racer', 'Extreme Tux Racer', 'The Extreme Tux Racer team', 2007, ['racing', 'action'], 'GPL-2.0', 'hard', {
    art: { palette: ['#cfe8f5', '#2e7fb8', '#f2c14e'], motif: 'mountain' },
    facts: ['A penguin slides downhill on its stomach collecting fish.'],
  }),
  og('brogue', 'Brogue', 'Brian Walker', 2009, ['roguelike', 'rpg'], 'AGPL-3.0', 'hard', {
    tags: ['indie', 'single-player', 'open-source', 'fantasy'],
    art: { palette: ['#151515', '#c8b273', '#4a7fb5'], motif: 'cave' },
    facts: ['Built around readable ASCII and lighting rather than sprites.'],
  }),
  og('naev', 'Naev', 'The Naev team', 2009, ['action', 'rpg', 'simulation'], 'GPL-3.0', 'expert', {
    perspective: 'top-down',
    tags: ['indie', 'single-player', 'open-source', 'sci-fi'],
    art: { palette: ['#0b1a2a', '#5bc8ff', '#e04b2a'], motif: 'orbit' },
  }),
  og('the-dark-mod', 'The Dark Mod', 'Broken Glass Studios', 2009, ['stealth', 'action-adventure'], 'GPL-3.0', 'expert', {
    perspective: 'first-person',
    tags: ['indie', 'single-player', 'open-source', 'first-person'],
    art: { palette: ['#1c1a17', '#b8912f', '#3a4a55'], motif: 'gothic' },
    facts: ['A standalone tribute to a first-person stealth series.'],
  }),
  og('0-ad', '0 A.D.', 'Wildfire Games', 2010, ['strategy'], 'GPL-2.0, art CC BY-SA', 'medium', {
    tags: ['indie', 'single-player', 'multiplayer', 'open-source'],
    art: { palette: ['#c9a227', '#5d7c3f', '#2f2a24'], motif: 'ruins' },
    facts: ['Ancient warfare, still officially in alpha after many years.'],
  }),
  og('minetest', 'Minetest', 'Perttu Ahola and contributors', 2010, ['sandbox', 'survival', 'adventure'], 'LGPL-2.1', 'easy', {
    perspective: 'first-person',
    tags: ['indie', 'multiplayer', 'open-source', 'survival'],
    art: { palette: ['#6b9c3c', '#8b5a2b', '#79b7e8'], motif: 'blocks' },
    facts: ['A voxel game engine as much as a game, later renamed Luanti.'],
  }),
  og('openra', 'OpenRA', 'The OpenRA developers', 2010, ['strategy'], 'GPL-3.0', 'hard', {
    perspective: 'isometric',
    tags: ['indie', 'multiplayer', 'open-source'],
    art: { palette: ['#8a7a3f', '#c0392b', '#2b3a2b'], motif: 'radar' },
    facts: ['Rebuilds several 1990s real-time strategy games on a modern engine.'],
  }),
  og('speed-dreams', 'Speed Dreams', 'The Speed Dreams team', 2010, ['racing', 'simulation'], 'GPL-2.0', 'expert', {
    art: { palette: ['#1f4b8a', '#e8e8e8', '#c0392b'], motif: 'loop' },
    facts: ['Began as a fork of an older open racing simulator.'],
  }),
  og('xonotic', 'Xonotic', 'Team Xonotic', 2011, ['fps', 'shooter', 'action'], 'GPL-3.0', 'medium', {
    perspective: 'first-person',
    tags: ['indie', 'multiplayer', 'open-source', 'sci-fi', 'first-person'],
    art: { palette: ['#e83b8c', '#2b2f4a', '#4ad9d9'], motif: 'neon' },
    facts: ['Fast arena combat with a rocket-jumping tradition.'],
  }),
  og('red-eclipse', 'Red Eclipse', 'The Red Eclipse team', 2011, ['fps', 'shooter', 'action'], 'zlib', 'expert', {
    perspective: 'first-person',
    tags: ['indie', 'multiplayer', 'open-source', 'first-person'],
    art: { palette: ['#c0392b', '#2b2f3a', '#e8c34a'], motif: 'maze' },
  }),
  og('unvanquished', 'Unvanquished', 'The Unvanquished team', 2012, ['fps', 'strategy', 'action'], 'GPL-3.0', 'expert', {
    perspective: 'first-person',
    tags: ['indie', 'multiplayer', 'open-source', 'sci-fi', 'first-person'],
    art: { palette: ['#3f7a5a', '#c9a227', '#1a1f24'], motif: 'cave' },
    facts: ['Humans with guns against aliens with claws, plus base building.'],
  }),
  og('cataclysm-dda', 'Cataclysm: Dark Days Ahead', 'The Cataclysm DDA contributors', 2013, ['roguelike', 'survival', 'rpg'], 'CC BY-SA 3.0', 'hard', {
    tags: ['indie', 'single-player', 'open-source', 'post-apocalyptic', 'survival'],
    art: { palette: ['#3a3a2a', '#8a2f2f', '#7a9c5a'], motif: 'overgrowth' },
  }),
  og('shattered-pixel-dungeon', 'Shattered Pixel Dungeon', 'Evan Debenham', 2014, ['roguelike', 'rpg'], 'GPL-3.0', 'hard', {
    platforms: ['pc', 'mobile'],
    tags: ['indie', 'single-player', 'open-source', 'fantasy', 'pixel-art'],
    art: { palette: ['#2b2233', '#c9a227', '#4a7fb5'], motif: 'cave' },
  }),
  og('endless-sky', 'Endless Sky', 'Michael Zahniser', 2015, ['simulation', 'action', 'rpg'], 'GPL-3.0', 'hard', {
    perspective: 'top-down',
    tags: ['indie', 'single-player', 'open-source', 'sci-fi'],
    art: { palette: ['#070b18', '#5bc8ff', '#e8c34a'], motif: 'orbit' },
    facts: ['Trade, fight and explore across a hand-written galaxy.'],
  }),
  og('mindustry', 'Mindustry', 'Anuken', 2017, ['strategy', 'action', 'simulation'], 'GPL-3.0', 'medium', {
    platforms: ['pc', 'mobile'],
    perspective: 'top-down',
    tags: ['indie', 'single-player', 'multiplayer', 'open-source'],
    art: { palette: ['#f2a03d', '#3a4a5c', '#4ad9a5'], motif: 'hex' },
    facts: ['Factory building and tower defence at the same time.'],
  }),
  og('veloren', 'Veloren', 'The Veloren contributors', 2018, ['action-rpg', 'rpg', 'sandbox'], 'GPL-3.0', 'medium', {
    tags: ['indie', 'multiplayer', 'open-source', 'fantasy', 'survival'],
    art: { palette: ['#4a7c5c', '#e8c34a', '#2b3a4a'], motif: 'mountain' },
    facts: ['A voxel role-playing world written in Rust.'],
  }),
];

export const OPEN_GAME_BY_ID = new Map(OPEN_GAMES.map((g) => [g.id, g]));
