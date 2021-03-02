using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializedRigidbody : MonoBehaviour, ISerializable
{
    Rigidbody _rb;
    Rigidbody rb { get { if (!_rb) _rb = GetComponent<Rigidbody>(); return _rb; } }

    public class Data : ISerializableData
    {
        public Vector3 velocity;
        public Vector3 angularVelocity;

        public string prefabName => "rb";
    }

    Data data;
    public ISerializableData SerializedData
    {
        get
        {
            data = new Data();

            data.velocity = rb.velocity;
            data.angularVelocity = rb.angularVelocity;

            return data;
        }
        set
        {
            data = (Data)value;

            rb.velocity = data.velocity;
            rb.angularVelocity = data.angularVelocity;
        }
    }
}
