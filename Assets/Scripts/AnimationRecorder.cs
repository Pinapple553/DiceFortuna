using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AnimationRecorder : MonoBehaviour
{
	[SerializeField] DiceManager diceManager;
	public int recFrameLength = 150; //make responsive
	private List<Dice> dice = new();
	public List<RecordingData> recordings;

	private Coroutine playback = null;

    private bool recording;

    public void StartSimulation(List<Dice> diceList)
    {
		if (playback != null)
		{
			StopCoroutine(playback);
			playback = null;
		}
		Debug.Log("Simulation started");
		recordings.Clear();
		dice.Clear();
		dice = diceList;

		EnablePhysics();
		GetInitialState();
		recording = true;
		RecordFrames();
	}
	private void GetInitialState()
	{
		foreach (var gameObject in dice)
		{
			Vector3 initialPosition = gameObject.transform.position;
			Quaternion initialRotation = gameObject.transform.rotation;

			Rigidbody rb = gameObject.GetComponent<Rigidbody>();
			rb.maxAngularVelocity = 1000;

			RecordingData data = new RecordingData(rb, initialPosition, initialRotation);
			recordings.Add(data);
		}
	}

	private void RecordFrames()
    {
		Physics.simulationMode = SimulationMode.Script;
		for (int i = 0; i < recFrameLength; i++) {
			Debug.Log("Frame");
			for (int j = 0; j < dice.Count; j++)
			{
				Vector3 position = dice[j].transform.position;
				Quaternion rotation = dice[j].transform.rotation;
				//bool isContactWithArena = diceManager.diceDataList[j].ui.isContactWithFloor;
				//bool isContactWithDice = diceManager.diceDataList[j].ui.isContactWithDice;
				bool isNotMoving = CheckObjectHasStopped(diceManager.diceDataList[j].rb);

				RecordedFrame frame = new RecordedFrame(position, rotation, isNotMoving);
				recordings[j].recordedAnimation.Add(frame);
			}
			Physics.Simulate(Time.fixedDeltaTime);
		}

		Physics.simulationMode = SimulationMode.FixedUpdate;

	}
	public void ResetToInitialState()
	{
		for (int i = 0; i < dice.Count; i++)
		{
			dice[i].transform.position = recordings[i].initialPosition;
			dice[i].transform.rotation = recordings[i].initialRotation;
		}
	}

	public void PlayRecording()
    {
		if (playback == null && recordings.Count > 0)
		{
			playback = StartCoroutine(Playback());
		}
	}
    IEnumerator Playback()
    {
		DisablePhysics();
		ResetToInitialState();

		for (int i = 0; i < recFrameLength; i++)
		{
			for (int j = 0; j < recordings.Count; j++)
			{
				Vector3 position = recordings[j].recordedAnimation[i].position;
				Quaternion rotation = recordings[j].recordedAnimation[i].rotation;
				dice[j].transform.position = position;
				dice[j].transform.rotation = rotation;
			}
			yield return new WaitForFixedUpdate();
		}

		playback = null;
	}
	public bool CheckObjectHasStopped(Rigidbody rb)
	{
		if (rb.linearVelocity == Vector3.zero && rb.angularVelocity == Vector3.zero)
		{
			return true;
		}
		else return false;
	}

	public void EnablePhysics()
	{
		for (int i = 0; i < recordings.Count; i++)
		{
			recordings[i].rb.useGravity = true;
			recordings[i].rb.isKinematic = false;
		}
	}

	public void DisablePhysics()
	{
		for (int i = 0; i < recordings.Count; i++)
		{
			recordings[i].rb.useGravity = false;
			recordings[i].rb.isKinematic = true;
		}
	}

	[System.Serializable]
	public struct RecordedFrame
	{
		public Vector3 position;
		public Quaternion rotation;
		public bool isNotMoving;

		public RecordedFrame(Vector3 position, Quaternion rotation, bool isNotMoving)
		{
			this.position = position;
			this.rotation = rotation;
			this.isNotMoving = isNotMoving;
		}
	}

	[System.Serializable]
	public struct RecordingData
	{
		public Rigidbody rb;
		public Vector3 initialPosition;
		public Quaternion initialRotation;
		public List<RecordedFrame> recordedAnimation;

		public RecordingData(Rigidbody rb, Vector3 initialPosition, Quaternion initialRotation)
		{
			this.rb = rb;
			this.initialPosition = initialPosition;
			this.initialRotation = initialRotation;
			this.recordedAnimation = new List<RecordedFrame>();
		}
	}
}