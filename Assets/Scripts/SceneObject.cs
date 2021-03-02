using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObject : MonoBehaviour, ISerializable
{
    [System.Serializable]
    public class Data : ISerializableData
    {
        public Vector3 position;
        public float f = 0;

        public string prefabName => "";
    }

    public Data data;
    public ISerializableData SerializedData
    {
        get
        {
            data.position = transform.localPosition;
            return data;
        }
        set
        {
            data = (Data)value;
            transform.localPosition = data.position;
            Debug.Log("Setting data!", this);
        }
    }
}
