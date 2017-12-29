﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class CommandController
    {
        public bool CommandRegisted { get; private set; }
        public List<IActionCommand> CommandList { get { return commandList; }}

        private List<IActionCommand> commandList = new List<IActionCommand>();
        private Dictionary<string, List<ActionCommand>> actionDic = new Dictionary<string, List<ActionCommand>>();//触发器
        private Dictionary<string, SequencesCommand> seqDic = new Dictionary<string, SequencesCommand>();
        private int totalCommand;
        private StepComplete onStepComplete;
        private CommandExecute commandExecute;
        private RegistCommandList onAllCommandRegisted;
        private UserError onUserError;
        internal void InitCommand(int totalCommand, CommandExecute onCommandRegistComplete, StepComplete onStepComplete,UserError onUserError, RegistCommandList onAllCommandRegisted)
        {
            this.totalCommand = totalCommand;
            this.onStepComplete = onStepComplete;
            this.onUserError = onUserError;
            this.commandExecute = onCommandRegistComplete;
            this.onAllCommandRegisted = onAllCommandRegisted;
            TryComplelteRegist();
        }
        public void RegistCommand(ActionCommand command)
        {
            if (actionDic.ContainsKey(command.StepName))
            {
                actionDic[command.StepName].Add(command);
            }
            else
            {
                actionDic[command.StepName] = new List<ActionCommand>() { command };
            }
            TryComplelteRegist();
        }

        private void TryComplelteRegist()
        {
            if (totalCommand <= commandList.Count)
            {
                RegistTriggerCommand();
                if(onAllCommandRegisted != null){
                    onAllCommandRegisted.Invoke(commandList);
                }
                CommandRegisted = true;
            }
        }

        private void OnOneCommandComplete(string stepName)
        {
            if (seqDic.ContainsKey(stepName))
            {
                var cmd = seqDic[stepName];
                if (!cmd.ContinueExecute())
                {
                    onStepComplete(stepName);
                }
            }
            else
            {
                onStepComplete(stepName);
            }
        }


        private void RegistTriggerCommand()
        {
            if (actionDic != null)
            {
                foreach (var item in actionDic)
                {
                    var stepName = item.Key;
                    if (item.Value.Count > 1)//多命令
                    {
                        item.Value.Sort();
                        var list = new List<IActionCommand>();
                        var total = item.Value.Count;
                        for (int i = 0; i < item.Value.Count; i++)
                        {
                            int index = i;
                            int totalcmd = total;
                            item.Value[index].RegistComplete(OnOneCommandComplete);
                            item.Value[index].RegistAsOperate(onUserError);
                            item.Value[index].onBeforeActive.AddListener((x) =>
                            {
                                OnCommandStartExecute(stepName, totalcmd, index);
                            });
                            list.Add(item.Value[index]);
                        }
                        var cmd = new SequencesCommand(stepName, list);
                        seqDic.Add(stepName, cmd);
                        commandList.Add(cmd);
                    }
                    else//单命令
                    {
                        var cmd = item.Value[0];
                        cmd.RegistComplete(OnOneCommandComplete);
                        cmd.RegistAsOperate(onUserError);
                        cmd.onBeforeActive.AddListener((x) =>
                        {
                            OnCommandStartExecute(stepName, 1, 1);
                        });
                        commandList.Add(cmd);
                    }

                }
            }
        }


        private void OnCommandStartExecute(string stepName, int totalCount, int currentID)
        {
            if (this.commandExecute != null)
            {
                commandExecute.Invoke(stepName, totalCount, currentID);
            }
        }
    }

}
