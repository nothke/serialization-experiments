using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class ComplexObject : MonoBehaviour, ISerializable<ComplexObject.Data>
{
    public float value = 1;
    public SimpleObject child;

    public class Data : ISerializableData
    {
        public string prefabName => "complex";
        public float value;

        //[JsonProperty(IsReference = true)]
        //public SimpleObject.Data childId;
    }

    public Data SerializedData
    {
        get
        {
            return new Data()
            {
                value = value,
                //childId = child ? child.SerializedData : null
            };
        }
        set
        {
            this.value = value.value;
        }
    }
}
