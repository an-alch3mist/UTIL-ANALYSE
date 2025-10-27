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

public enum ResourceType
{
	// Prefabs - will load from Resources/prefab/...
	prefab__proto__cube,      // → Resources/prefab/proto/cube.prefab

	// Sprites - will load from Resources/sprite/...
	// sprite__icon__health,        // → Resources/sprite/icon/health.png
	// sprite__ui__button,          // → Resources/sprite/ui/button.png

	// Audio - will load from Resources/audio/...
	audio__awavb, // awavb.wav (or .mp3, .ogg)

	// Materials
	mat__metal,      // → Resources/material/metal/rusty.mat

	// Data/Config files
	data__level,              // → Resources/data/level/1.json
	// config__gameplay,            // → Resources/config/gameplay.asset
}
