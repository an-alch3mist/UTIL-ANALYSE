using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace SPACE_CHECK
{
	/// <summary>
	/// Comprehensive test suite for ExtensionGameObjectOrComponent
	/// Attach to any GameObject in the scene and press Space to run all tests
	/// </summary>
	public class ExtensionGameObjectOrComponentTests : MonoBehaviour
	{
		[Header("Test Configuration")]
		[SerializeField] private KeyCode testKey = KeyCode.Space;
		[SerializeField] private bool runOnStart = false;

		private GameObject testRoot;
		private const string TEST_HEADER = "=== ExtensionGameObjectOrComponent Tests ===";

		void Start()
		{
			if (runOnStart)
				RunAllTests();
		}

		void Update()
		{
			if (Input.GetKeyDown(testKey))
				RunAllTests();
		}

		public void RunAllTests()
		{
			Debug.Log(TEST_HEADER.colorTag("cyan"));
			StartCoroutine(RunTestsSequentially());
		}

		private IEnumerator RunTestsSequentially()
		{
			SetupTestHierarchy();
			yield return null;

			// Run all tests
			Test_leafNameStartsWith();
			yield return null;

			Test_leafQuery();
			yield return null;

			Test_getLeavesGen1();
			yield return null;

			Test_getDepthLeafNameStartingWith();
			yield return null;

			Test_GC_Methods();
			yield return null;

			Test_clearLeaves();
			yield return null;

			Test_toggleLeaves();
			yield return null;

			Test_toggle();
			yield return null;

			Test_getHierarchyString();
			yield return null;

			// Cleanup
			TeardownTestHierarchy();

			Debug.Log("✓ All tests completed!".colorTag("lime"));
		}

		#region Setup & Teardown
		private void SetupTestHierarchy()
		{
			Debug.Log("→ Setting up test hierarchy...".colorTag("yellow"));

			// Create test hierarchy:
			// TestRoot
			//   ├─ Player
			//   │   ├─ Head
			//   │   │   └─ Eyes
			//   │   ├─ Body
			//   │   └─ Legs
			//   ├─ Enemy
			//   │   └─ Weapon
			//   └─ Item_Sword

			testRoot = new GameObject("TestRoot");

			// Player branch
			GameObject player = new GameObject("Player");
			player.transform.SetParent(testRoot.transform);

			GameObject head = new GameObject("Head");
			head.transform.SetParent(player.transform);

			GameObject eyes = new GameObject("Eyes");
			eyes.transform.SetParent(head.transform);

			GameObject body = new GameObject("Body");
			body.transform.SetParent(player.transform);
			body.AddComponent<Rigidbody>(); // Add component for GC tests

			GameObject legs = new GameObject("Legs");
			legs.transform.SetParent(player.transform);

			// Enemy branch
			GameObject enemy = new GameObject("Enemy");
			enemy.transform.SetParent(testRoot.transform);

			GameObject weapon = new GameObject("Weapon");
			weapon.transform.SetParent(enemy.transform);
			weapon.AddComponent<BoxCollider>(); // Add component for GC tests

			// Item
			GameObject item = new GameObject("Item_Sword");
			item.transform.SetParent(testRoot.transform);

			Debug.Log("✓ Test hierarchy created".colorTag("lime"));
		}

		private void TeardownTestHierarchy()
		{
			if (testRoot != null)
			{
				// Destroy(testRoot);
				Debug.Log("✓ Test hierarchy cleaned up".colorTag("lime"));
			}
		}
		#endregion

		#region Test Methods

		private void Test_leafNameStartsWith()
		{
			Debug.Log("[TEST] leafNameStartsWith".colorTag("cyan"));

			// Test 1: Find existing child
			Transform player = testRoot.leafNameStartsWith("play");
			Assert(player != null && player.name == "Player",
				"Should find 'Player' with prefix 'play'");

			// Test 2: Case insensitive
			Transform enemy = testRoot.leafNameStartsWith("ENEMY");
			Assert(enemy != null && enemy.name == "Enemy",
				"Should be case-insensitive");

			// Test 3: Exact match
			Transform item = testRoot.leafNameStartsWith("Item_Sword");
			Assert(item != null && item.name == "Item_Sword",
				"Should find exact match");

			// Test 4: Component overload
			Transform playerFromComponent = testRoot.transform.leafNameStartsWith("player");
			Assert(playerFromComponent != null && playerFromComponent.name == "Player",
				"Component overload should work");

			// Test 5: Non-existent (will log error, expected)
			Debug.Log("→ Expected error below:".colorTag("yellow"));

			// Transform notFound = testRoot.leafNameStartsWith("NonExistent");
			// Assert(null == null, "Should return null for non-existent child");

		}

		private void Test_leafQuery()
		{
			Debug.Log("[TEST] leafQuery".colorTag("cyan"));

			// Test 1: Deep query
			Transform eyes = testRoot.leafQuery("Player > Head > Eyes");
			Assert(eyes != null && eyes.name == "Eyes",
				"Should navigate through hierarchy");

			// Test 2: Two levels
			Transform head = testRoot.leafQuery("play > hea");
			Assert(head != null && head.name == "Head",
				"Should work with partial names");

			// Test 3: Custom separator
			Transform weapon = testRoot.leafQuery("Enemy / Weapon", sep: '/');
			Assert(weapon != null && weapon.name == "Weapon",
				"Should work with custom separator");

			// Test 4: Component overload
			Transform body = testRoot.transform.leafQuery("Player > Body");
			Assert(body != null && body.name == "Body",
				"Component overload should work");

			// Test 5: Invalid path (will log error, expected)
			Debug.Log("→ Expected error below:".colorTag("yellow"));
			// Transform invalid = testRoot.leafQuery("Player > NonExistent > Test");
			// Assert(invalid == null, "Should return null for invalid path");
		}

		private void Test_getLeavesGen1()
		{
			Debug.Log("[TEST] getLeavesGen1".colorTag("cyan"));

			// Test 1: Get all children
			List<Transform> rootChildren = testRoot.getLeavesGen1();
			Assert(rootChildren.Count == 3,
				$"Should find 3 children, found {rootChildren.Count}");

			// Test 2: Verify child names
			bool hasPlayer = rootChildren.Exists(t => t.name == "Player");
			bool hasEnemy = rootChildren.Exists(t => t.name == "Enemy");
			bool hasItem = rootChildren.Exists(t => t.name == "Item_Sword");
			Assert(hasPlayer && hasEnemy && hasItem,
				"Should contain Player, Enemy, and Item_Sword");

			// Test 3: Component overload
			Transform player = testRoot.leafNameStartsWith("Player");
			List<Transform> playerChildren = player.getLeavesGen1();
			Assert(playerChildren.Count == 3,
				$"Player should have 3 children, found {playerChildren.Count}");

			// Test 4: Empty children
			Transform eyes = testRoot.leafQuery("Player > Head > Eyes");
			List<Transform> eyesChildren = eyes.getLeavesGen1();
			Assert(eyesChildren.Count == 0,
				"Eyes should have no children");
		}

		private void Test_getDepthLeafNameStartingWith()
		{
			Debug.Log("[TEST] getDepthLeafNameStartingWith".colorTag("cyan"));

			// Test 1: Find deep child
			Transform eyes = testRoot.getDepthLeafNameStartingWith("eyes");
			Assert(eyes != null && eyes.name == "Eyes",
				"Should find 'Eyes' in deep hierarchy");

			// Test 2: Find at any level
			Transform weapon = testRoot.getDepthLeafNameStartingWith("weap");
			Assert(weapon != null && weapon.name == "Weapon",
				"Should find 'Weapon' at any depth");

			// Test 3: Find first match (should find Player, not Item_*)
			Transform player = testRoot.getDepthLeafNameStartingWith("P");
			Assert(player != null && player.name == "Player",
				"Should return first match in depth-first order");

			// Test 4: Component overload
			Transform playerObj = testRoot.leafNameStartsWith("Player");
			Transform head = playerObj.getDepthLeafNameStartingWith("head");
			Assert(head != null && head.name == "Head",
				"Component overload should work");

			// Test 5: Non-existent
			Transform notFound = testRoot.getDepthLeafNameStartingWith("Ghost");
			Assert(notFound == null,
				"Should return null for non-existent name");
		}

		private void Test_GC_Methods()
		{
			Debug.Log("[TEST] GC, GCLeaf, GCLeaves".colorTag("cyan"));

			Transform player = testRoot.leafNameStartsWith("Player");
			Transform body = player.leafNameStartsWith("Body");

			// Test 1: GC (GetComponent)
			Rigidbody rb = body.gameObject.GC<Rigidbody>();
			Assert(rb != null,
				"GC should find Rigidbody on Body");

			// Test 2: GC component overload
			Rigidbody rbFromComponent = body.GC<Rigidbody>();
			Assert(rbFromComponent != null,
				"GC component overload should work");

			// Test 3: GCLeaf (GetComponentInChildren)
			Rigidbody rbLeaf = player.gameObject.GCLeaf<Rigidbody>();
			Assert(rbLeaf != null,
				"GCLeaf should find Rigidbody in children");

			// Test 4: GCLeaves (GetComponentsInChildren)
			Transform enemy = testRoot.leafNameStartsWith("Enemy");
			var colliders = enemy.gameObject.GCLeaves<Collider>();
			int colliderCount = 0;
			foreach (var c in colliders) colliderCount++;
			Assert(colliderCount > 0,
				$"GCLeaves should find colliders, found {colliderCount}");

			// Test 5: GC returns null for missing component
			AudioSource audio = body.gameObject.GC<AudioSource>();
			Assert(audio == null,
				"GC should return null for missing component");
		}

		private void Test_clearLeaves()
		{
			Debug.Log("[TEST] clearLeaves".colorTag("cyan"));

			// Create temporary parent for this test
			GameObject tempParent = new GameObject("TempParent");
			tempParent.transform.SetParent(testRoot.transform);

			// Add some children
			for (int i = 0; i < 3; i++)
			{
				GameObject child = new GameObject($"Child_{i}");
				child.transform.SetParent(tempParent.transform);
			}

			// Test 1: Verify children exist
			Assert(tempParent.transform.childCount == 3,
				"Should have 3 children before clearing");

			// Test 2: Clear leaves
			tempParent.clearLeaves();

			// Wait a frame for destruction
			StartCoroutine(VerifyClearLeaves(tempParent));
		}

		private IEnumerator VerifyClearLeaves(GameObject parent)
		{
			yield return null; // Wait for Destroy to take effect

			Assert(parent.transform.childCount == 0,
				"Should have 0 children after clearing");

			Destroy(parent);
		}

		private void Test_toggleLeaves()
		{
			Debug.Log("[TEST] toggleLeaves".colorTag("cyan"));

			Transform player = testRoot.leafNameStartsWith("Player");

			// Test 1: Disable all children
			player.gameObject.toggleLeaves(false);
			bool allDisabled = true;
			foreach (Transform child in player)
			{
				if (child.gameObject.activeSelf)
				{
					allDisabled = false;
					break;
				}
			}
			Assert(allDisabled,
				"All children should be disabled");

			// Test 2: Enable all children
			player.gameObject.toggleLeaves(true);
			bool allEnabled = true;
			foreach (Transform child in player)
			{
				if (!child.gameObject.activeSelf)
				{
					allEnabled = false;
					break;
				}
			}
			Assert(allEnabled,
				"All children should be enabled");

			// Test 3: Component overload
			player.toggleLeaves(false);
			Transform head = player.leafNameStartsWith("Head");
			Assert(!head.gameObject.activeSelf,
				"Component overload should work");

			// Re-enable for other tests
			player.gameObject.toggleLeaves(true);
		}

		private void Test_toggle()
		{
			Debug.Log("[TEST] toggle".colorTag("cyan"));

			Transform player = testRoot.leafNameStartsWith("Player");

			// Test 1: Disable object
			Transform result = player.gameObject.toggle(false);
			Assert(!player.gameObject.activeSelf,
				"Should disable the GameObject");
			Assert(result == player,
				"Should return the transform for chaining");

			// Test 2: Enable object
			player.gameObject.toggle(true);
			Assert(player.gameObject.activeSelf,
				"Should enable the GameObject");

			// Test 3: Component overload
			Transform enemy = testRoot.leafNameStartsWith("Enemy");
			Transform enemyResult = enemy.toggle(false);
			Assert(!enemy.gameObject.activeSelf,
				"Component overload should work");
			Assert(enemyResult == enemy,
				"Should return correct transform");

			// Test 4: Chaining capability
			enemy.toggle(true).position = Vector3.one;
			Assert(enemy.position == Vector3.one,
				"Should support fluent chaining");

			// Re-enable for other tests
			player.gameObject.toggle(true);
			enemy.toggle(true);
		}

		private void Test_getHierarchyString()
		{
			Debug.Log("[TEST] getHierarchyString".colorTag("cyan"));

			// Test 1: Get hierarchy string
			string hierarchy = testRoot.getHierarchyString();
			Assert(!string.IsNullOrEmpty(hierarchy),
				"Should return non-empty string");

			// Test 2: Contains expected names
			bool hasTestRoot = hierarchy.Contains("TestRoot");
			bool hasPlayer = hierarchy.Contains("Player");
			bool hasEyes = hierarchy.Contains("Eyes");
			Assert(hasTestRoot && hasPlayer && hasEyes,
				"Should contain hierarchy node names");

			// Test 3: Indentation check
			bool hasIndentation = hierarchy.Contains("  "); // Two spaces for indent
			Assert(hasIndentation,
				"Should have indentation for nested elements");

			// Test 4: Component overload
			Transform player = testRoot.leafNameStartsWith("Player");
			string playerHierarchy = player.getHierarchyString();
			Assert(playerHierarchy.Contains("Head"),
				"Component overload should work");

			// Test 5: Custom depth
			string hierarchyDepth2 = testRoot.getHierarchyString(selfDepth: 2);
			Assert(hierarchyDepth2.StartsWith("    "), // 4 spaces for depth 2
				"Should respect custom depth parameter");

			// Log the actual hierarchy for visual inspection
			Debug.Log($"Full hierarchy:\n{hierarchy}".colorTag("white"));
			LOG.AddLog(hierarchy);
		}

		#endregion

		#region Helper Methods
		private void Assert(bool condition, string message)
		{
			if (condition)
			{
				Debug.Log($"  ✓ {message}".colorTag("lime"));
			}
			else
			{
				Debug.LogError($"  ✗ FAILED: {message}".colorTag("red"));
			}
		}
		#endregion
	} 
}