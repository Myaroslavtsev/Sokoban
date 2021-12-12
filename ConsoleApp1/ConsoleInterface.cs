using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    static class ConsoleInterface
    {
        const int animationMilliseconds = 700;
        const int cellMarkWidth = 1;
        private static string commandString;
        private static string commandResult;
        private static Dictionary<StaticCellType, Dictionary<DynamicCellType, string>> cellMarks;
        private static Dictionary<StaticCellType, Dictionary<DynamicCellType, ConsoleColor>> foregroundColors;
        private static Dictionary<StaticCellType, Dictionary<DynamicCellType, ConsoleColor>> backgroundColors;

        static void Main(string[] args)
        {
            Game game = new Game("");
            var watch = new Stopwatch();
            watch.Start();
            InitGame();
            do {
                Console.Clear();
                WriteHeader(game);
                DrawMap(5, game.Map);
                WriteFooter(game);
                while (!Console.KeyAvailable && watch.ElapsedMilliseconds < animationMilliseconds);
                DoGameEvents(game, watch);
            } while (!game.QuitRequested);
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void DoGameEvents(Game game, Stopwatch watch)
        {
            if (watch.ElapsedMilliseconds >= animationMilliseconds)
            {
                watch.Reset();
                watch.Start();
                game.UpdateCells();
            }
            if (Console.KeyAvailable)
                AnalyzeKey(game, Console.ReadKey(true));
            var resultMessage = game.CheckGameState();
            if (resultMessage != string.Empty)
                commandResult = resultMessage;
        }

        private static void AnalyzeKey(Game game, ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    commandResult = game.DoCommand(commandString);                    
                    commandString = "";
                    return;
                case ConsoleKey.UpArrow:
                    game.MovePlayer(new Point(0, -1));
                    return;
                case ConsoleKey.DownArrow:
                    game.MovePlayer(new Point(0, 1));
                    return;
                case ConsoleKey.LeftArrow:
                    game.MovePlayer(new Point(-1, 0));
                    return;
                case ConsoleKey.RightArrow:
                    game.MovePlayer(new Point(1, 0));
                    return;
                case ConsoleKey.Backspace:
                    commandString = commandString.Substring(0, Math.Max(0, commandString.Length - 1));
                    return;
            }
            commandString += keyInfo.KeyChar;
        }

        private static void DrawMap(int top, GameMap map)
        {            
            var width = map.Width;
            var height = map.Height;
            var left = Math.Max((37 - width * cellMarkWidth) / 2, 2); // 37 = header width + 2 * margin
            //var dynamicLayer = map.MakeDynamicLayer();
            var a = map.StaticLayer[4][5];
            for (var y = 0; y < height; y++)
                for(var x = 0; x < width; x++)
                {                    
                    var staticCell = map.StaticLayer[y][x] is null ? StaticCellType.NoCell : map.StaticLayer[y][x].CellType;
                    var dynamicCell = map.DynamicLayer[y][x] is null ? DynamicCellType.NoCell : map.DynamicLayer[y][x].CellType;
                    var foreColor = foregroundColors[staticCell][dynamicCell];
                    var backColor = backgroundColors[staticCell][dynamicCell];
                    var cellMark = cellMarks[staticCell][dynamicCell];
                    ConsoleWrite(foreColor, backColor, x * cellMarkWidth + left, y + top, cellMark);
                }
        }

        private static void WriteHeader(Game game)
        {
            ConsoleWrite(ConsoleColor.Yellow, ConsoleColor.DarkRed, 2, 1, " - = S O K O B A N   2 0 2 1 = - ");
            ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkYellow, 2, 2, "                 Game:           ");
            ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkGreen, 2, 3, "                                 ");
            if (game.GameOptions.Contains(GameOption.MoveLimit))
                ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkYellow, 3, 2, $"Moves {game.Map.Player.Moves}/{game.MaxMoves}");
            else
                ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkYellow, 3, 2, $"Moves {game.Map.Player.Moves}");
            if (game.Playable)
                ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkYellow, 28, 2, "RUN!");
            else
                ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkYellow, 28, 2, "over");
            string keylist = "";
            foreach (var key in game.Map.Player.Keys)
                keylist += key.ToString() + " ";
            ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkGreen, 3, 3, "Keys: " + keylist);
            ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkGreen, 19, 3, $"Bombs: {game.Map.Player.BombCount}");
        }

        private static void WriteFooter(Game game)
        {
            ConsoleWrite(ConsoleColor.Gray, ConsoleColor.Black, 2, 5 + game.Map.Height, "> ");
            ConsoleWrite(ConsoleColor.White, ConsoleColor.Black, 4, 5 + game.Map.Height, commandString);
            ConsoleWrite(ConsoleColor.DarkGreen, ConsoleColor.Black, 2, 6 + game.Map.Height, commandResult);
            Console.SetCursorPosition(4 + commandString.Length, 5 + game.Map.Height);
        }

        private static void ConsoleWrite(ConsoleColor foregroundColor, 
                                        ConsoleColor backgroundColor, 
                                        int left, int top, 
                                        string text)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.SetCursorPosition(left, top);
            Console.Write(text);
        }

        private static void InitGame()
        {
            commandString = "";
            commandResult = "Type help for help";
            cellMarks = new Dictionary<StaticCellType, Dictionary<DynamicCellType, string>> { 
                { StaticCellType.NoCell, new Dictionary<DynamicCellType, string> { 
                    { DynamicCellType.NoCell, " " }, 
                    { DynamicCellType.Box, "X" }, 
                    { DynamicCellType.Player, "P" } } 
                },
                { StaticCellType.Wall, new Dictionary<DynamicCellType, string> {
                    { DynamicCellType.NoCell, " " },
                    { DynamicCellType.Box, "X" },
                    { DynamicCellType.Player, "P" } }
                },
                { StaticCellType.Cage, new Dictionary<DynamicCellType, string> {
                    { DynamicCellType.NoCell, "*" },
                    { DynamicCellType.Box, "V" },
                    { DynamicCellType.Player, "P" } }
                },
                { StaticCellType.Plate, new Dictionary<DynamicCellType, string> {
                    { DynamicCellType.NoCell, "_" },
                    { DynamicCellType.Box, "X" },
                    { DynamicCellType.Player, "P" } }
                },
                { StaticCellType.Key, new Dictionary<DynamicCellType, string> {
                    { DynamicCellType.NoCell, "k" },
                    { DynamicCellType.Box, "X" },
                    { DynamicCellType.Player, "P" } }
                },
                { StaticCellType.Door, new Dictionary<DynamicCellType, string> {
                    { DynamicCellType.NoCell, " " },
                    { DynamicCellType.Box, "X" },
                    { DynamicCellType.Player, "P" } }
                },
                { StaticCellType.Bomb, new Dictionary<DynamicCellType, string> {
                    { DynamicCellType.NoCell, "b" },
                    { DynamicCellType.Box, "X" },
                    { DynamicCellType.Player, "P" } }
                },
            };
            foregroundColors = new Dictionary<StaticCellType, Dictionary<DynamicCellType, ConsoleColor>> {
                { StaticCellType.NoCell, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.DarkYellow },
                    { DynamicCellType.Player, ConsoleColor.Red } }
                },
                { StaticCellType.Wall, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.DarkYellow },
                    { DynamicCellType.Player, ConsoleColor.Red } }
                },
                { StaticCellType.Cage, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.DarkGreen },
                    { DynamicCellType.Box, ConsoleColor.Green },
                    { DynamicCellType.Player, ConsoleColor.Red } }
                },
                { StaticCellType.Plate, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Gray },
                    { DynamicCellType.Box, ConsoleColor.DarkYellow },
                    { DynamicCellType.Player, ConsoleColor.Red } }
                },
                { StaticCellType.Key, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Gray },
                    { DynamicCellType.Box, ConsoleColor.DarkYellow },
                    { DynamicCellType.Player, ConsoleColor.Red } }
                },
                { StaticCellType.Door, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.DarkYellow },
                    { DynamicCellType.Player, ConsoleColor.Red } }
                },
                { StaticCellType.Bomb, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.DarkRed },
                    { DynamicCellType.Box, ConsoleColor.DarkYellow },
                    { DynamicCellType.Player, ConsoleColor.Red } }
                },
            };
            backgroundColors = new Dictionary<StaticCellType, Dictionary<DynamicCellType, ConsoleColor>> {
                { StaticCellType.NoCell, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.Black },
                    { DynamicCellType.Player, ConsoleColor.Black } }
                },
                { StaticCellType.Wall, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.DarkRed },
                    { DynamicCellType.Box, ConsoleColor.DarkRed },
                    { DynamicCellType.Player, ConsoleColor.DarkRed } }
                },
                { StaticCellType.Cage, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.Black },
                    { DynamicCellType.Player, ConsoleColor.Black } }
                },
                { StaticCellType.Plate, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.Black },
                    { DynamicCellType.Player, ConsoleColor.Black } }
                },
                { StaticCellType.Key, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.Black },
                    { DynamicCellType.Player, ConsoleColor.Black } }
                },
                { StaticCellType.Door, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.DarkMagenta },
                    { DynamicCellType.Box, ConsoleColor.DarkMagenta },
                    { DynamicCellType.Player, ConsoleColor.DarkMagenta } }
                },
                { StaticCellType.Bomb, new Dictionary<DynamicCellType, ConsoleColor> {
                    { DynamicCellType.NoCell, ConsoleColor.Black },
                    { DynamicCellType.Box, ConsoleColor.Black },
                    { DynamicCellType.Player, ConsoleColor.Black } }
                },
            };
        }
    }
}