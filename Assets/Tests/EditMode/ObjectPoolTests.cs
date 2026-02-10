using NUnit.Framework;
using UnityEngine;
using EmpireOfGlass.Core;

namespace EmpireOfGlass.Tests.EditMode
{
    /// <summary>
    /// Unit tests for ObjectPool.
    /// Tests pooling, spawning, returning, and expansion logic.
    /// </summary>
    public class ObjectPoolTests
    {
        private GameObject poolObject;
        private ObjectPool objectPool;
        private GameObject testPrefab;

        [SetUp]
        public void SetUp()
        {
            // Create test prefab
            testPrefab = new GameObject("TestPrefab");

            // Create ObjectPool
            poolObject = new GameObject("ObjectPool");
            objectPool = poolObject.AddComponent<ObjectPool>();

            // Register a test pool
            objectPool.RegisterPool("test_object", testPrefab, 5, true);
        }

        [TearDown]
        public void TearDown()
        {
            if (poolObject != null)
            {
                Object.DestroyImmediate(poolObject);
            }

            if (testPrefab != null)
            {
                Object.DestroyImmediate(testPrefab);
            }

            // Clean up any spawned objects
            var spawnedObjects = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in spawnedObjects)
            {
                if (obj.name.Contains("TestPrefab"))
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        [Test]
        public void RegisterPool_CreatesPool()
        {
            // Act
            var stats = objectPool.GetPoolStats("test_object");

            // Assert
            Assert.IsTrue(stats.IsValid, "Pool should be valid after registration");
            Assert.AreEqual(5, stats.AvailableCount, "Pool should have 5 available objects");
        }

        [Test]
        public void SpawnFromPool_ReturnsValidObject()
        {
            // Act
            GameObject obj = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);

            // Assert
            Assert.IsNotNull(obj, "Spawned object should not be null");
            Assert.IsTrue(obj.activeSelf, "Spawned object should be active");

            // Cleanup
            if (obj != null) Object.DestroyImmediate(obj);
        }

        [Test]
        public void SpawnFromPool_DecreasesAvailableCount()
        {
            // Arrange
            var initialStats = objectPool.GetPoolStats("test_object");

            // Act
            GameObject obj = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);
            var finalStats = objectPool.GetPoolStats("test_object");

            // Assert
            Assert.AreEqual(initialStats.AvailableCount - 1, finalStats.AvailableCount, "Available count should decrease by 1");

            // Cleanup
            if (obj != null) Object.DestroyImmediate(obj);
        }

        [Test]
        public void ReturnToPool_IncreasesAvailableCount()
        {
            // Arrange
            GameObject obj = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);
            var statsAfterSpawn = objectPool.GetPoolStats("test_object");

            // Act
            objectPool.ReturnToPool("test_object", obj);
            var statsAfterReturn = objectPool.GetPoolStats("test_object");

            // Assert
            Assert.AreEqual(statsAfterSpawn.AvailableCount + 1, statsAfterReturn.AvailableCount, "Available count should increase by 1");
        }

        [Test]
        public void ReturnToPool_DeactivatesObject()
        {
            // Arrange
            GameObject obj = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);

            // Act
            objectPool.ReturnToPool("test_object", obj);

            // Assert
            Assert.IsFalse(obj.activeSelf, "Returned object should be inactive");
        }

        [Test]
        public void SpawnFromPool_ExpandsWhenExhausted()
        {
            // Arrange
            GameObject[] objects = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                objects[i] = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);
            }

            // Pool is now exhausted
            var statsExhausted = objectPool.GetPoolStats("test_object");
            Assert.AreEqual(0, statsExhausted.AvailableCount, "Pool should be exhausted");

            // Act - spawn one more (should expand)
            GameObject extraObj = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);

            // Assert
            Assert.IsNotNull(extraObj, "Pool should expand and provide new object");

            // Cleanup
            foreach (var obj in objects)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            if (extraObj != null) Object.DestroyImmediate(extraObj);
        }

        [Test]
        public void GetActiveObjectCount_ReturnsCorrectCount()
        {
            // Arrange
            GameObject obj1 = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);
            GameObject obj2 = objectPool.SpawnFromPool("test_object", Vector3.zero, Quaternion.identity);

            // Act
            int activeCount = objectPool.GetActiveObjectCount("test_object");

            // Assert
            Assert.AreEqual(2, activeCount, "Active count should be 2");

            // Cleanup
            if (obj1 != null) Object.DestroyImmediate(obj1);
            if (obj2 != null) Object.DestroyImmediate(obj2);
        }

        [Test]
        public void SpawnFromPool_SetsPositionAndRotation()
        {
            // Arrange
            Vector3 expectedPos = new Vector3(10, 20, 30);
            Quaternion expectedRot = Quaternion.Euler(45, 90, 135);

            // Act
            GameObject obj = objectPool.SpawnFromPool("test_object", expectedPos, expectedRot);

            // Assert
            Assert.AreEqual(expectedPos, obj.transform.position, "Position should match");
            Assert.AreEqual(expectedRot, obj.transform.rotation, "Rotation should match");

            // Cleanup
            if (obj != null) Object.DestroyImmediate(obj);
        }

        [Test]
        public void GetPoolStats_ReturnsInvalidForNonexistentPool()
        {
            // Act
            var stats = objectPool.GetPoolStats("nonexistent_pool");

            // Assert
            Assert.IsFalse(stats.IsValid, "Stats should be invalid for nonexistent pool");
        }

        [Test]
        public void SpawnFromPool_ReturnsNullForNonexistentPool()
        {
            // Act
            GameObject obj = objectPool.SpawnFromPool("nonexistent_pool", Vector3.zero, Quaternion.identity);

            // Assert
            Assert.IsNull(obj, "Should return null for nonexistent pool");
        }

        [Test]
        public void RegisterPool_PreventsDoubleRegistration()
        {
            // Arrange
            var initialStats = objectPool.GetPoolStats("test_object");

            // Act - try to register same pool again
            objectPool.RegisterPool("test_object", testPrefab, 10, true);

            // Assert - pool should remain unchanged
            var finalStats = objectPool.GetPoolStats("test_object");
            Assert.AreEqual(initialStats.AvailableCount, finalStats.AvailableCount, "Pool should not be modified");
        }
    }
}
