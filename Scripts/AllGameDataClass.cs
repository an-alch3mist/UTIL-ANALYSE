using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// ============= USAGE EXAMPLES =============
/// <summary>
/// Example data class for player stats
/// </summary>
[System.Serializable]
public class PlayerStats
{
	public string playerName = "Player";
	public int level = 1;
	public float health = 100f;
	public int experience = 0;

	public PlayerStats()
	{
		playerName = "Player";
		level = 1;
		health = 100f;
		experience = 0;
	}

	public override string ToString()
	{
		return $"name: {this.playerName}, level: {this.level}";
		// return base.ToString();
	}
}