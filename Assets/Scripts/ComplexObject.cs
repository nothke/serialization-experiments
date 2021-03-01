using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexObject : MonoBehaviour, ISerializable<ComplexObject.Data>, ISerializablePrefabLink
{
    public float value = 1;
    public string prefabName => "box";

    public class Data : ISerializableData
    {
        public string prefabName => "complex";
        public float value;
        public int parentedId;
    }
    
    public Data Serialize()
    {
        return new Data()
        {
            value = this.value,
            parentedId = gameObject.GetInstanceID()
        };
    }

    public void Deserialize(Data data)
    {
        Data d = data as Data;
        this.value = d.value;
    }
}
