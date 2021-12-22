using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Sokoban
{
    class Game
    {
        public GameMap Map { get; private set; }
        public bool MapChanged { get; set; }
        public HashSet<GameOption> GameOptions { get; private set; }
        public bool Playable { get; private set; }

        public Game(string filename)
        {
            GenerateGameObjects();
            SetInitialGameData(filename);
        }

        public string Start()
        {
            var gameState = CheckGameState();
            if (gameState == string.Empty)
                Playable = true;
            return gameState;
        }

        public void Stop()
        {
            Playable = false;
        }        

        public void UpdateCells()
        {
            if (GameOptions.Contains(GameOption.Gravity))            
                RealizeGravity();
            RealizePortals();
            PerformCellActions();
        }        

        public string CheckGameState()
        {
            if (!Playable)
                return string.Empty;
            if (PlayerWin())
            {
                Playable = false;
                MapChanged = true;
                return "Player win!";
            }
            if (MoveLimitReached())
            {
                Playable = false;
                MapChanged = true;
                return "No more moves";
            }
            return string.Empty;
        }

        public string CheckAndInvertOption(string optionName)
        {
            if (Enum.TryParse(typeof(GameOption), optionName, true, out object option))
                if (option.ToString().ToLower() == optionName &&
                    Enum.IsDefined(typeof(GameOption), option))
                    return InvertOption((GameOption)option);
            return string.Empty;
        }

        public void MovePlayer(Point direction)
        {
            if (!Playable)
                return;
            var newPos = Map.Player.Position.Add(direction);
            var boxCount = Map.MovableBoxes(newPos, direction, GameOptions);
            if (!Map.PositionPossible(newPos) || boxCount < 0)
                return;            
            if ((Map.StaticLayer.GetByPosition(newPos) is null) ||
                Map.StaticLayer.GetByPosition(newPos).AllowsToEnter(Map.Player, Map, GameOptions))
            {
                Map.Player.CellAction = new MapCellAction(direction, false, null);
            }
            for (var i = 0; i < boxCount; i++)
            {
                Map.DynamicLayer.GetByPosition(newPos).CellAction = new MapCellAction(direction, false, null);
                newPos = newPos.Add(direction);
            }
            PerformCellActions();
        }

        public string MakeOptionList()
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

        private void RealizeGravity()
        {
            var down = new Point(0, 1);
            for (var y = Map.Height - 2; y >= 0; y--)
            {
                for (var x = 0; x < Map.Width; x++)
                {
                    if (Map.DynamicLayer.GetByPosition(x, y) is Box)
                    {
                        if ((Map.DynamicLayer.GetByPosition(x, y+1) is null) && 
                            ((Map.StaticLayer.GetByPosition(x, y+1) is null) ||
                            Map.StaticLayer.GetByPosition(x, y+1).
                                AllowsToEnter(Map.DynamicLayer.GetByPosition(x, y) as Box, Map, GameOptions)))
                            Map.DynamicLayer.GetByPosition(x, y).CellAction = new MapCellAction(down, false, null);
                    }
                }
                PerformCellActions();
            }
        }

        private void RealizePortals()
        {
            foreach(var cell in Map.StaticLayer.Cells)
                if ((cell is Portal) && !(Map.DynamicLayer.GetByPosition(cell.Position) is null))                
                    cell.AllowsToEnter(Map.DynamicLayer.GetByPosition(cell.Position), Map, GameOptions);                
        }

        private void PerformCellActions()
        {
            MapChanged = false;
            MapChanged |= Map.StaticLayer.DoCellActions();
            MapChanged |= Map.DynamicLayer.DoCellActions();
        }

        private bool PlayerWin()
        {
            var finished = true;
            foreach (var cell in Map.DynamicLayer.Cells)
                if (cell is Box && !(Map.StaticLayer.GetByPosition(cell.Position) is Cage))
                {
                    finished = false;
                    break;
                }
            return finished;
        }

        private bool MoveLimitReached()
        {
            return GameOptions.Contains(GameOption.MoveLimit) && 
                Map.Player.Moves >= Map.Player.MaxMoves;
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

        private void GenerateGameObjects()
        {
            Map = new GameMap();
            GameOptions = new HashSet<GameOption>();
        }

        private void SetInitialGameData(string filename)
        {
            Files.LoadGame(filename, this);
        }
    }
}