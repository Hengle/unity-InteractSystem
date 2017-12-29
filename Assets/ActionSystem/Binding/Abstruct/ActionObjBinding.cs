﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;

[RequireComponent(typeof(ActionObj))]
public class ActionObjBinding : MonoBehaviour {
    protected ActionObj actionObj;
    private EventController _eventCtrl;
    protected EventController eventCtrl { get { transform.RetriveEventCtrl(ref _eventCtrl);return _eventCtrl; } }
    private Config _config;
    protected Config config { get { transform.RetriveConfig(ref _config); return _config; } }
    protected virtual void Awake()
    {
        actionObj = gameObject.GetComponent<ActionObj>();
        actionObj.onBeforeStart.AddListener(OnBeforeActive);
        actionObj.onBeforeComplete.AddListener(OnBeforeComplete);
        actionObj.onBeforeUnDo.AddListener(OnBeforeUnDo);
    }
    private void OnDestroy()
    {
        if (actionObj)
        {
            actionObj.onBeforeStart.RemoveListener(OnBeforeActive);
            actionObj.onBeforeComplete.RemoveListener(OnBeforeComplete);
            actionObj.onBeforeUnDo.RemoveListener(OnBeforeUnDo);
        }
    }
    protected virtual void OnBeforeActive(bool forceAuto) { }
    protected virtual void OnBeforeComplete(bool force) { }
    protected virtual void OnBeforeUnDo() { }
}
