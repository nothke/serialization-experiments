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
        EditorApplication.hierarchyChanged += EditorApplication_hierarchyChanged;
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            Debug.Log("IDHelper: Resetting InstanceIDs on exiting play mode before duplicate check");
            SetInstanceIDsBeforeIDsGetChanceTo();
        }
    }

    private static void EditorApplication_hierarchyChanged()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName != name)
        {
            name = activeSceneName;
            OnChange?.Invoke(name);
            Debug.Log("IDHelper: Resetting InstanceIDs on scene load before duplicate check");
            SetInstanceIDsBeforeIDsGetChanceTo();
        }
    }

    public static void SetInstanceIDsBeforeIDsGetChanceTo()
    {
        var ids = Object.FindObjectsOfType<ID>();
        foreach (var id in ids)
        {
            id.OverrideInstanceID();
        }
    }
}
