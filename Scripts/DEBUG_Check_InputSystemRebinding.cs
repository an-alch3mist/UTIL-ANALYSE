using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//

using SPACE_UTIL;

namespace SPACE_CHECK
{
	public class DEBUG_Check_InputSystemRebinding : MonoBehaviour
	{
		// Track the active rebinding operation and button
		private InputActionRebindingExtensions.RebindingOperation activeRebindingOperation;
		private Button activeRebindingButton;
		private string activeRebindingOriginalText;

		private void Awake()
		{
			Debug.Log(C.method("Awake", this, "white"));
		}

		private void Start()
		{
			try
			{
				Debug.Log($"success parsing {GameDataType.inputKeyBindings}".colorTag("lime"));
				// Load from Saved GameData
				GameStore.PlayerIA.LoadBindingOverridesFromJson(LOG.LoadGameData(GameDataType.inputKeyBindings));
			}
			catch (Exception)
			{
				Debug.Log($"error parsing {GameDataType.inputKeyBindings}".colorTag("red"));
				// throw;
			}
		}

		private void Update()
		{

			if (INPUT.M.InstantDown(1))
			{
				StopAllCoroutines();
				CancelActiveRebinding(); // Also cancel rebinding operation
				StartCoroutine(STIMULATE());
			}
		}

		// Cancel any active rebinding operation
		private void CancelActiveRebinding()
		{
			if (activeRebindingOperation != null)
			{
				try
				{
					// Cancel will trigger OnCancel callback which disposes the operation
					activeRebindingOperation.Cancel();
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Error canceling rebinding operation: {e.Message}");
				}
				finally
				{
					// Ensure we clear the reference
					activeRebindingOperation = null;
				}

				// Restore the original button text
				if (activeRebindingButton != null && !string.IsNullOrEmpty(activeRebindingOriginalText))
				{
					activeRebindingButton.setBtnTxt(activeRebindingOriginalText);
					activeRebindingButton = null;
					activeRebindingOriginalText = null;
				}
			}
		}

		// custom key rebind
		IEnumerator STIMULATE()
		{
			yield return null;
			yield return RebindingSystemAnalysis();
			yield break;
		}

		IEnumerator RebindingSystemAnalysis()
		{
			yield return null;
			PlayerInputActions IA = GameStore.PlayerIA;
			this.UIIAMapIteration(IA.character.Get());
		}

		[SerializeField] Transform _contentScrollViewTr;
		[SerializeField] GameObject _templateRowPrefab;
		[SerializeField] GameObject _buttonPrefab;
		void UIIAMapIteration(InputActionMap actionMap)
		{
			// LOG.H("=== SIMPLE ITERATION ===");
			this._contentScrollViewTr.clearLeaves();

			// Iterate through all actions in the map
			foreach (InputAction action in actionMap.actions)
			{
				Transform newRowTr = null;
				if (action.bindings[0].isComposite == false)
				{
					newRowTr = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr).transform;
					newRowTr.clearLeaves();

					GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>().setBtnTxt(action.name);
				}

				// LOG.AddLog($"Action: {action.name}");

				// Iterate through bindings for this action
				// Note: bindings is a ReadOnlyArray
				for (int i = 0; i < action.bindings.Count; i++)
				{
					InputBinding binding = action.bindings[i];

					// Skip composite bindings (they're containers for other bindings)
					if (binding.isComposite)
					{
						LOG.AddLog($"  Binding[{i}]: COMPOSITE '{binding.name}'");
						continue;
					}

					// Skip part of composite bindings if you only want the main bindings
					if (binding.isPartOfComposite)
					{
						// do nothing
						// LOG.AddLog($"  Binding[{i}]: Part of composite - {binding.name} = {binding.effectivePath}");
						continue;
					}

					// Create button with rebinding functionality
					Button btn = GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>();
					btn.setBtnTxt(binding.GetDisplayString());

					// Capture the current action and binding index for the closure
					InputAction currentAction = action;
					int currentBindingIndex = i;

					// Add click listener to trigger rebinding
					btn.onClick.AddListener(() =>
					{
						// Cancel any existing rebinding operation before starting a new one
						CancelActiveRebinding();

						this.StopAllCoroutines();
						this.StartCoroutine(PerformRebinding(action, currentBindingIndex, btn));
					});

					// This is a regular binding
					LOG.AddLog($"  Binding[{i}]: {binding.effectivePath ?? "EMPTY"}");
				}
			}

			LOG.HEnd("=== SIMPLE ITERATION ===");
		}

		IEnumerator PerformRebinding(InputAction action, int bindingIndex, Button button)
		{
			PlayerInputActions IA = GameStore.PlayerIA;

			// Store the original text and button reference
			activeRebindingButton = button;
			activeRebindingOriginalText = action.bindings[bindingIndex].GetDisplayString();

			// Update button text to show waiting state
			button.setBtnTxt("Press key...");

			LOG.H($"Rebinding {action.name}");
			LOG.AddLog($"Original binding: {action.bindings[bindingIndex].effectivePath}", "");

			// Disable the action map
			IA.character.Disable();

			bool done = false;
			Debug.Log($"Press any key to rebind {action.name} (ESC to cancel)...".colorTag("lime"));

			activeRebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
				.WithControlsExcluding("Mouse")
				.WithCancelingThrough("<Keyboard>/escape")
				.WithTimeout(10f)
				.OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog($".OnComplete() NewBinding: {action.bindings[bindingIndex].effectivePath}");
					Debug.Log($".OnComplete() {action.bindings[bindingIndex].effectivePath}".colorTag("lime"));

					// Update button text with new binding
					button.setBtnTxt(action.bindings[bindingIndex].GetDisplayString());

					// Deselect the button
					if (EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject(null);

					// Clear the active rebinding tracking
					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					done = true;

					// Safely dispose the operation
					try
					{
						op?.Dispose();
					}
					catch (Exception e)
					{
						Debug.LogWarning($"Error disposing rebinding operation in OnComplete: {e.Message}");
					}
				}))
				.OnCancel((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog(".OnCancel() with esc");
					Debug.Log(".OnCancel()".colorTag("lime"));

					// Restore original button text
					button.setBtnTxt(action.bindings[bindingIndex].GetDisplayString());

					// Deselect the button
					if (EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject(null);

					// Clear the active rebinding tracking
					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					done = true;

					// Safely dispose the operation
					try
					{
						op?.Dispose();
					}
					catch (Exception e)
					{
						Debug.LogWarning($"Error disposing rebinding operation in OnCancel: {e.Message}");
					}
				}))
				.Start();

			// Wait for user input
			while (!done)
				yield return null;

			// Re-enable the action map
			IA.character.Enable();

			LOG.H("After Binding override JSON");
			LOG.AddLog(IA.SaveBindingOverridesAsJson(), "json");
			LOG.HEnd("After Binding override JSON");

			LOG.SaveGameData(GameDataType.inputKeyBindings, IA.SaveBindingOverridesAsJson());
		}

		private void OnDisable()
		{
			// Clean up any active rebinding when the component is disabled
			CancelActiveRebinding();
		}

		private void OnDestroy()
		{
			// Clean up any active rebinding when the component is destroyed
			CancelActiveRebinding();
		}
	}
}