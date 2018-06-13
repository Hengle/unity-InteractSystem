﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace InteractSystem.Common.Actions
{
    /// <summary>
    /// 用于原料的吸取和填入
    /// </summary>
    public class ChargeTool : PickUpAbleElement
    {
        [SerializeField]
        private ChargeData startData;
        [SerializeField]
        private List<string> _supportType;
        [SerializeField]
        private float _capacity;
        [SerializeField]
        private float triggerRange = 0.5f;
        private Vector3 startPos;
        private ChargeData chargeData;
        public ChargeEvent onLoad { get; set; }
        public ChargeEvent onCharge { get; set; }

        public bool Used { get; set; }
        public GameObject Body
        {
            get
            {
                return gameObject;
            }
        }
        public bool charged { get { return chargeData.type != null && chargeData.value > 0; } }
        public ChargeData data { get { return (ChargeData)chargeData; } }
        public float capacity { get { return _capacity; } }

        public bool Active { get; private set; }
        public float Range { get { return triggerRange; } }

        public bool IsRuntimeCreated { get; set; }

        protected override string LayerName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool OperateAble
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private ElementController elementCtrl;

        protected override void Awake()
        {
            base.Awake();
            elementCtrl = ElementController.Instence;
            elementCtrl.RegistElement(this);
        }
        protected override void Start()
        {
            base.Start();
            startPos = transform.localPosition;
            LoadData(transform.position, startData, null);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (elementCtrl != null)
                elementCtrl.RemoveElement(this);
        }
        protected override void OnPickDown()
        {
            transform.localPosition = startPos;
        }
        protected override void OnPickUp()
        {
        }
        protected override void OnPickStay()
        {
        }
        protected override void OnSetPosition(Vector3 arg0)
        {
            base.OnSetPosition(arg0);
            transform.position = arg0;
        }
        internal bool CanLoad(string type)
        {
            return _supportType.Contains(type);
        }
        /// <summary>
        /// 吸入
        /// </summary>
        /// <param name="chargeResource"></param>
        internal void LoadData(Vector3 center, ChargeData data, UnityAction onComplete)
        {
            chargeData = data;

            if (onLoad != null)
            {
                onLoad.Invoke(center, data, onComplete);
            }
            else
            {
                if (onComplete != null)
                    onComplete.Invoke();
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        internal void OnCharge(Vector3 center, float value, UnityAction onComplete)
        {
            var left = data.value - value;
            if (onCharge != null)
            {
                var d = new ChargeData(data.type, value);
                onCharge.Invoke(center, d, onComplete);
            }
            else
            {
                if (onComplete != null) onComplete.Invoke();
            }
            chargeData.value = left;
        }
        public override void StepActive()
        {
            base.StepActive();
            PickUpItem.PickUpAble = true;
            Active = true;
        }

        public override void StepComplete()
        {
            base.StepComplete();
            PickUpItem.PickUpAble = false;
            Active = true;
        }

        public override void StepUnDo()
        {
            base.StepUnDo();
            PickUpItem.PickUpAble = false;
            if(!string.IsNullOrEmpty(chargeData.type)) OnCharge(transform.position, chargeData.value, null);
            LoadData(transform.position, startData, null);
            Active = false;
        }
        public override void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public override void AutoExecute()
        {
            throw new NotImplementedException();
        }
    }
}