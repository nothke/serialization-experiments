﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnotherLinkable : MonoBehaviour, ISerializableItem, ISerializableLinksHandler
{
    public AnotherLinkable child;

    [System.Serializable]
    public class Data : ISerializableData
    {
        public string prefabName => "another_linkable";
        public float health;
        [HideInInspector] public int childId = -1;
    }

    public Data data;
    public ISerializableData SerializedData { get => data; set => data = (Data)value; }

    public void OnSerializeLinks(ref ISerializableData data)
    {
        if (child != null)
            (data as Data).childId = Serializer.e.GetIdOf(child);
    }

    public void OnDeserializeLinks(in ISerializableData data)
    {
        var d = data as Data;
        if (d.childId != -1)
        {
            var sc = Serializer.e.GetSpawnedFromId((data as Data).childId);
            var t = (sc as MonoBehaviour).transform;
            t.parent = transform;
        }
    }
}