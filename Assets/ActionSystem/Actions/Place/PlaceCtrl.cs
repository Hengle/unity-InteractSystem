﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    public sealed class PlaceCtrl : OperateController, IPlaceState
    {
        public IHighLightItems highLight;
        private PickUpController pickCtrl { get { return ActionSystem.Instence.pickupCtrl; } }
        public PlaceElement pickedUpObj { get { return pickCtrl.pickedUpObj is PlaceElement ? pickCtrl.pickedUpObj as PlaceElement : null; } }
        public PlaceObj installPos;
        public bool installAble;
        public string resonwhy;
        public bool activeNotice { get { return Config.highLightNotice; } }
        public float hitDistence { get { return Config.hitDistence; } }

        private int _placePosLayerMask = 0;
        public int PlacePoslayerMask { get { if(_placePosLayerMask == 0) _placePosLayerMask = LayerMask.GetMask(Layers.placePosLayer); return _placePosLayerMask; } }

        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Place;
            }
        }

        public const float minDistence = 1f;
        public Ray ray;
        public RaycastHit hit;
        public RaycastHit[] hits;
      
        private IPlaceState current;

        public PlaceCtrl()
        {
            highLight = new ShaderHighLight();
        }

        #region 鼠标操作事件
        public override void Update()
        {
            if (pickCtrl.PickedUp)
            {
                UpdatePlaceState();

                if (pickedUpObj && Input.GetMouseButtonDown(0))
                {
                    TryPlaceObject();
                    pickCtrl.PickDown();
                }
            }
        }

        public void UpdatePlaceState()
        {
            if (pickedUpObj == null) return;

            if (!pickedUpObj.Active)
            {
                resonwhy = "当前步骤无需该零件!";
                installAble = false;
            }
            else
            {
                ray = viewCamera.ScreenPointToRay(Input.mousePosition);
                hits = Physics.RaycastAll(ray, hitDistence, PlacePoslayerMask);
                if (hits != null || hits.Length > 0)
                {
                    var hitedObj = false;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        installPos = hits[i].collider.GetComponentInParent<PlaceObj>();
                        if (installPos)
                        {
                            current = installPos.PlaceState;
                            hitedObj = true;
                            installAble = CanPlace(installPos, pickCtrl.pickedUpObj, out resonwhy);
                            if (installAble)
                            {
                                break;
                            }
                        }
                    }
                    if (!hitedObj)
                    {
                        installAble = false;
                        resonwhy = "零件放置位置不正确";
                    }
                }
            }

            if (installAble)
            {
                //可安装显示绿色
                if (activeNotice) highLight.HighLightTarget(pickedUpObj.Render, Color.green);
            }
            else
            {
                //不可安装红色
                if (activeNotice) highLight.HighLightTarget(pickedUpObj.Render, Color.red);
            }
        }
        /// <summary>
        /// 尝试安装元素
        /// </summary>
        void TryPlaceObject()
        {
            ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            if (installAble)
            {
                PlaceObject(installPos, pickedUpObj);
            }
            else
            {
                PlaceWrong(pickedUpObj);
                SetUserErr(resonwhy);
            }

            installAble = false;

            if (activeNotice)
            {
                highLight.UnHighLightTarget(pickedUpObj.Render);
            }
        }

        public void PlaceObject(PlaceObj pos, PlaceElement pickup)
        {
            current.PlaceObject(pos, pickup);
        }

        public bool CanPlace(PlaceObj placeItem, PickUpAbleItem element, out string why)
        {
            return current.CanPlace(placeItem, element, out why);
        }

        public void PlaceWrong(PlaceElement pickup)
        {
            current.PlaceWrong(pickup);
        }
        #endregion
    }
}
