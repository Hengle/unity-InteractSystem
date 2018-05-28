﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;

public class singleCommand : MonoBehaviour {
    public ActionCommand command;
    public bool autoExecute;
    private void Start()
    {
        command.RegistComplete((x) => { Debug.Log(x +":Completed"); });
    }
    private void OnGUI()
    {
        if(GUILayout.Button("StartCommand"))
        {
            command.StartExecute(autoExecute);
        }
        if (GUILayout.Button("EndCommand"))
        {
            command.EndExecute();
        }
        if (GUILayout.Button("EndStarted"))
        {
            command.ActionObjCtrl.CompleteOneStarted();
        }
        if (GUILayout.Button("UnDoCommand"))
        {
            command.UnDoExecute();
        }
    }
}
