using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

[DefaultExecutionOrder(-50)] // InputSystem -> -100
public class GameINITManager : MonoBehaviour
{
	[TextArea(minLines: 5, maxLines: 10)]
	[SerializeField] string README = @"Assign @Start Scene of The Game To INIT GameStore
for InputActions:  initialize instance, initIAEvents can be done later(enable/disable) etc";

	private void Awake()
	{
		Debug.Log(C.method("Awake", this));
		InitGameStore();
	}

	private void InitGameStore()
	{
		GameStore.playerIA = new PlayerInputActions(); // InitIA can be done Later
		this.TryOverideLoadPlayerIA();
	}

	private void TryOverideLoadPlayerIA()
	{
		try
		{
			Debug.Log($"success parsing {GameDataType.inputKeyBindings} loaded IA with overiden bindings".colorTag("lime"));
			// Load from Saved GameData
			GameStore.playerIA.LoadBindingOverridesFromJson(LOG.LoadGameData(GameDataType.inputKeyBindings));
		}
		catch (Exception)
		{
			Debug.Log($"error parsing {GameDataType.inputKeyBindings} so loaded default IA".colorTag("red"));
		}
	}
}
