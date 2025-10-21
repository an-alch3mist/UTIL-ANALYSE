/*

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


	// prototype -> it works
	public class DEBUG_Check_InputSystemRebinding_prev : MonoBehaviour
	{
		private void Awake()
		{
			Debug.Log(C.method("Awake", this, "white"));
		}

		private void Start()
		{
			PlayerInputActions IA = GameStore.PlayerIA;
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(STIMULATE());
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
			#region rebind system -> works
			PlayerInputActions IA = GameStore.PlayerIA;
			var jumpAction = IA.character.jump;
			int bindingIndex = 1;

			LOG.H("Before Override");
			LOG.AddLog(IA.SaveBindingOverridesAsJson(), "json");
			LOG.AddLog($"Original binding: {jumpAction.bindings[bindingIndex].effectivePath}", "");
			LOG.HEnd("Before Override");

			// Disable
			IA.character.Disable();

			bool done = false;
			Debug.Log("Press any key to rebind (ESC to cancel)...".colorTag("lime"));
			jumpAction.PerformInteractiveRebinding(bindingIndex)
				.WithControlsExcluding("Mouse")
				.WithCancelingThrough("<Keyboard>/escape")
				.WithTimeout(10f)
				.OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.H("Interactive After New Rebinding");
					LOG.AddLog((string)$".OnComplete() NewBinding: {jumpAction.bindings[(int)bindingIndex].effectivePath}");
					LOG.HEnd("Interactive After New Rebinding");

					Debug.Log($".OnComplete() {jumpAction.bindings[bindingIndex].effectivePath}".colorTag("lime"));
					done = true;
					op.Dispose();
				}))
				.OnCancel((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog((string)".OnCancel() with esc");
					Debug.Log(".OnCancel()".colorTag("lime"));
					done = true;
					op.Dispose();
				}))
				.Start();

			// Wait for user input
			while (!done)
				yield return null;

			// Re-enable
			IA.character.Enable();

			LOG.H("After Binding override JSON");
			LOG.AddLog(IA.SaveBindingOverridesAsJson(), "json");
			LOG.HEnd("After Binding overide JSON");
			#endregion

			this.SimpleIAMapIteration(IA.character.Get());
			this.UIIAMapIteration(IA.character.Get());
		}
		/// <summary>
		/// Simple iteration through all actions and their bindings
		/// </summary>
		void SimpleIAMapIteration(InputActionMap actionMap)
		{
			LOG.H("=== SIMPLE ITERATION ===");

			// Iterate through all actions in the map
			foreach (InputAction action in actionMap.actions)
			{
				LOG.AddLog($"Action: {action.name}");

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
						LOG.AddLog($"  Binding[{i}]: Part of composite - {binding.name} = {binding.effectivePath}");
						continue;
					}

					// This is a regular binding
					LOG.AddLog($"  Binding[{i}]: {binding.effectivePath ?? "EMPTY"}");
				}
			}

			LOG.HEnd("=== SIMPLE ITERATION ===");
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

					// OLD
					// GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>().setBtnTxt(binding.effectivePath);

					// NEW
					GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>().setBtnTxt(binding.GetDisplayString());

					// This is a regular binding
					LOG.AddLog($"  Binding[{i}]: {binding.effectivePath ?? "EMPTY"}");
				}
			}

			LOG.HEnd("=== SIMPLE ITERATION ===");
		}

	}

*/

/* v0.1 -> it works */
/* without save/reset btn */
/*
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
			PlayerInputActions IA = GameStore.playerIA;
			this.UIIAMapIteration(IA.character.Get());
		}

		[SerializeField] Transform _contentScrollViewTr;
		[SerializeField] GameObject _templateRowPrefab;
		[SerializeField] GameObject _buttonPrefab;
		[SerializeField] Button _resetBinding;
		[SerializeField] Button _saveBinding;


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
			PlayerInputActions IA = GameStore.playerIA;

			// Store the original text and button reference
			activeRebindingButton = button;
			activeRebindingOriginalText = action.bindings[bindingIndex].GetDisplayString();

			// Update button text to show waiting state
			button.setBtnTxt("Press Any key....");

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

*/


/* vLatest */
/* with save/reset btn */
/*
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
using SPACE_UTIL;

namespace SPACE_CHECK
{
	public class DEBUG_Check_InputSystemRebinding : MonoBehaviour
	{
		// Track the active rebinding operation and button
		private InputActionRebindingExtensions.RebindingOperation activeRebindingOperation;
		private Button activeRebindingButton;
		private string activeRebindingOriginalText;

		// Cancellation keys
		private List<string> cancelKeys = new List<string>
		{
			"<Keyboard>/escape",
			"<Keyboard>/backspace",
			"<Keyboard>/delete"
		};

		private void Awake()
		{
			Debug.Log(C.method("Awake", this, "white"));
		}

		private void Start()
		{
			// start of rebinding UI initialization and routine
			StopAllCoroutines();
			CancelActiveRebinding();
			StartCoroutine(STIMULATE());

			// Hook up reset and save buttons
			if (_resetBinding != null)
			{
				_resetBinding.onClick.AddListener(ResetAllBindingsToDefault);
			}

			if (_saveBinding != null)
			{
				_saveBinding.onClick.AddListener(SaveBindings);
			}
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(1))
			{
				// the start of routine
			}
		}

		// Cancel any active rebinding operation
		private void CancelActiveRebinding()
		{
			if (activeRebindingOperation != null)
			{
				try
				{
					activeRebindingOperation.Cancel();
				}
				catch (Exception e)
				{
					Debug.Log($"Error canceling rebinding operation: {e.Message}".colorTag("yellow"));
				}
				finally
				{
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

		IEnumerator STIMULATE()
		{
			yield return null;
			yield return RebindingSystemAnalysis();
			yield break;
		}

		IEnumerator RebindingSystemAnalysis()
		{
			yield return null;
			PlayerInputActions IA = GameStore.playerIA;
			this.UIIAMapIteration(IA.character.Get());
		}

		[SerializeField] Transform _contentScrollViewTr;
		[SerializeField] GameObject _templateRowPrefab;
		[SerializeField] GameObject _buttonPrefab;
		[SerializeField] Button _resetBinding;
		[SerializeField] Button _saveBinding;

		void UIIAMapIteration(InputActionMap actionMap)
		{
			this._contentScrollViewTr.clearLeaves();
			foreach (InputAction action in actionMap.actions)
			{
				Transform newRowTr = null;
				if (action.bindings[0].isComposite == false)
				{
					newRowTr = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr).transform;
					newRowTr.clearLeaves();
					GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>().setBtnTxt(action.name);
				}
				for (int i = 0; i < action.bindings.Count; i++)
				{
					InputBinding binding = action.bindings[i];

					if (binding.isComposite)
					{
						LOG.AddLog($"  Binding[{i}]: COMPOSITE '{binding.name}'");
						continue;
					}
					if (binding.isPartOfComposite)
					{
						// do nothing
						continue;
					}

					// Create button with rebinding functionality
					Button btn = GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>();
					btn.setBtnTxt(binding.GetDisplayString());

					InputAction currentAction = action;
					int currentBindingIndex = i;

					btn.onClick.AddListener(() =>
					{
						CancelActiveRebinding();
						this.StopAllCoroutines();
						this.StartCoroutine(PerformRebinding(action, currentBindingIndex, btn));
					});

					LOG.AddLog($"  Binding[{i}]: {binding.effectivePath ?? "EMPTY"}");
				}
			}

			LOG.HEnd("=== SIMPLE ITERATION ===");
		}

		IEnumerator PerformRebinding(InputAction action, int bindingIndex, Button button)
		{
			PlayerInputActions IA = GameStore.playerIA;

			activeRebindingButton = button;
			activeRebindingOriginalText = action.bindings[bindingIndex].GetDisplayString();

			button.setBtnTxt("Press Any key....");

			LOG.H($"Rebinding {action.name}");
			LOG.AddLog($"Original binding: {action.bindings[bindingIndex].effectivePath}", "");

			// disable character actionMap for rebinding
			IA.character.Disable();

			bool done = false;
			bool wasCancelled = false;
			Debug.Log($"Press any key to rebind {action.name} (ESC/Backspace/Delete to cancel)...".colorTag("lime"));

			activeRebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
				.WithControlsExcluding("Mouse")
				.OnMatchWaitForAnother(0.1f)
				.WithTimeout(10f)
				.OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog($".OnComplete() NewBinding: {action.bindings[bindingIndex].effectivePath}");
					Debug.Log($".OnComplete() {action.bindings[bindingIndex].effectivePath}".colorTag("lime"));

					button.setBtnTxt(action.bindings[bindingIndex].GetDisplayString());

					if (EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject(null);

					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					done = true;

					try
					{
						op?.Dispose();
					}
					catch (Exception e)
					{
						Debug.Log($"Error disposing rebinding operation in OnComplete: {e.Message}".colorTag("yellow"));
					}
				}))
				.OnCancel((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog(".OnCancel() with cancel key");
					Debug.Log(".OnCancel()".colorTag("lime"));

					button.setBtnTxt(action.bindings[bindingIndex].GetDisplayString());

					if (EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject(null);

					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					wasCancelled = true;
					done = true;

					try
					{
						op?.Dispose();
					}
					catch (Exception e)
					{
						Debug.Log($"Error disposing rebinding operation in OnCancel: {e.Message}".colorTag("yellow"));
					}
				}))
				.Start();

			// Manual cancellation check for multiple keys
			while (!done)
			{
				// Check each cancel key manually
				foreach (string cancelPath in cancelKeys)
				{
					// cancel can also be done via keys
					if (IsCancelKeyPressed(cancelPath))
					{
						Debug.Log($"Cancel key pressed: {cancelPath}".colorTag("yellow"));
						activeRebindingOperation?.Cancel();
						break;
					}
				}
				yield return null;
			}

			// reEnable character actionMap, done with a certain Rebinding/cancelRebinding
			IA.character.Enable();

			if (!wasCancelled)
			{
				// successfully assigned
			}
		}

		// Helper method to check if a cancel key is pressed
		private bool IsCancelKeyPressed(string controlPath)
		{
			var control = InputSystem.FindControl(controlPath);
			if (control is UnityEngine.InputSystem.Controls.ButtonControl button)
			{
				return button.wasPressedThisFrame;
			}
			return false;
		}

		// RESET FUNCTIONALITY
		/// <summary>
		/// Reset a specific binding to its default value
		/// </summary>
		private void ResetBindingToDefault(InputAction action, int bindingIndex)
		{
			action.RemoveBindingOverride(bindingIndex);
			Debug.Log($"Reset binding {bindingIndex} of action '{action.name}' to default".colorTag("cyan"));
		}

		/// <summary>
		/// Reset all bindings for a specific action to defaults
		/// </summary>
		private void ResetActionToDefault(InputAction action)
		{
			action.RemoveAllBindingOverrides();
			Debug.Log($"Reset all bindings for action '{action.name}' to defaults".colorTag("cyan"));
		}

		/// <summary>
		/// Reset all bindings in an action map to defaults
		/// </summary>
		private void ResetActionMapToDefault(InputActionMap actionMap)
		{
			actionMap.RemoveAllBindingOverrides();
			Debug.Log($"Reset all bindings in action map '{actionMap.name}' to defaults".colorTag("cyan"));
		}

		/// <summary>
		/// Reset ALL bindings in the entire InputActionAsset to defaults
		/// </summary>
		public void ResetAllBindingsToDefault()
		{
			PlayerInputActions IA = GameStore.playerIA;

			// Method 1: Reset the entire asset
			IA.RemoveAllBindingOverrides();

			Debug.Log("All input bindings reset to defaults! (note: not saved yet)".colorTag("green"));

			// Do not Save override until save was pressed
			// Clear saved overrides
			// LOG.SaveGameData(GameDataType.inputKeyBindings, "");

			// Refresh the UI
			StartCoroutine(RebindingSystemAnalysis());
		}

		/// <summary>
		/// Save current bindings
		/// </summary>
		private void SaveBindings()
		{
			PlayerInputActions IA = GameStore.playerIA;
			string json = IA.SaveBindingOverridesAsJson();
			LOG.SaveGameData(GameDataType.inputKeyBindings, json);
			Debug.Log("Bindings saved!".colorTag("green"));
			LOG.AddLog("Saved JSON:", "json");
			LOG.AddLog(json, "");
		}

		/// <summary>
		/// Load saved bindings (call this on game start)
		/// </summary>
		public void LoadSavedBindings()
		{
			PlayerInputActions IA = GameStore.playerIA;
			// Assuming you have a method to load the saved JSON
			string savedJson = LOG.LoadGameData(GameDataType.inputKeyBindings);

			if (!string.IsNullOrEmpty(savedJson))
			{
				IA.LoadBindingOverridesFromJson(savedJson);
				Debug.Log("Loaded saved bindings".colorTag("green"));
			}
		}

		private void OnDisable()
		{
			CancelActiveRebinding();
		}

		private void OnDestroy()
		{
			CancelActiveRebinding();
		}
	}
}
*/