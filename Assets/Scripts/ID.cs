using System.Collections;
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
        // This whole region is for detecting when an object has been duplicated in editor,
        // so that the new ID can be automatically assigned
        // IDHelper.cs makes sure there are no false positives detected on scene load and exiting play mode.

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

            // Wait a bit for the IDHelper to run and prevent duplicate detection
            // on scene load and on exiting play mode
            StartCoroutine(PostAwake());
        }

        IEnumerator PostAwake()
        {
            // Silly hack because of Unity's inconsistent editor callback scheduling
            yield return null;
            yield return null;
            yield return null;
            CheckDuplicate();
        }

        
        void CheckDuplicate()
        {
            //Debug.Log("Checking duplicate");

            if (prevInstanceID == 0)
            {
                Debug.Log("Getting new InstanceID");
                prevInstanceID = GetInstanceID();
            }

            if (prevInstanceID != GetInstanceID())
            {
                Debug.Log("Duplication detected! " + prevInstanceID + " != " + GetInstanceID());
                prevInstanceID = GetInstanceID();
                SetNewEditor();
            }
        }

        // Called by IDHelper
        public void OverrideInstanceID()
        {
            prevInstanceID = GetInstanceID();
        }
#endif
    }
}