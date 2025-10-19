using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//
using SPACE_UTIL;

namespace SPACE_CHECK
{
	public class DEBUG_Check : MonoBehaviour
	{
		private void Awake()
		{
			Debug.Log(C.method("Awake", this, "white"));
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(STIMULATE());
			}
		}

		[SerializeField] InputActionAsset _IAAsset;
		// custom key rebind
		IEnumerator STIMULATE()
		{
			yield return null;

			PlayerInputActions IA = GameStore.PlayerIA;
			var jumpAction = IA.character.jump;
			int bindingIndex = 1;

			LOG.H("Before Override");
			LOG.SaveLog(IA.SaveBindingOverridesAsJson(), "json");
			LOG.SaveLog($"Original binding: {jumpAction.bindings[bindingIndex].effectivePath}", "");
			LOG.HEnd("Before Override");

			// Disable
			IA.character.Disable();

			bool done = false;
			Debug.Log("Press any key to rebind (ESC to cancel)...".colorTag("lime"));
			jumpAction.PerformInteractiveRebinding(bindingIndex)
				.WithControlsExcluding("Mouse")
				.WithCancelingThrough("<Keyboard>/escape")
				.WithTimeout(10f)
				.OnComplete(op =>
				{
					LOG.H("Interactive After New Rebinding");
					LOG.SaveLog($".OnComplete() NewBinding: {jumpAction.bindings[bindingIndex].effectivePath}");
					LOG.HEnd("Interactive After New Rebinding");

					Debug.Log($".OnComplete() {jumpAction.bindings[bindingIndex].effectivePath}".colorTag("lime"));
					done = true;
					op.Dispose();
				})
				.OnCancel(op =>
				{
					LOG.SaveLog(".OnCancel() with esc");
					Debug.Log(".OnCancel()".colorTag("lime"));
					done = true;
					op.Dispose();
				})
				.Start();

			// Wait for user input
			while (!done) yield return null;

			// Re-enable
			IA.character.Enable();

			LOG.H("After Binding override JSON");
			LOG.SaveLog(IA.SaveBindingOverridesAsJson(), "json");
			LOG.HEnd("After Binding overide JSON");
		}

		IEnumerator STIMULATE_0()
		{
			yield return null;

			// Get the player's instance
			PlayerInputActions IA = GameStore.PlayerIA;
			var jumpAction = IA.character.jump;
			// int bindingIndex = IAUtil.getkeyboardBindingIndex(jumpAction);
			int bindingIndex = 1;

			LOG.H("Before Override");
			LOG.SaveLog(jumpAction.bindings.ToTable(name: "LIST<> BINDING"));
			LOG.SaveLog($"Original binding: {jumpAction.bindings[bindingIndex].effectivePath}", "");
			LOG.HEnd("Before Override");

			// PROPER REBINDING WORKFLOW
			// 1. Disable the entire action map(in this case: character actionMap) (recommended) or specific action
			IA.character.Disable();

			// 2. Find the specific binding index (more robust than hardcoding 0)

			// 3. Apply the override
			jumpAction.ApplyBindingOverride(bindingIndex, "<Keyboard>/j");

			// 4. Re-enable
			IA.character.Enable();

			LOG.H("After Override");
			LOG.SaveLog($"New binding: {jumpAction.bindings[bindingIndex].effectivePath}", "");
			LOG.SaveLog($"Effective binding: {jumpAction.bindings[bindingIndex].effectivePath}", "");
			LOG.HEnd("After Override");

			yield return null;
		}

		void CheckInputStore_JSON_SaveLoad()
		{
			#region inputStore Init /* */
			/*
				inputStore = new InputStore()
				{
					ACTION_MAP = new List<ActionMap>()
					{
						new ActionMap()
						{
							name = "character",
							ACTION = new List<Action>()
							{
								new Action()
								{
									name = "jump",
									BINDING = new List<Binding>()
									{
										new Binding()
										{
											name = "space",
											keyCode = KeyCode.Space,
										},
										new Binding()
										{
											name = "j",
											keyCode = KeyCode.J,
										},
									}
								},
								new Action()
								{
									name = "shoot",
									BINDING = new List<Binding>()
									{
										new Binding()
										{
											name = "s",
											keyCode = KeyCode.S,
										}
									}
								},
							}
						},
						new ActionMap()
						{
							name = "car",
							ACTION = new List<Action>()
							{
								new Action()
								{
									name = "brake",
									BINDING = new List<Binding>()
									{
										new Binding()
										{
											name = "space",
											keyCode = KeyCode.Space,
										},
									}
								},
								new Action()
								{
									name = "shoot",
									BINDING = new List<Binding>()
									{
										new Binding()
										{
											name = "s",
											keyCode = KeyCode.S,
										}
									}
								},
							}
						},
					}
				};
				*/
			#endregion

			InputStore inputStore = C.FromJson<InputStore>(LOG.LoadGame);
			Debug.Log(inputStore.ToJson());

		}
	}

	public static class IAUtil
	{
		public static int getkeyboardBindingIndex(InputAction inputAction)
		{
			int bindingIndex = -1;
			for (int i = 0; i < inputAction.bindings.Count; i += 1)
				if (inputAction.bindings[i].effectivePath.Contains("<Keyboard>"))
				{
					bindingIndex = i;
					break;
				}
			//
			Debug.Log("bindingIndex: " + bindingIndex);
			if (bindingIndex == -1)
			{
				Debug.Log("No keyboard binding found!".colorTag("red"));
				return -1;
			}
			//
			return bindingIndex;
		}
	}


	[System.Serializable]
	public class InputStore
	{
		public List<ActionMap> ACTION_MAP;
	}

	#region InputStore Desendents
	[System.Serializable]
	public class ActionMap
	{
		public string name;
		public List<Action> ACTION;
	}

	[System.Serializable]
	public class Action
	{
		public string name;
		public List<Binding> BINDING;
	}

	[System.Serializable]
	public class Binding
	{
		public string name = "";
		public KeyCode keyCode;
	} 
	#endregion
}