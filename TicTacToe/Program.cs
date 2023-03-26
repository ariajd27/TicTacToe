using MonteCarlo;
using System;
using System.Linq;

namespace TicTacToe
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool noai = args.Any(s => s == "noai");
            bool aionly = args.Any(s => s == "aionly");
            if (noai && aionly)
            {
                Console.WriteLine("That doesn't make any sense, how can a game have no AI but be AI only?");
                Environment.Exit(1);
            }

            bool lesscalc = args.Any(s => s == "lesscalc");
            bool morecalc = args.Any(s => s == "morecalc");
            if (lesscalc && morecalc)
            {
                Console.WriteLine("That doesn't make any sense, how can there be both less and more calculation?");
                Environment.Exit(1);
            }
            int thoroughness = lesscalc ? 5000 
                : morecalc ? 50000 
                : 20000;

            var game = new TicTacToeState();

            if (!aionly)
            {
                Console.WriteLine();
                Console.WriteLine("Greetings, human!");
                Console.WriteLine();
                Console.WriteLine("The format for identifying spaces is four digits, 0 - 2, representing:");
                Console.WriteLine("1. number of columns down from top the relevant mini-board is,");
                Console.WriteLine("2. number of columns across from left the relevant mini-board is,");
                Console.WriteLine("3. number of columns down from top the space on the relevant mini-board is, and");
                Console.WriteLine("4. number of columns across from left the space on the relevant mini-board is.");
                Console.WriteLine("For example, the top-left corner of the full board is 0000, and the center is 1111.");
                Console.WriteLine();
            }

            Console.WriteLine($"Current player: {game.CurrentPlayer}");
            Console.WriteLine();
            Console.WriteLine(game);

            while (game.Actions.Any())
            {
                int position;
                if (!aionly)
                {
                    Console.WriteLine();
                    Console.WriteLine("Choose a free space.");
                    position = -1;
                    while (true)
                    {
                        var input = Console.ReadLine();
                        if (input == null || input.Length != 4 || input.Any(c => c != '0' && c != '1' && c != '2'))
                        {
                            Console.WriteLine("This input could not be parsed. Please try again.");
                            continue;
                        }

                        position = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            position *= 3;
                            position += int.Parse(input[i].ToString());
                        }
                        break;
                    }
                    Console.WriteLine();
                    game.ApplyAction(new TicTacToeAction(position, TicTacToePlayer.X));
                }

                if (!noai)
                {
                    var computer = MonteCarloTreeSearch.GetTopActions(game, thoroughness).ToList();
                    Console.WriteLine();
                    if (computer.Count > 0)
                    {
                        //Console.WriteLine("Computer's ranked plays:");
                        //foreach (var a in computer)
                        //    Console.WriteLine($"\t{a.Action}\t{a.NumWins}/{a.NumRuns} ({a.NumWins / a.NumRuns})");
                        game.ApplyAction(computer[0].Action);
                    }

                    position = -1;
                }

                Console.Clear();
                if (game.Actions.Any())
                {
                    Console.WriteLine($"Current player: {game.CurrentPlayer}");
                }
                else if (game.GetResult(TicTacToePlayer.X) != 0.5)
                {
                    string winner = game.GetResult(TicTacToePlayer.X) == 1 ? "X" : "O";
                    Console.WriteLine($"{winner} has won!");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Interestingly, the game seems to have ended in a stalemate.");
                    Console.WriteLine("How strange.");
                    Environment.Exit(0);
                }
                Console.WriteLine();
                Console.WriteLine(game);
            }
        }
    }
}