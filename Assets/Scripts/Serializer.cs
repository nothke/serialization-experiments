using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using Newtonsoft.Json.Converters;

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

    void Start()
    {
        var all = FindObjectsOfType<MonoBehaviour>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is ISerializable sobj)
            {
                Debug.Log("Found " + all[i].name);

                var loc = new Loc(all[i].transform);

                var str = JsonConvert.SerializeObject(loc, Formatting.Indented);

                str += JsonConvert.SerializeObject(sobj.data, Formatting.Indented);


                if (all[i] is ISerializablePrefabLink spref)
                {
                    JsonConvert.SerializeObject(spref.prefabName);
                }

                Debug.Log(str);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
