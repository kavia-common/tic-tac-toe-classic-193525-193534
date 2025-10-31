# Tic Tac Toe - Backend

This workspace contains a .NET 8 minimal API that provides a simple Tic Tac Toe backend with in-memory state.

Service details:
- Port: 3001 (launch profile)
- Swagger UI: http://localhost:3001/docs
- Health: GET http://localhost:3001/

Endpoints:
1) GET /api/game/state
   - Returns the current game state
   - Response example:
     {
       "board": [[" "," "," "],[" "," "," "],[" "," "," "]],
       "currentPlayer": "X",
       "status": "in_progress",
       "winner": null
     }

2) POST /api/game/move
   - Applies a move for the current player
   - Body JSON: { "row": 0, "col": 1 }  // zero-based indices
   - Responses:
     - 200: Updated game state
     - 400: { "error": "message" } for invalid moves

3) POST /api/game/reset
   - Resets the game to a new empty board
   - Response: fresh game state

Sample curl commands:
- Get state:
  curl -s http://localhost:3001/api/game/state | jq .

- Make a move (row 0, col 0):
  curl -s -X POST http://localhost:3001/api/game/move \
    -H "Content-Type: application/json" \
    -d '{"row":0,"col":0}' | jq .

- Reset:
  curl -s -X POST http://localhost:3001/api/game/reset | jq .

Notes:
- In-memory singleton state; not intended for multi-instance scaling.
- CORS is enabled for local development (AllowAll).
- No external configuration or environment variables required.
