using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastObject : MonoBehaviour, ISerializableItem
{
    [System.Serializable]
    public class Data : SerializableData
    {
        public override string prefabName => "fast";
        public float a;
        public float b;
    }

    public Data _data;
    public ISerializableData SerializedData { get => _data; set => _data = value as Data; }
}
