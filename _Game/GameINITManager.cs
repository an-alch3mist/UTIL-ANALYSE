using System;
using System.IO;
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
		GameStore.playerIA = new PlayerInputActions(); this.TryLoadBindingOverridesFromJson(GameStore.playerIA); // InitIAEvents can be done Later

		// in future
		// GameStore.A = LOG.LoadGameData<PlayerStats>(GameDataType.playerStats); // if parsing fail -> return default values via new T()
		// GameStore.Setting = LOG.LoadGameData<GameSetting>(GameDataType.setting); // if parsing fail -> return default values via new T()
	}

	#region TryLoadBindingOverridesFromJson
	/// <summary>
	/// Load game data and override it to existing playerIA
	/// do nothing if parsing fails
	/// </summary>
	private void TryLoadBindingOverridesFromJson(PlayerInputActions playerIA)
	{
		try
		{
			Debug.Log($"success parsing {GameDataType.inputKeyBindings} loaded IA with overiden bindings".colorTag("lime"));
			// Load from Saved GameData
			playerIA.LoadBindingOverridesFromJson(LOG.LoadGameData(GameDataType.inputKeyBindings));
		}
		catch (Exception)
		{
			Debug.Log($"error parsing {GameDataType.inputKeyBindings} so loaded default IA".colorTag("red"));
		}
	}


	#endregion
}
