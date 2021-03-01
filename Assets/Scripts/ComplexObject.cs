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

    ISerializableData ISerializable.data
    {
        get
        {
            return new Data()
            {
                value = this.value,
                parentedId = gameObject.GetInstanceID()
            };
        }

        set
        {
            Data d = value as Data;
            this.value = d.value;
        }
    }
}
