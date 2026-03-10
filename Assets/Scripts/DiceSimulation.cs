using UnityEngine;

[System.Serializable]
public struct DiceSimulation
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 force;
    public Vector3 torque;

    public DiceSimulation(Vector3 p, Quaternion r, Vector3 f, Vector3 t)
    {
        position = p;
        rotation = r;
        force = f;
        torque = t;
    }
}

