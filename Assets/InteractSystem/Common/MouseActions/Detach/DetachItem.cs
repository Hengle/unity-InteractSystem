﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace InteractSystem.Actions
{
    /// <summary>
    /// 移除一定的范围并添加刚体组件
    /// </summary>

    public class DetachItem : ActionItem
    {
        public override bool OperateAble
        {
            get
            {
                return targets.Count == 0;
            }
        }

        private Vector3 startPos;
        private Quaternion startRot;
        public PickUpAbleFeature pickupableFeature = new PickUpAbleFeature();
        public CompleteAbleItemFeature completeAbleFeature = new CompleteAbleItemFeature();
        [SerializeField]
        private DetachRule rule;
        [SerializeField,Range(0.1f,10f),Attributes.CustomField("时间比(自动时)")]
        protected float speedSpan = 1;
        protected float autoCompleteTime { get { return Config.Instence.autoExecuteTime/ speedSpan; } }
        protected CoroutineController coroutineCtrl { get { return CoroutineController.Instence; } }
        public const string layer = "i:detachitem";

        protected override void Start()
        {
            base.Start();
            if (rule) rule = Instantiate(rule);
            startPos = transform.localPosition;
            startRot = transform.localRotation;
        }
        protected override List<ActionItemFeature> RegistFeatures()
        {
            var features = base.RegistFeatures();

            pickupableFeature.Init(this, layer);
            pickupableFeature.RegistOnSetPosition(SetPosition);
            features.Add(pickupableFeature);

            completeAbleFeature.Init(this, OnAutoExecute);
            features.Add(completeAbleFeature);

            return features;
        }
        public void OnAutoExecute(UnityEngine.Object node)
        {
            coroutineCtrl.DelyExecute(OnDetach, autoCompleteTime);
        }
        protected override void OnSetActive(UnityEngine.Object target)
        {
            base.OnSetActive(target);
            Notice(transform);
        }
        protected override void OnSetInActive(UnityEngine.Object target)
        {
            base.OnSetInActive(target);
            UnNotice(transform);
            if (!OperateAble)
            {
                if(rule) rule.OnDetach(this);
            }
        }
        public override void UnDoChanges(UnityEngine.Object target)
        {
            base.UnDoChanges(target);
            if (rule)
                rule.UnDoDetach();
            transform.localPosition = startPos;
            transform.localRotation = startRot;
        }

        internal void OnDetach()
        {
            if (rule){
                rule.OnDetach(this);
            }
            completeAbleFeature.OnComplete(firstLock);
        }
  
        private void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }
    }
}