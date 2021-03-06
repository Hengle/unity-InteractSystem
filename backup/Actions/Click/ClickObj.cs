﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem.Actions
{
    [AddComponentMenu(MenuName.ClickObj)]
    public class ClickObj : RuntimeObj<ClickItem>
    {
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Click;
            }
        }
        public override List<string> NeedElements
        {
            get
            {
                return clickList;
            }
        }
        [SerializeField]
        private List<string> clickList = new List<string>();
        private ClickItem[] finalGroup;
        private int clickedIndex = 0;


        protected override void OnAdd(ClickItem arg0)
        {
            base.OnAdd(arg0);
            arg0.RegistOnClick(TryComplete);
        }

        protected override void OnRemove(ClickItem arg0)
        {
            base.OnRemove(arg0);
            arg0.RemoveOnClicked(TryComplete);
        }

        private void TryComplete()
        {
            if (!Started || Completed) return;
        }

        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            startedList.Add(this);
            OnStepActive();
            if (auto){
                AutoClickItems();
            }
            else
            {
                AngleOnActive();
            }
        }

        /// <summary>
        /// 将所能点击的目标设置为激活状态
        /// </summary>
        private void OnStepActive()
        {
            if (clickList.Count > clickedIndex)
            {
                var key = clickList[clickedIndex];
                var elements = elementPool.FindAll(x => x.Name == key);
                elements.ForEach(element =>
                {
                    if(element.ClickAble)
                    {
                        element.StepActive();
                    }
                });
            }
        }

        /// <summary>
        /// 箭头提醒一个其中一个目标
        /// </summary>
        private void AngleOnActive()
        {
        }


        /// <summary>
        /// 自动点击目标元素
        /// </summary>
        private void AutoClickItems()
        {

        }

        public override void OnEndExecute(bool force)
        {
            base.OnEndExecute(force);
        }
        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
        }


    
    }

}