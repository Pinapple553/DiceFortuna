using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

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

    public DiceSimulation Initialize(Transform origin, Transform target,
        float forceMax, float torqueMax, float randomness)
    {
        Vector3 direction = (target.position - origin.position).normalized;
        direction += Random.insideUnitSphere * randomness;
        direction.y = Mathf.Abs(direction.y);

        Vector3 force = direction * Random.Range(forceMax * 0.5f, forceMax);
        Vector3 torque = Random.insideUnitSphere * torqueMax;

        Quaternion rot = Random.rotation;

        return new DiceSimulation(transform.position, rot, force, torque);
    }

    public void ThrowDice(DiceSimulation sim)
    {
        transform.position = sim.position;
        transform.rotation = sim.rotation;

        rb.isKinematic = false;
        rb.AddForce(sim.force, ForceMode.Impulse);
        rb.AddTorque(sim.torque, ForceMode.Impulse);

        StartCoroutine(WaitForStop());
    }

    IEnumerator WaitForStop()
    {
        float stableTime = 0f;

        while (stableTime < stableTimeRequired)
        {
            if (rb.linearVelocity.sqrMagnitude < 0.01f &&
                rb.angularVelocity.sqrMagnitude < 0.01f)
                stableTime += Time.deltaTime;
            else
                stableTime = 0f;

            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        IsStopped = true;
    }

    public void CalculateResult()
    {
        simulatedFaceResult = GetTopSideValue();

        DiceSide side = GetTopSide();

        if (side.effect != null)
            side.effect.ApplyEffect(this);
    }

    DiceSide GetTopSide()
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
        var side = GetTopSide();
        return side != null ? side.value : 0;
    }

    public void RotateToFace(int faceValue)
    {
        foreach (var side in diceSides)
        {
            if (side.value == faceValue)
            {
                Quaternion targetRot =
                    Quaternion.FromToRotation(side.sideTransform.up, Vector3.up) * transform.rotation;

                transform.rotation = targetRot;
                alteredFaceResult = faceValue;
                return;
            }
        }
    }
}