using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexObject : MonoBehaviour, ISerializable, ISerializablePrefabLink
{
    public float value = 1;
    public string prefabName => "box";

    public class Data : ISerializableData
    {
        public string prefabName => "box";
        public float value;
        public int parentedId;
    }

    public ISerializableData data => new Data()
    {
        value = this.value,
        parentedId = gameObject.GetInstanceID()
    };
}
