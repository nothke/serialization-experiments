using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

using Nothke.Serialization;

public class ComplexObject : MonoBehaviour, ISerializable, ISerializablePrefabInstance, ISerializableLink
{
    public float value = 1;
    public SimpleObject child;

    public string PrefabName => "complex";

    public class Data : ISerializableData
    {
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

        d.value = 666;

        if (child)
            d.childId = Serializer.e.GetIdOf(child);
    }

    public void OnDeserializeLinks(in ISerializableData data)
    {
        var d = data as Data;

        if (d.childId != -1)
        {
            //Debug.Log("Deserializing link: " + d.childId);
            var sobComp = Serializer.e.GetSpawnedFromId(d.childId);
            Transform t = (sobComp as MonoBehaviour).transform;
            t.parent = transform;
        }
    }
}
