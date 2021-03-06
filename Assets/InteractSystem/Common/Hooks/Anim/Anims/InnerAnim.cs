﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System;
using InteractSystem.Graph;

namespace InteractSystem.Hooks
{
    public class InnerAnim : AnimPlayer
    {
        public Animation anim;
        public string animName;
        private AnimationState state;
        private float animTime;
        private Coroutine coroutine;

        protected override void Awake()
        {
            base.Awake();
            if (anim == null)
                anim = GetComponentInChildren<Animation>();

            if (string.IsNullOrEmpty(animName))
                animName = anim.clip.name;
        }

        protected void Init()
        {
            gameObject.SetActive(true);
            anim.playAutomatically = false;
            anim.wrapMode = WrapMode.Once;
            RegisterEvent();
        }

        protected void RegisterEvent()
        {
            state = anim[animName];
            animTime = state.length;
            anim.cullingType = AnimationCullingType.AlwaysAnimate;
            anim.clip = anim.GetClip(animName);
        }


        IEnumerator DelyStop()
        {
            float waitTime = animTime / Mathf.Abs(state.speed);
            yield return new WaitForSeconds(waitTime);
            OnAnimComplete();
        }

        private void SetCurrentAnim(float time)
        {
            anim.clip = anim.GetClip(animName);
            state = anim[animName];
            state.normalizedTime = time;
            state.speed = 0;
            anim.Play();
        }
        protected override void OnSetActive(UnityEngine.Object target)
        {
            base.OnSetActive(target);
            Init();
        }
        protected override void OnPlayAnim(UnityEngine.Object arg0)
        {
            base.OnPlayAnim(arg0);
            state.normalizedTime = reverse ? 1 : 0f;
            state.speed = reverse ? -duration : duration;
            anim.Play();
            if (coroutine == null)
                coroutine = StartCoroutine(DelyStop());
        }

        protected override void OnSetInActive(UnityEngine.Object target)
        {
            base.OnSetInActive(target);
            if (log) Debug.Log("StepComplete:" + this);
            SetCurrentAnim(reverse ? 0 : 1);
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }

        public override void UnDoChanges(UnityEngine.Object target)
        {
            base.UnDoChanges(target);
            SetCurrentAnim(reverse ? 1 : 0);
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }

        //public override void SetPosition(Vector3 pos)
        //{
        //    transform.position = pos;
        //}

        public override void SetVisible(bool visible)
        {
            if (anim != null)
            {
                anim.gameObject.SetActive(visible);
            }
        }
    }
}