using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchSerializer : MonoBehaviour
{
    public GameObject prefab;
    public int count = 1000;

    float serializeTime;
    float deserializeTime;

    void Start()
    {
        StartCoroutine(Run());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            StartCoroutine(Run());

        if (Input.GetKeyDown(KeyCode.D))
        {
            float t = Time.realtimeSinceStartup;
            Serializer.e.Deserialize();
            deserializeTime = (Time.realtimeSinceStartup - t);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Serialize: " + serializeTime);
        GUILayout.Label("Deserialize: " + deserializeTime);
    }

    IEnumerator Run()
    {
        yield return null;

        List<GameObject> gos = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, Random.insideUnitSphere * 100, Random.rotation);
            gos.Add(go);
            go.AddComponent<ID>().SetNew();
        }

        yield return null;
        float t = Time.realtimeSinceStartup;
        Serializer.e.Serialize();
        serializeTime = (Time.realtimeSinceStartup - t);
        yield return null;

        for (int i = gos.Count - 1; i >= 0; i--)
        {
            Destroy(gos[i]);
        }
    }
}
