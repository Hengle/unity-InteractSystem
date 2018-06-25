﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace InteractSystem.Actions
{
    [System.Serializable]
    public class LinkInfo:IComparable<LinkInfo>

    {
        public string itemName;
        public int nodeId;
        public Vector3 relativePos;
        public Vector3 relativeDir;

        public int CompareTo(LinkInfo other)
        {
            if(nodeId > other.nodeId)
            {
                return 1;
            }
            else if(nodeId < other.nodeId)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}