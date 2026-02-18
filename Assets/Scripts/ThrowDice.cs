using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowDice : MonoBehaviour
{
	public AnimationRecorder animRecorder;

	[Header("Dice Throw Parameters")]
	[SerializeField] private float rollForce = 5f;
	[SerializeField] private float rollTorque = 2f;
	[SerializeField] private float directionRandomness = 0.2f;
	[SerializeField] private float stableTimeRequired = 0.5f;
	[SerializeField] private Transform throwOrigin;
	[SerializeField] private Transform throwTarget;

	[SerializeField] private List<Dice> diceList = new List<Dice>();
	public List<DiceData> diceDataList;
	private List<Dice> activeDices = new List<Dice>();
	public List<int> targetedResult;

	private PlayerController controller;
	private bool diceThrown;


	[System.Serializable]
	public struct DiceData
	{
		public GameObject diceObject;
		public Rigidbody rb;
		public Dice diceLogic;

		public DiceData(GameObject diceObject)
		{
			this.diceObject = diceObject;
			this.rb = diceObject.GetComponent<Rigidbody>();
			this.diceLogic = diceObject.transform.GetChild(0).GetComponent<Dice>();
			this.rb.maxAngularVelocity = 1000;
		}
	}

	[System.Serializable]
	public struct InitialState
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 force;
		public Vector3 torque;

		public InitialState(Vector3 position, Quaternion rotation,
							Vector3 force, Vector3 torque)
		{
			this.position = position;
			this.rotation = rotation;
			this.force = force;
			this.torque = torque;
		}
	}

	private void Awake()
	{
		controller = new PlayerController();
		controller.Dice.Roll.performed += RollAllDice;
		controller.Dice.Reset.performed += DestroyDice;
	}

	private void OnEnable()
	{
		controller.Dice.Enable();
	}
	private void OnDisable()
	{
		controller.Dice.Disable();
	}

	private void RollAllDice(InputAction.CallbackContext ctx)
	{
		if (diceList.Count > 0)
		{
				CreateDice();
		}
		animRecorder.StartSimulation(diceList);

		diceThrown = true;
		StartCoroutine(WaitForDiceToStop());
	}
	private void CreateDice()
	{
		foreach (Dice dice in diceList)
		{
			Instantiate(dice, throwOrigin.position, Quaternion.identity);
			activeDices.Add(dice);
		}
		for (int i = 0; i < diceList.Count; i++)
		{
			InitialState initial = SetInitialState(rollForce, rollTorque, directionRandomness);

			diceDataList[i].diceLogic.Reset();
			diceDataList[i].diceObject.transform.position = initial.position;
			diceDataList[i].diceObject.transform.rotation = initial.rotation;
			diceDataList[i].rb.useGravity = true;
			diceDataList[i].rb.isKinematic = false;
			diceDataList[i].rb.linearVelocity = initial.force;
			diceDataList[i].rb.AddTorque(initial.torque, ForceMode.VelocityChange);
		}
	}
	private void DestroyDice(InputAction.CallbackContext ctx)
	{
		foreach (Dice dice in activeDices)
		{
			Destroy(dice.gameObject);
		}
		activeDices.Clear();
	}
	private InitialState SetInitialState(float forceMax=5f, float torqueMax =2f, float directionRandomAmount = 0.1f)
	{
		float x = transform.position.x + UnityEngine.Random.Range(-transform.localScale.x / 2, transform.localScale.x / 2);
		float y = transform.position.y + UnityEngine.Random.Range(-transform.localScale.y / 2, transform.localScale.y / 2);
		float z = transform.position.z + UnityEngine.Random.Range(-transform.localScale.z / 2, transform.localScale.z / 2);
		Vector3 position = new Vector3(x, y, z);

		x = UnityEngine.Random.Range(0, 360);
		y = UnityEngine.Random.Range(0, 360);
		z = UnityEngine.Random.Range(0, 360);
		Quaternion rotation = Quaternion.Euler(x, y, z);

		x = UnityEngine.Random.Range(0, forceMax);
		y = UnityEngine.Random.Range(0, forceMax);
		z = UnityEngine.Random.Range(0, forceMax);
		Vector3 force = new Vector3(x, -y, z);

		x = UnityEngine.Random.Range(0, torqueMax);
		y = UnityEngine.Random.Range(0, torqueMax);
		z = UnityEngine.Random.Range(0, torqueMax);
		Vector3 torque = new Vector3(x, y, z);

		//move this to where dice are thrown
		Vector3 direction = (throwTarget.position - throwOrigin.position).normalized;
		direction += UnityEngine.Random.insideUnitSphere * directionRandomness;
		direction.y = Mathf.Abs(direction.y);

		return new InitialState(position,rotation,force,torque);
	}
	/*
	private void SimulatePhysics()
	{
		Physics.simulationMode = SimulationMode.Script;
		for (int i = 0; i < 100; i++)
		{
			Physics.Simulate(Time.fixedDeltaTime);
		}
		Physics.simulationMode = SimulationMode.FixedUpdate;
		GetTopSideValue();
	}
	private IEnumerator WaitForDiceToStop()
	{
		float stableTime = 0f;

		while (stableTime < stableTimeRequired)
		{
			if (rb.linearVelocity.sqrMagnitude < 0.001f && rb.angularVelocity.sqrMagnitude < 0.001f)
			{
				stableTime += Time.deltaTime;
			}
			else
			{
				stableTime = 0f;
			}

			yield return null;
		}

		yield return new WaitForSeconds(0.1f);

		int result = GetTopSideValue();
		diceRollText.text = $"Roll: {result}";
		StartCoroutine(ShowDiceResult());
	}

	private int GetTopSideValue()
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
	private void ResetDice(InputAction.CallbackContext ctx)
	{
		StopAllCoroutines();

		rb.constraints = RigidbodyConstraints.None;
		rb.isKinematic = false;

		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		rb.transform.position = startPosition;
		rb.transform.rotation = startRotation;

		rb.constraints = RigidbodyConstraints.FreezeAll;

		diceThrown = false;
		diceRollText.text = "";
	}
	private IEnumerator ShowDiceResult()
	{
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		rb.isKinematic = true;

		Vector3 start = rb.position;
		Vector3 target = new Vector3(0, 10, 0);


		float elapsedDiceTime = 0f;
		float duration = 1.5f;

		while (elapsedDiceTime < duration)
		{
			elapsedDiceTime += Time.deltaTime;
			float progress = Mathf.Clamp01(elapsedDiceTime / duration);
			rb.transform.position = Vector3.Lerp(rb.transform.position, target, progress);
			yield return null;
		}
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}
	*/
}