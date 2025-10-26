using UnityEngine;
using System.Collections;

/// <summary>
/// Game data types - add new entries here for each saveable data type
/// Each enum value maps to a JSON file: LOG/GameData/{enumName}.json
/// </summary>
public enum GameDataType
{
	inputKeyBindings,
	playerStats,
}

