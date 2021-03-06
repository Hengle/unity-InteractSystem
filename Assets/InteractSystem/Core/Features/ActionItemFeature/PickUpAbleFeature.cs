﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace InteractSystem
{
    [Serializable]
    public class PickUpAbleFeature: ClickAbleFeature{
        private PickUpAbleComponent _pickUpAbleItem;
        private PickUpAbleComponent pickUpAbleItem
        {
            get
            {
                if (_pickUpAbleItem == null)
                {
                    _pickUpAbleItem = collider.gameObject.GetComponent<PickUpAbleComponent>();
                    if (_pickUpAbleItem == null)
                    {
                        _pickUpAbleItem = collider.gameObject.AddComponent<PickUpAbleComponent>();
                        _pickUpAbleItem.PickUpAble = interactAble;
                        _pickUpAbleItem.onPickUp = new UnityEvent();
                        _pickUpAbleItem.onPickDown = new UnityEvent();
                        _pickUpAbleItem.onPickStay = new UnityEvent();
                    }
                }
                return _pickUpAbleItem;
            }
        }
        
        public override string LayerName
        {
            get
            {
                _layerName = PickUpAbleItem.layer;
                return _layerName;
            }

            set
            {
                _layerName = value;
            }
        }
		
        public void OnPickDown()
        {
            pickUpAbleItem.OnPickDown();
        }
        public void OnPickUp()
        {

            pickUpAbleItem.OnPickUp();
        }
        public void OnPickStay()
        {
            pickUpAbleItem.OnPickStay();
        }

        public void RegistOnPickDown(UnityAction onPickDown)
        {
            pickUpAbleItem.onPickDown.AddListener(onPickDown);
        }
        public void RegistOnPickStay(UnityAction onPickStay)
        {
            pickUpAbleItem.onPickStay.AddListener(onPickStay);
        }
        public void RegistOnPickUp(UnityAction onPickUp)
        {
            pickUpAbleItem.onPickUp.AddListener(onPickUp);
        }
        public void RemoveOnPickDown(UnityAction onPickDown)
        {
            pickUpAbleItem.onPickDown.RemoveListener(onPickDown);
        }
        public void RemoveOnPickStay(UnityAction onPickStay)
        {
            pickUpAbleItem.onPickStay.RemoveListener(onPickStay);
        }
        public void RemoveOnPickUp(UnityAction onPickUp)
        {
            pickUpAbleItem.onPickUp.RemoveListener(onPickUp);
        }

        public void RegistOnSetPosition(UnityAction<Vector3> onSetPosition)
        {
            pickUpAbleItem.onSetPosition += onSetPosition;
        }
        public void RegistOnSetViweForward(UnityAction<Vector3> onSetViewForward)
        {
            pickUpAbleItem.onSetViewForward += onSetViewForward;
        }
        public void RemoveOnSetPosition(UnityAction<Vector3> onSetPosition)
        {
            pickUpAbleItem.onSetPosition -= onSetPosition;
        }
        public void RemoveOnSetViweForward(UnityAction<Vector3> onSetViewForward)
        {
            pickUpAbleItem.onSetViewForward -= onSetViewForward;
        }

        public override void OnSetActive(UnityEngine.Object target)
        {
            base.OnSetActive(target);
            if (interactAble)
            {
                PickUpAble = true;
            }
        }

        public override void OnSetInActive(UnityEngine.Object target)
        {
            base.OnSetInActive(target);
            if (interactAble)
            {
                PickUpAble = false;
            }
        }

        public override void OnUnDo(UnityEngine.Object target)
        {
            base.OnUnDo(target);
            if (interactAble)
                PickUpAble = true;
        }
		
        public bool PickUpAble
        {
            get
            {
                return interactAble && pickUpAbleItem.PickUpAble;
            }
            set
            {
                pickUpAbleItem.PickUpAble = value;
            }
        }
    }

}