using NUnit.Framework;
using UnityEngine;
using EmpireOfGlass.Raid;

namespace EmpireOfGlass.Tests.EditMode
{
    [TestFixture]
    public class RaidManagerTests
    {
        private GameObject testObject;
        private RaidManager raidManager;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestRaidManager");
            raidManager = testObject.AddComponent<RaidManager>();
            raidManager.StartRaid(100);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void EvaluateSpin_TriplePig_TriggersRaid()
        {
            SpinResult result = raidManager.EvaluateSpin(new[]
            {
                SpinSymbol.Pig,
                SpinSymbol.Pig,
                SpinSymbol.Pig
            });

            Assert.IsTrue(result.TriggeredRaid);
            Assert.AreEqual(0, result.CoinsAwarded);
            Assert.AreEqual(0, result.ShieldsAwarded);
            Assert.AreEqual(0, result.AttacksAwarded);
        }

        [Test]
        public void EvaluateSpin_TripleCoin_AwardsJackpotCoins()
        {
            SpinResult result = raidManager.EvaluateSpin(new[]
            {
                SpinSymbol.Coin,
                SpinSymbol.Coin,
                SpinSymbol.Coin
            });

            Assert.GreaterOrEqual(result.CoinsAwarded, 1000);
            Assert.IsFalse(result.TriggeredRaid);
        }

        [Test]
        public void EvaluateSpin_MixedSymbols_AwardsPartialPayouts()
        {
            SpinResult result = raidManager.EvaluateSpin(new[]
            {
                SpinSymbol.Coin,
                SpinSymbol.Shield,
                SpinSymbol.Hammer
            });

            Assert.AreEqual(100, result.CoinsAwarded);
            Assert.AreEqual(1, result.ShieldsAwarded);
            Assert.AreEqual(1, result.AttacksAwarded);
            Assert.IsFalse(result.TriggeredRaid);
        }

        [Test]
        public void CalculateLoot_HighPrecision_HigherRewards()
        {
            RaidResult low = raidManager.CalculateLoot(0.2f);
            RaidResult high = raidManager.CalculateLoot(0.95f);

            Assert.Greater(high.Gold, low.Gold);
            Assert.Greater(high.Shards, low.Shards);
            Assert.Greater(high.LootTier, low.LootTier);
        }

        [Test]
        public void EvaluateFrequencyPrecision_AtTarget_IsOne()
        {
            float target = raidManager.TargetFrequency;
            float precision = raidManager.EvaluateFrequencyPrecision(target);
            Assert.AreEqual(1f, precision, 0.001f);
        }

        [Test]
        public void Spin_WhenUnavailable_ReturnsEmptyResult()
        {
            raidManager.StartRaid(0);

            // Consume all available spins.
            for (int i = 0; i < 6; i++)
            {
                raidManager.Spin();
            }

            SpinResult result = raidManager.Spin();
            Assert.AreEqual(0, result.CoinsAwarded + result.ShieldsAwarded + result.AttacksAwarded);
            Assert.IsFalse(result.TriggeredRaid);
        }
    }
}
