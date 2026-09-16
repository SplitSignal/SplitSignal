# API Contract

Source of truth for how the Unity client, the web companion, and the server
talk to each other. Changing anything here requires team agreement — see the
decision process in the project plan.

Status: **draft — nothing below is agreed yet.**

## Transport

- REST for session create/join and history: `http://<host>:<port>/api/...`
- WebSocket for live game events: `ws://<host>:<port>/ws?session=<code>&role=<role>`

## Roles

| Role | Client | Sees |
| ---- | ------ | ---- |
| `room`   | Unity | The 3D room |
| `partner`| Web   | Information and controls the room player lacks |

## REST endpoints

| Method | Path | Body | Returns |
| ------ | ---- | ---- | ------- |
| POST | `/api/sessions` | — | `{ sessionId, joinCode }` |
| POST | `/api/sessions/{joinCode}/join` | `{ role }` | `{ sessionId, role, state }` |
| GET  | `/api/sessions/{sessionId}` | — | current session state |

## WebSocket messages

Every message is JSON with a `type` and a `payload`.

### Client -> server

| type | payload | sent by |
| ---- | ------- | ------- |
| `interact` | `{ objectId, action }` | room |
| `control`  | `{ controlId, value }` | partner |

### Server -> client

| type | payload | sent to |
| ---- | ------- | ------- |
| `state`       | full session state | both, on connect |
| `objectUpdate`| `{ objectId, state }` | both |
| `puzzleSolved`| `{ puzzleId }` | both |
| `timer`       | `{ secondsRemaining }` | both |
| `gameOver`    | `{ result: "win" \| "loss" }` | both |

## Open questions

- [ ] Port number and host discovery on a local network
- [ ] Reconnect behavior if one client drops mid-session
- [ ] Database schema for sessions / events / results
- [ ] Join code format and length
