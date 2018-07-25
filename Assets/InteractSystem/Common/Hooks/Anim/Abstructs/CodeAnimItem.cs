﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace InteractSystem.Hooks
{
    public abstract class CodeAnimItem : AnimPlayer
    {
        [SerializeField]
        protected float time = 2f;
        protected Coroutine coroutine;
        [SerializeField]
        protected AnimationCurve animCurve;
    
        protected override void Start()
        {
            base.Start();
            InitState();
        }

        protected abstract void InitState();

        protected override void OnSetActive(UnityEngine.Object target)
        {
            base.OnSetActive(target);
            time = 1f / duration;
            coroutine = StartCoroutine(PlayAnim(onAutoPlayEnd));
        }

        protected override void OnSetInActive(UnityEngine.Object target)
        {
            base.OnSetInActive(target);
            StopAnim();
        }
        public override void UnDoChanges(UnityEngine.Object target)
        {
            base.UnDoChanges(target);
            StopAnim();
        }

        protected virtual void StopAnim() {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        protected float GetAnimValue(float value)
        {
            return animCurve.Evaluate(value);
        }
        protected abstract IEnumerator PlayAnim(UnityAction onComplete);
    }
}
