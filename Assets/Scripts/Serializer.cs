using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public interface ISerializable
{
    ISerializableData data { get; }
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
        Serialize();


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
                sob.data = sobj.data;

                game.sobs.Add(sob);
            }
        }

        string str = JsonConvert.SerializeObject(game, Formatting.Indented);
        Debug.Log(str);

        this.str = str;
    }

    void Deserialize()
    {

    }
}
