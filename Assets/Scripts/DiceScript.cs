using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceScript : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TMP_Text diceRollText;
    [Header("Dice Throw Parameters")]
	[SerializeField] private float rollForce = 5f;
	[SerializeField] private float rollTorque = 2f;
	[SerializeField] private float directionRandomness = 0.2f;
	[SerializeField] private Transform throwOrigin;
	[SerializeField] private Transform throwTarget;
	[SerializeField] private float stableTimeRequired = 0.5f;
    
    [SerializeField] private GameObject dicePrefab;
    private List<GameObject> activeDice = new List<GameObject>();

	private PlayerController controller;    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool diceThrown;

    [System.Serializable]
    public class DiceSide
    {
        public int value;
        public Transform sideTransform;
    }
    [SerializeField] private List<DiceSide> diceSides;

    private void Awake()
    {
        controller = new PlayerController();
        controller.Dice.Roll.performed += RollDice;
        controller.Dice.Reset.performed += ResetDice;

        startPosition = rb.position;
        startRotation = rb.rotation;
    }

    private void OnEnable()
    {
        controller.Dice.Enable();
    }
    private void OnDisable() 
    {
        controller.Dice.Disable(); 
    }

    private void RollDice(InputAction.CallbackContext ctx)
    {
        if (diceThrown)
        {
            ResetDice(ctx);
            return;
        }

		rb.constraints = RigidbodyConstraints.None;

        SetInitialState();
        SimulatePhysics();


		diceThrown = true;
        StartCoroutine(WaitForDiceToStop());
    }
    private void SetInitialState()
    {
		Vector3 direction = (throwTarget.position - throwOrigin.position).normalized;
		direction += UnityEngine.Random.insideUnitSphere * directionRandomness;
		direction.y = Mathf.Abs(direction.y);

		rb.AddForce(direction.normalized * rollForce, ForceMode.Impulse);
		rb.AddTorque(UnityEngine.Random.onUnitSphere * rollTorque, ForceMode.Impulse);
	}
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
}