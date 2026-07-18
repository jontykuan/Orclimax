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

        private GameState _currentState = GameState.Shop;
        private int _gold = 15; // Starting gold
        private int _currentStage = 1;

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
            // Initial unlocked characters (e.g. starting girl)
            UnlockedFemaleIds.Add("girl_01");
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
            CurrentState = GameState.Shop;
        }

        public void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;
        }
    }
}
