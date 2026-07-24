using Godot;
using System;
using System.Collections.Generic;
using Orclimax.Core;

namespace Orclimax.Autoload
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        [Signal] public delegate void StateChangedEventHandler(int newState);
        [Signal] public delegate void GoldChangedEventHandler(int newGold);
        [Signal] public delegate void StageChangedEventHandler(int newStage);
        [Signal] public delegate void VesselUnlockedEventHandler(string femaleId);

        private GameState _currentState = GameState.Title;
        private int _gold = 15;
        private int _currentStage = 1;

        public string CurrentSelectedStageId { get; set; } = "flame_spire";

        public const string TitleScenePath = "res://src/ui/title/TitleScreen.tscn";
        public const string VesselSelectScenePath = "res://src/ui/vessel/VesselUI.tscn";
        public const string BackpackScenePath = "res://src/ui/backpack/BackpackUI.tscn";
        public const string MapScenePath = "res://src/ui/map/MapUI.tscn";
        public const string CombatScenePath = "res://src/entities/player/Level.tscn";

        private readonly Dictionary<GameState, string> _scenePaths = new Dictionary<GameState, string>
        {
            { GameState.Title, TitleScenePath },
            { GameState.VesselSelect, VesselSelectScenePath },
            { GameState.Backpack, BackpackScenePath },
            { GameState.WorldMap, MapScenePath },
            { GameState.Combat, CombatScenePath }
        };

        public GameState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    EmitSignal(SignalName.StateChanged, (int)_currentState);
                }
            }
        }

        public int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
                EmitSignal(SignalName.GoldChanged, _gold);
            }
        }

        public int CurrentStage
        {
            get => _currentStage;
            set
            {
                _currentStage = value;
                EmitSignal(SignalName.StageChanged, _currentStage);
            }
        }

        // List of unlocked females
        public Godot.Collections.Array<string> UnlockedFemaleIds { get; private set; } = new Godot.Collections.Array<string>();

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Ready()
        {
            // Initial unlocked character: Human Maiden Maye
            if (!UnlockedFemaleIds.Contains("girl_maye"))
            {
                UnlockedFemaleIds.Add("girl_maye");
            }
            _gold = GameConfig.Instance != null ? GameConfig.Instance.InitialGold : 15;
        }

        public void UnlockFemale(string femaleId)
        {
            if (!UnlockedFemaleIds.Contains(femaleId))
            {
                UnlockedFemaleIds.Add(femaleId);
                EmitSignal(SignalName.VesselUnlocked, femaleId);
            }
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            if (_scenePaths.TryGetValue(newState, out string path))
            {
                GetTree().ChangeSceneToFile(path);
            }
        }

        public void StartNewGame()
        {
            Gold = GameConfig.Instance != null ? GameConfig.Instance.InitialGold : 15;
            CurrentStage = 1;
            GoToVesselSelect();
        }

        public void GoToTitle() => ChangeState(GameState.Title);
        public void GoToVesselSelect() => ChangeState(GameState.VesselSelect);
        public void GoToBackpack() => ChangeState(GameState.Backpack);
        public void GoToMap() => ChangeState(GameState.WorldMap);

        public void StartCombatNode()
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.StartCombat();
            }
            ChangeState(GameState.Combat);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public bool SpendGold(int amount)
        {
            if (Gold >= amount)
            {
                Gold -= amount;
                return true;
            }
            return false;
        }

        public void AdvanceStage()
        {
            CurrentStage++;
            int bonus = GameConfig.Instance != null ? GameConfig.Instance.StageClearGold : 10;
            AddGold(bonus);
            GoToMap();
        }

        public void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.F1)
                {
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                    DisplayServer.WindowSetSize(new Vector2I(800, 600));
                    CenterWindow();
                }
                else if (keyEvent.Keycode == Key.F2)
                {
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                    DisplayServer.WindowSetSize(new Vector2I(1600, 1200));
                    CenterWindow();
                }
                else if (keyEvent.Keycode == Key.F3 || keyEvent.Keycode == Key.F11)
                {
                    if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
                    {
                        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                        DisplayServer.WindowSetSize(new Vector2I(1600, 1200));
                        CenterWindow();
                    }
                    else
                    {
                        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                    }
                }
            }
        }

        private void CenterWindow()
        {
            var screenSize = DisplayServer.ScreenGetSize();
            var windowSize = DisplayServer.WindowGetSize();
            DisplayServer.WindowSetPosition((screenSize - windowSize) / 2);
        }
    }
}
