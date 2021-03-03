using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Serialization
{
    public class ID : MonoBehaviour
    {
        public int id;

        private void OnValidate()
        {
            if (id == 0)
                SetNew();
        }

        public void SetNew()
        {
            id = Random.Range(int.MinValue, int.MaxValue);
        }
    }
}