using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.Profiling;

namespace Nothke.Serialization
{
    /// <summary>
    /// Implement this on a behavior that is instantiated from a prefab.
    /// The prefab with the same name must be assigned in the Serializer's prefab list.
    /// </summary>
    public interface ISerializablePrefabInstance
    {
        string PrefabName { get; }
        ISerializableData SerializedData { get; set; }
    }

    /// <summary>
    /// Implement this on a behavior which exists on the start of the level and is not necessarily spawned from a prefab.
    /// </summary>
    public interface ISerializable
    {
        ISerializableData SerializedData { get; set; }
    }

    /// <summary>
    /// Implement this on a behavior which needs to connect to another object
    /// </summary>
    public interface ISerializableLinksHandler
    {
        void OnSerializeLinks(ref ISerializableData data);
        void OnDeserializeLinks(in ISerializableData data);
    }

    public interface ISerializableData { }

    public class Serializer : MonoBehaviour
    {
        public static Serializer e;
        void Awake() { e = this; }

        public GameObject[] prefabs;

        // Serialized structure
        [System.Serializable]
        public class GameData
        {
            public List<SerializedPrefabInstance> siobs = new List<SerializedPrefabInstance>();
            public List<SerializedObject> sobs = new List<SerializedObject>();
        }

        [System.Serializable]
        public struct SerializedPrefabInstance
        {
            public int id;
            public Vector3 pos;
            public Vector3 rot;
            public string prefab;
            public ISerializableData data;
        }

        [System.Serializable]
        public struct SerializedObject
        {
            public int id;
            public ISerializableData data;
        }

        // Serialization cache
        [System.NonSerialized]
        public Dictionary<string, GameObject> prefabByNameMap;
        Dictionary<ISerializablePrefabInstance, int> idBySpawnedMap;

        // Deserialization cache
        //List<ISerializablePrefabInstance> spawned;
        Dictionary<int, ISerializablePrefabInstance> spawnedByIdMap;

        string defaultFilePath = "scene.json";

        JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto, // Enables type recognition
            Formatting = Formatting.None // Set to indented to make it readable
        };

        string str;

        public void ValidateScene()
        {
            var ids = FindObjectsOfType<ID>(); // alloc

            foreach (var id in ids)
            {
                if (id.gameObject.GetComponent<ISerializable>() == null &&
                    id.gameObject.GetComponent<ISerializablePrefabInstance>() == null)
                {
                    Debug.LogError("Validator: Serializable interface not found on object with ID", id);
                }
            }
        }

        [ContextMenu("Test Save")]
        public void SerializeToDefaultFile()
        {
            Serialize(defaultFilePath);
        }

        public void Serialize(string filePath)
        {
            float t = Time.realtimeSinceStartup;

            CachePrefabsIfNeeded();

            var allIDs = FindObjectsOfType<ID>();

            if (allIDs.Length == 0)
            {
                Debug.LogWarning("Found no id objects, nothing to serialize");
                return;
            }

            GameData game = new GameData(); // alloc

            // preallocate to maximum even if we are not actually going to fill all
            var linkableSpis = new List<SerializedPrefabInstance>(allIDs.Length); // alloc
            var linkableSpiComps = new List<ISerializableLinksHandler>(allIDs.Length); // alloc
            var linkableSobs = new List<SerializedObject>(allIDs.Length); // alloc
            var linkableSobComps = new List<ISerializableLinksHandler>(allIDs.Length); // alloc
            idBySpawnedMap = new Dictionary<ISerializablePrefabInstance, int>(allIDs.Length); // alloc

            for (int i = 0; i < allIDs.Length; i++)
            {
                var spiComp = allIDs[i].GetComponent<ISerializablePrefabInstance>();
                var sobComp = allIDs[i].GetComponent<ISerializable>();

                if (spiComp != null)
                {
                    //Debug.Log("Found " + all[i].name);

                    SerializedPrefabInstance sob = new SerializedPrefabInstance
                    {
                        id = allIDs[i].id,
                        prefab = spiComp.PrefabName,
                        pos = allIDs[i].transform.position,
                        rot = allIDs[i].transform.eulerAngles,
                        data = spiComp.SerializedData
                    };

                    game.siobs.Add(sob);
                    idBySpawnedMap.Add(spiComp, sob.id);
                    //Debug.Log("Added " + sob.data.prefabName + " id: " + sob.id);

                    if (spiComp is ISerializableLinksHandler obCompLink)
                    {
                        linkableSpis.Add(sob);
                        linkableSpiComps.Add(obCompLink);
                    }
                }
                else if (sobComp != null)
                {
                    SerializedObject sob = new SerializedObject
                    {
                        id = allIDs[i].id,
                        data = sobComp.SerializedData
                    };

                    game.sobs.Add(sob);

                    if (sobComp is ISerializableLinksHandler obCompLink)
                    {
                        linkableSobs.Add(sob);
                        linkableSobComps.Add(obCompLink);
                    }
                }
                else
                    Debug.LogError("No ISerializable found for " + allIDs[i].name + ". You should probably remove the ID component", allIDs[i].gameObject);
            }

            // Pass 2: Serialize links
            for (int i = 0; i < linkableSpis.Count; i++)
            {
                var sob = linkableSpis[i];
                var obCompLink = linkableSpiComps[i];
                obCompLink.OnSerializeLinks(ref sob.data);
            }

            for (int i = 0; i < linkableSobs.Count; i++)
            {
                var sob = linkableSobs[i];
                var obCompLink = linkableSobComps[i];
                obCompLink.OnSerializeLinks(ref sob.data);
            }

            Profiler.BeginSample("JSON SerializeObject");
            str = JsonConvert.SerializeObject(game, jsonSettings);

            Profiler.EndSample();
            //Debug.Log(str);

            Profiler.BeginSample("Write to file");
            File.WriteAllText(filePath, str);
            Profiler.EndSample();

            Debug.Log("Serialization completed in: " + (Time.realtimeSinceStartup - t));
        }

        [ContextMenu("Deserialize")]
        public void Deserialize()
        {
            CachePrefabsIfNeeded();

            Profiler.BeginSample("Read from file");
            str = File.ReadAllText(defaultFilePath);
            Profiler.EndSample();

            Profiler.BeginSample("Deserialize");
            GameData game = JsonConvert.DeserializeObject<GameData>(str, jsonSettings);
            Profiler.EndSample();

            // Save a list of links
            List<ISerializableLinksHandler> links = new List<ISerializableLinksHandler>();
            //spawned = new List<ISerializablePrefabInstance>();
            spawnedByIdMap = new Dictionary<int, ISerializablePrefabInstance>();
            List<ISerializableData> linksDatas = new List<ISerializableData>();

            // Get all already exisitng ids in the scene
            var sceneIDs = FindObjectsOfType<ID>();
            var idSceneObjectMap = new Dictionary<int, ISerializable>(sceneIDs.Length);
            foreach (var id in sceneIDs)
            {
                idSceneObjectMap.Add(id.id, id.GetComponent<ISerializable>());
            }

            // First pass, instantiate items
            foreach (var obData in game.siobs)
            {
                string prefabName = obData.prefab;

                Debug.Assert(!string.IsNullOrEmpty(prefabName), "Deserialization: Attempting to spawn a prefab, but the name is empty", this);
                Debug.Assert(prefabByNameMap.ContainsKey(prefabName), "Deserialization: Attempting to spawn a prefab, but prefab " + prefabName + " is not part of the spawn list. Did you forget to add it?", this);

                var prefab = prefabByNameMap[prefabName];
                GameObject go = Instantiate(prefab); // alloc

                go.transform.position = obData.pos;
                go.transform.eulerAngles = obData.rot;
                //go.transform.localScale = obData.loc.scl;

                var obComp = go.GetComponentInChildren<ISerializablePrefabInstance>();
                Debug.Assert(obComp != null, "Deserialization: ISerializable component not found on the root of spawned GameObject. Did you forgot to apply the prefab with ISerializable component?", go);
                obComp.SerializedData = obData.data;

                // Set id
                var idComp = (obComp as Component).gameObject.GetComponent<ID>();
                if (!idComp) idComp = (obComp as Component).gameObject.AddComponent<ID>();
                idComp.id = obData.id;

                //spawned.Add(obComp);
                spawnedByIdMap.Add(idComp.id, obComp);

                if (obComp is ISerializableLinksHandler obCompLink)
                {
                    links.Add(obCompLink);
                    linksDatas.Add(obData.data);
                }
            }

            // Second pass, assign scene objects
            foreach (var obData in game.sobs)
            {
                if (!idSceneObjectMap.ContainsKey(obData.id))
                {
                    Debug.LogError("Deserialization: Serializable object not found in scene");
                    continue;
                }

                var scomp = idSceneObjectMap[obData.id];
                scomp.SerializedData = obData.data;

                if (scomp is ISerializableLinksHandler obCompLink)
                {
                    links.Add(obCompLink);
                    linksDatas.Add(obData.data);
                }
            }

            // Third pass, link
            for (int i = 0; i < links.Count; i++)
            {
                links[i].OnDeserializeLinks(linksDatas[i]);
            }

            Debug.Log("Deserialization: Ended");
        }

        public int GetIdOf(ISerializablePrefabInstance serializable)
        {
            Debug.Assert(idBySpawnedMap.ContainsKey(serializable), "linkMap does not contain an id for ISerializable " + serializable.PrefabName, (serializable as MonoBehaviour));
            return idBySpawnedMap[serializable];
        }

        public void DestroyAllSerializablePrefabInstances()
        {
            var allIDs = FindObjectsOfType<ID>();
            for (int i = allIDs.Length - 1; i >= 0; i--)
            {
                var item = allIDs[i].GetComponent<ISerializablePrefabInstance>();
                if (item != null)
                {
                    Destroy(allIDs[i].gameObject);
                }
            }
        }

        public ISerializablePrefabInstance GetSpawnedFromId(int i)
        {
            return spawnedByIdMap[i];
        }

        void CachePrefabsIfNeeded()
        {
            if (prefabByNameMap == null || prefabByNameMap.Count == 0)
            {
                prefabByNameMap = new Dictionary<string, GameObject>(prefabs.Length);
                foreach (var go in prefabs)
                    prefabByNameMap.Add(go.name, go);
            }
        }
    }
}