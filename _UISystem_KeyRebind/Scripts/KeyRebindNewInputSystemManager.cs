/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

using System.Linq;
using SPACE_UTIL;

public class KeyRebindNewInputSystemManager : MonoBehaviour
{
	[Header("Target Input Action Asset")]
	[SerializeField] private InputActionAsset IAControl;

	[Header("Target UI")]
	[SerializeField] Transform _actionMapBtnContainer;
	[SerializeField] Transform _actionRowContainer;

	[SerializeField] GameObject _templateActionMapBtn;
	[SerializeField] GameObject _templateActionRow;

	[SerializeField] GameObject _keyBindPopUp;

	private void Start()
	{
		Debug.Log(("Start(): " + this).colorTag());
		this.Init();
	}

	void Init()
	{
		Button firstActionMapBtn = null;
		foreach (var map in IAControl.actionMaps)
		{
			// actionMapBtn >>
			Button mapBtn = GameObject.Instantiate(this._templateActionMapBtn, this._actionMapBtnContainer).GC<Button>();
			if (firstActionMapBtn == null)
				firstActionMapBtn = mapBtn;

			TextMeshProUGUI mapText = mapBtn.gameObject.GCLeaf<TextMeshProUGUI>();
			mapText.text = map.name;

			mapBtn.onClick.AddListener(() =>
			{
				this._actionRowContainer.transform.clearLeaves(); // clear all leaf in Container
				foreach(var action in map.actions)
				{
					if (action.type != InputActionType.Button)
						continue;

					var BTN = GameObject.Instantiate(this._templateActionRow, this._actionRowContainer).GCLeaves<Button>().ToList();
					var TM = BTN.map(btn => btn.gameObject.GCLeaf<TextMeshProUGUI>()).ToList();

					// binding 0
					BTN[1].onClick.AddListener(() =>
					{
						this._keyBindPopUp.SetActive(true);
						StartCoroutine(keyBindPopUpRoutine(action, 0, BTN[1]));
					});


					TM[0].text = action.name;
					TM[1].text = action.GetBindingDisplayString(0); if (TM[1].text == "") TM[1].text = " ";
					if (action.bindings.Count > 1)
					{
						TM[2].text = action.GetBindingDisplayString(1);
						if (TM[2].text == "") TM[2].text = " ";
					}
				}
			});
			// << actionMapBtn

			firstActionMapBtn?.onClick.Invoke(); // if firstBtn exist ? perform all events subscribed to this button
			firstActionMapBtn.Select(); // to heighlight color tint
		}
	}

	KeyCode keyCodeDown;
	IEnumerator keyBindPopUpRoutine(InputAction inputActionRef, int bindingIndex, Button btn)
	{
		while(true)
		{
			if (Input.anyKeyDown)
			{
				keyCodeDown = KeyCode.I;
				break;
			}
			yield return null;
		}
		this._keyBindPopUp.SetActive(false);
		inputActionRef.ApplyBindingOverride(bindingIndex, "<Keyboard>/i");
		btn?.onClick.Invoke(); // redo UI elements
	}
}
*/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

using System.Linq;
using SPACE_UTIL;

public class KeyRebindNewInputSystemManager : MonoBehaviour
{
	[Header("Target Input Action Asset")]
	[SerializeField] private InputActionAsset IAControl;

	[Header("Target UI")]
	[SerializeField] Transform _actionMapBtnContainer;
	[SerializeField] Transform _actionRowContainer;

	[SerializeField] GameObject _templateActionMapBtn;
	[SerializeField] GameObject _templateActionRow;

	[SerializeField] GameObject _keyBindPopUp;
	[SerializeField] TextMeshProUGUI _keyBindPopUpText; // Optional: Add text to show "Press any key..."

	private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

	private void Start()
	{
		Debug.Log(("Start(): " + this).colorTag());
		this.Init();
	}

	void Init()
	{
		Button firstActionMapBtn = null;
		foreach (var map in IAControl.actionMaps)
		{
			// actionMapBtn >>
			Button mapBtn = GameObject.Instantiate(this._templateActionMapBtn, this._actionMapBtnContainer).GC<Button>();
			if (firstActionMapBtn == null)
				firstActionMapBtn = mapBtn;

			TextMeshProUGUI mapText = mapBtn.gameObject.GCLeaf<TextMeshProUGUI>();
			mapText.text = map.name;

			mapBtn.onClick.AddListener(() =>
			{
				this._actionRowContainer.transform.clearLeaves(); // clear all leaf in Container
				foreach (var action in map.actions)
				{
					if (action.type != InputActionType.Button)
						continue;

					var BTN = GameObject.Instantiate(this._templateActionRow, this._actionRowContainer).GCLeaves<Button>().ToList();
					var TM = BTN.map(btn => btn.gameObject.GCLeaf<TextMeshProUGUI>()).ToList();

					// binding 0
					BTN[1].onClick.AddListener(() =>
					{
						StartRebind(action, 0, BTN[1], mapBtn);
					});

					// binding 1 (if exists)
					if (action.bindings.Count > 1 && BTN.Count > 2)
					{
						BTN[2].onClick.AddListener(() =>
						{
							StartRebind(action, 1, BTN[2], mapBtn);
						});
					}

					TM[0].text = action.name;
					TM[1].text = action.GetBindingDisplayString(0); if (TM[1].text == "") TM[1].text = " ";
					if (action.bindings.Count > 1)
					{
						TM[2].text = action.GetBindingDisplayString(1);
						if (TM[2].text == "") TM[2].text = " ";
					}
				}
			});
			// << actionMapBtn

			firstActionMapBtn?.onClick.Invoke(); // if firstBtn exist ? perform all events subscribed to this button
			firstActionMapBtn.Select(); // to heighlight color tint
		}
	}

	void StartRebind(InputAction action, int bindingIndex, Button targetButton, Button mapBtn)
	{
		// Cancel any existing rebind operation
		_rebindOperation?.Cancel();

		// Disable the action temporarily
		action.Disable();

		this._keyBindPopUp.SetActive(true);
		if (_keyBindPopUpText != null)
			_keyBindPopUpText.text = "Press any key...";

		_rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
			.WithCancelingThrough("<Keyboard>/escape")
			.WithControlsExcluding("<Keyboard>/escape")
			.WithControlsExcluding("<Keyboard>/backspace")
			.WithControlsExcluding("<Mouse>/position")
			.WithControlsExcluding("<Mouse>/delta")
			.OnMatchWaitForAnother(0.1f)
			.OnComplete(operation =>
			{
				RebindComplete(action, targetButton, mapBtn);
				operation.Dispose();
			})
			.OnCancel(operation =>
			{
				RebindCanceled(action);
				operation.Dispose();
			})
			.Start();
	}

	void RebindComplete(InputAction action, Button targetButton, Button mapBtn)
	{
		this._keyBindPopUp.SetActive(false);

		// Re-enable the action
		action.Enable();

		// Update the UI by re-triggering the button
		targetButton?.transform.parent.GetComponentInChildren<Button>()?.onClick.Invoke();

		_rebindOperation?.Dispose();
		_rebindOperation = null;

		mapBtn.onClick.Invoke();
	}

	void RebindCanceled(InputAction action)
	{
		this._keyBindPopUp.SetActive(false);

		// Re-enable the action
		action.Enable();

		_rebindOperation?.Dispose();
		_rebindOperation = null;
	}

	private void OnDestroy()
	{
		// Clean up rebind operation if still active
		_rebindOperation?.Dispose();
	}
}