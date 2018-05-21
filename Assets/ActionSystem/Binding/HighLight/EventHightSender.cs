﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;

namespace WorldActionSystem.Binding
{
    public class EventHightSender : ActionObjEventSender
    {
        private bool noticeAuto { get { return Config.highLightNotice; } }
        private string highLight { get { return "HighLightObjects"; } }
        private string unhighLight { get { return "UnHighLightObjects"; } }
        protected override void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(key)){
                key = actionObj.Name;
            }
        }
      
        protected void Update()
        {
            if (!noticeAuto) return;
            if (actionObj.Completed) return;

            if (actionObj.Started & !actionObj.Completed)
            {
                SetElementState(true);
            }
            else
            {
                SetElementState(false);
            }
        }
        protected override void OnBeforeActive(bool forceAuto)
        {
            if (noticeAuto)
            {
                SetElementState(true);
            }
        }
        protected override void OnBeforeComplete(bool force)
        {
            if (noticeAuto)
            {
                SetElementState(false);
            }
        }
        protected override void OnBeforeUnDo()
        {
            if (noticeAuto)
            {
                SetElementState(false);
            }
        }
        protected void SetElementState(bool open)
        {
            if (open)
            {
                eventCtrl.NotifyObserver<string>(highLight, key);
            }
            else
            {
                eventCtrl.NotifyObserver<string>(unhighLight, key);
            }
        }

    }

}