using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.SceneManagement;
using Nothke.Serialization;

[InitializeOnLoad]
public static class IDHelper
{
    public delegate void ActiveSceneChangeDelegate(string name);
    public static event ActiveSceneChangeDelegate OnChange;

    private static string name;

    static IDHelper()
    {
        name = SceneManager.GetActiveScene().name;
        EditorApplication.hierarchyChanged += hierarchyWindowChanged;
        Debug.Log("Wat");
    }

    private static void hierarchyWindowChanged()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName != name)
        {
            name = activeSceneName;
            OnChange?.Invoke(name);
            Debug.Log("Invoked!");
            SetIDsBeforeAwake();
        }
    }

    public static void SetIDsBeforeAwake()
    {
        var ids = Object.FindObjectsOfType<ID>();
        foreach (var id in ids)
        {
            id.OverrideInstanceID();
        }
    }
}
