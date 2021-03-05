using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.Profiling;

namespace Nothke.Serialization
{
    /// <summary>
    /// Implement this on every behavior you wish to serialize.
    /// Use the data's setter and getter to implement custom behavior when serializaing and deserializing.
    /// </summary>
    public interface ISerializable
    {
        ISerializableData SerializedData { get; set; }
    }

    /// <summary>
    /// Implement this on a behavior that is instantiated from a prefab (in addition to ISerializable).
    /// The prefab with the same name must be assigned in the Serializer's prefab list.
    /// </summary>
    public interface ISerializablePrefabInstance
    {
        string PrefabName { get; }
    }

    /// <summary>
    /// Implement this on a behavior which needs to connect to another object (in addition to ISerializable).
    /// </summary>
    public interface ISerializableLink
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
            public List<SerializedPrefabInstance> spiobs = new List<SerializedPrefabInstance>();
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
        Dictionary<string, GameObject> prefabByNameMap;
        Dictionary<ISerializablePrefabInstance, int> idBySpawnedMap;

        // Deserialization cache
        Dictionary<int, ISerializablePrefabInstance> spawnedByIdMap;

        const string defaultFilePath = "scene.json";

        JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto, // Enables type recognition
            Formatting = Formatting.None // Set to indented to make it human readable
        };

        string str;

        /// <summary>
        /// Call this when debugging before Serialize() to test if the scene has been set up properly.
        /// Checks if there are no duplicate IDs, no IDs with no ISerializables etc.
        /// </summary>
        public void ValidateScene()
        {
            var ids = FindObjectsOfType<ID>(); // alloc
            HashSet<int> idset = new HashSet<int>();

            foreach (var id in ids)
            {
                if (id.gameObject.GetComponent<ISerializable>() == null)
                {
                    Debug.LogError("Validator: ISerializable interface not found on object with ID", id);

                    if (id.gameObject.GetComponent<ISerializablePrefabInstance>() != null)
                    {
                        Debug.LogError("Validator: An object implements ISerializablePrefabInstance but not ISerializable. You must implement ISerializable", id);
                    }
                }

                if (idset.Contains(id.id))
                    Debug.LogError("Validator: There is already an object with this id in the scene", id);

                idset.Add(id.id);
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
            var linkableSobs = new List<SerializedObject>(allIDs.Length); // alloc
            var linkableSobComps = new List<ISerializableLink>(allIDs.Length); // alloc
            idBySpawnedMap = new Dictionary<ISerializablePrefabInstance, int>(allIDs.Length); // alloc

            for (int i = 0; i < allIDs.Length; i++)
            {
                var spiComp = allIDs[i].GetComponent<ISerializablePrefabInstance>();
                var sobComp = allIDs[i].GetComponent<ISerializable>();

                // Needed for linking but not used if behavior is prefab instance
                SerializedObject sob = new SerializedObject
                {
                    id = allIDs[i].id,
                    data = sobComp.SerializedData
                };

                if (sobComp != null)
                {
                    if (spiComp != null) // is prefab instance
                    {
                        SerializedPrefabInstance spiob = new SerializedPrefabInstance
                        {
                            id = allIDs[i].id,
                            prefab = spiComp.PrefabName,
                            pos = allIDs[i].transform.position,
                            rot = allIDs[i].transform.eulerAngles,
                            data = sobComp.SerializedData
                        };

                        game.spiobs.Add(spiob);
                        idBySpawnedMap.Add(spiComp, spiob.id);

                        //Debug.Log("Added " + sob.data.prefabName + " id: " + sob.id);
                    }
                    else // is not prefab instance
                    {
                        game.sobs.Add(sob);
                    }

                    // is linkable
                    if (sobComp is ISerializableLink obCompLink)
                    {
                        linkableSobs.Add(sob);
                        linkableSobComps.Add(obCompLink);
                    }
                }
                else
                {
                    Debug.LogError("No ISerializable found for " + allIDs[i].name + ". You should probably remove the ID component", allIDs[i].gameObject);
                }
            }

            // Pass 2: Serialize Links
            for (int i = 0; i < linkableSobs.Count; i++)
            {
                var sob = linkableSobs[i];
                var obCompLink = linkableSobComps[i];
                obCompLink.OnSerializeLinks(ref sob.data); // Discarded?
            }

            // Serialize and save to file
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
            List<ISerializableLink> links = new List<ISerializableLink>();
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
            foreach (var obData in game.spiobs)
            {
                string prefabName = obData.prefab;

                Debug.Assert(!string.IsNullOrEmpty(prefabName), "Deserialization: Attempting to spawn a prefab, but the name is empty", this);
                Debug.Assert(prefabByNameMap.ContainsKey(prefabName), "Deserialization: Attempting to spawn a prefab, but prefab " + prefabName + " is not part of the spawn list. Did you forget to add it?", this);

                var prefab = prefabByNameMap[prefabName];
                GameObject go = Instantiate(prefab); // alloc

                go.transform.position = obData.pos;
                go.transform.eulerAngles = obData.rot;
                //go.transform.localScale = obData.loc.scl;

                var spiComp = go.GetComponentInChildren<ISerializablePrefabInstance>();
                Debug.Assert(spiComp != null, "Deserialization: ISerializable component not found on the root of spawned GameObject. Did you forgot to apply the prefab with ISerializable component?", go);

                var sobComp = go.GetComponentInChildren<ISerializable>();
                sobComp.SerializedData = obData.data;

                // Set id
                var idComp = (spiComp as Component).gameObject.GetComponent<ID>();
                if (!idComp) idComp = (spiComp as Component).gameObject.AddComponent<ID>();
                idComp.id = obData.id;

                //spawned.Add(obComp);
                spawnedByIdMap.Add(idComp.id, spiComp);

                if (spiComp is ISerializableLink obCompLink)
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

                if (scomp is ISerializableLink obCompLink)
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
            Debug.Assert(spawnedByIdMap.ContainsKey(i), "Spawned instance with id " + i + " not found");
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