﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace InteractSystem
{
    public abstract class CompleteAbleCollectNode<T> : RuntimeCollectNode<CompleteAbleActionItem> where T : CompleteAbleActionItem
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            currents.Clear();
            finalGroup = null;
        }

        protected override void OnAddedToPool(CompleteAbleActionItem arg0)
        {
            base.OnAddedToPool(arg0);
            RegistComplete(arg0);
        }


        protected override void OnRemovedFromPool(CompleteAbleActionItem arg0)
        {
            base.OnRemovedFromPool(arg0);
            RemoveComplete(arg0);
        }

        protected void TryAutoComplete(int index)
        {
            if (index < itemList.Count)
            {
                var key = itemList[index];
                var item = elementPool.Find(x => x.Name == key && x.Active && x.OperateAble && x is T) as T;
                if (item != null)
                {
                    item.RegistOnCompleteSafety(OnAutoComplete);
                    item.AutoExecute();
                }
                else
                {
                    Debug.LogError("have no active useful element Name:" + key);
                }
            }
        }

        private void OnAutoComplete(CompleteAbleActionItem arg0)
        {
            arg0.RemoveOnComplete(OnAutoComplete);
            TryAutoComplete(currents.Count);
        }

        protected virtual void TryComplete(CompleteAbleActionItem item)
        {
            if (statu != ExecuteStatu.Executing) return;//没有执行
            if (!item.OperateAble) return;//目标无法点击
            if (currents.Count >= itemList.Count) return;//超过需要
            if (!(item is CompleteAbleActionItem)) return;

            if (itemList[currents.Count] == item.Name)
            {
                currents.Add(item as CompleteAbleActionItem);
                item.RecordPlayer(this);
                item.StepComplete();
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
                foreach (var item in elementPool)
                {
                    Debug.Log(item.Name + ":" + item.OperateAble);
                }
                var elements = elementPool.FindAll(x => x.Name == key && x.OperateAble);
                elements.ForEach(element =>
                {
                    element.StepActive();
                    (element as CompleteAbleActionItem).RegistOnCompleteSafety(TryComplete);
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
        ///尝试将元素设置为结束状态
        /// </summary>
        /// <param name="undo"></param>
        protected override void CompleteElements(bool undo)
        {
            base.CompleteElements(undo);

            if (undo)
            {
                foreach (var item in currents)
                {
                    item.StepUnDo();
                    item.RemovePlayer(this);
                }
                currents.Clear();
            }
            else
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (currents.Count <= i)
                    {
                        var element = elementPool.Find(x => x.Name == itemList[i] && x.OperateAble);
                        if (element != null)
                        {
                            element.RecordPlayer(this);
                            element.StepComplete();
                            currents.Add(element);
                        }
                        else
                        {
                            Debug.LogError("缺少：" + itemList[i]);
                        }
                    }
                    else
                    {
                        var item = currents[i];
                        if (item.Active)
                        {
                            currents[i].StepComplete();
                        }
                        currents[i].RecordPlayer(this);
                    }
                }
            }
        }
        /// <summary>
        /// 注册结束事件（仅元素在本步骤开始后创建时执行注册）
        /// </summary>
        /// <param name="arg0"></param>
        protected void RegistComplete(CompleteAbleActionItem arg0)
        {
            if (Statu == ExecuteStatu.Executing)
            {
                if (arg0 is CompleteAbleActionItem)
                {
                    // 注册元素结束事件
                    (arg0 as CompleteAbleActionItem).RegistOnCompleteSafety(TryComplete);
                }
            }
        }
        /// <summary>
        /// 注销结束事件（仅元素被销毁后执行）
        /// </summary>
        /// <param name="arg0"></param>
        protected void RemoveComplete(CompleteAbleActionItem arg0)
        {
            if (arg0 is CompleteAbleActionItem)
            {
                (arg0 as CompleteAbleActionItem).RemoveOnComplete(TryComplete);
            }
        }

    }
}