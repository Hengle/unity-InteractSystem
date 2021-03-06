﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace InteractSystem.Actions
{
    public static class LinkUtil
    {
        public static bool log = false;
        private static PreviewController previewCtrl { get { return PreviewController.Instence; } }
        private static AngleCtroller angleCtrl { get { return AngleCtroller.Instence; } }
        private static MatchType matchType { get { return Config.Instence.linkMatchType; } }

        public static void UpdateBrotherPos(LinkItem target, List<LinkItem> context)
        {
            if (target.ChildNodes == null || target.ChildNodes.Count == 0) return;

            context.Add(target);

            foreach (var item in target.ChildNodes)
            {
                if (item.ConnectedNode != null && !context.Contains(item.ConnectedNode.Body))
                {
                    var targetNode = item.ConnectedNode.Body;
                    var linkInfo = item.ConnectedNode.connectAble.Find(x => x.itemName == target.Name && x.nodeId == item.NodeID);

                    LinkUtil.ResetTargetTranform(targetNode, target, linkInfo.relativePos, linkInfo.relativeDir);
                    UpdateBrotherPos(targetNode, context);
                }
            }
        }

        public static void RecordTransform(LinkInfo nodeArecored, LinkInfo nodeBrecored, Transform ourItem, Transform otherItem)
        {
            var parent = ourItem.parent;
            ourItem.SetParent(otherItem);
            //当ourItem作为otherItem子物体时的坐标
            nodeArecored.relativePos = ourItem.localPosition;
            nodeArecored.relativeDir = ourItem.localEulerAngles;
            ourItem.SetParent(parent);
            parent = otherItem.parent;
            otherItem.SetParent(ourItem);
            //当otherItem作为ourItem子物体时的坐标
            nodeBrecored.relativePos = otherItem.localPosition;
            nodeBrecored.relativeDir = otherItem.localEulerAngles;
            otherItem.SetParent(parent);
        }

        public static void GetWorldPosFromTarget(LinkItem target, Vector3 rPos, Vector3 rdDir, out Vector3 position, out Vector3 dir)
        {
            var temp = new GameObject("temp");
            temp.transform.SetParent(target.transform);
            temp.transform.localPosition = rPos;
            temp.transform.localEulerAngles = rdDir;

            position = temp.transform.position;
            dir = temp.transform.eulerAngles;
            UnityEngine.Object.Destroy(temp);
        }
        public static void ResetTargetTranform(LinkItem target, LinkItem otherParent, Vector3 rPos, Vector3 rdDir)
        {
            var parent = target.Trans.parent;
            target.Trans.SetParent(otherParent.Trans);
            //当target作为otherParent子物体并移开的坐标
            target.Trans.localPosition = rPos;
            target.Trans.localEulerAngles = rdDir;
            target.Trans.SetParent(parent);
        }

        public static void AttachNodes(LinkPort moveAblePort, LinkPort staticPort)
        {
            moveAblePort.ConnectedNode = staticPort;
            staticPort.ConnectedNode = moveAblePort;
            moveAblePort.ResetTransform();
        }

        public static void DetachNodes(LinkPort moveAblePort, LinkPort staticPort)
        {
            if (moveAblePort != null) moveAblePort.ConnectedNode = null;
            if (staticPort != null) staticPort.ConnectedNode = null;
        }

        public static void ClampRotation(Transform target)
        {
            Vector3 newRot = target.eulerAngles;
            System.Func<float, float> clamp = (value) =>
            {
                float newValue = value % 360;
                if (newValue < 45)
                {
                    newValue = 0;
                }
                else
                {
                    if (newValue < 135)
                    {
                        newValue = 90;
                    }
                    else
                    {
                        if (newValue < 225)
                        {
                            newValue = 180;
                        }
                        else
                        {
                            if (newValue < 315)
                            {
                                newValue = -90;
                            }
                        }
                    }
                }
                return newValue;
            };

            newRot.x = clamp(newRot.x);
            newRot.y = clamp(newRot.y);
            newRot.z = clamp(newRot.z);

            target.eulerAngles = newRot;
        }
        public static void Clamp(Transform target, int d = 2)
        {
            var pos = target.position;
            var rot = target.eulerAngles;
            var size = target.localScale;
            Func<Vector3, Vector3> Round = (vector) =>
            {
                vector.x = (float)Math.Round(vector.x, d);
                vector.y = (float)Math.Round(vector.y, d);
                vector.z = (float)Math.Round(vector.z, d);
                return vector;
            };
            pos = Round(pos);
            rot = Round(rot);
            size = Round(size);
            target.position = pos;
            target.eulerAngles = rot;
            target.localScale = size;
        }

        /// <summary>
        /// 空间查找触发的点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static bool FindTriggerNodes(LinkPort item, out List<LinkPort> nodes)
        {
            nodes = null;
            Collider[] colliders = Physics.OverlapSphere(item.Pos, item.Range, LayerMask.GetMask(LinkPort.layer));
            if (colliders != null && colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    LinkPort tempNode = collider.GetComponentInParent<LinkPort>();
                    if (tempNode == null || tempNode == item || tempNode.Body == item.Body)
                    {
                        continue;
                    }
                    else
                    {
                        if (nodes == null)
                            nodes = new List<Actions.LinkPort>();
                        nodes.Add(tempNode);
                    }

                }
            }
            return nodes != null && nodes.Count > 0;
        }



        /// <summary>
        /// 空间查找可以可以连接的点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool FindInstallableNode(LinkPort item, out LinkPort node)
        {
            if (item.ConnectedNode != null)
            {
                node = item.ConnectedNode;
                return false;
            }
            switch (matchType)
            {
                case MatchType.ColliderRange:
                    return FindInstallableNode_ColliderRange(item, out node);
                case MatchType.WindowPosition:
                default:
                    return FindInstallableNode_WindowPosition(item, out node);

            }
        }

        /// <summary>
        /// 按屏幕坐标查找匹配点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool FindInstallableNode_WindowPosition(LinkPort item, out LinkPort node)
        {
            var linkPosts = SelectPorts(CameraController.Instence.currentCamera.transform.position, item.Pos, item.Range);
            if (linkPosts == null || linkPosts.Length == 0)
            {
                node = null;
                return false;
            }
            else
            {
                for (int i = 0; i < linkPosts.Length; i++)
                {
                    LinkPort tempNode = linkPosts[i];
                    if (JudgeLinkPort(item, tempNode))
                    {
                        node = tempNode;
                        return true;
                    }
                }
                node = null;
                return false;
            }
        }
        private static LinkPort[] SelectPorts(Vector3 cameraPos, Vector3 worldCenter, float range)
        {
            var dir = worldCenter - cameraPos;
            var quaternion = Quaternion.FromToRotation(Vector3.forward, dir.normalized);

            if (Input.GetKey(KeyCode.A))
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = worldCenter;
                cube.transform.rotation = quaternion;
                var bs = new Vector3(range, range, 100);
                cube.transform.localScale = bs;
            }

            var boxSize = new Vector3(range, range, 100);
            var hits = Physics.BoxCastAll(worldCenter, boxSize * 0.5f, dir, quaternion, 0.01f, LayerMask.GetMask(LinkPort.layer));

            var items = from hit in hits
                        let port = hit.collider.gameObject.GetComponentInParent<LinkPort>()
                        where port != null
                        select port;
            return items.ToArray();
        }
        /// <summary>
        /// 按空间坐标的方式查找匹配点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool FindInstallableNode_ColliderRange(LinkPort item, out LinkPort node)
        {
            Collider[] colliders = Physics.OverlapSphere(item.Pos, item.Range, LayerMask.GetMask(LinkPort.layer));
            if (colliders != null && colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    LinkPort tempNode = collider.GetComponentInParent<LinkPort>();
                    if (JudgeLinkPort(item, tempNode))
                    {
                        node = tempNode;
                        return true;
                    }
                }
            }
            node = null;
            return false;
        }
        /// <summary>
        /// 判断两个点能否连接
        /// </summary>
        /// <param name="item"></param>
        /// <param name="tempNode"></param>
        /// <returns></returns>
        private static bool JudgeLinkPort(LinkPort item, LinkPort tempNode)
        {
            if (tempNode == null || tempNode == item || tempNode.Body == item.Body || tempNode.ConnectedNode != null)
            {
                return false;
            }
            //主被动动连接点，非自身点，相同名，没有建立连接
            var linkInfo = item.connectAble.Find((x) => x.itemName == tempNode.Body.Name && x.nodeId == tempNode.NodeID);
            if (linkInfo != null)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 激活匹配点
        /// </summary>
        /// <param name="pickedUp"></param>
        public static void TryActiveLinkPorts(LinkItem pickedUp)
        {
            var linkItems = GameObject.FindObjectsOfType<LinkItem>();
            var linkPorts = new List<PreviewSet>();
            for (int i = 0; i < pickedUp.ChildNodes.Count; i++)
            {
                var node = pickedUp.ChildNodes[i];
                if (node.ConnectedNode == null && node.connectAble.Count > 0)
                {
                    for (int j = 0; j < node.connectAble.Count; j++)
                    {
                        var info = node.connectAble[j];

                        var otheritems = (from x in linkItems
                                          where (x != null && x != pickedUp && x.Name == info.itemName)
                                          where !HaveConnected(pickedUp, x)
                                          select x);
                        Debug.Assert(otheritems != null && otheritems.Count() > 0);
                        foreach (var otheritem in otheritems)
                        {
                            if (otheritem != null)
                            {
                                var otherNode = otheritem.ChildNodes[info.nodeId];
                                if (otherNode != null && otherNode.ConnectedNode == null)
                                {
                                    if (log) Debug.Log("在" + otheritem + "的" + info.nodeId + "端口上显示出 + " + pickedUp);
                                    var set = new PreviewSet();
                                    LinkUtil.GetWorldPosFromTarget(otheritem, info.relativePos, info.relativeDir, out set.position, out set.eulerAngle);
                                    linkPorts.Add(set);
                                }
                            }
                        }

                    }
                }
            }
            if (linkPorts.Count > 0)
                previewCtrl.Notice(pickedUp.Body, linkPorts.ToArray());
        }

        /// <summary>
        /// 判断两个物体是否已经连接
        /// </summary>
        /// <param name="linkItemA"></param>
        /// <param name="linkItemB"></param>
        /// <returns></returns>
        public static bool HaveConnected(LinkItem linkItemA, LinkItem linkItemB)
        {
            var connectedPorts = from port in linkItemA.GetLinkedPorts()
                                 select port.ConnectedNode;
            foreach (var item in linkItemB.GetLinkedPorts())
            {
                if (connectedPorts.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 清除连接点
        /// </summary>
        /// <param name="linkItem"></param>
        public static void ClearActivedLinkPort(LinkItem linkItem)
        {
            if (log) Debug.Log("清除所有" + linkItem + "的拷贝");
            previewCtrl.UnNotice(linkItem.Body);
        }
    }
}