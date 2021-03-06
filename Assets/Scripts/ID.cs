using UnityEngine;

namespace Nothke.Serialization
{
    [ExecuteInEditMode]
    public class ID : MonoBehaviour
    {
        [Header("Set to 0 to get a new one")]
        public int id;

        public void SetNew()
        {
            id = Random.Range(int.MinValue, int.MaxValue);
        }

#if UNITY_EDITOR
        [SerializeField]
        int prevInstanceID = 0;

        public void SetNewEditor()
        {
            var so = new UnityEditor.SerializedObject(this);
            so.FindProperty("id").intValue = Random.Range(int.MinValue, int.MaxValue);
            so.ApplyModifiedProperties();
        }

        private void OnValidate()
        {
            if (id == 0)
                SetNewEditor();
        }

        void Awake()
        {
            if (Application.isPlaying)
                return;

            // DUplication detection
            if (prevInstanceID == 0)
            {
                Debug.Log("Getting new InstanceID");
                prevInstanceID = GetInstanceID();
            }

            if (prevInstanceID != GetInstanceID() && GetInstanceID() < 0)
            {
                Debug.Log("Duplicated and object with ID");
                prevInstanceID = GetInstanceID();
                SetNewEditor();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void SceneLoaded()
        {
            Debug.LogWarning("Called!");
            var ids = FindObjectsOfType<ID>();
            foreach (var id in ids)
            {
                id.prevInstanceID = id.GetInstanceID();
            }
        }
#endif
    }
}