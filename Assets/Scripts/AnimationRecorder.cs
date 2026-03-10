using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRecorder : MonoBehaviour
{
    private List<Dice> dice = new();
    private List<List<DiceFrame>> recordings = new();

    private Vector3[] startPos;
    private Quaternion[] startRot;
    private Vector3 startForce;

    private bool recording;

    public void StartSimulation(List<Dice> diceList)
    {
        Physics.simulationMode = SimulationMode.Script;
        dice = diceList;

        recordings.Clear();
        startPos = new Vector3[dice.Count];
        startRot = new Quaternion[dice.Count];

        for (int i = 0; i < dice.Count; i++)
        {
            recordings.Add(new List<DiceFrame>());

            startPos[i] = dice[i].transform.position;
            startRot[i] = dice[i].transform.rotation;
        }

        recording = true;
        StartCoroutine(RecordFrames());
        Physics.simulationMode = SimulationMode.FixedUpdate;
    }

    IEnumerator RecordFrames()
    {
        while (recording)
        {
            for (int i = 0; i < dice.Count; i++)
            {
                recordings[i].Add(
                    new DiceFrame(
                        dice[i].transform.position,
                        dice[i].transform.rotation
                    )
                );
            }

            yield return null;
        }
    }

    public void StopRecording()
    {
        recording = false;
    }

    public void ResetToInitialState()
    {
        for (int i = 0; i < dice.Count; i++)
        {
            Rigidbody rb = dice[i].GetComponent<Rigidbody>();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            dice[i].transform.position = startPos[i];
            dice[i].transform.rotation = startRot[i];
        }
    }

    public void PlayRecording()
    {
        StartCoroutine(Playback());
    }

    IEnumerator Playback()
    {
        if (recordings.Count == 0)
            yield break;

        int frameCount = recordings[0].Count;

        // disable physics
        for (int i = 0; i < dice.Count; i++)
        {
            dice[i].GetComponent<Rigidbody>().isKinematic = true;
        }

        for (int f = 0; f < frameCount; f++)
        {
            for (int d = 0; d < dice.Count; d++)
            {
                dice[d].transform.position = recordings[d][f].position;
                dice[d].transform.rotation = recordings[d][f].rotation;
            }

            yield return null;
        }
    }
}