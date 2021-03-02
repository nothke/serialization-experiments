using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public interface ISerializable
{
    ISerializableData SerializedData { get; set; }
}

public interface ISerializableLinksHandler
{
    void OnDeserializeHandleLinks();
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
    public GameObject[] prefabs;
    [System.NonSerialized]
    public Dictionary<string, GameObject> prefabsDict;

    public struct Loc
    {
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scl;

        public Loc(Transform t)
        {
            pos = t.position;
            rot = t.eulerAngles;
            scl = t.localScale;
        }
    }

    public struct SerializedGameObject
    {
        public Loc loc;
        public ISerializableData data;
    }

    [System.Serializable]
    public class GameData
    {
        public List<SerializedGameObject> sobs = new List<SerializedGameObject>();
    }

    string str;

    void Start()
    {
        prefabsDict = new Dictionary<string, GameObject>(prefabs.Length);
        foreach (var go in prefabs)
            prefabsDict.Add(go.name, go);

        Serialize();

        Deserialize();
    }

    void Serialize()
    {
        // Gets all MonoBehaviours, might be slow
        var all = FindObjectsOfType<MonoBehaviour>();
        GameData game = new GameData();

        for (int i = 0; i < all.Length; i++)
        {
            Debug.Log("F " + all[i].name + ", type: " + all[i].GetType());

            //var test = (ISerializable)all[i];
            //if (test != null)
            //Debug.Log("WORKS!");

            if (all[i] is ISerializable sobj)
            {
                Debug.Log("Found " + all[i].name);

                SerializedGameObject sob = new SerializedGameObject();

                sob.loc = new Loc(all[i].transform);
                sob.data = sobj.SerializedData;

                game.sobs.Add(sob);
            }
        }

        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        string str = JsonConvert.SerializeObject(game, settings);
        Debug.Log(str);

        this.str = str;
    }

    void Deserialize()
    {
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        GameData game = JsonConvert.DeserializeObject<GameData>(str, settings);

        // Save a list of links
        List<ISerializableLinksHandler> links = new List<ISerializableLinksHandler>();

        // First pass, instantiate
        foreach (var obData in game.sobs)
        {
            string prefabName = obData.data.prefabName;

            Debug.Assert(!string.IsNullOrEmpty(prefabName), "Attempting to spawn a prefab, but the name is empty", this);
            Debug.Assert(prefabsDict.ContainsKey(prefabName), "Attempting to spawn a prefab, but prefab " + prefabName + " is not part of the spawn list. Did you forget to add it?", this);

            var prefab = prefabsDict[obData.data.prefabName];
            GameObject go = Instantiate(prefab);

            go.transform.position = obData.loc.pos;
            go.transform.eulerAngles = obData.loc.rot;
            go.transform.localScale = obData.loc.scl;

            var obComp = go.GetComponent<ISerializable>();
            obComp.SerializedData = obData.data as SerializableData;

            if (obComp is ISerializableLinksHandler obCompLink)
                links.Add(obCompLink);

            //Debug.Log(str);
        }

        // Second pass, link
        foreach (var obCompLink in links)
        {
            obCompLink.OnDeserializeHandleLinks();
        }
    }
}
