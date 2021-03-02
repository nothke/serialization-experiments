using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class ComplexObject : MonoBehaviour, ISerializable, ISerializableLinksHandler
{
    public float value = 1;
    public SimpleObject child;

    public class Data : ISerializableData
    {
        public string prefabName => "complex";
        public float value;

        public int childId;
    }

    public ISerializableData SerializedData
    {
        get
        {
            return new Data()
            {
                value = value,
                childId = -1 // Gets set during OnSerializeLinks pass
            };
        }
        set
        {
            this.value = ((Data)value).value;
        }
    }

    public void OnSerializeLinks(ref ISerializableData data)
    {
        var d = data as Data;
        d.childId = Serializer.e.GetIdOf(child);
    }

    public void OnDeserializeLinks(in ISerializableData data)
    {
        var d = data as Data;
        Debug.Log("Deserializing link: " + d.childId);
        var sobComp = Serializer.e.GetSpawnedComponent(d.childId);
        Transform t = (sobComp as MonoBehaviour).transform;
        t.parent = transform;
    }
}
