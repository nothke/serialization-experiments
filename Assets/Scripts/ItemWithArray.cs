using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWithArray : MonoBehaviour, ISerializableItem
{
    [System.Serializable]
    public class Data : ISerializableData
    {
        public int[] array;

        public string prefabName => "item_with_array";
    }

    public Data data;
    public ISerializableData SerializedData { get => data; set => data = value as Data; }
}
