using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Serialization;

public class SimpleObject : MonoBehaviour, ISerializablePrefabInstance
{
    public float floatValue;
    public int intValue;

    public string PrefabName => "simple";

    public class Data : ISerializableData
    {
        public float value;
        public int intValue;
    }

    public ISerializableData SerializedData
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
