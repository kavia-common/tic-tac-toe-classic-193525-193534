using System;
using System.Linq;

namespace TicTacToeBackend.Services
{
    /// <summary>
    /// Service that holds an in-memory Tic Tac Toe game state and core game logic.
    /// Suitable for a single-instance preview. Not thread-safe for multi-instance scenarios.
    /// </summary>
    public class GameService
    {
        private readonly object _lock = new();
        private char[,] _board;
        private char _currentPlayer;
        private string _status; // "in_progress" | "won" | "draw"
        private char? _winner;

        public GameService()
        {
            Reset();
        }

        /// <summary>
        /// Returns a snapshot of the current game state.
        /// </summary>
        public GameState GetState()
        {
            lock (_lock)
            {
                return new GameState
                {
                    Board = new[]
                    {
                        new[] { _board[0,0].ToString(), _board[0,1].ToString(), _board[0,2].ToString() },
                        new[] { _board[1,0].ToString(), _board[1,1].ToString(), _board[1,2].ToString() },
                        new[] { _board[2,0].ToString(), _board[2,1].ToString(), _board[2,2].ToString() }
                    },
                    CurrentPlayer = _currentPlayer.ToString(),
                    Status = _status,
                    Winner = _winner?.ToString()
                };
            }
        }

        /// <summary>
        /// Attempts to make a move at the specified row and column.
        /// Validates turn, position, and game status; updates state and evaluates outcomes.
        /// </summary>
        /// <param name="row">0-based row</param>
        /// <param name="col">0-based col</param>
        /// <param name="error">If false, contains error message</param>
        /// <returns>True if move applied</returns>
        public bool TryMakeMove(int row, int col, out string error)
        {
            lock (_lock)
            {
                error = string.Empty;

                if (_status != "in_progress")
                {
                    error = "Game is not in progress. Reset to start a new game.";
                    return false;
                }

                if (row < 0 || row > 2 || col < 0 || col > 2)
                {
                    error = "Move out of bounds. Row and col must be in range [0,2].";
                    return false;
                }

                if (_board[row, col] != ' ')
                {
                    error = "Cell already occupied.";
                    return false;
                }

                _board[row, col] = _currentPlayer;

                if (HasWon(_currentPlayer))
                {
                    _status = "won";
                    _winner = _currentPlayer;
                    return true;
                }

                if (IsBoardFull())
                {
                    _status = "draw";
                    _winner = null;
                    return true;
                }

                // Switch turns
                _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
                return true;
            }
        }

        /// <summary>
        /// Resets the game to initial state.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _board = new char[3, 3];
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        _board[r, c] = ' ';

                _currentPlayer = 'X';
                _status = "in_progress";
                _winner = null;
            }
        }

        private bool IsBoardFull()
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (_board[r, c] == ' ')
                        return false;
            return true;
        }

        private bool HasWon(char player)
        {
            // rows
            for (int r = 0; r < 3; r++)
                if (_board[r, 0] == player && _board[r, 1] == player && _board[r, 2] == player)
                    return true;

            // cols
            for (int c = 0; c < 3; c++)
                if (_board[0, c] == player && _board[1, c] == player && _board[2, c] == player)
                    return true;

            // diagonals
            if (_board[0, 0] == player && _board[1, 1] == player && _board[2, 2] == player)
                return true;

            if (_board[0, 2] == player && _board[1, 1] == player && _board[2, 0] == player)
                return true;

            return false;
        }
    }

    /// <summary>
    /// DTO returned by the state endpoint.
    /// </summary>
    public class GameState
    {
        public string[][] Board { get; set; } = Array.Empty<string[]>();
        public string CurrentPlayer { get; set; } = "X";
        public string Status { get; set; } = "in_progress";
        public string? Winner { get; set; } = null;
    }

    /// <summary>
    /// DTO for move requests.
    /// </summary>
    public class MoveRequest
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }
}
