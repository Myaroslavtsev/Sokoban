using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sokoban
{
    static class Files
    {
        public static string LoadGame(string filename, Game game)
        {
            var fullFileName = Directory.GetCurrentDirectory() + "\\levels\\" + filename;
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\levels\\"))
                return "Directory \\levels not found";
            if (!File.Exists(fullFileName))
                return $"File {filename} not found";
            if (!IsValidFile(fullFileName))
                return $"File {filename} is not a saved game";
            var readResult = ReadGameFromFile(filename, fullFileName, game);
            if (readResult == string.Empty)
            {
                game.MapChanged = true;
                return $"{filename} loaded";
            }
            return readResult;
        }

        public static string SaveGame(string filename, Game game)
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\levels\\"))
                return "Directory \\levels not found";
            var fullFileName = Directory.GetCurrentDirectory() + "\\levels\\" + filename;
            FileInfo fi = null;
            try
            {
                fi = new FileInfo(fullFileName);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }
            if (fi is null)
                return $"File name {filename} is invalid";
            var overwritten = File.Exists(fullFileName);
            SaveGameToFile(fullFileName, game);
            if (overwritten)
                return $"File {filename} overwritten";
            return $"File {filename} created";
        }

        public static string GetLevelList()
        {
            var directoryPath = Directory.GetCurrentDirectory() + "\\levels\\";
            if (!Directory.Exists(directoryPath))
                return "Directory \\levels not found";
            string[] files = Directory.GetFiles(directoryPath, "*.csv");
            var validFileCount = 0;
            var result = "";
            foreach(var file in files)
            {
                if (IsValidFile(file))
                {
                    result += $"   {Path.GetFileName(file)}\r\n";
                    validFileCount++;
                }
            }
            if (validFileCount > 0)
                return $"{validFileCount} valid game files in \\levels directory:\r\n" + result;
            return "No valid saved game files found";
        }

        public static string GetHelpMessage()
        {
            return
                  "Press arrow keys to play if game is running.\r\n" +
                "  Command line commands implemented:\r\n" +
                "   quit - quit without saving\r\n" +
                "   levels - get available level list\r\n" +
                "   load - load saved game\r\n" +
                "   load filename.csv - load game level\r\n" +
                "   save - save current game\r\n" +
                "   save filename.csv - save game to file\r\n" +
                "   options - print active option list\r\n" +
                "     gravity - turn gravity on/off\r\n" +
                "     movelimit - turn level move limit on/off\r\n" +
                "   addmoves 8 - increase move limit by 8\r\n" +
                "   setforce 3 - allows move 3 boxes at once\r\n" +
                "   about - get developer's email\r\n" +
                "   help - repeat this message\r\n";
        }

        public static string GetAboutMessage()
        {
            return
                  "(c) myaroslavtsev(at)yandex.ru\r\n" +
                "      December 2021\r\n";
        }

        private static string FinishFileRead(StreamReader sr, string filename)
        {
            sr.Close();
            return $"File {filename} has invalid structure";
        }

        private static void SaveGameToFile(string fullFileName, Game game)
        {
            StreamWriter sw = new StreamWriter(fullFileName);
            sw.WriteLine("Sokoban 2021;");
            sw.Write(WriteGameOptions(game.GameOptions));
            sw.Write(WriteStaticCellMap(game.Map));
            sw.Write(WriteUniqueCellList(game.Map));
            sw.Write(WriteDynamicObjectList(game.Map));
            sw.Close();
        }

        private static string ReadGameFromFile(string filename, string fullFileName, Game game)
        {
            game.Stop();
            StreamReader sr = new StreamReader(fullFileName);
            var line = sr.ReadLine();
            if (!line.ToLower().StartsWith("sokoban 2021"))
                return FinishFileRead(sr, filename);
            if (!ReadGameOptions(sr, game))
                return FinishFileRead(sr, filename);
            if (!ReadStaticCellMap(sr, game.Map))
                return FinishFileRead(sr, filename);
            if (!ReadUniqueCellList(sr, game.Map))
                return FinishFileRead(sr, filename);
            if (!ReadDynamicObjectList(sr, game.Map))
                return FinishFileRead(sr, filename);
            if (!ReadPlayerData(sr, game.Map))
                return FinishFileRead(sr, filename);
            sr.Close();
            game.Map.GenerateDynamicLayer();
            game.Map.UpdateDynamicLayer();
            return game.Start();
        }

        private static bool ReadGameOptions(StreamReader sr, Game game)
        {
            string[] dataCells = sr.ReadLine().ToLower().Split(';');
            if (dataCells.Length < 2 || dataCells[0] != "options")
                return false;
            if (!int.TryParse(dataCells[1], out int optionCount))
                return false;
            for (var i = 2; i < optionCount + 2 && i < dataCells.Length; i++)
                game.CheckAndInvertOption(dataCells[i]);
            return true;
        }

        private static string WriteGameOptions(HashSet<GameOption> options)
        {
            var result = $"Options;{options.Count}";
            foreach (var option in options)
                result += $";{option}";
            result += "\r\n";
            return result;
        }

        private static bool ReadStaticCellMap(StreamReader sr, GameMap map)
        {
            // header
            string[] cellData = sr.ReadLine().ToLower().Split(';');            
            if (cellData.Length < 3 || cellData[0] != "staticcells")
                return false;
            if (!int.TryParse(cellData[1], out int width))
                return false;
            if (!int.TryParse(cellData[2], out int height))
                return false;
            // body
            map.StaticLayer = new List<List<IStaticCell>>();
            for (var y = 0; y < height; y++)
            {
                map.StaticLayer.Add(new List<IStaticCell>());
                cellData = sr.ReadLine().ToLower().Split(';');
                if (cellData.Length < 1)
                    return false;
                for (var x = 0; x < width; x++)
                    if (x < cellData[0].Length)
                        map.StaticLayer[y].Add(CharToStaticCell(cellData[0][x], 0));
                    else
                        map.StaticLayer[y].Add(null);
            }                
            return true;
        }        

        private static string WriteStaticCellMap(GameMap map)
        {
            var result = $"StaticCells;{map.Width};{map.Height}\r\n";
            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                    if ((map.StaticLayer[y][x] is IStaticCell) &&
                        !(map.StaticLayer[y][x] is IStaticCellWithID))
                        result += map.StaticLayer[y][x].DataFileChar;
                    else                    
                        if (map.StaticLayer[y][x] is IStaticCellWithID)
                            result += "?";
                        else
                            result += " ";                    
                result += "\r\n";                
            }
            return result;
        }

        private static bool ReadUniqueCellList(StreamReader sr, GameMap map)
        {
            // header
            string[] cellData = sr.ReadLine().ToLower().Split(';');
            if (cellData.Length < 2 || cellData[0] != "uniquecells")
                return false;
            if (!int.TryParse(cellData[1], out int cellCount))
                return false;
            // cell list
            for (var i = 0; i < cellCount; i++)
            {
                cellData = sr.ReadLine().ToLower().Split(';');
                if (cellData.Length < 4)
                    return false;
                if (!int.TryParse(cellData[1], out int x))
                    return false;
                if (!int.TryParse(cellData[2], out int y))
                    return false;
                if (!int.TryParse(cellData[3], out int id))
                    return false;
                map.StaticLayer[y][x] = CharToStaticCell(cellData[0][0], id);
            }
            return true;
        }

        private static string WriteUniqueCellList(GameMap map)
        {
            var result = "";
            int count = 0;
            for (var y = 0; y < map.Height; y++)
                for (var x = 0; x < map.Width; x++)
                    if (map.StaticLayer[y][x] is IStaticCellWithID)
                    {
                        result += $"{map.StaticLayer[y][x].DataFileChar};{x};{y};" +
                            $"{(map.StaticLayer[y][x] as IStaticCellWithID).ID}\r\n";
                        count++;
                    }
            result = $"UniqueCells;{count}\r\n" + result;
            return result;
        }

        private static bool ReadDynamicObjectList(StreamReader sr, GameMap map)
        {
            map.DynamicCells = new List<IDynamicCell>();
            // header
            string[] cellData = sr.ReadLine().ToLower().Split(';');
            if (cellData.Length < 2 || cellData[0] != "dynamiccells")
                return false;
            if (!int.TryParse(cellData[1], out int cellCount))
                return false;
            // cell list
            for (var i = 0; i < cellCount; i++)
            {
                cellData = sr.ReadLine().ToLower().Split(';');
                if (cellData.Length < 3)
                    return false;
                if (!int.TryParse(cellData[1], out int x))
                    return false;
                if (!int.TryParse(cellData[2], out int y))
                    return false;
                map.DynamicCells.Add(CharToDynamicCell(cellData[0][0], x, y));
            }
            return true;
        }

        private static bool ReadPlayerData(StreamReader sr, GameMap map)
        {
            string[] playerData = sr.ReadLine().ToLower().Split(';');
            if (playerData.Length < 7 || playerData[0] != "@")
                return false;
            if (!int.TryParse(playerData[1], out int x))
                return false;
            if (!int.TryParse(playerData[2], out int y))
                return false;            
            if (!int.TryParse(playerData[3], out int moves))
                return false;
            if (!int.TryParse(playerData[4], out int maxMoves))
                return false;
            if (!int.TryParse(playerData[5], out int force))
                return false;
            if (!int.TryParse(playerData[6], out int bombCount))
                return false;
            map.Player = new GamePlayer(x, y)
            {
                MaxMoves = maxMoves,
                Moves = moves,
                Force = force,
                BombCount = bombCount,
                Keys = new List<int>()
            };
            string[] keyData = sr.ReadLine().ToLower().Split(';');
            if (keyData.Length < 2 || keyData[0] != "keys")
                return false;
            if (!int.TryParse(keyData[1], out int keyCount))
                return false;
            for (var i = 0; i < keyCount; i++)
            {
                if (!int.TryParse(keyData[i + 2], out int key))
                    return false;
                map.Player.Keys.Add(key);
            }
            return true;
        }

        private static string WriteDynamicObjectList(GameMap map)
        {
            var result = $"DynamicCells;{map.DynamicCells.Count}\r\n";
            foreach (var cell in map.DynamicCells)
            {
                if (cell is Box)
                    result += $"{cell.DataFileChar};{cell.Position.X};{cell.Position.Y}\r\n";
            }
            result += $"{map.Player.DataFileChar};{map.Player.Position.X};{map.Player.Position.Y};" +
                $"{map.Player.Moves};{map.Player.MaxMoves};{map.Player.Force};{map.Player.BombCount}\r\n" +
                $"Keys;{map.Player.Keys.Count}";
            foreach (var key in map.Player.Keys)
                result += $";{key}";
            result += $"\r\n";
            return result;
        }

        private static bool IsValidFile(string fullFileName)
        {
            FileInfo fi = null;
            try
            {
                fi = new FileInfo(fullFileName);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }
            if ((fi is null) || !File.Exists(fullFileName))
                return false;
            StreamReader sr = new StreamReader(fullFileName);
            var firstLine = sr.ReadLine();
            sr.Close();
            if (firstLine != null &&
                firstLine.ToLower().StartsWith("sokoban 2021"))
                return true;
            return false;
        }

        private static IStaticCell CharToStaticCell(char cellCode, int id)
        {
            return cellCode switch
            {
                '#' => new Wall(),
                '*' => new Cage(),
                '=' => new Bomb(),
                '_' => new Plate(id),
                '+' => new Key(id),
                '|' => new Door(id),
                _   => null,
            };
        }

        private static IDynamicCell CharToDynamicCell(char cellCode, int x, int y)
        {
            return cellCode switch
            {
                '%' => new Box(x, y),
                _   => null,
            };
        }
    }
}