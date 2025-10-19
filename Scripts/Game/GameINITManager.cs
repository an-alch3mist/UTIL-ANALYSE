using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
		GameStore.PlayerIA = new PlayerInputActions(); // InitIA can be done Later
	}
}
