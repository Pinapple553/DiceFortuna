using UnityEngine;

[System.Serializable]
public struct DiceFrame
{
        public Vector3 position;
        public Quaternion rotation;

        public DiceFrame(Vector3 p, Quaternion r)
        {
            position = p;
            rotation = r;
        }
}