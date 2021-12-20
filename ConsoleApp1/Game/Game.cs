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
            {
                RealizeGravity();
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
            if ((Map.StaticLayer[newPos.Y][newPos.X] is null) ||
                Map.StaticLayer[newPos.Y][newPos.X].AllowsToEnter(Map.Player, Map, GameOptions))
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
                    if (Map.DynamicLayer[y][x] is Box)
                    {
                        if ((Map.DynamicLayer[y + 1][x] is null) && ((Map.StaticLayer[y + 1][x] is null) ||
                            Map.StaticLayer[y + 1][x].AllowsToEnter(Map.DynamicLayer[y][x] as Box, Map, GameOptions)))
                            Map.DynamicLayer[y][x].CellAction = new DynamicAction(down, false, null);
                    }
                }
                PerformCellActions();
            }
        }

        private void PerformCellActions()
        {            
            PerformStaticCellActions();   
            PerformDynamicCellActions();
            PerformPlayerActions(Map.Player);
            Map.UpdateDynamicLayer();
        }

        private void PerformStaticCellActions()
        {
            for (var y = 0; y < Map.Height; y++)
                for (var x = 0; x < Map.Width; x++)
                {
                    if (!(Map.StaticLayer[y][x] is null) &&
                        !(Map.StaticLayer[y][x].CellAction is null))
                    
                        if (Map.StaticLayer[y][x].CellAction.WillTransform)
                        {
                            Map.StaticLayer[y][x] = Map.StaticLayer[y][x].CellAction.TransformTo;
                            MapChanged = true;
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
                    {
                        Map.DynamicCells[i].Position =
                            Map.DynamicCells[i].Position.Add(Map.DynamicCells[i].CellAction.Move);
                        MapChanged = true;
                    }
                    if (Map.DynamicCells[i].CellAction.WillTransform)
                    {
                        Map.DynamicCells[i] = Map.DynamicCells[i].CellAction.TransformTo;
                        MapChanged = true;
                    }                    
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
                    MapChanged = true;
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
            // following 3 lines are for debug only
            /*Map.GenerateTestMap(); 
            MaxMoves = 100; 
            Playable = true;*/
        }
    }
}