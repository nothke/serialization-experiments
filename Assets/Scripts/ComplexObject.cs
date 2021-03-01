using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexObject : MonoBehaviour, ISerializable, ISerializablePrefabLink
{
    public float value = 1;
    public string prefabName => "box";

    public class Data : ISerializableData
    {
        public string prefabName => "complex";
        public float value;
        public int parentedId;
    }
    
    public ISerializableData Serialize()
    {
        return new Data()
        {
            value = this.value,
            parentedId = gameObject.GetInstanceID()
        };
    }

    public void Deserialize(ISerializableData data)
    {
        Data d = data as Data;
        this.value = d.value;
    }
}
