﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using InteractSystem.Actions;
using System;
using System.Linq;

namespace InteractSystem
{
    [System.Serializable]
    public class QueueCollectNodeFeature : CollectNodeFeature
    {
        public UnityAction onComplete { get; private set; }
        protected List<ISupportElement> currents = new List<ISupportElement>();
        public QueueCollectNodeFeature(System.Type type) : base(type, false) { }

        public override void OnEnable()
        {
            base.OnEnable();
            currents.Clear();
            finalGroup = null;
        }

        protected override void OnAddedToPool(ISupportElement arg0)
        {
            base.OnAddedToPool(arg0);
            if (SupportType(arg0.GetType()))
            {
                RegistComplete(arg0);
            }
        }

        public override void SetTarget(Graph.OperaterNode node)
        {
            base.SetTarget(node);
            onComplete = () =>
            {
                node.OnEndExecute(false);
            };
        }

        protected override void OnRemovedFromPool(ISupportElement arg0)
        {
            base.OnRemovedFromPool(arg0);
            if (!SupportType(arg0.GetType())) return;
            RemoveComplete(arg0 as ActionItem);
        }

        protected void TryAutoComplete(int index)
        {
            if (index < itemList.Count)
            {
                var key = itemList[index];
                var item = elementPool.Find(x => x.Name == key && x.Actived && x is ActionItem && (x as ActionItem).OperateAble) as ActionItem;
                if (item != null)
                {
                    var completeFeature = item.RetriveFeature<CompleteAbleItemFeature>();
                    if (completeFeature != null)
                    {
                        completeFeature.RegistOnCompleteSafety(target, OnAutoComplete);
                        completeFeature.AutoExecute(target);
                    }
                }
                else
                {
                    Debug.LogError("have no active useful element Name:" + key);
                }
            }
        }

        public override void OnEndExecute(bool force)
        {
            base.OnEndExecute(force);

            if (onComplete != null)
            {
                onComplete.Invoke();
            }
            else
            {
                Debug.LogError("onComplete not registed:" + target);
            }
        }

        private void OnAutoComplete(CompleteAbleItemFeature arg0)
        {
            arg0.RemoveOnComplete(target);
            TryAutoComplete(currents.Count);
        }

        protected virtual void TryComplete(CompleteAbleItemFeature item)
        {
            if (target.Statu != ExecuteStatu.Executing) return;//没有执行
            if (!item.target.OperateAble) return;//目标无法点击
            if (currents.Count >= itemList.Count) return;//超过需要

            if (itemList[currents.Count] == item.target.Name)
            {
                Debug.Log("add:" + item.target);
                currents.Add(item.target as ActionItem);
                item.target.RecordPlayer(target);
                SetInActiveElement(item.target);
            }

            if (currents.Count >= itemList.Count)
            {
                finalGroup = currents.ToArray();
                OnEndExecute(false);
            }
            else
            {
                FindOperateAbleItems();
            }
        }


        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            FindOperateAbleItems();
            if (auto)
            {
                AutoCompleteItems();
            }
        }

        /// <summary>
        /// 将所能点击的目标设置为激活状态
        /// </summary>
        private void FindOperateAbleItems()
        {
            if (itemList.Count > currents.Count)
            {
                var key = itemList[currents.Count];

                var elements = elementPool.FindAll(x => x.Name == key && (x as ActionItem).OperateAble);

                elements.ForEach(element =>
                {
                    if (element.OperateAble && target.Statu == ExecuteStatu.Executing)
                    {
                        //element.StepActive();
                        ActiveElement(element);
                    }

                    var feature = (element as ActionItem).RetriveFeature<CompleteAbleItemFeature>();

                    if (feature != null)
                    {
                        feature.RegistOnCompleteSafety(target, TryComplete);
                    }
                    else
                    {
                        Debug.LogError("element have no completeAble feature:", element as ActionItem);
                    }
                });
            }
        }
        /// <summary>
        /// 自动点击目标元素
        /// </summary>
        protected virtual void AutoCompleteItems()
        {
            TryAutoComplete(0);
        }

        /// <summary>
        ///尝试将元素设置为非激活状态
        /// </summary>
        /// <param name="undo"></param>
        protected override void CompleteElements(bool undo)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                var elementName = itemList[i];
                var elements = elementPool.FindAll(x => x.Name == elementName);

                foreach (var item in elements)
                {
                    item.SetInActive(target);

                    if (undo){
                        UndoElement(item);
                        item.RemovePlayer(target);
                    }
                }

                ///锁定元素并结束
                if (currents.Count <= i && !undo)
                {
                    var element = elements.FirstOrDefault();
                    if (element != null) {
                        currents.Add(element);
                    }
                    else
                    {
                        Debug.LogError("缺少：" + itemList[i]);
                    }
                }

                //记录使用
                if(!undo)
                {
                    var item = currents[i];
                    (item as ActionItem).RecordPlayer(target);
                    SetInActiveElement(item);
                }
            }

            if (undo)
            {
                currents.Clear();
            }
        }

        /// <summary>
        /// 注册结束事件（仅元素在本步骤开始后创建时执行注册）
        /// </summary>
        /// <param name="arg0"></param>
        protected void RegistComplete(ISupportElement arg0)
        {
            if (target.Statu == ExecuteStatu.Executing)
            {
                if (arg0 is ActionItem)
                {
                    // 注册元素结束事件
                    var feature = (arg0 as ActionItem).RetriveFeature<CompleteAbleItemFeature>();
                    feature.RegistOnCompleteSafety(target, TryComplete);
                }
            }
        }
        /// <summary>
        /// 注销结束事件（仅元素被销毁后执行）
        /// </summary>
        /// <param name="arg0"></param>
        protected void RemoveComplete(ActionItem arg0)
        {
            if (arg0 is ActionItem)
            {
                var feature = arg0.RetriveFeature<CompleteAbleItemFeature>();
                if (feature == null)
                {
                    Debug.Log(arg0 + "中没有:CompleteAbleItemFeature");
                }
                else
                {
                    feature.RemoveOnComplete(target);
                }
            }
        }

    }
}