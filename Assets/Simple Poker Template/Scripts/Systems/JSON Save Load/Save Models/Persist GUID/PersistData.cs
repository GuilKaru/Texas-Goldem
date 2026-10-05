using System;
using UnityEngine;

namespace SimplePoker.SaveLoad
{
    [ExecuteInEditMode]
    public class PersistData : MonoBehaviour
    {
        public string GUID;

        private void Awake()
        {
            Guid guid = Guid.NewGuid();
            if (GUID == "")
                GUID = guid.ToString();
        }

    }
}