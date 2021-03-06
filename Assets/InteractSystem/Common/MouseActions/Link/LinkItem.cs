﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace InteractSystem.Actions
{
    public class LinkItem : PickUpAbleItem
    {
        [SerializeField, Attributes.DefultGameObject("显示物体")]
        private GameObject m_viewObj;//可选择提示
        [SerializeField]
        private List<LinkPort> _childNodes = new List<LinkPort>();
        public Transform Trans
        {
            get
            {
                return transform;
            }
        }
        public ElementController elementCtrl { get { return ElementController.Instence; } }
        public List<LinkPort> GroupNodes
        {
            get
            {
                _groupNodes.Clear();
                linkLock.Clear();
                RetiveNodes(linkLock, this);
                return _groupNodes;
            }
        }
        public List<LinkPort> ChildNodes
        {
            get
            {
                InitPorts();
                return _childNodes;
            }
        }
        public bool Used { get { return elementCtrl.IsLocked(this); } }
        public bool isMatching { get; internal set; }
        public override bool OperateAble
        {
            get
            {
                return CalcuteConnected() < ChildNodes.Count;
            }
        }
        //private event UnityAction onConnected;
        private Vector3 startPos;
        private Quaternion startRot;
        private Vector3 lastForward = Vector3.forward;
        private List<LinkPort> _groupNodes = new List<LinkPort>();
        private List<LinkItem> linkLock = new List<LinkItem>();
        private float posHoldTime = 1f;
        private float posHoldTimer;
        private Vector3 mousePosCatch;

        protected override void Awake()
        {
            base.Awake();
            InitPorts();
        }

        protected override void Start()
        {
            base.Start();
            startPos = transform.position;
            startRot = transform.localRotation;
        }

        protected override void Update()
        {
            base.Update();
            UpdateMatchTime();
        }
        

        private void InitPorts()
        {
#if UNITY_EDITOR
            _childNodes.Clear();
#endif
            if (_childNodes == null || _childNodes.Count == 0)
            {
                var nodeItems = GetComponentsInChildren<LinkPort>(true);
                _childNodes.AddRange(nodeItems);
            }
        }

        public LinkPort[] GetLinkedPorts()
        {
            var connenctedPos = new List<LinkPort>();
            foreach (var item in ChildNodes)
            {
                if (item.ConnectedNode != null)
                {
                    connenctedPos.Add(item);
                }
            }
            return connenctedPos.ToArray();
        }

        private void RetiveNodes(List<LinkItem> context, LinkItem linkItem)
        {
            context.Add(linkItem);
            _groupNodes.AddRange(linkItem.ChildNodes);

            foreach (var item in linkItem.ChildNodes)
            {
                if (item.ConnectedNode != null && !context.Contains(item.ConnectedNode.Body))
                {
                    RetiveNodes(context, item.ConnectedNode.Body);
                }
            }
        }

        private int CalcuteConnected()
        {
            int count = 0;
            foreach (var child in ChildNodes)
            {
                if (child.ConnectedNode != null)
                {
                    count++;
                }
            }
            return count;
        }
        protected override void RegistPickupableEvents()
        {
            pickUpableFeature.RegistOnSetViweForward(OnSetViewForward);
            pickUpableFeature.RegistOnSetPosition(OnSetPosition);
        }


        protected void OnSetPosition(Vector3 pos)
        {
            if (!isMatching)
            {
                transform.position = pos;
                linkLock.Clear();
                OnTranformChanged(linkLock);
            }
            else
            {
                mousePosCatch = pos;
            }
        }

        private void UpdateMatchTime()
        {
            if (isMatching)
            {
                if (posHoldTimer < posHoldTime)
                {
                    posHoldTimer += Time.deltaTime;
                }
                else
                {
                    isMatching = false;
                    posHoldTimer = 0;
                    OnSetPosition(mousePosCatch);
                }
            }
        }

        protected void OnSetViewForward(Vector3 forward)
        {
            if (!isMatching)
            {
                if (lastForward == Vector3.zero)
                {
                    lastForward = forward;
                }
                else if (lastForward != forward)
                {
                    transform.localRotation = Quaternion.FromToRotation(lastForward, forward) * transform.localRotation;
                    linkLock.Clear();
                    OnTranformChanged(linkLock);
                    lastForward = forward;
                }
            }

        }

        public void OnTranformChanged(List<LinkItem> context)
        {
            LinkUtil.UpdateBrotherPos(this, context);
        }

        /// <summary>
        /// 判断是否已经连接过了
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool HaveConnected(LinkItem item)
        {
            bool connected = false;
            foreach (var child in item.ChildNodes)
            {
                connected |= (child.ConnectedNode != null);
            }
            return connected;
        }
        public override void UnDoChanges(UnityEngine.Object target)
        {
            base.UnDoChanges(target);
            mousePosCatch = transform.position = startPos;
            transform.rotation = startRot;

            foreach (var item in ChildNodes)
            {
                LinkUtil.DetachNodes(item, item.ConnectedNode);
            }
        }

        public override void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

    }
}