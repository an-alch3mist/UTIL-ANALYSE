using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPACE_TransparentOverlayUsingWindowAPI
{
	public class TransparentOverlayUsingWindowAPIFrameRate : MonoBehaviour
	{
		[SerializeField] int _frameRate = 20;
		private void Start()
		{
			Debug.Log("Start(): " + this);
			Application.targetFrameRate = this._frameRate;
		}
	}

}