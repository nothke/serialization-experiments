using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObject : MonoBehaviour, ISerializable<SimpleObject.Data>
{
    public float floatValue;
    public int intValue;

    public class Data : SerializableData
    {
        public string prefabName => "simple";
        public float value;
        public int intValue;
    }

    public Data SerializedData
    {
        get => new Data()
        {
            value = floatValue,
            intValue = intValue
        };

        set
        {
            floatValue = value.value;
            intValue = value.intValue;
        }
    }
}
