using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

// must be applied at beginning of game to each characters, vehicle, etc anything that has keyBindings
public class DEBUG_IAEventsController : MonoBehaviour
{
	private void Awake()
	{
		Debug.Log(("Awake(): " + this).colorTag("orange"));
		// this.IA = new PlayerInputActions();
		this.IA = GameStore.playerIA;
		this.InitIAEvents();
	}

	[Header("just to log")]
	[SerializeField] Vector2 movementDir;
	void InitIAEvents()
	{
		IA.character.jump.performed += (ctx) => this.Jump();
		IA.character.shoot.performed += (ctx) => this.Shoot();

		IA.character.walk.performed += (ctx) => this.movementDir = ctx.ReadValue<Vector2>();
		IA.character.walk.canceled += (ctx) => this.movementDir = Vector2.zero;
	}
	void Jump()
	{
		Debug.Log("Jump()");
	}
	void Shoot()
	{
		Debug.Log("Shoot()");
	}

	PlayerInputActions IA;
	#region enable/disable IA
	private void OnEnable()
	{
		Debug.Log(("OneEnable(): " + this).colorTag("orange"));
		IA.Enable();
	}

	private void OnDisable()
	{
		Debug.Log(("OnDisable(): " + this).colorTag("magenta"));
		IA.Disable();
	} 
	#endregion
}
