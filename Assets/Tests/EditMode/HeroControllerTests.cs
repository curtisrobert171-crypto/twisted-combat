using NUnit.Framework;
using UnityEngine;
using EmpireOfGlass.Core;

namespace EmpireOfGlass.Tests.EditMode
{
    /// <summary>
    /// Unit tests for HeroController health, damage, and movement logic.
    /// </summary>
    [TestFixture]
    public class HeroControllerTests
    {
        private GameObject testObject;
        private HeroController heroController;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestHero");
            testObject.AddComponent<Rigidbody>();
            heroController = testObject.AddComponent<HeroController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void Hero_StartsAlive()
        {
            Assert.IsTrue(heroController.IsAlive);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            int initialHealth = heroController.CurrentHealth;
            heroController.TakeDamage(20);
            Assert.Less(heroController.CurrentHealth, initialHealth);
        }

        [Test]
        public void TakeDamage_ToZero_KillsHero()
        {
            int health = heroController.CurrentHealth;
            heroController.TakeDamage(health);
            Assert.IsFalse(heroController.IsAlive);
            Assert.AreEqual(0, heroController.CurrentHealth);
        }

        [Test]
        public void Revive_RestoresHealth()
        {
            heroController.TakeDamage(heroController.CurrentHealth);
            Assert.IsFalse(heroController.IsAlive);
            
            heroController.Revive(50);
            Assert.IsTrue(heroController.IsAlive);
            Assert.Greater(heroController.CurrentHealth, 0);
        }

        [Test]
        public void TakeDamage_NeverGoesNegative()
        {
            heroController.TakeDamage(99999);
            Assert.AreEqual(0, heroController.CurrentHealth);
        }
    }
}
