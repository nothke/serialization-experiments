using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Serialization;

public class FastObject : MonoBehaviour, ISerializablePrefabInstance
{
    public string PrefabName => "fast";

    [System.Serializable]
    public class Data : ISerializableData
    {
        public float a;
        public float b;
    }

    public Data _data;
    public ISerializableData SerializedData { get => _data; set => _data = value as Data; }
}
