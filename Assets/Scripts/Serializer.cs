using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

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
        CachePrefabs();

        // Gets all MonoBehaviours, might be slow
        var all = FindObjectsOfType<MonoBehaviour>();
        GameData game = new GameData();

        var linkableSobs = new List<SerializedGameObject>();
        var linkables = new List<ISerializableLinksHandler>();

        linkMap = new Dictionary<ISerializable, int>();

        for (int i = 0; i < all.Length; i++)
        {
            //Debug.Log("F " + all[i].name + ", type: " + all[i].GetType());

            if (all[i] is ISerializable sobj)
            {
                //Debug.Log("Found " + all[i].name);

                SerializedGameObject sob = new SerializedGameObject();

                sob.id = game.sobs.Count;
                sob.loc = new Loc(all[i].transform);
                sob.data = sobj.SerializedData;

                game.sobs.Add(sob);
                linkMap.Add(sobj, sob.id);

                if (all[i] is ISerializableLinksHandler obCompLink)
                {
                    linkableSobs.Add(sob);
                    linkables.Add(obCompLink);
                }
            }
        }

        // Pass 2: Serialize links
        for (int i = 0; i < linkableSobs.Count; i++)
        {
            var sob = linkableSobs[i];
            var obCompLink = linkables[i];
            obCompLink.OnSerializeLinks(ref sob.data);
        }



        string str = JsonConvert.SerializeObject(game, jsonSettings);
        Debug.Log(str);

        this.str = str;

        File.WriteAllText("scene.json", str);
    }

    public int GetIdOf(ISerializable serializable)
    {
        return linkMap[serializable];
    }

    [ContextMenu("Deserialize")]
    void Deserialize()
    {
        CachePrefabs();

        str = File.ReadAllText("scene.json");

        GameData game = JsonConvert.DeserializeObject<GameData>(str, jsonSettings);

        // Save a list of links
        List<ISerializableLinksHandler> links = new List<ISerializableLinksHandler>();
        spawned = new List<ISerializable>();
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

            spawned.Add(obComp);

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
    }

    public ISerializable GetSpawnedFromId(int i)
    {
        return spawned[i];
    }
}
