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

        private void OnValidate()
        {
            if (id == 0)
                SetNew();
        }

        void Awake()
        {
            if (Application.isPlaying)
                return;

            // DUplication detection
            if (prevInstanceID == 0)
            {
                prevInstanceID = GetInstanceID();
            }

            if (prevInstanceID != GetInstanceID() && GetInstanceID() < 0)
            {
                Debug.Log("Duplicated and object with ID");
                prevInstanceID = GetInstanceID();
                SetNew();
            }
        }
#endif
    }
}