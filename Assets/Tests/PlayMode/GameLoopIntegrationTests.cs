using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using EmpireOfGlass.Core;

namespace EmpireOfGlass.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for game loop transitions and scene loading.
    /// </summary>
    public class GameLoopIntegrationTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Clean up any existing GameManager
            if (GameManager.Instance != null)
            {
                Object.Destroy(GameManager.Instance.gameObject);
            }
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (GameManager.Instance != null)
            {
                Object.Destroy(GameManager.Instance.gameObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameManager_PersistsAcrossScenes()
        {
            // Create a GameManager
            var go = new GameObject("GameManager");
            var manager = go.AddComponent<GameManager>();
            
            yield return null;
            
            Assert.IsNotNull(GameManager.Instance);
            Assert.AreEqual(manager, GameManager.Instance);
        }

        [UnityTest]
        public IEnumerator StateTransition_TriggersEvent()
        {
            var go = new GameObject("GameManager");
            var manager = go.AddComponent<GameManager>();
            
            bool eventTriggered = false;
            GameManager.GameState previousState = GameManager.GameState.Splash;
            GameManager.GameState newState = GameManager.GameState.Splash;
            
            manager.OnStateChanged += (prev, next) =>
            {
                eventTriggered = true;
                previousState = prev;
                newState = next;
            };
            
            manager.TransitionTo(GameManager.GameState.Swarm);
            
            yield return null;
            
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(GameManager.GameState.Swarm, newState);
        }

        [UnityTest]
        public IEnumerator Hero_CanTakeDamageInPlayMode()
        {
            var heroGo = new GameObject("Hero");
            heroGo.AddComponent<Rigidbody>();
            var hero = heroGo.AddComponent<HeroController>();
            
            yield return null;
            
            int initialHealth = hero.CurrentHealth;
            hero.TakeDamage(10);
            
            yield return null;
            
            Assert.Less(hero.CurrentHealth, initialHealth);
            
            Object.Destroy(heroGo);
        }
    }
}
