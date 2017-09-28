﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class RotateAnimController:ActionCtroller
    {
        private RotObj selectedObj;
        private RaycastHit hit;
        private Ray ray;
        private float distence = 10;

        public RotateAnimController(float distence,ActionCommand trigger):base(trigger)
        {
            this.distence = distence;
        }

        public override IEnumerator Update()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();

                if (TrySelectRotateObj())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        yield return TransformSelected(selectedObj);
                    }
                }
            }
        }
        private bool TrySelectRotateObj()
        {
            if (trigger.viewCamera == null) return false;

            ray = trigger.viewCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, distence, (1 << Setting.rotateItemLayer)))
            {
                selectedObj = hit.collider.GetComponent<RotObj>();
            }

            return selectedObj != null;
        }

        IEnumerator TransformSelected(RotObj selectedObj)
        {
            Vector3 originalTargetPosition = selectedObj.transform.position;
            Vector3 axis = selectedObj.Direction;
            Vector3 previousMousePosition = Vector3.zero;
            while (!Input.GetMouseButtonUp(0) && selectedObj.Started)
            {
                ray = trigger.viewCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePosition = GeometryUtil.LinePlaneIntersect(ray.origin, ray.direction, originalTargetPosition, axis);
                if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero && IsInCercle(mousePosition))
                {
                    var vec1 = previousMousePosition - selectedObj.transform.position;
                    var vec2 = mousePosition - selectedObj.transform.position;
                    float rotateAmount = (Vector3.Angle(Vector3.Cross(vec1, vec2), axis) < 180f ? 1 : -1)
                        * Vector3.Angle(vec1, vec2) * selectedObj.rotSpeed;
                    selectedObj.Rotate(rotateAmount);
                }

                previousMousePosition = mousePosition;

                yield return null;
            }

           yield return selectedObj.Clamp();

            if (selectedObj.TryMarchRot())
            {
                selectedObj.OnEndExecute();
            }
        }
        private bool IsInCercle(Vector3 pos)
        {
            return Vector3.Distance(selectedObj.transform.position, pos) < selectedObj.triggerRadius;
        }
        public override void OnStartExecute(bool forceAuto)
        {
            base.OnStartExecute(forceAuto);
            if (forceAuto)
            {
                OnEndExecute();//直接结束??
                trigger.Complete();
            }
        }
    }

}