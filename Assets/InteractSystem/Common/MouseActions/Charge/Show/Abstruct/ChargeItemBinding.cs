﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace InteractSystem.Actions
{
    [RequireComponent(typeof(ChargeItem))]
    public abstract class ChargeItemBinding : ChargeBinding
    {
        protected ChargeItem target;
        protected virtual void Awake()
        {
            target = GetComponent<ChargeItem>();
            target.onCharge = OnCharge;
        }

        protected abstract void OnCharge(Vector3 center, ChargeData data, UnityAction onComplete);
    }
}