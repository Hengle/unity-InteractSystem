﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class InstallState : IPlaceState
    {
        public ControllerType CtrlType { get { return ControllerType.Place; } }

        public bool Active { get; private set; }

        public bool CanPlace(PlaceObj placeObj, PickUpAbleItem element, out string why)
        {
            why = null;
            var canplace = true;
            if (placeObj == null)
            {
                Debug.LogError("【配制错误】:零件未挂InstallObj脚本");
            }
            else if (!placeObj.Started)
            {
                canplace = false;
                why = "操作顺序错误";
            }
            else if (placeObj.AlreadyPlaced)
            {
                canplace = false;
                why = "已经安装";
            }

            else if (element.Name != placeObj.Name)
            {
                canplace = false;
                why = "零件不匹配";
            }
            else
            {
                canplace = true;
            }
            return canplace;
        }

        public void PlaceObject(PlaceObj pos, PlaceElement pickup)
        {
            pos.Attach(pickup);
            pickup.QuickInstall(pos,true);
            pickup.PickUpAble = false;
        }

        public void PlaceWrong(PlaceElement pickup)
        {
            if (pickup)
            {
                pickup.NormalUnInstall();
            }
        }
    }

}