﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class InstallCtrl
    {
        private InstallTrigger trigger;
        public StepComplete onStepComplete;
        private ElementGroup elementGroup;
        public bool Active { get; private set; }
        IHighLightItems highLight;
        private InstallItem pickedUpObj;
        private bool pickedUp;
        private InstallObj installPos;
        private Ray ray;
        private RaycastHit hit;
        private RaycastHit[] hits;
        private bool installAble;
        private string resonwhy;
        private float distence { get { return trigger.distence; } set { trigger.distence = value; } }
        private string currStepName { get { return trigger.StepName; } }
        private List<InstallObj> installObjs { get { return trigger.InstallObjs; } }
        public UserError InstallErr;
        private Coroutine coroutine;

        public InstallCtrl(InstallTrigger trigger)
        {
            this.trigger = trigger;
            this.elementGroup = trigger.ElementGroup();
            highLight = new ShaderHighLight();
            highLight.SetState(trigger.highLight);
        }

        #region 鼠标操作事件
        IEnumerator Update()
        {
            elementGroup.onInstall += OnEndInstall;

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("OnLeftMouseClicked");
                    OnLeftMouseClicked();
                }
                else if (pickedUp)
                {
                    UpdateInstallState();
                    MoveWithMouse(distence += Input.GetAxis("Mouse ScrollWheel"));
                }
                yield return null;
            }
        }

        public void OnLeftMouseClicked()
        {
            if (!pickedUp)
            {
                SelectAnElement();
            }
            else
            {
                TryInstallObject();
            }
        }

        /// <summary>
        /// 在未屏幕锁的情况下选中一个没有元素
        /// </summary>
        void SelectAnElement()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, (1 << Setting.installObjLayer)))
            {
                pickedUpObj = hit.collider.GetComponent<InstallItem>();
                if (pickedUpObj != null && elementGroup.PickUpObject(pickedUpObj))
                {
                    pickedUp = true;

                    if (!PickUpedCanInstall())
                    {
                        if (highLight != null) highLight.HighLightTarget(pickedUpObj.Render, Color.yellow);
                    }
                    else
                    {
                        if (highLight != null) highLight.HighLightTarget(pickedUpObj.Render, Color.cyan);
                    }
                }
            }
        }

        private bool PickUpedCanInstall()
        {
            bool canInstall = false;
            List<InstallObj> poss = GetNotInstalledPosList();
            for (int i = 0; i < poss.Count; i++)
            {
                if (!HaveInstallObjInstalled(poss[i]) && IsInstallStep(poss[i]) && elementGroup.CanInstallToPos(poss[i]))
                {
                    canInstall = true;
                }
            }
            return canInstall;
        }


        public void UpdateInstallState()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, 100, (1 << Setting.installPosLayer));
            if (hits != null || hits.Length > 0)
            {
                bool hited = false;
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.name == pickedUpObj.name)
                    {
                        hited = true;
                        installPos = hits[i].collider.GetComponent<InstallObj>();
                        if (installPos == null)
                        {
                            Debug.LogError("零件未挂InstallObj脚本");
                        }
                        else if (!IsInstallStep(installPos))
                        {
                            installAble = false;
                            resonwhy = "当前安装步骤并非" + installPos.StepName;
                        }
                        else if (HaveInstallObjInstalled(installPos))
                        {
                            installAble = false;
                            resonwhy = "安装点已经安装了其他零件";
                        }
                        else if (!elementGroup.CanInstallToPos(installPos))
                        {
                            installAble = false;
                            resonwhy = "拿起零件和安装点不对应";
                        }
                        else
                        {
                            installAble = true;
                        }
                    }
                }
                if (!hited)
                {
                    installAble = false;
                    resonwhy = "不要乱放零件";
                }
            }

            if (installAble)
            {
                //可安装显示绿色
                if (highLight != null) highLight.HighLightTarget(pickedUpObj.Render, Color.green);
            }
            else
            {
                //不可安装红色
                if (highLight != null) highLight.HighLightTarget(pickedUpObj.Render, Color.red);
            }
        }

        /// <summary>
        /// 尝试安装元素
        /// </summary>
        void TryInstallObject()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (installAble)
            {
                elementGroup.InstallPickedUpObject(installPos);
            }
            else
            {
                OnInstallErr(resonwhy);
                elementGroup.PickDownPickedUpObject();
            }

            pickedUp = false;
            installAble = false;
            if (highLight != null) highLight.UnHighLightTarget(pickedUpObj.Render);
        }

        private Ray disRay;
        private RaycastHit disHit;
        private string obstacle = "Obstacle";

        /// <summary>
        /// 跟随鼠标
        /// </summary>
        void MoveWithMouse(float dis)
        {
            disRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(disRay, out disHit, dis,1<<Setting.obstacleLayer))
            {
                pickedUpObj.transform.position = disHit.point;
            }
            else
            {
                pickedUpObj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dis));
            }
        }
        #endregion

        private void OnEndInstall()
        {
            if (AllElementInstalled())
            {
                List<InstallObj> posList = GetInstalledPosList();
                elementGroup.SetCompleteNotify(posList);
                if (onStepComplete != null)
                    onStepComplete.Invoke(currStepName);
            }
        }

        /// <summary>
        /// 结束当前步骤安装
        /// </summary>
        /// <param name="stepName"></param>
        public void EndInstall()
        {
            List<InstallObj> posList = GetNotInstalledPosList();
            elementGroup.QuickInstallObjListObjects(posList);
            SetSepComplete();
        }

        public void SetStapActive()
        {
            SetObjsActive();
            List<InstallObj> posList = GetNotInstalledPosList();
            elementGroup.SetStartNotify(posList);
            if (coroutine == null) coroutine = trigger.StartCoroutine(Update());
        }

        /// <summary>
        /// 自动安装部分需要进行自动安装的零件
        /// </summary>
        /// <param name="stepName"></param>
        public void AutoInstallWhenNeed(bool forceAuto)
        {
            List<InstallObj> posList = null;
            if (forceAuto)
            {
                posList = GetNotInstalledPosList();
            }
            else
            {
                posList = GetNeedAutoInstallObjList();
            }

            if (posList != null) elementGroup.InstallObjListObjects(posList);

            pickedUp = false;
        }

        public void UnInstall(string stepName)
        {
            SetStapActive();
            List<InstallObj> posList = GetInstalledPosList();
            elementGroup.UnInstallObjListObjects(posList);
            SetSepUnDo();
        }

        public void QuickUnInstall()
        {
            SetStapActive();
            List<InstallObj> posList = GetInstalledPosList();
            elementGroup.QuickUnInstallObjListObjects(posList);
            SetSepUnDo();
        }

        private void OnInstallErr(string err)
        {
            if (InstallErr != null)
            {
                InstallErr.Invoke(currStepName, err);
            }
        }

        private List<InstallObj> GetInstalledPosList()
        {
            var list = installObjs.FindAll(x => x.Installed);
            return list;
        }
        private List<InstallObj> GetNotInstalledPosList()
        {
            var list = installObjs.FindAll(x => !x.Installed);
            return list;
        }
        private List<InstallObj> GetNeedAutoInstallObjList()
        {
            var list = installObjs.FindAll(x => x.autoInstall);
            return list;
        }
        private bool HaveInstallObjInstalled(InstallObj obj)
        {
            return obj.Installed;
        }
        private bool IsInstallStep(InstallObj obj)
        {
            return installObjs.Contains(obj);
        }
        private void SetSepComplete()
        {
            if (coroutine != null)
                trigger.StopCoroutine(coroutine);
            coroutine = null;
            elementGroup.onInstall -= OnEndInstall;
        }
        private bool AllElementInstalled()
        {
            var notInstalls = installObjs.FindAll(x => !x.Installed);
            return notInstalls.Count == 0;
        }
        private void SetObjsActive()
        {
            foreach (var item in installObjs)
            {
                item.StartExecute();
            }
        }
        private void SetSepUnDo()
        {
            foreach (var item in installObjs)
            {
                item.UnDoExecute();
            }
        }
    }

}