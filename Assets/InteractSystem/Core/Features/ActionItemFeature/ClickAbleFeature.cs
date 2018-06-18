﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace InteractSystem
{
    [System.Serializable]
    public class ClickAbleFeature : ActionItemFeature
    {
        [SerializeField, Attributes.DefultCollider]
        protected Collider _collider;

        public Collider collider
        {
            get
            {
                if (_collider == null)
                    collider = target.GetComponentInChildren<Collider>();
                return _collider;
            }
            protected set
            {
                _collider = value;
            }
        }

        public override void Awake()
        {
            base.Awake();
            InitLayer();
        }

        private void InitLayer()
        {
            collider.gameObject.layer = LayerMask.NameToLayer(LayerName);
            collider.enabled = false;
        }

        public string LayerName { get; set; }


        public override void StepActive()
        {
            base.StepActive();
            collider.enabled = true;
        }
        public override void StepUnDo()
        {
            base.StepUnDo();
            collider.enabled = false;
        }
        public override void StepComplete()
        {
            base.StepComplete();
            collider.enabled = false;
        }
    }
}