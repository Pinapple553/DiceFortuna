using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class DiceRoll : MonoBehaviour
{
    [Header("References")]
    public DiceData diceData;
    public GameObject[] faceDetectors;

    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private TMP_Text diceRollText;
    private PlayerController controller;
    private Vector3 originPosition;

    [System.Serializable]
    public class DiceSide
    {
        public string sideValue;
        public GameObject sideObject;
    }
    public List<DiceSide> diceSides;
    private Dictionary<string, GameObject> lookup; 

    bool diceThrown = false;

    private void Awake()
    {
        controller = new PlayerController();
        controller.Dice.Roll.performed += ctx => RollDice();
        controller.Dice.Reset.performed += ctx => ResetDice();

        originPosition = rb.transform.position;
        //connects side value to object
        lookup = new Dictionary<string, GameObject>();
        foreach (var obj in diceSides)
        {
            lookup[obj.sideValue] = obj.sideObject;
        }
    }
    private void OnEnable()
    {
        controller.Dice.Enable();
    }
    private void OnDisable()
    {
        controller.Dice.Disable();
    }
    private Action<InputAction.CallbackContext> RollDice()
    {
        if (!diceThrown)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(new Vector3(UnityEngine.Random.Range(1f, 5f), UnityEngine.Random.Range(1f, 5f), UnityEngine.Random.Range(1f, 5f)), ForceMode.Impulse);
            rb.AddTorque(new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f)), ForceMode.Impulse);
            diceThrown = true;
            StartCoroutine(GetDiceScore());
        }
        return null;
    }
    IEnumerator GetDiceScore()
    {
        bool isMoving = true;
        Vector3 currentPos = rb.transform.position;
        Vector3 lastPos = Vector3.zero;
        while (isMoving)
        {
            currentPos = rb.transform.position;
            if (lastPos == currentPos)
            {
                foreach (var side in diceSides)
                {
                    lookup.TryGetValue(side.sideValue, out var gameObject);
                    Rigidbody sideRb = gameObject.GetComponent<Rigidbody>();            
                }
                isMoving = false;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                lastPos = currentPos;
            }

        }
        int diceRoll = Mathf.FloorToInt(UnityEngine.Random.Range(0,9));
        diceRollText.text = "Roll:"+diceRoll;
        yield return null;
    }
    private Action<InputAction.CallbackContext> ResetDice()
    {
        StopAllCoroutines();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.transform.position = originPosition;
        diceThrown = false;
        return null;
    }
}
