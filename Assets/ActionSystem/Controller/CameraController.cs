﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    internal class CameraController : MonoBehaviour
    {
        private static List<CameraNode> cameraNodes = new List<CameraNode>();
        public static Camera viewCamera { get; set; }
        private static Camera mainCamera { get; set; }
        private static CameraNode currentNode;
        private static Camera activeCamera { get; set; }
        private static Transform viewCameraParent;
        private static Coroutine coroutine;
        public static CameraController Instence { get; private set; }
        private static UnityAction onComplete;
        private static ViewCamera _cameraView;
        private static ViewCamera cameraView
        {
            get
            {
                if (_cameraView == null)
                {
                    _cameraView = viewCamera.GetComponent<ViewCamera>();
                }
                return _cameraView;
            }
        }
        public void Awake()
        {
            Instence = this;
            viewCameraParent = transform;
            if (viewCamera == null)
            {
                viewCamera = GetComponentInChildren<Camera>(true);
            }
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                SetTransform(mainCamera.transform);
            }
            else
            {
                Debug.LogError("场景没有主摄像机");
            }
            viewCamera.gameObject.SetActive(mainCamera == null);
        }
        public static void RegistNode(CameraNode node)
        {
            if (!cameraNodes.Contains(node))
            {
                cameraNodes.Add(node);
            }
        }
        public static void SetViewCamera(UnityAction onComplete, string id = null)
        {
            StopCoroutine();
            CameraController.onComplete = onComplete;
            var node = cameraNodes.Find(x => x != null && x.ID == id);
            if (node != currentNode || node == null)
            {
                if (node == null)//移动到主摄像机
                {
                    coroutine = Instence.StartCoroutine(MoveCameraToMainCamera());
                }
                else //移动到新坐标
                {
                    coroutine = Instence.StartCoroutine(MoveCameraToNode(node));
                }
                currentNode = node;
            }
            else
            {
                OnStepComplete();
            }
        }

        internal static Camera GetViewCamera(string cameraID)
        {
            if (!Setting.useOperateCamera)
            {
                return mainCamera;
            }
            else if (string.IsNullOrEmpty(cameraID))
            {
                return mainCamera;
            }
            else if (cameraNodes.Find(x => x.ID == cameraID))
            {
                return viewCamera;
            }
            else
            {
                return mainCamera;
            }
        }

        static IEnumerator MoveCameraToMainCamera()
        {
            if (mainCamera != null)
            {
                var startPos = viewCamera.transform.position;
                var startRot = viewCamera.transform.rotation;
                for (float i = 0; i < 1; i += Time.deltaTime)
                {
                    viewCamera.transform.position = Vector3.Lerp(startPos, mainCamera.transform.position, i);
                    viewCamera.transform.rotation = Quaternion.Lerp(startRot, mainCamera.transform.rotation, i);
                    yield return null;
                }

                SetTransform(mainCamera.transform);

                viewCamera.gameObject.SetActive(false);
                mainCamera.gameObject.SetActive(true);
                viewCamera.transform.SetParent(viewCameraParent);
            }
            OnStepComplete();//
        }
        static IEnumerator MoveCameraToNode(CameraNode target)
        {
            if (mainCamera != null)
            {
                viewCamera.gameObject.SetActive(true);
                mainCamera.gameObject.SetActive(false);
            }
            cameraView.enabled = false;

            var startPos = viewCamera.transform.position;
            var startRot = viewCamera.transform.rotation;
            for (float i = 0; i < target.LerpTime; i += Time.deltaTime)
            {
                viewCamera.transform.position = Vector3.Lerp(startPos, target.transform.position, i);
                viewCamera.transform.rotation = Quaternion.Lerp(startRot, target.transform.rotation, i);
                yield return null;
            }

            viewCamera.transform.SetParent(target.transform);
            SetCameraInfo(target);
            OnStepComplete();
        }
        private static void SetTransform(Transform target)
        {
            cameraView.SetSelf(target.transform.position, target.transform.rotation);
        }
        private static void SetCameraInfo(CameraNode target)
        {
            if (target.MoveAble)
            {
                cameraView.enabled = true;
                cameraView.distence = target.Distence;
                cameraView.SetSelf(target.transform.position, target.Rotation);
            }
            cameraView.targetView = target.CameraField;
        }
        private static void StopCoroutine()
        {
            if (coroutine != null)
            {
                Instence.StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        private void OnDestroy()
        {
            onComplete = null;
            cameraNodes.Clear();
        }
        private static void OnStepComplete()
        {
            if (onComplete != null)
            {
                //Debug.Log("Camera CallBack " + DateTime.Now.ToString("mm:ss"));
                onComplete.Invoke();
                onComplete = null;
            }
        }
    }

}