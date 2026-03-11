using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class DiceManager : MonoBehaviour
{
    [SerializeField] public List<Dice> diceList;
    [SerializeField] private UIManager ui;
    [SerializeField] private AnimationRecorder anim;

    [Header("Dice Throw Parameters")]
    [SerializeField] private float rollForce = 6f;
    [SerializeField] private float rollTorque = 8f;
    [SerializeField] private float directionRandomness = 0.2f;
    [SerializeField] private float stableTimeRequired = 0.5f;

    [SerializeField] private Transform throwOrigin;
    [SerializeField] private Transform throwTarget;

    [Header("Rig")]
    [SerializeField] private int forcedValue = -1;

    private List<Dice> activeDice = new();
    private PlayerController controller;

	public List<DiceData> diceDataList;

	private void Awake()
    {
        controller = new PlayerController();
           diceDataList = new List<DiceData>();
        //controller.Dice.Roll.performed += ctx => RollRoutine();
        //controller.Dice.Reset.performed += ResetDice;
    }
    private void OnEnable()
    {
        controller.Dice.Enable();
    }
    private void OnDisable()
    {
        controller.Dice.Disable();
    }
    public bool AddDice(Dice dice)
    {
        if (diceList.Count < 9)
        {
            diceList.Add(dice);
            return true;
        }
        return false;
    }
    public bool RemoveDice(Dice dice)
    {
        for (int i = diceList.Count-1; i >= 0 ; i--)
        {
            if (diceList[i].diceType == dice.diceType)
            {
                diceList.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public void RollRoutine()
    {
        ResetDice(default);
        CreateDice();

		anim.StartSimulation(activeDice);

		foreach (var dice in activeDice)
		{
			dice.CalculateResult();
		}
        anim.ResetToInitialState();

        if (forcedValue > 0)
        {
            foreach (var dice in activeDice)
            {
                dice.RotateToFace(forcedValue);
            }
        }

        foreach (var dice in activeDice)
        {
            dice.GetComponent<Rigidbody>().isKinematic = true;
        }
        anim.PlayRecording();
    }
    private void CreateDice()
    {
        foreach (var dice in diceList)
        {
			DiceData d = new DiceData(Instantiate(dice));
			d.diceLogic.SetStableTime(stableTimeRequired);
			diceDataList.Add(d);
			activeDice.Add(d.diceLogic);

		}
		for (int i = 0; i < activeDice.Count; i++)
		{
			InitialState initial = SetInitialState();

			diceDataList[i].diceLogic.Reset();
			diceDataList[i].diceObject.transform.position = initial.position;
			diceDataList[i].diceObject.transform.rotation = initial.rotation;
			diceDataList[i].rb.useGravity = true;
			diceDataList[i].rb.isKinematic = false;
			diceDataList[i].rb.linearVelocity = initial.force;
			diceDataList[i].rb.AddTorque(initial.torque, ForceMode.VelocityChange);
		}
	}
	private InitialState SetInitialState()
	{
		float x = transform.position.x + Random.Range(-transform.localScale.x / 2, transform.localScale.x / 2);
		float y = transform.position.y + Random.Range(-transform.localScale.y / 2,transform.localScale.y / 2);
		float z = transform.position.z + Random.Range(-transform.localScale.z / 2,transform.localScale.z / 2);
		Vector3 position = new Vector3(x, y, z);

		Vector3 direction = (throwTarget.position - throwOrigin.position).normalized;
		direction += Random.insideUnitSphere * 5;
		direction.y = Mathf.Abs(direction.y);

		Vector3 force = direction * Random.Range(rollForce * 0.5f, rollForce);
		Vector3 torque = Random.insideUnitSphere * rollTorque;

		Quaternion rotation = Random.rotation;

		return new InitialState(position, rotation, force, torque);
	}
	private void ResetDice(InputAction.CallbackContext ctx)
    {
        foreach (var d in activeDice)
            Destroy(d.gameObject);

        activeDice.Clear();
    }
    public List<int> GetResults()
    {
        List<int> results = new();
        foreach (var d in activeDice)
        {
            results.Add(d.GetTopSideValue());
        }
        return results;
    }
    public int GetMaxResult()
    {
        int max = 0;
        foreach (var d in diceList)
        {
            max+= d.MaxFaceValue();
        }
        return max;
    }
    public int GetMinResult()
    {
        int min = 0;
        foreach (var d in diceList)
        {
            min += d.MinFaceValue();
        }
        return min;
    }
}
[System.Serializable]
public struct DiceData
{
	public Dice diceObject;
	public Rigidbody rb;
	public Dice diceLogic;

	public DiceData(Dice diceObject)
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
	public InitialState(Vector3 position, Quaternion rotation,Vector3 force, Vector3 torque)
	{
		this.position = position;
		this.rotation = rotation;
		this.force = force;
		this.torque = torque;
	}
}

