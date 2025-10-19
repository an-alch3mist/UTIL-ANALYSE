using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using SPACE_UTIL;

/// <summary>
/// Generalized rebinding system that works with any InputActionAsset
/// Handles composite bindings, duplicate detection, save/load, and more
/// </summary>
public class GeneralizedRebindUI : MonoBehaviour
{
	[Header("Input Asset")]
	[SerializeField] private InputActionAsset inputActions;

	[Header("UI References")]
	[SerializeField] private Transform actionMapContainer;
	[SerializeField] private GameObject actionMapButtonTemplate;
	[SerializeField] private GameObject actionRowTemplate;
	[SerializeField] private Transform actionRowHolder;
	[SerializeField] private GameObject rebindPopup;
	[SerializeField] private TextMeshProUGUI popupPromptText;
	[SerializeField] private Button resetAllButton;

	[Header("Settings")]
	[SerializeField] private string saveKeyPrefix = "InputRebinds_";
	[SerializeField] private bool preventDuplicates = true;
	[SerializeField] private float rebindTimeout = 5f;

	private Dictionary<string, GameObject> actionMapButtons = new Dictionary<string, GameObject>();
	private Dictionary<string, List<GameObject>> actionRows = new Dictionary<string, List<GameObject>>();
	private InputActionRebindingExtensions.RebindingOperation currentRebind;
	private string currentActionMapName;
	private bool isRebindingComposite = false;

	private void Start()
	{
		if (inputActions == null)
		{
			Debug.LogError("InputActionAsset is not assigned!");
			return;
		}

		InitializeUI();
		LoadBindings();

		if (resetAllButton != null)
			resetAllButton.onClick.AddListener(ResetAllBindings);

		if (rebindPopup != null)
			rebindPopup.SetActive(false);
	}

	private void OnDestroy()
	{
		currentRebind?.Dispose();
	}

	private void InitializeUI()
	{
		// Clear existing UI
		ClearUI();

		// Create action map buttons
		foreach (var actionMap in inputActions.actionMaps)
		{
			CreateActionMapButton(actionMap);
		}

		// Show first action map by default
		if (inputActions.actionMaps.Count > 0)
		{
			ShowActionMap(inputActions.actionMaps[0].name);
		}
	}

	private void CreateActionMapButton(InputActionMap actionMap)
	{
		if (actionMapButtonTemplate == null || actionMapContainer == null) return;

		GameObject buttonObj = Instantiate(actionMapButtonTemplate, actionMapContainer);
		buttonObj.SetActive(true);

		// Set button text
		var textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
		if (textComponent != null)
			textComponent.text = FormatDisplayName(actionMap.name);

		// Setup button click
		var button = buttonObj.GetComponent<Button>();
		if (button != null)
		{
			string mapName = actionMap.name;
			button.onClick.AddListener(() => ShowActionMap(mapName));
		}

		actionMapButtons[actionMap.name] = buttonObj;
	}

	private void ShowActionMap(string actionMapName)
	{
		currentActionMapName = actionMapName;

		// Hide all action rows
		foreach (var rowList in actionRows.Values)
		{
			foreach (var row in rowList)
				row.SetActive(false);
		}

		// Show or create rows for this action map
		if (!actionRows.ContainsKey(actionMapName))
		{
			CreateActionRowsForMap(actionMapName);
		}

		if (actionRows.ContainsKey(actionMapName))
		{
			foreach (var row in actionRows[actionMapName])
				row.SetActive(true);
		}

		// Update button states
		foreach (var kvp in actionMapButtons)
		{
			var button = kvp.Value.GetComponent<Button>();
			if (button != null)
			{
				var colors = button.colors;
				colors.normalColor = kvp.Key == actionMapName ? new Color(0.6f, 0.8f, 0.6f) : Color.white;
				button.colors = colors;
			}
		}
	}

	private void CreateActionRowsForMap(string actionMapName)
	{
		var actionMap = inputActions.FindActionMap(actionMapName);
		if (actionMap == null) return;

		actionRows[actionMapName] = new List<GameObject>();

		foreach (var action in actionMap.actions)
		{
			// Group bindings by composite
			var processedBindings = new HashSet<int>();

			for (int i = 0; i < action.bindings.Count; i++)
			{
				if (processedBindings.Contains(i)) continue;

				var binding = action.bindings[i];

				// Skip if part of composite (will be handled by composite parent)
				if (binding.isPartOfComposite) continue;

				if (binding.isComposite)
				{
					// Handle composite binding
					CreateActionRow(action, i, true);

					// Mark composite parts as processed
					for (int j = i + 1; j < action.bindings.Count && action.bindings[j].isPartOfComposite; j++)
					{
						processedBindings.Add(j);
					}
				}
				else
				{
					// Handle simple binding
					CreateActionRow(action, i, false);
				}
			}
		}
	}

	private void CreateActionRow(InputAction action, int bindingIndex, bool isComposite)
	{
		if (actionRowTemplate == null || actionRowHolder == null) return;

		GameObject rowObj = Instantiate(actionRowTemplate, actionRowHolder);
		rowObj.SetActive(false);

		var binding = action.bindings[bindingIndex];

		// Find UI components (adjust child indices to match your template)
		var actionNameText = rowObj.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
		var bindingButton = rowObj.transform.GetChild(1).gameObject.GetComponent<Button>();
		var bindingText = bindingButton?.GetComponentInChildren<TextMeshProUGUI>();
		var resetButton = rowObj.transform.GetChild(2).gameObject.GetComponent<Button>();

		// Setup action name
		if (actionNameText != null)
		{
			string displayName = FormatDisplayName(action.name);
			if (!string.IsNullOrEmpty(binding.groups))
				displayName += $" ({binding.groups})";
			actionNameText.text = displayName;
		}

		// Setup binding button
		if (bindingButton != null && bindingText != null)
		{
			UpdateBindingDisplay(action, bindingIndex, bindingText);

			bindingButton.onClick.AddListener(() => StartRebind(action, bindingIndex, isComposite, bindingText));
		}

		// Setup reset button
		if (resetButton != null)
		{
			resetButton.onClick.AddListener(() => ResetBinding(action, bindingIndex, bindingText));
		}

		if (!actionRows.ContainsKey(action.actionMap.name))
			actionRows[action.actionMap.name] = new List<GameObject>();

		actionRows[action.actionMap.name].Add(rowObj);
	}

	private void UpdateBindingDisplay(InputAction action, int bindingIndex, TextMeshProUGUI textComponent)
	{
		if (textComponent == null) return;

		var binding = action.bindings[bindingIndex];

		if (binding.isComposite)
		{
			// Display composite as "W/A/S/D" or similar
			var compositeParts = new List<string>();
			for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
			{
				string partDisplay = action.GetBindingDisplayString(i,
					InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
				compositeParts.Add(partDisplay);
			}
			textComponent.text = string.Join("/", compositeParts);
		}
		else
		{
			textComponent.text = action.GetBindingDisplayString(bindingIndex,
				InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
		}
	}

	private void StartRebind(InputAction action, int bindingIndex, bool isComposite, TextMeshProUGUI displayText)
	{
		// Disable the entire action map to ensure action can be rebound
		if (action.actionMap.enabled)
		{
			action.actionMap.Disable();
		}

		// Track if we're rebinding a composite
		isRebindingComposite = isComposite;

		if (isComposite)
		{
			StartCompositeRebind(action, bindingIndex, displayText);
		}
		else
		{
			StartSingleRebind(action, bindingIndex, displayText);
		}
	}

	private void StartSingleRebind(InputAction action, int bindingIndex, TextMeshProUGUI displayText)
	{
		var binding = action.bindings[bindingIndex];

		ShowPopup($"Press a key for '{FormatDisplayName(action.name)}'...\n\nESC to cancel");

		currentRebind?.Cancel();
		currentRebind = action.PerformInteractiveRebinding(bindingIndex)
			.WithTimeout(rebindTimeout)
			.WithCancelingThrough("<Keyboard>/escape")
			.OnMatchWaitForAnother(0.1f);

		if (preventDuplicates)
		{
			currentRebind.OnPotentialMatch(operation =>
			{
				if (IsDuplicateBinding(action, bindingIndex, operation))
				{
					ShowPopup("⚠️ KEY ALREADY BOUND! ⚠️\n\nThis key is already used by another action.\nPress a different key...\n\nESC to cancel");

					// Don't cancel the operation, just ignore this match and wait for another input
					return;
				}
			});
		}

		currentRebind
			.OnComplete(operation =>
			{
				Debug.Log($"Rebind complete for {action.name}[{bindingIndex}]");
				Debug.Log($"New binding: {action.bindings[bindingIndex].effectivePath}");
				Debug.Log($"Override path: {action.bindings[bindingIndex].overridePath}");

				UpdateBindingDisplay(action, bindingIndex, displayText);
				CleanupRebind(action);
				SaveBindings();
			})
			.OnCancel(operation =>
			{
				CleanupRebind(action);
			})
			.Start();
	}

	private System.Collections.IEnumerator RestartRebindAfterDelay(InputAction action, int bindingIndex, TextMeshProUGUI displayText, bool isCompositePart)
	{
		yield return new WaitForSeconds(0.5f);

		if (isCompositePart)
			RebindCompositePart(action, bindingIndex, displayText);
		else
			StartSingleRebind(action, bindingIndex, displayText);
	}

	private void StartCompositeRebind(InputAction action, int bindingIndex, TextMeshProUGUI displayText)
	{
		var binding = action.bindings[bindingIndex];
		int firstPartIndex = bindingIndex + 1;

		if (firstPartIndex >= action.bindings.Count || !action.bindings[firstPartIndex].isPartOfComposite)
		{
			CleanupRebind(action);
			return;
		}

		RebindCompositePart(action, firstPartIndex, displayText);
	}

	private void RebindCompositePart(InputAction action, int bindingIndex, TextMeshProUGUI displayText)
	{
		var binding = action.bindings[bindingIndex];
		string partName = FormatDisplayName(binding.name);

		ShowPopup($"Press a key for '{partName}'...\n\nESC to cancel");

		currentRebind?.Cancel();

		// Don't re-enable the action map between composite parts
		currentRebind = action.PerformInteractiveRebinding(bindingIndex)
			.WithTimeout(rebindTimeout)
			.WithCancelingThrough("<Keyboard>/escape")
			.OnMatchWaitForAnother(0.1f);

		if (preventDuplicates)
		{
			currentRebind.OnPotentialMatch(operation =>
			{
				if (IsDuplicateBinding(action, bindingIndex, operation))
				{
					ShowPopup($"⚠️ KEY ALREADY BOUND! ⚠️\n\nThis key is already used by another action.\nPress a different key for '{partName}'...\n\nESC to cancel");

					// Don't cancel the operation, just ignore this match and wait for another input
					return;
				}
			});
		}

		currentRebind
			.OnComplete(operation =>
			{
				Debug.Log($"Composite part rebind complete for {action.name}[{bindingIndex}] - {partName}");
				Debug.Log($"New binding: {action.bindings[bindingIndex].effectivePath}");

				// Dispose current rebind operation
				operation?.Dispose();
				currentRebind = null;

				// Find the composite parent
				int compositeIndex = bindingIndex - 1;
				while (compositeIndex >= 0 && !action.bindings[compositeIndex].isComposite)
					compositeIndex--;

				// Check if there are more parts
				int nextPartIndex = bindingIndex + 1;
				if (nextPartIndex < action.bindings.Count && action.bindings[nextPartIndex].isPartOfComposite)
				{
					// Continue to next part WITHOUT re-enabling action map
					RebindCompositePart(action, nextPartIndex, displayText);
				}
				else
				{
					// All parts complete - now we can clean up
					isRebindingComposite = false;
					UpdateBindingDisplay(action, compositeIndex, displayText);
					CleanupRebind(action);
					SaveBindings();
				}
			})
			.OnCancel(operation =>
			{
				isRebindingComposite = false;
				CleanupRebind(action);
			})
			.Start();
	}

	private bool IsDuplicateBinding(InputAction action, int bindingIndex, InputActionRebindingExtensions.RebindingOperation operation)
	{
		string newPath = operation.selectedControl?.path;
		if (string.IsNullOrEmpty(newPath)) return false;

		Debug.Log($"Checking duplicate for new path: {newPath}");

		// Check all actions in all action maps
		foreach (var actionMap in inputActions.actionMaps)
		{
			foreach (var otherAction in actionMap.actions)
			{
				for (int i = 0; i < otherAction.bindings.Count; i++)
				{
					var otherBinding = otherAction.bindings[i];

					// Skip composite headers
					if (otherBinding.isComposite)
						continue;

					// Skip the binding we're currently rebinding
					if (otherAction == action && i == bindingIndex)
						continue;

					// Get the effective path (includes overrides)
					string existingPath = otherBinding.effectivePath;

					// Also check the override path if it exists
					if (!string.IsNullOrEmpty(otherBinding.overridePath))
					{
						existingPath = otherBinding.overridePath;
					}

					Debug.Log($"Comparing with {otherAction.name}[{i}]: {existingPath}");

					// Compare paths
					if (existingPath == newPath)
					{
						Debug.LogWarning($"DUPLICATE FOUND! {newPath} is already bound to {otherAction.name}");
						return true;
					}
				}
			}
		}

		return false;
	}

	private void CleanupRebind(InputAction action)
	{
		currentRebind?.Dispose();
		currentRebind = null;

		// Only re-enable if we're completely done (not in middle of composite rebinding)
		if (!isRebindingComposite && !action.actionMap.enabled)
		{
			action.actionMap.Enable();
		}

		HidePopup();
	}

	private void ResetBinding(InputAction action, int bindingIndex, TextMeshProUGUI displayText)
	{
		var binding = action.bindings[bindingIndex];

		if (binding.isComposite)
		{
			// Reset all parts of composite
			for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
			{
				action.RemoveBindingOverride(i);
			}
		}
		else
		{
			action.RemoveBindingOverride(bindingIndex);
		}

		UpdateBindingDisplay(action, bindingIndex, displayText);
		SaveBindings();
	}

	private void ResetAllBindings()
	{
		inputActions.RemoveAllBindingOverrides();

		// Update all displays
		foreach (var actionMap in inputActions.actionMaps)
		{
			if (actionRows.ContainsKey(actionMap.name))
			{
				// Refresh all rows by recreating them
				foreach (var row in actionRows[actionMap.name])
				{
					Destroy(row);
				}
				actionRows[actionMap.name].Clear();
				CreateActionRowsForMap(actionMap.name);
			}
		}

		// Show current action map
		if (!string.IsNullOrEmpty(currentActionMapName))
		{
			ShowActionMap(currentActionMapName);
		}

		SaveBindings();
	}

	private void SaveBindings()
	{
		string rebinds = inputActions.SaveBindingOverridesAsJson();
		PlayerPrefs.SetString(saveKeyPrefix + inputActions.name, rebinds);
		PlayerPrefs.Save();
	}

	private void LoadBindings()
	{
		string rebinds = PlayerPrefs.GetString(saveKeyPrefix + inputActions.name, string.Empty);
		if (!string.IsNullOrEmpty(rebinds))
		{
			inputActions.LoadBindingOverridesFromJson(rebinds);
		}
	}

	private void ShowPopup(string message)
	{
		if (rebindPopup != null)
		{
			rebindPopup.SetActive(true);
			if (popupPromptText != null)
				popupPromptText.text = message;
		}
	}

	private void HidePopup()
	{
		if (rebindPopup != null)
			rebindPopup.SetActive(false);
	}

	private void ClearUI()
	{
		foreach (var buttonObj in actionMapButtons.Values)
			Destroy(buttonObj);
		actionMapButtons.Clear();

		foreach (var rowList in actionRows.Values)
		{
			foreach (var row in rowList)
				Destroy(row);
		}
		actionRows.Clear();
	}

	private string FormatDisplayName(string name)
	{
		// Convert camelCase or PascalCase to Title Case with spaces
		if (string.IsNullOrEmpty(name)) return name;

		var result = System.Text.RegularExpressions.Regex.Replace(name, "(\\B[A-Z])", " $1");
		return char.ToUpper(result[0]) + result.Substring(1);
	}
}