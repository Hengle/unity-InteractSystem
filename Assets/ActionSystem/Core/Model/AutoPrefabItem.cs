﻿using UnityEngine;
using System;
namespace WorldActionSystem
{

    [System.Serializable]
    public class AutoPrefabItem : IComparable<AutoPrefabItem>
    {
#if UNITY_EDITOR
        public int instanceID;
#endif
        private string _id;
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = CalcuteID(prefab, matrix);
                }
                return _id;
            }
        }
        public Matrix4x4 matrix;
        public GameObject prefab;
        public bool ignore;
        public static string CalcuteID(GameObject prefab, Matrix4x4 matrix)
        {
            string _id = null;
            var name = prefab == null ? "Null" : prefab.name;
            _id = string.Format("[{0}][{1}]", name, matrix);
            return _id;
        }
        public int CompareTo(AutoPrefabItem other)
        {
            if (prefab == null || other.prefab == null) return 0;
            return string.Compare(prefab.name, other.prefab.name);
        }
    }
}