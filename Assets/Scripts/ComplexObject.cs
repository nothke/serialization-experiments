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

    public Data SerializedData
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
            this.value = value.value;
        }
    }
}
