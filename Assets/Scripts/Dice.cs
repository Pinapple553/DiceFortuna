using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    public string diceType;

    [Header("Dice Sides")]
    [SerializeField] private List<DiceSide> diceSides = new();

    public bool IsStopped { get; private set; }

    private float stableTimeRequired = 0.5f;

    public int simulatedFaceResult = -1;
    public int alteredFaceResult = -1;

    [System.Serializable]
    public class DiceSide
    {
        public int value;
        public DiceSideEffect effect;
        public Transform sideTransform;
    }

    public void SetStableTime(float t)
    {
        stableTimeRequired = t;
    }
    public void Reset()
    {
        stableTimeRequired = 0;
		simulatedFaceResult = -1;
        alteredFaceResult = -1;
}
    public void CalculateResult()
    {
        simulatedFaceResult = GetTopSideValue();

        DiceSide side = GetTopSide();

        if (side.effect != null)
        {
            side.effect.ApplyEffect(this);
        }
    }
    public DiceSide GetTopSide()
    {
        DiceSide best = null;
        float bestDot = -1f;

        foreach (var side in diceSides)
        {
            float dot = Vector3.Dot(side.sideTransform.up, Vector3.up);

            if (dot > bestDot)
            {
                bestDot = dot;
                best = side;
            }
        }

        return best;
    }
    public int GetTopSideValue()
    {
        DiceSide side = GetTopSide();
        return side.value;
    }
    public void RotateToFace(int faceValue)
    {
        DiceSide targetSide = null;

        foreach (var side in diceSides)
        {
            if (side.value == faceValue)
            {
                targetSide = side;
                break;
            }
        }

        if (targetSide == null) return;

        Quaternion targetRot = Quaternion.FromToRotation(targetSide.sideTransform.up, Vector3.up) * transform.rotation;

        transform.rotation = targetRot;
        alteredFaceResult = faceValue;
    }

    public int MaxFaceValue()
    {
        int max = 0;
        foreach (var side in diceSides)
        {
            if (side.value > max)
                max = side.value;
        }
        return max;
    }
    public int MinFaceValue()
    {
        int min = int.MaxValue;
        foreach (var side in diceSides)
        {
            if (side.value < min)
                min = side.value;
        }
        return min;
    }
}