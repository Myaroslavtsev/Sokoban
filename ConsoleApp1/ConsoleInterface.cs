using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

// todo:
// + start tests
// + text commands => interface
// portals; bomb doors
// one style of map layers
// common interface to all cells
// + draw when changed only
// + directory selection
// move player - where to check boxes count ?

namespace Sokoban
{
    static class ConsoleInterface
    {
        const int animationMilliseconds = 700;
        const int cellMarkWidth = 1;
        private static string commandString;
        private static string commandResult;
        private static Dictionary<CellTypes, Dictionary<CellTypes, string>> cellMarks;
        private static Dictionary<CellTypes, Dictionary<CellTypes, ConsoleColor>> foregroundColors;
        private static Dictionary<CellTypes, Dictionary<CellTypes, ConsoleColor>> backgroundColors;
        
        private static bool quitRequested;
        private static bool footerChanged = true;
        private static string currDirectory;

        static void Main(string[] args)
        {
            currDirectory = Directory.GetCurrentDirectory() + "\\levels\\";
            Game game = new Game(currDirectory + "savegame.csv");
            var watch = new Stopwatch();
            watch.Start();
            InitDrawData();
            Draw(game);
            while (!quitRequested) 
            {                
                while (!(Console.KeyAvailable ||
                    watch.ElapsedMilliseconds > animationMilliseconds));
                DoGameEvents(game, watch);
                Draw(game);
            } 
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void Draw(Game game)
        {
            if (footerChanged) 
                Console.Clear();
            if (game.MapChanged || footerChanged)
            {
                WriteHeader(game); 
                DrawMap(5, game.Map);
                WriteFooter(game);
            }                
            game.MapChanged = false;
            footerChanged = false;
        }

        private static void DoGameEvents(Game game, Stopwatch watch)
        {
            if (watch.ElapsedMilliseconds >= animationMilliseconds)
            {
                watch.Reset();
                watch.Start();
                if (game.Playable)
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
                    commandResult = StartCommand(game, commandString);                    
                    commandString = "";
                    footerChanged = true;
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
                    footerChanged = true;
                    return;
            }
            commandString += keyInfo.KeyChar;
            footerChanged = true;
        }

        private static string StartCommand(Game game, string command)
        {
            var commandWords = command.ToLower().Split(' ');
            switch (commandWords[0])
            {
                case "quit":
                    quitRequested = true;
                    return "";
                case "help":
                    return Files.GetHelpMessage();
                case "about":
                    return Files.GetAboutMessage();
                case "levels":
                    return Files.GetLevelList(currDirectory);
                case "options":
                    return game.MakeOptionList();
                case "directory":
                    return currDirectory;
                case "cd":
                    return ChangeCurrentDirectory(commandWords);
                case "load":
                    return commandWords.Length == 1 ?
                        Files.LoadGame(currDirectory + "savegame.csv", game) : Files.LoadGame(currDirectory + commandWords[1], game);
                case "save":
                    return commandWords.Length == 1 ?
                        Files.SaveGame(currDirectory + "savegame.csv", game) : Files.SaveGame(currDirectory + commandWords[1], game);
                case "addmoves":
                    return commandWords.Length == 1 ?
                        "Move count not specified" : AddMoves(game, commandWords[1]);
                case "setforce":
                    return commandWords.Length == 1 ?
                        "Force value not specified" : SetPlayerForce(game, commandWords[1]);
            }
            if (game.Playable)
            {
                var invertOptionResult = game.CheckAndInvertOption(commandWords[0]);
                if (invertOptionResult != string.Empty)
                    return invertOptionResult;
            }
            return "Type help for help";
        }

        private static string ChangeCurrentDirectory(string[] commandWords)
        {
            if (commandWords.Length < 2)
                return "Directory name not specified";
            if (commandWords[1][commandWords[1].Length - 1] != '\\')
                commandWords[1] += '\\';
            if (!Directory.Exists(commandWords[1]))
                return "Directory name incorrect";
            currDirectory = commandWords[1];
            return "Saved games path changed";
        }

        private static string AddMoves(Game game, string moves)
        {
            if (!game.Playable)
                return "You are not playing";
            if (!game.GameOptions.Contains(GameOption.MoveLimit))
                return "There is no move limit";
            bool isParsable = int.TryParse(moves, out int moveCount);
            if (!isParsable || !game.Map.Player.AddMoves(ref moveCount))
                return "Incorrect move value";
            return $"{moveCount} moves added";
        }

        private static string SetPlayerForce(Game game, string force)
        {
            if (!game.Playable)
                return "You are not playing";
            bool isParsable = int.TryParse(force, out int forceValue);
            if (!isParsable || !game.Map.Player.SetForce(forceValue))
                return "Incorrect force value";
            return $"Player force set to {game.Map.Player.Force}";
        }

        private static void DrawMap(int top, GameMap map)
        {            
            var width = map.Width;
            var height = map.Height;
            var left = Math.Max((37 - width * cellMarkWidth) / 2, 2); // 37 = header width + 2 * margin
            for (var y = 0; y < height; y++)
                for(var x = 0; x < width; x++)
                {                    
                    var staticCell = map.StaticLayer.GetByPosition(x, y);
                    var dynamicCell = map.DynamicLayer.GetByPosition(x, y);
                    var staticCellType = staticCell is null ?
                        CellTypes.NoCell : staticCell.CellType;
                    var dynamicCellType = dynamicCell is null ?
                        CellTypes.NoCell : dynamicCell.CellType;
                    var foreColor = foregroundColors[staticCellType][dynamicCellType];
                    var backColor = backgroundColors[staticCellType][dynamicCellType];
                    var cellMark = cellMarks[staticCellType][dynamicCellType];
                    ConsoleWrite(foreColor, backColor, x * cellMarkWidth + left, y + top, cellMark);
                }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(4 + commandString.Length, 5 + map.Height);
        }

        private static void WriteHeader(Game game)
        {
            ConsoleWrite(ConsoleColor.Yellow, ConsoleColor.DarkRed, 2, 1, " - = S O K O B A N   2 0 2 1 = - ");
            ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkYellow, 2, 2, "                 Game:           ");
            ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkGreen, 2, 3, "                                 ");
            if (game.GameOptions.Contains(GameOption.MoveLimit))
                ConsoleWrite(ConsoleColor.White, ConsoleColor.DarkYellow, 3, 2, $"Moves {game.Map.Player.Moves}/{game.Map.Player.MaxMoves}");
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

        private static void InitDrawData()
        {
            commandString = "";
            commandResult = "Type help for help";
            cellMarks = new Dictionary<CellTypes, Dictionary<CellTypes, string>> { 
                { CellTypes.NoCell, new Dictionary<CellTypes, string> { 
                    { CellTypes.NoCell, " " }, 
                    { CellTypes.Box, "X" }, 
                    { CellTypes.Player, "P" } } 
                },
                { CellTypes.Wall, new Dictionary<CellTypes, string> {
                    { CellTypes.NoCell, " " },
                    { CellTypes.Box, "X" },
                    { CellTypes.Player, "P" } }
                },
                { CellTypes.Cage, new Dictionary<CellTypes, string> {
                    { CellTypes.NoCell, "*" },
                    { CellTypes.Box, "V" },
                    { CellTypes.Player, "P" } }
                },
                { CellTypes.Plate, new Dictionary<CellTypes, string> {
                    { CellTypes.NoCell, "_" },
                    { CellTypes.Box, "X" },
                    { CellTypes.Player, "P" } }
                },
                { CellTypes.Key, new Dictionary<CellTypes, string> {
                    { CellTypes.NoCell, "k" },
                    { CellTypes.Box, "X" },
                    { CellTypes.Player, "P" } }
                },
                { CellTypes.Door, new Dictionary<CellTypes, string> {
                    { CellTypes.NoCell, " " },
                    { CellTypes.Box, "X" },
                    { CellTypes.Player, "P" } }
                },
                { CellTypes.Bomb, new Dictionary<CellTypes, string> {
                    { CellTypes.NoCell, "b" },
                    { CellTypes.Box, "X" },
                    { CellTypes.Player, "P" } }
                },
            };
            foregroundColors = new Dictionary<CellTypes, Dictionary<CellTypes, ConsoleColor>> {
                { CellTypes.NoCell, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.DarkYellow },
                    { CellTypes.Player, ConsoleColor.Red } }
                },
                { CellTypes.Wall, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.DarkYellow },
                    { CellTypes.Player, ConsoleColor.Red } }
                },
                { CellTypes.Cage, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.DarkGreen },
                    { CellTypes.Box, ConsoleColor.Green },
                    { CellTypes.Player, ConsoleColor.Red } }
                },
                { CellTypes.Plate, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Gray },
                    { CellTypes.Box, ConsoleColor.DarkYellow },
                    { CellTypes.Player, ConsoleColor.Red } }
                },
                { CellTypes.Key, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Gray },
                    { CellTypes.Box, ConsoleColor.DarkYellow },
                    { CellTypes.Player, ConsoleColor.Red } }
                },
                { CellTypes.Door, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.DarkYellow },
                    { CellTypes.Player, ConsoleColor.Red } }
                },
                { CellTypes.Bomb, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.DarkRed },
                    { CellTypes.Box, ConsoleColor.DarkYellow },
                    { CellTypes.Player, ConsoleColor.Red } }
                },
            };
            backgroundColors = new Dictionary<CellTypes, Dictionary<CellTypes, ConsoleColor>> {
                { CellTypes.NoCell, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.Black },
                    { CellTypes.Player, ConsoleColor.Black } }
                },
                { CellTypes.Wall, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.DarkRed },
                    { CellTypes.Box, ConsoleColor.DarkRed },
                    { CellTypes.Player, ConsoleColor.DarkRed } }
                },
                { CellTypes.Cage, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.Black },
                    { CellTypes.Player, ConsoleColor.Black } }
                },
                { CellTypes.Plate, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.Black },
                    { CellTypes.Player, ConsoleColor.Black } }
                },
                { CellTypes.Key, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.Black },
                    { CellTypes.Player, ConsoleColor.Black } }
                },
                { CellTypes.Door, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.DarkMagenta },
                    { CellTypes.Box, ConsoleColor.DarkMagenta },
                    { CellTypes.Player, ConsoleColor.DarkMagenta } }
                },
                { CellTypes.Bomb, new Dictionary<CellTypes, ConsoleColor> {
                    { CellTypes.NoCell, ConsoleColor.Black },
                    { CellTypes.Box, ConsoleColor.Black },
                    { CellTypes.Player, ConsoleColor.Black } }
                },
            };
        }
    }
}