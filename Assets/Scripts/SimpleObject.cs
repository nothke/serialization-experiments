using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObject : MonoBehaviour, ISerializable<SimpleObject.Data>
{
    public float floatValue;
    public int intValue;

    public class Data : ISerializableData
    {
        public string prefabName => "simple";
        public float value;
        public int intValue;
    }

    public ISerializableData data
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

    public Data Serialize()
    {
        return new Data()
        {
            value = floatValue,
            intValue = intValue
        };
    }

    public void Deserialize(Data data)
    {
        floatValue = data.value;
        intValue = data.intValue;
    }
}
