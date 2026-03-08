using UnityEngine;

[System.Serializable]
public struct DiceSimulation
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 force;
    public Vector3 torque;

    public DiceSimulation(Vector3 pos, Quaternion rot, Vector3 f, Vector3 t)
    {
        position = pos;
        rotation = rot;
        force = f;
        torque = t;
    }
}