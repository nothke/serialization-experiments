using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class ComplexObject : MonoBehaviour, ISerializable
{
    public float value = 1;
    public SimpleObject child;

    public class Data : SerializableData
    {
        public override string prefabName => "complex";
        public float value;

        //[JsonProperty(IsReference = true)]
        //public SimpleObject.Data childId;
    }

    public SerializableData SerializedData
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
            this.value = ((Data)value).value;
        }
    }

}
