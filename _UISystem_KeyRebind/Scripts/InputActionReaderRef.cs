using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

namespace SPACE_KeyRebinding
{
	/// <summary>
	/// Complete guide to reading and modifying InputActionAsset programmatically.
	/// Shows all the syntax you need to work with Input Actions.
	/// </summary>
	public class InputActionReaderRef : MonoBehaviour
	{
		[Header("Target Input Action Asset")]
		[SerializeField] private InputActionAsset inputActionAsset;

		[ContextMenu("Read Complete InputActionAsset")]
		public void ReadCompleteInputActionAsset()
		{
			if (inputActionAsset == null)
			{
				Debug.LogError("Please assign an InputActionAsset!");
				return;
			}

			Debug.Log("═══════════════════════════════════════════════════════");
			Debug.Log($"<b>Reading InputActionAsset: {inputActionAsset.name}</b>");
			Debug.Log("═══════════════════════════════════════════════════════");

			// LEVEL 1: ACTION MAPS
			foreach (InputActionMap actionMap in inputActionAsset.actionMaps)
			{
				Debug.Log($"\n📁 <color=cyan><b>ACTION MAP: {actionMap.name}</b></color>");
				Debug.Log($"   ID: {actionMap.id}");
				Debug.Log($"   Enabled: {actionMap.enabled}");
				Debug.Log($"   Total Actions: {actionMap.actions.Count}");

				// LEVEL 2: ACTIONS
				foreach (InputAction action in actionMap.actions)
				{
					Debug.Log($"\n   ⚡ <color=yellow><b>ACTION: {action.name}</b></color>");
					Debug.Log($"      ID: {action.id}");
					Debug.Log($"      Type: {action.type}");
					Debug.Log($"      Expected Control Type: {action.expectedControlType}");
					Debug.Log($"      Enabled: {action.enabled}");
					Debug.Log($"      Total Bindings: {action.bindings.Count}");

					// LEVEL 3: BINDINGS
					for (int i = 0; i < action.bindings.Count; i++)
					{
						InputBinding binding = action.bindings[i];

						Debug.Log($"\n      🔗 <color=lime><b>BINDING[{i}]</b></color>");
						Debug.Log($"         Path: {binding.path}");
						Debug.Log($"         Override Path: {binding.overridePath}");
						Debug.Log($"         Effective Path: {binding.effectivePath}");
						Debug.Log($"         Name: {binding.name}");
						Debug.Log($"         ID: {binding.id}");
						Debug.Log($"         Is Composite: {binding.isComposite}");
						Debug.Log($"         Is Part Of Composite: {binding.isPartOfComposite}");
						Debug.Log($"         Groups: {binding.groups}");
						Debug.Log($"         Interactions: {binding.interactions}");
						Debug.Log($"         Processors: {binding.processors}");

						// Display string (user-friendly name)
						string displayString = action.GetBindingDisplayString(i);
						Debug.Log($"         <color=orange>Display String: {displayString}</color>");
					}
				}
			}

			Debug.Log("\n═══════════════════════════════════════════════════════");
		}

		// ═════════════════════════════════════════════════════════════
		// BASIC READING OPERATIONS
		// ═════════════════════════════════════════════════════════════

		[ContextMenu("Example 1: Get All Action Maps")]
		public void Example1_GetAllActionMaps()
		{
			Debug.Log("--- Example 1: Get All Action Maps ---");

			foreach (InputActionMap map in inputActionAsset.actionMaps)
			{
				Debug.Log($"Action Map: {map.name}");
			}
		}

		[ContextMenu("Example 2: Get Specific Action Map")]
		public void Example2_GetSpecificActionMap()
		{
			Debug.Log("--- Example 2: Get Specific Action Map ---");

			// Method 1: By name
			InputActionMap characterMap = inputActionAsset.FindActionMap("character");
			if (characterMap != null)
			{
				Debug.Log($"Found action map: {characterMap.name}");
			}

			// Method 2: Direct access by index
			InputActionMap firstMap = inputActionAsset.actionMaps[0];
			Debug.Log($"First action map: {firstMap.name}");
		}

		[ContextMenu("Example 3: Get All Actions in a Map")]
		public void Example3_GetAllActionsInMap()
		{
			Debug.Log("--- Example 3: Get All Actions in Map ---");

			InputActionMap characterMap = inputActionAsset.FindActionMap("character");
			if (characterMap != null)
			{
				foreach (InputAction action in characterMap.actions)
				{
					Debug.Log($"Action: {action.name} (Type: {action.type})");
				}
			}
		}

		[ContextMenu("Example 4: Get Specific Action")]
		public void Example4_GetSpecificAction()
		{
			Debug.Log("--- Example 4: Get Specific Action ---");

			// Method 1: From action map
			InputActionMap characterMap = inputActionAsset.FindActionMap("character");
			InputAction jumpAction = characterMap.FindAction("jump");
			Debug.Log($"Found action: {jumpAction.name}");

			// Method 2: Direct from asset (uses "mapName/actionName" format)
			InputAction shootAction = inputActionAsset.FindAction("character/shoot");
			Debug.Log($"Found action: {shootAction.name}");

			// Method 3: Using array indexing
			InputAction firstAction = characterMap.actions[0];
			Debug.Log($"First action: {firstAction.name}");
		}

		[ContextMenu("Example 5: Get All Bindings for Action")]
		public void Example5_GetAllBindingsForAction()
		{
			Debug.Log("--- Example 5: Get All Bindings for Action ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/walk");

			for (int i = 0; i < jumpAction.bindings.Count; i++)
			{
				InputBinding binding = jumpAction.bindings[i];
				string displayString = jumpAction.GetBindingDisplayString(i);

				Debug.Log($"Binding {i}: {displayString} (Path: {binding.effectivePath})");
			}
		}

		[ContextMenu("Example 6: Check Binding Properties")]
		public void Example6_CheckBindingProperties()
		{
			Debug.Log("--- Example 6: Check Binding Properties ---");

			InputAction walkAction = inputActionAsset.FindAction("character/walk");

			for (int i = 0; i < walkAction.bindings.Count; i++)
			{
				InputBinding binding = walkAction.bindings[i];

				Debug.Log($"\nBinding {i}:");
				Debug.Log($"  Is Composite: {binding.isComposite}");
				Debug.Log($"  Is Part Of Composite: {binding.isPartOfComposite}");

				if (binding.isComposite)
				{
					Debug.Log($"  Composite Type: {binding.name}");
				}

				if (binding.isPartOfComposite)
				{
					Debug.Log($"  Composite Part: {binding.name}");
				}
			}
		}

		// ═════════════════════════════════════════════════════════════
		// MODIFYING BINDINGS
		// ═════════════════════════════════════════════════════════════

		[ContextMenu("Example 7: Override a Binding")]
		public void Example7_OverrideBinding()
		{
			Debug.Log("--- Example 7: Override a Binding ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");

			// Get the first binding index (usually index 0)
			int bindingIndex = 0;

			// Show original binding
			Debug.Log($"Original: {jumpAction.GetBindingDisplayString(bindingIndex)}");

			// Override with a new key (e.g., change Space to E)
			jumpAction.ApplyBindingOverride(bindingIndex, "<Keyboard>/e");

			// Show new binding
			Debug.Log($"After Override: {jumpAction.GetBindingDisplayString(bindingIndex)}");
		}

		[ContextMenu("Example 8: Override Using InputBinding")]
		public void Example8_OverrideUsingInputBinding()
		{
			Debug.Log("--- Example 8: Override Using InputBinding ---");

			InputAction shootAction = inputActionAsset.FindAction("character/shoot");

			// Create a new InputBinding
			InputBinding newBinding = new InputBinding
			{
				overridePath = "<Keyboard>/f"
			};

			// Apply the override
			shootAction.ApplyBindingOverride(0, newBinding);

			Debug.Log($"New binding: {shootAction.GetBindingDisplayString(0)}");
		}

		[ContextMenu("Example 9: Remove Binding Override")]
		public void Example9_RemoveBindingOverride()
		{
			Debug.Log("--- Example 9: Remove Binding Override ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");

			Debug.Log($"Before removal: {jumpAction.GetBindingDisplayString(0)}");

			// Remove override for specific binding
			jumpAction.RemoveBindingOverride(0);

			Debug.Log($"After removal: {jumpAction.GetBindingDisplayString(0)}");
		}

		[ContextMenu("Example 10: Remove All Overrides")]
		public void Example10_RemoveAllOverrides()
		{
			Debug.Log("--- Example 10: Remove All Overrides ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");

			// Remove all overrides for this action
			jumpAction.RemoveAllBindingOverrides();

			Debug.Log("All overrides removed!");
		}

		[ContextMenu("Example 11: Clear/Empty a Binding")]
		public void Example11_ClearBinding()
		{
			Debug.Log("--- Example 11: Clear a Binding ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");

			// Clear a binding by setting empty path
			jumpAction.ApplyBindingOverride(0, new InputBinding { overridePath = "" });

			Debug.Log($"Binding cleared: {jumpAction.GetBindingDisplayString(0)}");
		}

		// ═════════════════════════════════════════════════════════════
		// ADVANCED QUERIES
		// ═════════════════════════════════════════════════════════════

		[ContextMenu("Example 12: Find Binding Index by Path")]
		public void Example12_FindBindingIndexByPath()
		{
			Debug.Log("--- Example 12: Find Binding Index by Path ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");

			// Find which binding index has a specific path
			for (int i = 0; i < jumpAction.bindings.Count; i++)
			{
				if (jumpAction.bindings[i].effectivePath == "<Keyboard>/space")
				{
					Debug.Log($"Found <Keyboard>/space at index: {i}");
					break;
				}
			}
		}

		[ContextMenu("Example 13: Get Non-Composite Bindings Only")]
		public void Example13_GetNonCompositeBindingsOnly()
		{
			Debug.Log("--- Example 13: Get Non-Composite Bindings Only ---");

			InputAction walkAction = inputActionAsset.FindAction("character/walk");

			Debug.Log("Non-composite bindings:");
			for (int i = 0; i < walkAction.bindings.Count; i++)
			{
				InputBinding binding = walkAction.bindings[i];

				// Skip composites and their parts
				if (!binding.isComposite && !binding.isPartOfComposite)
				{
					Debug.Log($"  [{i}] {walkAction.GetBindingDisplayString(i)}");
				}
			}
		}

		[ContextMenu("Example 14: Check if Binding is Empty")]
		public void Example14_CheckIfBindingIsEmpty()
		{
			Debug.Log("--- Example 14: Check if Binding is Empty ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");

			for (int i = 0; i < jumpAction.bindings.Count; i++)
			{
				string path = jumpAction.bindings[i].effectivePath;
				bool isEmpty = string.IsNullOrEmpty(path);

				Debug.Log($"Binding {i}: {(isEmpty ? "EMPTY" : path)}");
			}
		}

		[ContextMenu("Example 15: Get Default Binding Path")]
		public void Example15_GetDefaultBindingPath()
		{
			Debug.Log("--- Example 15: Get Default Binding Path ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");

			// The 'path' property gives you the original/default binding
			// The 'effectivePath' gives you the current binding (with overrides)

			for (int i = 0; i < jumpAction.bindings.Count; i++)
			{
				InputBinding binding = jumpAction.bindings[i];

				Debug.Log($"Binding {i}:");
				Debug.Log($"  Default Path: {binding.path}");
				Debug.Log($"  Effective Path: {binding.effectivePath}");
				Debug.Log($"  Override Path: {binding.overridePath}");
			}
		}

		// ═════════════════════════════════════════════════════════════
		// PRACTICAL EXAMPLES
		// ═════════════════════════════════════════════════════════════

		[ContextMenu("Example 16: Swap Two Bindings")]
		public void Example16_SwapTwoBindings()
		{
			Debug.Log("--- Example 16: Swap Two Bindings ---");

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");
			InputAction shootAction = inputActionAsset.FindAction("character/shoot");

			// Get current paths
			string jumpPath = jumpAction.bindings[0].effectivePath;
			string shootPath = shootAction.bindings[0].effectivePath;

			Debug.Log($"Before swap - Jump: {jumpPath}, Shoot: {shootPath}");

			// Swap them
			jumpAction.ApplyBindingOverride(0, shootPath);
			shootAction.ApplyBindingOverride(0, jumpPath);

			Debug.Log($"After swap - Jump: {jumpAction.bindings[0].effectivePath}, Shoot: {shootAction.bindings[0].effectivePath}");
		}

		[ContextMenu("Example 17: Duplicate Binding to Multiple Actions")]
		public void Example17_DuplicateBindingToMultipleActions()
		{
			Debug.Log("--- Example 17: Duplicate Binding to Multiple Actions ---");

			// Make both jump and shoot use the same key (Space)
			string targetKey = "<Keyboard>/space";

			InputAction jumpAction = inputActionAsset.FindAction("character/jump");
			InputAction shootAction = inputActionAsset.FindAction("character/shoot");

			jumpAction.ApplyBindingOverride(0, targetKey);
			shootAction.ApplyBindingOverride(0, targetKey);

			Debug.Log($"Both actions now use: {targetKey}");
		}

		[ContextMenu("Example 18: Reset All Bindings to Default")]
		public void Example18_ResetAllBindingsToDefault()
		{
			Debug.Log("--- Example 18: Reset All Bindings to Default ---");

			foreach (InputActionMap map in inputActionAsset.actionMaps)
			{
				map.RemoveAllBindingOverrides();
				Debug.Log($"Reset action map: {map.name}");
			}

			Debug.Log("All bindings reset to default!");
		}

		// ═════════════════════════════════════════════════════════════
		// UTILITY METHODS
		// ═════════════════════════════════════════════════════════════

		/// <summary>
		/// Get a clean list of all actions with their bindable indices
		/// </summary>
		public Dictionary<string, List<int>> GetBindableActionsMap()
		{
			var result = new Dictionary<string, List<int>>();

			foreach (var map in inputActionAsset.actionMaps)
			{
				foreach (var action in map.actions)
				{
					string key = $"{map.name}/{action.name}";
					result[key] = new List<int>();

					for (int i = 0; i < action.bindings.Count; i++)
					{
						var binding = action.bindings[i];

						// Only add non-composite bindings
						if (!binding.isComposite && !binding.isPartOfComposite)
						{
							result[key].Add(i);
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Print a summary of the InputActionAsset
		/// </summary>
		[ContextMenu("Print Summary")]
		public void PrintSummary()
		{
			Debug.Log($"<b>InputActionAsset Summary: {inputActionAsset.name}</b>");
			Debug.Log($"Total Action Maps: {inputActionAsset.actionMaps.Count}");

			int totalActions = 0;
			int totalBindings = 0;

			foreach (var map in inputActionAsset.actionMaps)
			{
				totalActions += map.actions.Count;
				foreach (var action in map.actions)
				{
					totalBindings += action.bindings.Count;
				}
			}

			Debug.Log($"Total Actions: {totalActions}");
			Debug.Log($"Total Bindings: {totalBindings}");
		}
	}
}