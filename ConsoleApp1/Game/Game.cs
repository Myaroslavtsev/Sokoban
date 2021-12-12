using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Sokoban
{
    class Game
    {        
        public bool QuitRequested { get; private set; }
        //public string CommandResult { get; private set; }
        public GameMap Map { get; private set; }        
        public int MaxMoves { get; private set; }
        public HashSet<GameOption> GameOptions { get; private set; }
        public bool Playable { get; private set; }

        public Game(string filename)
        {
            GenerateGameObjects();
            SetInitialGameData(filename);
        }

        public void MovePlayer(Point direction)
        {
            if (!Playable)
                return;
            var newPos = Map.Player.Position.Add(direction);
            var boxCount = MovableBoxes(newPos, direction);
            if (!Map.Possible(newPos) || boxCount < 0)
                return;
            if ((Map.StaticLayer[newPos.Y][newPos.X] is null) ||
                Map.StaticLayer[newPos.Y][newPos.X].AllowsToEnter(Map.Player, this))
            {
                Map.Player.CellAction = new DynamicAction(direction, false, null);
            }
            for (var i = 0; i < boxCount; i++)
            {
                Map.DynamicLayer[newPos.Y][newPos.X].CellAction = new DynamicAction(direction, false, null);
                newPos = newPos.Add(direction);
            }
            PerformCellActions();                        
        }

        public void UpdateCells()
        {
            if (GameOptions.Contains(GameOption.Gravity))
            {
                var down = new Point(0, 1);
                for (var y = Map.Height - 2; y >= 0; y--)
                {
                    for (var x = 0; x < Map.Width; x++)
                    {
                        if (Map.DynamicLayer[y][x] is Box)
                        {
                            if ((Map.DynamicLayer[y + 1][x] is null) &&((Map.StaticLayer[y + 1][x] is null) ||
                                Map.StaticLayer[y + 1][x].AllowsToEnter(Map.DynamicLayer[y][x] as Box, this)))
                                Map.DynamicLayer[y][x].CellAction = new DynamicAction(down, false, null);
                        }
                    }
                    PerformCellActions();
                }
            }
            PerformCellActions();            
        }

        public string CheckGameState()
        {
            if (!Playable)
                return string.Empty;                        
            if (PlayerWin())
            {
                Playable = false;
                return "Player win!";
            }
            if (MoveLimitReached())
            {
                Playable = false;
                return "No more moves";
            }
            return string.Empty;
        }

        public string DoCommand(string command)
        {
            var commandWords = command.ToLower().Split(' ');
            switch (commandWords[0])
            {
                case "quit":
                    QuitRequested = true;
                    return "";
                case "help":
                    return Files.GetHelpMessage();
                case "about":
                    return Files.GetAboutMessage();
                case "levels":
                    return Files.GetLevelList();
                case "options":
                    return MakeOptionList();
                case "load":
                    return commandWords.Length == 1 ? 
                        Files.LoadGame("savegame.csv") : Files.LoadGame(commandWords[1]);
                case "save":
                    return commandWords.Length == 1 ? 
                        Files.SaveGame("savegame.csv") : Files.SaveGame(commandWords[1]);
                case "addmoves":
                    return commandWords.Length == 1 ? 
                        "Move count not specified" : AddMoves(commandWords[1]);
                case "setforce":
                    return commandWords.Length == 1 ? 
                        "Force value not specified" : SetPlayerForce(commandWords[1]);
            }
            object option;
            if (Enum.TryParse(typeof(GameOption), commandWords[0], true, out option))
                if (option.ToString().ToLower() == commandWords[0] && 
                    Enum.IsDefined(typeof(GameOption), option)) 
                    return InvertOption((GameOption)option);
            return "Type help for help";
        }

        private int MovableBoxes(Point position, Point direction)
        {
            int boxCount = 0;
            while (Map.Possible(position) && (Map.DynamicLayer[position.Y][position.X] is Box))
            {
                boxCount++;
                position = position.Add(direction);
            }
            if (boxCount > Map.Player.Force)
                return -1;
            if (boxCount == 0)
                return 0;
            if (Map.Possible(position) && ((Map.StaticLayer[position.Y][position.X] is null) ||
                Map.StaticLayer[position.Y][position.X].AllowsToEnter(
                Map.DynamicLayer[position.Y - direction.Y][position.X - direction.X], this)))
                return boxCount;
            return -1;
        }

        private void PerformCellActions()
        {            
            PerformStaticCellActions();   
            PerformDynamicCellActions();
            PerformPlayerActions(Map.Player);
            Map.MakeDynamicLayer();
        }

        private void PerformStaticCellActions()
        {
            for (var y = 0; y < Map.Height; y++)
                for (var x = 0; x < Map.Width; x++)
                {
                    if (!(Map.StaticLayer[y][x] is null) &&
                        !(Map.StaticLayer[y][x].CellAction is null))
                    {
                        if (Map.StaticLayer[y][x].CellAction.WillTransform)
                            Map.StaticLayer[y][x] = Map.StaticLayer[y][x].CellAction.TransformTo;

                    }
                    if (!(Map.StaticLayer[y][x] is null) &&
                        !(Map.StaticLayer[y][x].CellAction is null))
                        Map.StaticLayer[y][x].CellAction = null;
                }
        }

        private void PerformDynamicCellActions()
        {
            for (int i = 0; i < Map.DynamicCells.Count; i++)
            {
                if (!(Map.DynamicCells[i].CellAction is null))
                {
                    if (Map.DynamicCells[i].CellAction.Move.X != 0 || Map.DynamicCells[i].CellAction.Move.Y != 0)
                        Map.DynamicCells[i].Position = 
                            Map.DynamicCells[i].Position.Add(Map.DynamicCells[i].CellAction.Move);
                    if (Map.DynamicCells[i].CellAction.WillTransform)
                        Map.DynamicCells[i] = Map.DynamicCells[i].CellAction.TransformTo;
                    
                }
                if (!(Map.DynamicCells[i].CellAction is null))
                    Map.DynamicCells[i].CellAction = null;
            }
        }

        private void PerformPlayerActions(GamePlayer player)
        {
            if (player.CellAction != null)
            {
                if (player.CellAction.Move.X != 0 || player.CellAction.Move.Y != 0)
                {
                    player.Position = player.Position.Add(player.CellAction.Move);
                    player.CountMove();
                }
                player.CellAction = null;
            }
        }

        private bool PlayerWin()
        {
            var finished = true;
            foreach (var cell in Map.DynamicCells)
                if (cell is Box && !(Map.StaticLayer[cell.Position.Y][cell.Position.X] is Cage))
                {
                    finished = false;
                    break;
                }
            return finished;
        }

        private bool MoveLimitReached()
        {
            return GameOptions.Contains(GameOption.MoveLimit) && Map.Player.Moves >= MaxMoves;
        }

        private string InvertOption(GameOption option)
        {
            if (GameOptions.Contains(option))
            {
                GameOptions.Remove(option);
                return $"Option {option} switched off";
            }
            GameOptions.Add(option);
            return $"Option {option} switched on";
        }

        private string MakeOptionList()
        {
            if (GameOptions.Count == 0)
                return "No active options";
            var optionList = "";
            foreach (var option in GameOptions)
            {
                optionList += option.ToString() + ", ";
            }
            return optionList.Substring(0, Math.Max(0, optionList.Length - 2));
        }

        private string AddMoves(string moves)
        {            
            if (!Playable)
                return "You are not playing";
            if (!GameOptions.Contains(GameOption.MoveLimit))
                return "There is no move limit";
            bool isParsable = int.TryParse(moves, out int moveCount);
            if (!isParsable || !Map.Player.AddMoves(ref moveCount))
                return "Incorrect move value";
            return $"{moveCount} moves added";
        }

        private string SetPlayerForce(string force)
        {
            if (!Playable)
                return "You are not playing";
            bool isParsable = int.TryParse(force, out int forceValue);
            if (!isParsable || !Map.Player.SetForce(forceValue))
                return "Incorrect force value";
            return $"Player force set to {Map.Player.Force}";
        }

        private void GenerateGameObjects()
        {
            Map = new GameMap();
            GameOptions = new HashSet<GameOption>();
        }

        private void SetInitialGameData(string filename)
        {
            Files.LoadGame(filename);
            Map.GenerateTestMap(); // use for debug only
            MaxMoves = 100;
            Playable = true; // for debug
        }
    }
}
