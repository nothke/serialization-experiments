using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastObject : MonoBehaviour, ISerializable<FastObject.Data>
{
    [System.Serializable]
    public class Data : SerializableData
    {
        public string prefabName => "fast";
        public float a;
        public float b;
    }

    public Data _data;
    public Data SerializedData { get => _data; set => _data = value; }
}
