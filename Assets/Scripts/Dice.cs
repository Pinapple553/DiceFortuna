using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dice : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody rb;
	[Header("Debug")]
	public int defaultFaceResult = -1;
	public int alteredFaceResult = -1;

	[System.Serializable]
	public class DiceSide
	{
		public int value;
		public string specialEffect = "none";
		public Transform sideTransform;
	}
	[SerializeField] private List<DiceSide> diceSides;

	public void Reset()
	{
		defaultFaceResult = -1;
		alteredFaceResult = -1;
	}
	public void RotateDice(int alteredFaceResult)
	{
		if (alteredFaceResult != 8)
		{
			this.alteredFaceResult = alteredFaceResult;
			Vector3 rotate = new Vector3(0, 90, 0);
			transform.Rotate(rotate);
		}
		else
		{
			this.alteredFaceResult = defaultFaceResult;
		}
	}
	public int GetTopSideValue()
	{
		DiceSide topSide = null;
		float highestDot = -1f;

		foreach (var side in diceSides)
		{
			float dot = Vector3.Dot(side.sideTransform.up, Vector3.up);
			if (dot > highestDot)
			{
				highestDot = dot;
				topSide = side;
			}
		}
		return topSide != null ? topSide.value : 0; //if something goes wrong return 0
	}
}
