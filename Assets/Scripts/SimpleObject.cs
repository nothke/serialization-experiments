using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObject : MonoBehaviour, ISerializable
{
    public float value;
    public int intValue;

    public class Data : ISerializableData
    {
        public string prefabName => "simple_object";
        public float value;
        public int intValue;
    }

    public ISerializableData data => new Data()
    {
        value = value,
        intValue = intValue
    };
}
