using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.Profiling;

public interface ISerializable
{
    ISerializableData SerializedData { get; set; }
}

public interface ISerializableLinksHandler
{
    void OnSerializeLinks(ref ISerializableData data);
    void OnDeserializeLinks(in ISerializableData data);
}

public interface ISerializableLinksHandler<T> where T : ISerializableData
{
    void OnSerializeLinks(ref T data);
    void OnDeserializeLinks(in T data);
}

public interface ISerializablePrefabLink
{
    string prefabName { get; }
}

public interface ISerializableData
{
    string prefabName { get; }
}

public abstract class SerializableData : ISerializableData
{
    public abstract string prefabName { get; }
}

public class Serializer : MonoBehaviour
{
    public static Serializer e;
    void Awake() { e = this; }

    public GameObject[] prefabs;
    [System.NonSerialized]
    public Dictionary<string, GameObject> prefabsDict;
    Dictionary<ISerializable, int> linkMap;

    List<ISerializable> spawned;
    Dictionary<int, ISerializable> spawnedByIdMap;

    public struct Loc
    {
        public Vector3 pos;
        public Vector3 rot;
        //public Vector3 scl;

        public Loc(Transform t)
        {
            pos = t.position;
            rot = t.eulerAngles;
            //scl = t.localScale;
        }
    }

    public struct SerializedGameObject
    {
        public int id;
        public Loc loc;
        public ISerializableData data;
    }

    [System.Serializable]
    public class GameData
    {
        public List<SerializedGameObject> sobs = new List<SerializedGameObject>();
    }

    string str;

    JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.None
    };


    IEnumerator Start()
    {
        prefabsDict = new Dictionary<string, GameObject>(prefabs.Length);
        foreach (var go in prefabs)
            prefabsDict.Add(go.name, go);

        yield return null;

        Serialize();

        yield return null;

        File.WriteAllText("scene.json", str);

        yield return null;

        Deserialize();
    }

    void CachePrefabs()
    {
        if (prefabsDict == null || prefabsDict.Count == 0)
        {
            prefabsDict = new Dictionary<string, GameObject>(prefabs.Length);
            foreach (var go in prefabs)
                prefabsDict.Add(go.name, go);
        }
    }


    [ContextMenu("Serialize")]
    public void Serialize()
    {
        float t = Time.realtimeSinceStartup;

        CachePrefabs();

        //// Gets all MonoBehaviours, might be slow
        var allIDs = FindObjectsOfType<ID>();
        GameData game = new GameData();

        // preallocate to maximum even if we are not actually going to fill all
        var linkableSobs = new List<SerializedGameObject>(allIDs.Length);
        var linkables = new List<ISerializableLinksHandler>(allIDs.Length);
        linkMap = new Dictionary<ISerializable, int>(allIDs.Length);

        for (int i = 0; i < allIDs.Length; i++)
        {
            var sobj = allIDs[i].GetComponent<ISerializable>();

            if (sobj == null)
            {
                Debug.LogError("No ISerializable found for " + allIDs[i].name + ". You should probably remove the ID component", allIDs[i].gameObject);
                continue;
            }

            //Debug.Log("Found " + all[i].name);

            SerializedGameObject sob = new SerializedGameObject();

            sob.id = allIDs[i].id;
            sob.loc = new Loc(allIDs[i].transform);
            sob.data = sobj.SerializedData;

            game.sobs.Add(sob);
            linkMap.Add(sobj, sob.id);
            //Debug.Log("Added " + sob.data.prefabName + " id: " + sob.id);

            if (sobj is ISerializableLinksHandler obCompLink)
            {
                linkableSobs.Add(sob);
                linkables.Add(obCompLink);
            }
        }

        // Pass 2: Serialize links
        for (int i = 0; i < linkableSobs.Count; i++)
        {
            var sob = linkableSobs[i];
            var obCompLink = linkables[i];
            obCompLink.OnSerializeLinks(ref sob.data);
        }



        Profiler.BeginSample("JSON SerializeObject");
        str = JsonConvert.SerializeObject(game, jsonSettings);
        Profiler.EndSample();
        //Debug.Log(str);

        Profiler.BeginSample("Write to file");
        File.WriteAllText("scene.json", str);
        Profiler.EndSample();

        Debug.Log("Serialization completed in: " + (Time.realtimeSinceStartup - t));
    }

    public int GetIdOf(ISerializable serializable)
    {
        Debug.Assert(linkMap.ContainsKey(serializable), "linkMap does not contain an id for ISerializable " + serializable.SerializedData.prefabName, (serializable as MonoBehaviour));
        return linkMap[serializable];
    }

    [ContextMenu("Deserialize")]
    public void Deserialize()
    {
        CachePrefabs();

        Profiler.BeginSample("Read from file");
        str = File.ReadAllText("scene.json");
        Profiler.EndSample();

        Profiler.BeginSample("Deserialize");
        GameData game = JsonConvert.DeserializeObject<GameData>(str, jsonSettings);
        Profiler.EndSample();

        // Save a list of links
        List<ISerializableLinksHandler> links = new List<ISerializableLinksHandler>();
        spawned = new List<ISerializable>();
        spawnedByIdMap = new Dictionary<int, ISerializable>();
        List<ISerializableData> linksDatas = new List<ISerializableData>();

        // First pass, instantiate
        foreach (var obData in game.sobs)
        {
            string prefabName = obData.data.prefabName;

            Debug.Assert(!string.IsNullOrEmpty(prefabName), "Deserialization: Attempting to spawn a prefab, but the name is empty", this);
            Debug.Assert(prefabsDict.ContainsKey(prefabName), "Deserialization: Attempting to spawn a prefab, but prefab " + prefabName + " is not part of the spawn list. Did you forget to add it?", this);

            var prefab = prefabsDict[obData.data.prefabName];
            GameObject go = Instantiate(prefab);

            go.transform.position = obData.loc.pos;
            go.transform.eulerAngles = obData.loc.rot;
            //go.transform.localScale = obData.loc.scl;

            var obComp = go.GetComponentInChildren<ISerializable>();
            Debug.Assert(obComp != null, "Deserialization: ISerializable component not found on the root of spawned GameObject. Did you forgot to apply the prefab with ISerializable component?", go);
            obComp.SerializedData = obData.data;

            // Set id
            var idComp = (obComp as Component).gameObject.AddComponent<ID>();
            idComp.id = obData.id;

            spawned.Add(obComp);
            spawnedByIdMap.Add(idComp.id, obComp);

            if (obComp is ISerializableLinksHandler obCompLink)
            {
                links.Add(obCompLink);
                linksDatas.Add(obData.data);
            }

            //Debug.Log(str);
        }

        // Second pass, link
        for (int i = 0; i < links.Count; i++)
        {
            links[i].OnDeserializeLinks(linksDatas[i]);
        }

        Debug.Log("Deserialization: Ended");
    }

    public ISerializable GetSpawnedFromId(int i)
    {
        return spawnedByIdMap[i];
    }
}
