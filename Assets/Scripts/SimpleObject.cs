using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObject : MonoBehaviour, ISerializable
{
    public float floatValue;
    public int intValue;

    public class Data : SerializableData
    {
        public override string prefabName => "simple";
        public float value;
        public int intValue;
    }

    public SerializableData SerializedData
    {
        get => new Data()
        {
            value = floatValue,
            intValue = intValue
        };

        set
        {
            var d = value as Data;
            floatValue = d.value;
            intValue = d.intValue;
        }
    }
}
