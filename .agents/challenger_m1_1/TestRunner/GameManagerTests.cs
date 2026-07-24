using System.Collections.Generic;
using NUnit.Framework;
using Orclimax.Autoload;
using Orclimax.Core;

namespace Orclimax.EmpiricalTests
{
    [TestFixture]
    public class GameManagerTests
    {
        private GameManager _gameManager;

        [SetUp]
        public void Setup()
        {
            _gameManager = new GameManager();
        }

        [Test]
        public void InitialState_IsTitle()
        {
            Assert.That(_gameManager.CurrentState, Is.EqualTo(GameState.Title));
        }

        [Test]
        public void StateTransitions_PropagateToAll7States_AndEmitStateChangedSignal()
        {
            var emittedStates = new List<int>();
            _gameManager.Connect(GameManager.SignalName.StateChanged, Callable.From<int>(state =>
            {
                emittedStates.Add(state);
            }));

            var statesToTest = new[]
            {
                GameState.VesselSelect,
                GameState.Backpack,
                GameState.WorldMap,
                GameState.Combat,
                GameState.GameOver,
                GameState.Victory,
                GameState.Title
            };

            foreach (var state in statesToTest)
            {
                _gameManager.CurrentState = state;
                Assert.That(_gameManager.CurrentState, Is.EqualTo(state));
            }

            var expectedEmitted = new List<int>
            {
                (int)GameState.VesselSelect,
                (int)GameState.Backpack,
                (int)GameState.WorldMap,
                (int)GameState.Combat,
                (int)GameState.GameOver,
                (int)GameState.Victory,
                (int)GameState.Title
            };

            Assert.That(emittedStates, Is.EqualTo(expectedEmitted));
        }

        [Test]
        public void SelfTransition_DoesNotEmitSignal()
        {
            int signalCount = 0;
            _gameManager.Connect(GameManager.SignalName.StateChanged, Callable.From<int>(_ =>
            {
                signalCount++;
            }));

            _gameManager.CurrentState = GameState.Title; // Already Title
            Assert.That(signalCount, Is.EqualTo(0));
        }

        [Test]
        public void GoldChangedSignal_EmitsOnGoldPropertySetAndAddSpend()
        {
            var goldHistory = new List<int>();
            _gameManager.Connect(GameManager.SignalName.GoldChanged, Callable.From<int>(newGold =>
            {
                goldHistory.Add(newGold);
            }));

            _gameManager.Gold = 50;
            _gameManager.AddGold(20);
            bool spentSuccess = _gameManager.SpendGold(30);
            bool spentFailed = _gameManager.SpendGold(100);

            Assert.That(spentSuccess, Is.True);
            Assert.That(spentFailed, Is.False);
            Assert.That(_gameManager.Gold, Is.EqualTo(40));

            var expectedGoldHistory = new List<int> { 50, 70, 40 };
            Assert.That(goldHistory, Is.EqualTo(expectedGoldHistory));
        }

        [Test]
        public void StageChangedSignal_EmitsOnCurrentStagePropertySet()
        {
            var stageHistory = new List<int>();
            _gameManager.Connect(GameManager.SignalName.StageChanged, Callable.From<int>(newStage =>
            {
                stageHistory.Add(newStage);
            }));

            _gameManager.CurrentStage = 2;
            _gameManager.CurrentStage = 5;

            Assert.That(stageHistory, Is.EqualTo(new List<int> { 2, 5 }));
        }

        [Test]
        public void TriggerGameOver_SetsStateToGameOver_AndEmitsSignal()
        {
            int lastEmittedState = -1;
            _gameManager.Connect(GameManager.SignalName.StateChanged, Callable.From<int>(s =>
            {
                lastEmittedState = s;
            }));

            _gameManager.TriggerGameOver();

            Assert.That(_gameManager.CurrentState, Is.EqualTo(GameState.GameOver));
            Assert.That(lastEmittedState, Is.EqualTo((int)GameState.GameOver));
        }

        [Test]
        public void All7EnumValues_AreUniqueAndCoverExpectedIndices()
        {
            var values = (GameState[])System.Enum.GetValues(typeof(GameState));
            Assert.That(values.Length, Is.EqualTo(7));
            Assert.That((int)GameState.Title, Is.EqualTo(0));
            Assert.That((int)GameState.VesselSelect, Is.EqualTo(1));
            Assert.That((int)GameState.Backpack, Is.EqualTo(2));
            Assert.That((int)GameState.WorldMap, Is.EqualTo(3));
            Assert.That((int)GameState.Combat, Is.EqualTo(4));
            Assert.That((int)GameState.GameOver, Is.EqualTo(5));
            Assert.That((int)GameState.Victory, Is.EqualTo(6));
        }
    }
}
