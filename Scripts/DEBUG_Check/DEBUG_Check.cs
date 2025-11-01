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
//
using SPACE_UTIL;

namespace SPACE_CHECK
{
	public class DEBUG_Check : MonoBehaviour
	{
		private void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(STIMULATE());
			}
		}
	
		[SerializeField] GameObject pfBullet;
		[SerializeField] Sprite towerSprite;
		IEnumerator STIMULATE()
		{
			#region framRate
			yield return null;
			#endregion

			// Tower tower = new Tower();
			// LOG.AddLog(tower.ToJson());

			//GameObject gameObject = R.get<GameObject>(ResourceType.prefab__proto__cube);
			// R.preloadAll(C.getEnumValues<ResourceType>()); // error
			// R.preloadAll<ResourceType>(); // error

			// var clip = R.get<AudioClip>(ResourceType.audio__awavb); // error
			// var clip = R.get<AudioClip>("audio/awavb.wav"); // error

			// var clip = Resources.Load<GameObject>("cube"); // error
			// var clip = Resources.Load<GameObject>("prefab/proto/cube"); // works
			// var clip = Resources.Load<Material>("mat/metal"); // works
			// var clip = Resources.Load<TextAsset>("data/level"); // works

			// var clip = Resources.Load<AudioClip>("audio/awavb"); // ✅ CORRECT - no extension
			// LOG.AddLog(clip.name);
			// LOG.AddLog(R.getHeirarchy());

			R.preloadAll(C.getEnumValues<ResourceType>().map(en => (object)en).ToArray());
			LOG.AddLog(R.getHeirarchy(), syntaxType: " ");
			LOG.AddLog(R.stats(), syntaxType: " ");

			string name = R.get<GameObject>(ResourceType.prefab__proto__cube).name;
			Debug.Log(name);
		}
	}

	public enum TowerType
	{
		cannon,
		catapult,
	}

	public enum TowerTargetType
	{
		mostHealth,
		mostSpeed,
		mostArmour,
		leastHealth,
		leastSpeed,
		leastArmour,
	}

	public enum ResourcePrefabType
	{
		bullet__sphere,
		bullet__arrow,
		sprite__cannon,
	}

	[System.Serializable]
	public class Tower
	{
		public v2 pos = new v2(10, 10);
		public TowerType towerType = TowerType.cannon;
		public Vector3 bulletPoint; // public Transform bulletPoint;
		public Vector3 currTr; // public Transform currTr;
		public ResourcePrefabType prefabBullet; // public GameObject prefabBullet;
		public ResourcePrefabType spriteRefForUI; // public Sprite spriteRefForUI;
		public float fireInterval = 1f;
	
		public TowerTargetType primaryTowerTargetType = TowerTargetType.leastArmour, secondaryTowerTargetType = TowerTargetType.leastHealth;
	}
}