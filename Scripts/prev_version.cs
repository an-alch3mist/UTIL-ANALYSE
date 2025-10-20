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