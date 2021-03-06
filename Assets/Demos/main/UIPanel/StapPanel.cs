﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace InteractSystem
{
    public class StapPanel:MonoBehaviour
    {
        private int backNumInput = 0;
        private string jumpStapInput;
        private int forwardNumInput = 0;
        public Config config;
        public GameObject panel;
        public ActionGroup group;

        public Button accept;
        public Button start;
        public Button backAstep;
        public Button backASmallstep;
        public Button backMutiStap;
        public Button toTargetStap;
        public Button skipAStap;
        public Button skipASmallStap;
        public Button skipMutiStap;
        public Button toEnd;

        public Toggle notice;
        public Toggle autoNext;
        public Toggle autoPlay;

        public InputField nameField;
        public Button create;

        ICommandController remoteController { get { return group.RemoteController; } }

        public List<Binding.ActionItemBinding> actionItemBindings;
        /// <summary>
        /// 注册按扭事件
        /// </summary>
        void Awake()
        {
            Config.SetConfig(config);
            Config.Instence.actionItemBindings.AddRange(actionItemBindings);
            RegistEvents();
        }

        private void Start()
        {
            panel.SetActive(false);
            group.LunchActionSystem();
            group.EventTransfer.onUserError += (x, y) => { Debug.Log(string.Format("{0}：{1}", x, y)); };
            panel.SetActive(true);
        }
        private void OnDestroy()
        {
            if (group) Destroy(group);
        }

        public void OnbackNumInputEndEdit(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                backNumInput = -int.Parse(value);
            }
            else
            {
                backNumInput = default(int);
            }
        }
        public void OnjumpStapInputEndEdit(string value)
        {
            jumpStapInput = value;
        }
        public void OnforwardNumInputEndEdit(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                forwardNumInput = int.Parse(value);
            }
            else
            {
                forwardNumInput = default(int);
            }
        }

     
       
        private void RegistEvents()
        {
            accept.onClick.AddListener(OnAcceptButtonCilcked);
            start.onClick.AddListener(OnToStartButtonClicked);
            backAstep.onClick.AddListener(OnBackAStapButtonClicked);
            backMutiStap.onClick.AddListener(OnBackMutiButtonClicked);
            toTargetStap.onClick.AddListener(OnToGargetButtonClicked);
            skipAStap.onClick.AddListener(OnSkipAstepButtonClicekd);
            skipMutiStap.onClick.AddListener(OnSkipMutiButtonClicked);
            toEnd.onClick.AddListener(ToEndButtonClicked);
            notice.onValueChanged.AddListener(OnNoticeStateChanged);
            autoPlay.onValueChanged.AddListener(OnAutoPlayStateChanged);
            create.onClick.AddListener(CreateAnElement);
            skipASmallStap.onClick.AddListener(OnSkipSmallStepClicked);
            backASmallstep.onClick.AddListener(OnBackSmallStepClicked);
            //accept.onClick.AddListener(OnSelected);
            start.onClick.AddListener(OnStapChange);
            backAstep.onClick.AddListener(OnStapChange);
            backMutiStap.onClick.AddListener(OnStapChange);
            toTargetStap.onClick.AddListener(OnStapChange);
            skipAStap.onClick.AddListener(OnStapChange);
            skipMutiStap.onClick.AddListener(OnStapChange);
            toEnd.onClick.AddListener(OnStapChange);
        }

        private void OnBackSmallStepClicked()
        {
            ActionCtroller.Instence.OnUnDoExecuteOne();
        }

        private void OnSkipSmallStepClicked()
        {
            ActionCtroller.Instence.OnEndExecuteStarted();
        }

        private void CreateAnElement()
        {
            ElementController.Instence.TryCreateElement<ISupportElement>(nameField.text, ActionSystem.Instence.transform);
        }

        private void OnAutoPlayStateChanged(bool arg0)
        {
            OnAcceptButtonCilcked();
        }

        void OnAcceptButtonCilcked()
        {
            if (remoteController.CurrCommand != null)
            {
                remoteController.StartExecuteCommand(OnEndExecute, autoPlay.isOn);
                if (remoteController.CurrCommand != null)
                {
                    textShow.text = remoteController.CurrCommand.StepName;
                }
            }
            else
            {
                textShow.text = "结束";
            }
        }
        void OnToStartButtonClicked()
        {
            remoteController.ToAllCommandStart();
            if (autoNext.isOn)
            {
                OnAcceptButtonCilcked();
            }
        }
        void OnBackAStapButtonClicked()
        {
            remoteController.UnDoCommand();
            if (autoNext.isOn)
            {
                OnAcceptButtonCilcked();
            }
        }

        void OnBackMutiButtonClicked()
        {
            remoteController.ExecuteMutliCommand(backNumInput);
            if (autoNext.isOn)
            {
                OnAcceptButtonCilcked();
            }
        }
        void OnToGargetButtonClicked()
        {
            remoteController.ToTargetCommand(jumpStapInput);
            if (autoNext.isOn)
            {
                OnAcceptButtonCilcked();
            }
        }
        void OnSkipAstepButtonClicekd()
        {
            remoteController.ExecuteMutliCommand(1);
            if (autoNext.isOn)
            {
                OnAcceptButtonCilcked();
            }
        }
        void OnSkipMutiButtonClicked()
        {
            remoteController.ExecuteMutliCommand(forwardNumInput);
            if (autoNext.isOn)
            {
                OnAcceptButtonCilcked();
            }
        }
        void ToEndButtonClicked()
        {
            remoteController.ToAllCommandEnd();
            if (autoNext.isOn)
            {
                OnAcceptButtonCilcked();
            }
        }
        void OnStapChange()
        {
            if (autoNext.isOn)
            {
                textShow.text = remoteController.CurrCommand != null ? remoteController.CurrCommand.StepName : "结束";
            }
            else
            {
                textShow.text = "点击接收任务";
            }
        }
        void OnEndExecute(bool haveNext)
        {
            if (haveNext)
            {
                if (autoNext.isOn) OnAcceptButtonCilcked();
                OnStapChange();
            }
            if (!haveNext)
            {
                textShow.text = "完成";
            }
        }
        void OnNoticeStateChanged(bool isOn)
        {
            Config.Instence.highLightNotice = isOn;
        }
        public Text textShow;
    }
}