using MonteCarlo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class TicTacToePlayer : IPlayer
    {
        public bool IsX { get; }

        private TicTacToePlayer (bool isX)
        {
            IsX = isX;
        }

        public TicTacToePlayer NextPlayer => IsX ? O : X;
        public TicTacToePlayer LastPlayer => IsX ? O : X;

        public static TicTacToePlayer X = new TicTacToePlayer(true);
        public static TicTacToePlayer O = new TicTacToePlayer(false);

        public override string ToString()
        {
            return IsX ? "X" : "O";
        }
    }
    
    public struct TicTacToeAction : IAction
    {
        public int Position { get; private set; }
        public TicTacToePlayer Player { get; private set; }

        public TicTacToeAction(int position, TicTacToePlayer player)
        {
            if (position < 0 || position > 80) throw new ArgumentException("all items in position must be between 0 and 80, inclusive", nameof(position));

            Position = position;
            Player = player;
        }

        public override string ToString()
        {
            string frontendPosition = (Position / 27).ToString() 
                + (Position % 27 / 9).ToString()
                + (Position % 9 / 3).ToString()
                + (Position % 3).ToString();
            return $"{Player}@{frontendPosition}";
        }
    }

    public class TicTacToeState : IState<TicTacToePlayer, TicTacToeAction>
    {
        private IList<TicTacToePlayer> board;
        private int lastMovePosition;
        public TicTacToePlayer CurrentPlayer { get; private set; }

        private static int[][] winningCombos = new[]
        {
            new[] { 0, 1, 2 },
            new[] { 0, 4, 8 },
            new[] { 0, 3, 6 },
            new[] { 1, 4, 7 },
            new[] { 2, 4, 6 },
            new[] { 2, 5, 8 },
            new[] { 3, 4, 5 },
            new[] { 6, 7, 8 }
        };

        public TicTacToeState(IList<TicTacToePlayer> board, int lastMovePosition, TicTacToePlayer currentPlayer)
        {
            this.board = board;
            this.lastMovePosition = lastMovePosition;
            CurrentPlayer = currentPlayer;
        }

        public TicTacToeState() : this(new TicTacToePlayer[81], -1, TicTacToePlayer.X) { } 

        private bool MiniHasWinner (int miniBoard, TicTacToePlayer forPlayer)
        {
            return winningCombos.Any(c => c.All(i => board[9 * miniBoard + i] != null && board[9 * miniBoard + i].IsX == forPlayer.IsX));
        }
        private bool HasWinner (TicTacToePlayer forPlayer)
        {
            TicTacToePlayer[] bigBoard = new TicTacToePlayer[9];
            for (int i = 0; i < 9; i++)
            {
                if (MiniHasWinner(i, forPlayer)) bigBoard[i] = TicTacToePlayer.X;
            }

            return winningCombos.Any(c => c.All(i => bigBoard[i] != null && bigBoard[i].IsX == forPlayer.IsX));
        }
        private bool IsFinished (int miniBoard)
        {
            if (MiniHasWinner(miniBoard, TicTacToePlayer.X) || MiniHasWinner(miniBoard, TicTacToePlayer.O)) return true;
            return board.Take(9 + miniBoard * 9).TakeLast(9).All(c => c != null);
        }

        public double GetResult(TicTacToePlayer forPlayer)
        {
            return HasWinner(forPlayer) ? 1 
                : HasWinner(forPlayer.NextPlayer) ? 0 
                : 0.5;
        }

        public IList<TicTacToeAction> Actions
        {
            get
            {
                // there are no valid actions from a game over state
                if (HasWinner(TicTacToePlayer.X) || HasWinner(TicTacToePlayer.O))
                {
                    return new TicTacToeAction[0];
                }

                // playing on the corresponding mini board is mandatory if possible
                if (lastMovePosition != -1 && !IsFinished(lastMovePosition))
                {
                    return board.Take(9 + lastMovePosition * 9).TakeLast(9)
                        .Select((player, position) => new { player, position })
                        .Where(o => o.player == null)
                        .Select((o) => new TicTacToeAction(9 * lastMovePosition + o.position, CurrentPlayer))
                        .ToList();
                }

                // free placement if that board's done, though! or if it's the first move
                return board.Take(81)
                    .Select((player, position) => new { player, position })
                    .Where(o => o.player == null)
                    .Select((o) => new TicTacToeAction(o.position, CurrentPlayer))
                    .ToList();
            }
        }

        public void ApplyAction(TicTacToeAction action)
        {
            board[action.Position] = action.Player;
            lastMovePosition = action.Position % 9;
            CurrentPlayer = CurrentPlayer.NextPlayer;
        }

        public IState<TicTacToePlayer, TicTacToeAction> Clone()
        {
            return new TicTacToeState(board.ToArray(), lastMovePosition, CurrentPlayer);
        }

        public override string ToString()
        {
            string output = "";
            int[] posCorrespondences = new int[]
            {
                0, 1, 2, -1, 9, 10, 11, -1, 18, 19, 20, -4,
                3, 4, 5, -1, 12, 13, 14, -1, 21, 22, 23, -4,
                6, 7, 8, -1, 15, 16, 17, -1, 24, 25, 26, -4,
                -2, -2, -2, -3, -2, -2, -2, -3, -2, -2, -2, -4,
                27, 28, 29, -1, 36, 37, 38, -1, 45, 46, 47, -4,
                30, 31, 32, -1, 39, 40, 41, -1, 48, 49, 50, -4,
                33, 34, 35, -1, 42, 43, 44, -1, 51, 52, 53, -4,
                -2, -2, -2, -3, -2, -2, -2, -3, -2, -2, -2, -4,
                54, 55, 56, -1, 63, 64, 65, -1, 72, 73, 74, -4,
                57, 58, 59, -1, 66, 67, 68, -1, 75, 76, 77, -4,
                60, 61, 62, -1, 69, 70, 71, -1, 78, 79, 80
            };
            for (int i = 0; i < posCorrespondences.Length; i++)
            {
                if (posCorrespondences[i] >= 0)
                {
                    output += board[posCorrespondences[i]];
                    if (board[posCorrespondences[i]] == null)
                    {
                        output += Actions.Any(o => o.Position == posCorrespondences[i]) ? "!" : " ";
                    }
                }
                else
                {
                    switch (posCorrespondences[i])
                    {
                        case -1: output += "|"; break;
                        case -2: output += "-"; break;
                        case -3: output += "+"; break;
                        case -4: output += "\n"; break;
                    }
                }
            }
            return output;
        }
    }
}
