using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastObject : MonoBehaviour, IFastSer<FastObject.Data>
{
    [System.Serializable]
    public class Data : ISerializableData
    {
        public string prefabName => "fast";
        public float a;
        public float b;
    }

    public Data _data;
    public Data SData { get => _data; set => _data = value; }
}
