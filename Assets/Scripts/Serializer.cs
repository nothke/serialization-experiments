using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public interface ISerializable
{
    ISerializableData Serialize();
    void Deserialize(ISerializableData data);
}

public interface ISerDat<T> where T : ISerializableData
{
    T Serialize();
    void Deserialize(T data);
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
        var all = FindObjectsOfType<MonoBehaviour>();
        GameData game = new GameData();

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is ISerializable sobj)
            {
                Debug.Log("Found " + all[i].name);

                SerializedGameObject sob = new SerializedGameObject();

                sob.loc = new Loc(all[i].transform);
                sob.data = sobj.Serialize();

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

        foreach (var obData in game.sobs)
        {
            var prefab = prefabsDict[obData.data.prefabName];
            GameObject go = Instantiate(prefab);

            go.transform.position = obData.loc.pos;
            go.transform.eulerAngles = obData.loc.rot;
            go.transform.localScale = obData.loc.scl;

            var obComp = go.GetComponent<ISerializable>();
            obComp.Deserialize(obData.data);
        }
    }
}
