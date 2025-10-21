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
using SPACE_SYNTAX;

// checksum
namespace SPACE_CHECK
{
	public class DEBUG_Check_InputSystemRebinding : MonoBehaviour
	{
		#region SEEK
		// Track the active rebinding operation and button
		private InputActionRebindingExtensions.RebindingOperation activeRebindingOperation;
		private Button activeRebindingButton;
		private string activeRebindingOriginalText;

		// Cancellation keys
		private List<string> cancelKeys = new List<string>
		{
			"<Keyboard>/escape",
			"<Keyboard>/backspace",
			"<Keyboard>/delete",
		}; 
		#endregion

		private void Awake()
		{
			// Debug.Log(C.method("Awake", this, "white"));
		}

		[TextArea(minLines: 22, maxLines: 24)]
		[SerializeField] string README = $@"# file structure: 
UIRebindingSystem( -> Attach {typeof(DEBUG_Check_InputSystemRebinding).Name}.cs to UIRebindingSystem )
	template--scroll_view/viewport/content
		template--row
			action name button(0)
			binding button(1)
			binding button(2)
			.
			.
	save / reset [panel]
		start button
		save button
	close button [button]

# reference [serializeField]
- template -- button Prefab
- contentHolder (got contentSizeFitter, VerticalLayoutGroup components Attached)
- template -- row Prefab
- save, reset, close window button

# make sure: 
- There is PlayerInputActions.cs generated from inputAction
- GameStore.playerIA shall lead to one of its instance";

		PlayerInputActions IA;
		private void OnEnable()
		{
			Debug.Log(C.method("OnEnable", this, "white"));

			// playerIA from GameStore
			this.IA = GameStore.playerIA;

			// start of rebinding UI initialization and routine
			StopAllCoroutines();
			CancelActiveRebinding();
			StartCoroutine(STIMULATE());

			// Hook up reset and save buttons
			if (this._resetBinding != null)
				_resetBinding.onClick.AddListener(ResetAllBindingsToDefault);

			if (this._saveBinding != null)
				_saveBinding.onClick.AddListener(SaveBindings);

			if (this._closeBtn != null)
				_closeBtn.onClick.AddListener(() => { this.gameObject.SetActive(false); });
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(1))
			{
				// the start of routine
				// started from Start() [UnityLifeCycle]
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

		[Header("templateUI/UI elem reference")]
		[SerializeField] Transform _contentScrollViewTr;
		[SerializeField] GameObject _templateRowPrefab;
		[SerializeField] GameObject _buttonPrefab;
		[SerializeField] Button _resetBinding;
		[SerializeField] Button _saveBinding;
		[SerializeField] Button _closeBtn;

		IEnumerator RebindingSystemAnalysis()
		{
			yield return null;
			this.UIIAMapIteration(this.IA);
		}

		void UIIAMapIteration(PlayerInputActions IA)
		{
			// Clear existing UI
			this._contentScrollViewTr.clearLeaves();

			// Iterate through ALL action maps
			foreach (var actionMap in IA.asset.actionMaps)
			{
				// Create a header/separator for each action map
				GameObject headerRow = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr); headerRow.transform.clearLeaves();

				// Create a non-clickable label button for the action map name
				Button headerBtn = GameObject.Instantiate(this._buttonPrefab, headerRow.transform).GC<Button>();
				headerBtn.setBtnTxt($"== {actionMap.name.ToUpper()} ==");
				headerBtn.interactable = false; // Make it non-interactive

				// rebinding buttons
				foreach (InputAction action in actionMap.actions)
				{
					Transform newRowTr = null;

					// do nothing if its composite action
					if (action.bindings[0].isComposite == true)
						continue;

					// button to show case action
					newRowTr = GameObject.Instantiate(this._templateRowPrefab, this._contentScrollViewTr).transform; newRowTr.clearLeaves();
					GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>().setBtnTxt(action.name);

					for (int i = 0; i < action.bindings.Count; i += 1)
					{
						InputBinding binding = action.bindings[i];

						if (binding.isComposite)
						{
							LOG.AddLog($"  Binding[{i}]: COMPOSITE '{binding.name}'");
							continue;
						}
						if (binding.isPartOfComposite)
						{
							continue;
						}

						// Create button with rebinding functionality
						Button btn = GameObject.Instantiate(this._buttonPrefab, newRowTr).GC<Button>();

						UpdateButtonText(btn, action, i);

						InputAction currentAction = action;
						int currentBindingIndex = i;

						btn.onClick.AddListener(() =>
						{
							CancelActiveRebinding();
							this.StopAllCoroutines();

							if (EventSystem.current != null)
								EventSystem.current.SetSelectedGameObject(null);

							this.StartCoroutine(PerformRebinding(currentAction, currentBindingIndex, btn));
						});

						LOG.AddLog($"  Binding[{i}]: {binding.effectivePath ?? "EMPTY"}");
					}
				}
			}
		}
		IEnumerator PerformRebinding(InputAction action, int bindingIndex, Button button)
		{
			activeRebindingButton = button;
			// MODIFY: Store action and binding index instead of text string
			// We'll use these to get the correct display text when needed
			activeRebindingOriginalText = null; // Not needed anymore, but keeping for cleanup

			button.setBtnTxt("Press Any key....");

			LOG.H($"Rebinding {action.name}");
			LOG.AddLog($"Original binding: {action.bindings[bindingIndex].effectivePath}", "");

			// Get the action map that this action belongs to
			InputActionMap actionMap = action.actionMap;
			// disable character actionMap for rebinding
			// actionMap.Disable();
			IA.Disable();

			bool done = false;
			bool wasCancelled = false;
			Debug.Log($"Press any key to rebind {action.name} (ESC/Backspace/Delete to cancel)...".colorTag("lime"));

			activeRebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
				.WithControlsExcluding("Mouse")
				// REMOVE: These don't reliably prevent anyKey binding
				// .WithControlsExcluding("<Keyboard>/escape")
				// .WithControlsExcluding("<Keyboard>/backspace")
				// .WithControlsExcluding("<Keyboard>/delete")
				// .WithControlsExcluding("<Keyboard>/enter")

				// ADD: Use OnPotentialMatch to intercept BEFORE binding completes
				.OnPotentialMatch(op =>
				{
					// Check if the potential match is one of our cancel keys
					var control = op.candidates.Count > 0 ? op.candidates[0] : null;
					if (control != null)
					{
						string path = control.path.ToLower();

						// If it's a cancel key, manually cancel the operation
						if (path.Contains("/escape") ||
							path.Contains("/backspace") ||
							path.Contains("/delete") ||
							path.Contains("/enter") ||
							path.Contains("/numpadEnter"))
						{
							Debug.Log($"Intercepted cancel key: {control.path}".colorTag("orange"));
							op.Cancel();
							return;
						}
					}
				})

				.OnMatchWaitForAnother(0.05f)
				.WithTimeout(10f)
				.OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>)(op =>
				{
					LOG.AddLog($".OnComplete() NewBinding: {action.bindings[bindingIndex].effectivePath}");
					Debug.Log($".OnComplete() {action.bindings[bindingIndex].effectivePath}".colorTag("lime"));

					// MODIFY: Use helper method to update button text
					UpdateButtonText(button, action, bindingIndex);

					if (EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject(null);

					activeRebindingOperation = null;
					activeRebindingButton = null;
					activeRebindingOriginalText = null;

					done = true;

					try
					{
						// completed -> op.Dispose()
						// canceled -> op.Cancel()
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

					// MODIFY: Use helper method to restore button text based on current binding state
					UpdateButtonText(button, action, bindingIndex);

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
				foreach (string cancelPath in cancelKeys)
				{
					if (IsCancelKeyPressed(cancelPath)) // not required since the input leading to cancel are intercepted in Potential Match which fires before OnComplete, or OnCancel
					{
						Debug.Log($"Cancel key pressed: {cancelPath}".colorTag("yellow"));
						activeRebindingOperation?.Cancel();
						break;
					}
				}
				yield return null;
			}

			// reEnable character actionMap, done with a certain Rebinding/cancelRebinding
			// actionMap.Enable();
			IA.Enable();

			if (wasCancelled == false)
			{
				// successfully assigned
			}
		}

		#region Helper

		// MODIFY: Add new helper method to update button text based on binding
		private void UpdateButtonText(Button button, InputAction action, int bindingIndex)
		{
			string displayText = action.bindings[bindingIndex].ToDisplayString();

			// If the binding is empty or returns empty string, show "Not Bound"
			if (string.IsNullOrEmpty(displayText) ||
				string.IsNullOrEmpty(action.bindings[bindingIndex].effectivePath))
			{
				displayText = "Not Bound";
			}

			button.setBtnTxt(displayText);
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
		#endregion

		/// <summary>
		/// Reset ALL bindings in the entire InputActionAsset to defaults
		/// </summary>
		public void ResetAllBindingsToDefault()
		{
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
			Debug.Log(C.method("OnDisable", this, "orange"));
			CancelActiveRebinding();
		}
		private void OnDestroy()
		{
			Debug.Log(C.method("OnDestroy", this, "orange"));
			CancelActiveRebinding();
		}
	}
}