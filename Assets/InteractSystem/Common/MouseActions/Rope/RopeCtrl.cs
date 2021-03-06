﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;


namespace InteractSystem.Actions
{
    /// <summary>
    /// 将ropeItem安装到指定RopeObj上
    /// 然后安装内部的点到对就的RopeObj上
    /// </summary>
    public class RopeCtrl : PCOperateCtrl<RopeCtrl>,IUpdateAble
    {
        private RopeItem ropeTarget { get { return ropeSelected == null ? null :  ropeSelected.BindingTarget; } }
        private RopeElement ropeSelected;
        private Collider pickUpedRopeNode;
        private Collider placePosNode;
        private bool pickDownAble;
        private float elementDistence;
        private Ray ray;
        private Ray disRay;

        private RaycastHit hit;
        private RaycastHit disHit;
        private RaycastHit[] hits;
        private string resonwhy;
        private int ropePosLayerMask { get { return LayerMask.GetMask(RopeItem.layer); } }
        private int obstacleLayerMask { get { return LayerMask.GetMask(Layers.obstacleLayer); } }
        private int ropeNodeLayerMask { get { return LayerMask.GetMask(RopeElement.ropeItemLayer); } }
        private float hitDistence { get { return Config.Instence.hitDistence; } }
        private PickUpController pickCtrl { get { return PickUpController.Instence; ; } }

        public RopeCtrl()
        {
            pickCtrl.onPickup += (OnPickUp);
        }

        private void OnPickUp(PickUpAbleComponent arg0)
        {
            var ropeElement = arg0.GetComponentInParent<RopeElement>();
            if (ropeElement)
            {
                ropeSelected = ropeElement;
            }
        }

        public void Update()
        {
            //if (ropeSelected == null) return;

            if (pickUpedRopeNode == null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    TrySelectNode();
                }
            }
            else
            {
                RopeNodeMoveWithMouse(elementDistence += Input.GetAxis("Mouse ScrollWheel"));
                UpdateInstallRopeNode();
            }
        }
        private void TrySelectNode()
        {
            ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, hitDistence, ropeNodeLayerMask))
            {
                var obj = hit.collider.GetComponentInParent<RopeElement>();
                if (obj != null && obj.Actived)//正在进行操作
                {
                    obj.BindingTarget.PickupCollider(hit.collider);
                    ropeSelected = obj;
                    pickUpedRopeNode = hit.collider;
                    Debug.Log("Select: " + pickUpedRopeNode);
                    elementDistence = Vector3.Distance(viewCamera.transform.position, pickUpedRopeNode.transform.position);
                }
            }
        }

        private void UpdateInstallRopeNode()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceNode();
            }
            else
            {
                ray = viewCamera.ScreenPointToRay(Input.mousePosition);
                hits = Physics.RaycastAll(ray, hitDistence, ropePosLayerMask);
                if (hits != null || hits.Length > 0)
                {
                    bool hited = false;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (pickUpedRopeNode == null) return;
                        var target = hits[i].collider.GetComponentInParent<RopeItem>();

                        if (hits[i].collider.name == pickUpedRopeNode.name && target == ropeTarget)
                        {
                            hited = true;
                            placePosNode = hits[i].collider;
                            pickDownAble = CanPlaceNode(ropeTarget, ropeSelected, pickUpedRopeNode, out resonwhy);
                        }
                    }
                    if (!hited)
                    {
                        pickDownAble = false;
                        resonwhy = "零件放置位置不正确";
                    }
                }
            }

        }
        private void RopeNodeMoveWithMouse(float distence)
        {
            disRay = viewCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(disRay, out disHit, distence, obstacleLayerMask))
            {
                if (!ropeSelected.TryMoveToPos(pickUpedRopeNode, disHit.point))
                {
                    ropeTarget.PickDownCollider(pickUpedRopeNode);
                    pickUpedRopeNode = null;
                    //ropeSelected = null;
                }
            }
            else
            {
                var pos = disRay.GetPoint(elementDistence);
                if (!ropeSelected.TryMoveToPos(pickUpedRopeNode, pos))
                {
                    ropeTarget.PickDownCollider(pickUpedRopeNode);
                    pickUpedRopeNode = null;
                    //ropeSelected = null;
                }
            }
        }

        private void TryPlaceNode()
        {
            ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            if (pickDownAble)
            {
                PlaceNode(pickUpedRopeNode, placePosNode);
            }
            else
            {
                PlaceNodeWrong(ropeTarget, pickUpedRopeNode);
                SetUserErr(resonwhy);
            }
            pickUpedRopeNode = null;
            //ropeSelected = null;
            pickDownAble = false;
        }

        private static bool CanPlaceNode(RopeItem ropeTarget, RopeElement ropeSelected, Collider collider, out string resonwhy)
        {
            resonwhy = null;
            //if (ropeSelected.BindingTarget != ropeTarget)
            //{
            //    resonwhy = "对象不匹配";
            //}
            //else 
            if (ropeTarget == null)
            {
                resonwhy = "目标点父级没有挂RopeObj脚本";
            }
            else if (ropeTarget.Connected)
            {
                resonwhy = "目标点已经完成连接";
            }
            else if (!ropeTarget.CanInstallCollider(collider))
            {
                resonwhy = "坐标点已经占用";
            }
            return resonwhy == null;
        }

        private void PlaceNode(Collider collider,Collider placePosNode)
        {
            Debug.Log("PlaceNode");
            ropeTarget.TryInstallRopeItem(collider, placePosNode);
        }

        private void PlaceNodeWrong(RopeItem ropeItem, Collider collider)
        {
            Debug.Log("PlaceNodeWrong");
            ropeItem.PickDownCollider(collider);
        }
    }
}