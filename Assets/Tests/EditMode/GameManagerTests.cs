using NUnit.Framework;
using UnityEngine;
using EmpireOfGlass.Core;

namespace EmpireOfGlass.Tests.EditMode
{
    /// <summary>
    /// Unit tests for GameManager state transitions and forced rotation loop.
    /// </summary>
    [TestFixture]
    public class GameManagerTests
    {
        private GameObject testObject;
        private GameManager gameManager;

        [SetUp]
        public void SetUp()
        {
            // Clean up any existing instance
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }

            testObject = new GameObject("TestGameManager");
            gameManager = testObject.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void GameManager_InitializesSingleton()
        {
            Assert.IsNotNull(GameManager.Instance);
        }

        [Test]
        public void TransitionTo_ChangesState()
        {
            gameManager.TransitionTo(GameManager.GameState.Swarm);
            Assert.AreEqual(GameManager.GameState.Swarm, gameManager.CurrentState);
        }

        [Test]
        public void AdvanceLoop_FollowsRotation_SwarmToCity()
        {
            gameManager.TransitionTo(GameManager.GameState.Swarm);
            gameManager.AdvanceLoop();
            Assert.AreEqual(GameManager.GameState.City, gameManager.CurrentState);
        }

        [Test]
        public void AdvanceLoop_FollowsRotation_CityToRaid()
        {
            gameManager.TransitionTo(GameManager.GameState.City);
            gameManager.AdvanceLoop();
            Assert.AreEqual(GameManager.GameState.Raid, gameManager.CurrentState);
        }

        [Test]
        public void AdvanceLoop_FollowsRotation_RaidToSwarm()
        {
            gameManager.TransitionTo(GameManager.GameState.Raid);
            gameManager.AdvanceLoop();
            Assert.AreEqual(GameManager.GameState.Swarm, gameManager.CurrentState);
        }

        [Test]
        public void StartFTUE_TransitionsToSwarm()
        {
            gameManager.StartFTUE();
            Assert.AreEqual(GameManager.GameState.Swarm, gameManager.CurrentState);
        }
    }
}
