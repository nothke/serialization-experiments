using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Nothke.Serialization.Testing
{
    public class TestSerializer : MonoBehaviour
    {
        [Header("Press S to serialize, D to deserialize")]
        public bool _;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
                Save();

            if (Input.GetKeyDown(KeyCode.D))
                Load();
        }
    }
}