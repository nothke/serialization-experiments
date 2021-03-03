using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Nothke.Serialization.Testing
{
    public class TestSerializer : MonoBehaviour
    {
        [ContextMenu("Save")]
        public void Save()
        {
            Serializer.e.ValidateScene();
            Serializer.e.SerializeToDefaultFile();
        }

        [ContextMenu("Load")]
        public void Load()
        {
            StartCoroutine(LoadCo());
        }

        IEnumerator LoadCo()
        {
            Serializer.e.DestroyAllSerializablePrefabInstances();
            yield return null;
            Serializer.e.Deserialize();
        }
    }
}