using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sokoban
{
    static class Files
    {
        public static string LoadGame(string filename)
        {
            var fullFileName = Directory.GetCurrentDirectory() + "\\levels\\" + filename;
            if (!File.Exists(fullFileName))
                return $"File {filename} not found";
            // loading file
            return $"{filename} loaded";
        }

        public static string SaveGame(string filename)
        {
            var fullFileName = Directory.GetCurrentDirectory() + "\\levels\\" + filename;
            FileInfo fi = null;
            try
            {
                fi = new FileInfo(fullFileName);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }
            if (ReferenceEquals(fi, null))
                return $"File name {filename} is invalid";
            var overwritten = File.Exists(fullFileName);
            // saving file
            if (overwritten)
                return $"File {filename} overwritten";
            return $"File {filename} created";
        }

        public static string GetLevelList()
        {
            return "Valid game files in \\levels directory:\r\n";
        }

        public static string GetHelpMessage()
        {
            return
                "Press arrow keys to play if game is running.\r\n" +
                "  Command line commands implemented:\r\n" +
                "   quit - quit without saving\r\n" +
                //"   levels - get available level list\r\n" +
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
    }
}
