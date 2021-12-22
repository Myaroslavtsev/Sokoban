using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sokoban
{
    static class Files
    {
        public static string LoadGame(string fileName, Game game)
        {
            if (!File.Exists(fileName))
                return $"File {Path.GetFileName(fileName)} not found";
            if (!IsValidFile(fileName))
                return $"File {Path.GetFileName(fileName)} is not a saved game";
            var readResult = ReadGameFromFile(fileName, game);
            if (readResult == string.Empty)
            {
                game.MapChanged = true;
                return $"{Path.GetFileName(fileName)} loaded";
            }
            return readResult;
        }

        public static string SaveGame(string fileName, Game game)
        {
            if (!IsValidPath(fileName, true))
                return $"{fileName} is not a valid file path";
            var overwritten = File.Exists(fileName);
            SaveGameToFile(fileName, game);
            if (overwritten)
                return $"File {Path.GetFileName(fileName)} overwritten";
            return $"File {Path.GetFileName(fileName)} created";
        }

        public static string GetLevelList(string currDirectory)
        {
            if (!Directory.Exists(currDirectory))
                return "Directory not found";
            string[] files = Directory.GetFiles(currDirectory, "*.csv");
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
                return $"{validFileCount} valid game files in current directory:\r\n" + result;
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
                "   directory - levels directory name\r\n" +
                "   cd dirname - change directory\r\n" +
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

        private static string ReadGameFromFile(string fileName, Game game)
        {
            game.Stop();
            StreamReader sr = new StreamReader(fileName);
            var line = sr.ReadLine();
            if (!line.ToLower().StartsWith("sokoban 2021"))
                return FinishFileRead(sr, fileName);
            if (!ReadGameOptions(sr, game))
                return FinishFileRead(sr, fileName);
            if (!ReadStaticCellMap(sr, game.Map))
                return FinishFileRead(sr, fileName);
            if (!ReadUniqueCellList(sr, game.Map))
                return FinishFileRead(sr, fileName);
            if (!ReadDynamicObjectList(sr, game.Map))
                return FinishFileRead(sr, fileName);
            if (!ReadPlayerData(sr, game.Map))
                return FinishFileRead(sr, fileName);
            sr.Close();
            return game.Start();
        }

        private static bool ReadGameOptions(StreamReader sr, Game game)
        {            
            game.GameOptions.Clear();
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
            map.StaticLayer = new MapLayer(width, height);
            for (var y = 0; y < height; y++)
            {
                cellData = sr.ReadLine().ToLower().Split(';');
                if (cellData.Length < 1)
                    return false;
                for (var x = 0; x < width; x++)
                    if (x < cellData[0].Length)
                        if (cellData[0][x] == '*' || cellData[0][x] == '#' || cellData[0][x] == '=')
                            map.StaticLayer.AddCell(CharToMapCell(cellData[0][x], x, y, 0, 0, 0));                    
            }                
            return true;
        }        

        private static string WriteStaticCellMap(GameMap map)
        {
            var result = $"StaticCells;{map.Width};{map.Height}\r\n";
            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    var cell = map.StaticLayer.GetByPosition(x, y);
                    if (cell is null)
                        result += " ";
                    else
                        if ((cell is ICellWithID) || (cell is ICellWithDestination))
                            result += "?";
                        else
                            result += cell.DataFileChar;
                }
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
                if (CharToCellType(cellData[0][0]) == CellTypes.Portal)
                {
                    if (!int.TryParse(cellData[3], out int destinationX))
                        return false;
                    if (!int.TryParse(cellData[4], out int destinationY))
                        return false;
                    map.StaticLayer.AddCell(CharToMapCell(cellData[0][0], x, y, 0, destinationX, destinationY));
                }
                else
                {
                    if (!int.TryParse(cellData[3], out int id))
                        return false;
                    map.StaticLayer.AddCell(CharToMapCell(cellData[0][0], x, y, id, 0, 0));
                }
            }
            return true;
        }

        private static string WriteUniqueCellList(GameMap map)
        {
            var result = "";
            var count = 0;
            foreach (var cell in map.StaticLayer.Cells)
            {
                if (cell is ICellWithID)
                {
                    result += $"{cell.DataFileChar};{cell.Position.X};{cell.Position.Y};" +
                        $"{(cell as ICellWithID).ID}\r\n";
                    count++;
                }
                if (cell is ICellWithDestination)
                {
                    result += $"{cell.DataFileChar};{cell.Position.X};{cell.Position.Y};" +
                        $"{(cell as ICellWithDestination).Destination.X};" +
                        $"{(cell as ICellWithDestination).Destination.Y}\r\n";
                    count++;
                }
            }
            result = $"UniqueCells;{count}\r\n" + result;
            return result;
        }

        private static bool ReadDynamicObjectList(StreamReader sr, GameMap map)
        {
            map.DynamicLayer = new MapLayer(map.StaticLayer.Width, map.StaticLayer.Height);
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
                map.DynamicLayer.AddCell(CharToMapCell(cellData[0][0], x, y, 0, 0, 0));
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
            map.DynamicLayer.AddCell(new GamePlayer(x, y)
                {
                    MaxMoves = maxMoves,
                    Moves = moves,
                    Force = force,
                    BombCount = bombCount,
                    Keys = new List<int>()
                }
            );
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
            var result = "";
            var count = 0;
            foreach (var cell in map.DynamicLayer.Cells)
            {
                if (cell is Box)
                {
                    result += $"{cell.DataFileChar};{cell.Position.X};{cell.Position.Y}\r\n";
                    count++;
                }
            }
            result = $"DynamicCells;{count}\r\n" + result;
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

        private static bool IsValidPath(string path, bool allowRelativePaths = false)
        {
            bool isValid = true;
            try
            {
                string fullPath = Path.GetFullPath(path);
                if (allowRelativePaths)
                {
                    isValid = Path.IsPathRooted(path);
                }
                else
                {
                    string root = Path.GetPathRoot(path);
                    isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                }
            }
            catch (Exception ex)
            {
                isValid = false;
            }
            return isValid;
        }

        private static CellTypes CharToCellType(char cellCode)
        {
            return cellCode switch
            {
                '_' => CellTypes.Plate,
                '+' => CellTypes.Key,
                '|' => CellTypes.Door,
                '0' => CellTypes.Portal,
                '#' => CellTypes.Wall,
                '*' => CellTypes.Cage,
                '=' => CellTypes.Bomb,
                '%' => CellTypes.Box,
                _ => CellTypes.NoCell
            };
        }

        private static IMapCell CharToMapCell(char cellCode, int x, int y, 
            int id, int destinationX, int destinationY)
        {
            return cellCode switch
            {
                '_' => new Plate(x, y, id),
                '+' => new Key(x, y, id),
                '|' => new Door(x, y, id),
                '0' => new Portal(x, y, destinationX, destinationY),
                '#' => new Wall(x, y),
                '*' => new Cage(x, y),
                '=' => new Bomb(x, y),
                '%' => new Box(x, y),
                _ => null,
            };
        }
    }
}