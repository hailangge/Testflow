﻿using System.Collections.Generic;
using System.Text;
using Testflow.CoreCommon.Data;
using Testflow.Runtime;
using Testflow.SlaveCore.Runner.Model;
using Testflow.Usr;

namespace Testflow.SlaveCore.Data
{
    /// <summary>
    /// 当前执行信息的缓存
    /// </summary>
    internal class ExecutionInfo : ICloneableClass<ExecutionInfo>
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public int Session { get; }

        /// <summary>
        /// 当前序列号
        /// </summary>
        public int Sequence { get; private set; }

        /// <summary>
        /// 协程ID
        /// </summary>
        public int CoroutineId { get; }

        /// <summary>
        /// 当前执行目标
        /// </summary>
        public TargetOperation Operation { get; private set; }

        /// <summary>
        /// 当前的执行步骤实体对象
        /// </summary>
        public StepTaskEntityBase StepEntity { get; private set; }

        /// <summary>
        /// 执行目标名称
        /// </summary>
        public string TargetName { get; private set; }

        /// <summary>
        /// 执行参数
        /// </summary>
        public List<string> Arguments { get; }

        public ExecutionInfo(int session, int coroutineId)
        {
            this.Session = session;
            this.CoroutineId = coroutineId;
            this.Arguments = new List<string>(5);
            Reset();
        }

        public void Reset()
        {
            this.Operation = TargetOperation.None;
            this.StepEntity = null;
            this.TargetName = string.Empty;
            this.Sequence = int.MinValue;
            this.Arguments.Clear();
        }

        public void Reset(ExecutionInfo executionInfo)
        {
            this.SetTarget(executionInfo.Operation, executionInfo.TargetName, executionInfo.Arguments.ToArray());
        }

        public void SequenceStart(int sequence)
        {
            this.Sequence = sequence;
        }

        public void StepStart(StepTaskEntityBase taskEntity)
        {
            this.StepEntity = taskEntity;
            this.Operation = TargetOperation.None;
            this.TargetName = string.Empty;
            if (Arguments.Count > 0)
            {
                this.Arguments.Clear();
            }
        }

        public void SetTarget(TargetOperation target, string targetName, params string[] arguments)
        {
            this.Operation = target;
            this.TargetName = targetName;
            if (Arguments.Count > 0)
            {
                this.Arguments.Clear();
            }
            if (arguments.Length > 0)
            {
                this.Arguments.AddRange(arguments);
            }
        }

        public void StepOver(StepTaskEntityBase taskEntity)
        {
            this.StepEntity = taskEntity;
            this.Operation = TargetOperation.Over;
            this.TargetName = string.Empty;
            if (Arguments.Count > 0)
            {
                this.Arguments.Clear();
            }
        }

        public void SequenceOver(int sequenceIndex)
        {
            this.Sequence = sequenceIndex;
            this.StepEntity = null;
            this.Operation = TargetOperation.Over;
            this.TargetName = string.Empty;
            if (Arguments.Count > 0)
            {
                this.Arguments.Clear();
            }
        }

        public CallStack GetCurrentStack()
        {
            return StepEntity?.GetStack() ?? CallStack.GetEmptyStack(Session, Sequence);
        }

        public ExecutionInfo Clone()
        {
            ExecutionInfo executionInfo = new ExecutionInfo(Session, CoroutineId)
            {
                Sequence = this.Sequence,
                StepEntity = this.StepEntity,
            };
            executionInfo.SetTarget(this.Operation, TargetName, this.Arguments.ToArray());
            return executionInfo;
        }

        public override string ToString()
        {
            const char delim = ' ';
            StringBuilder dataCache = new StringBuilder(100);
            if (null != StepEntity)
            {
                dataCache.Append("Step:").Append(StepEntity.GetStack()).Append(delim);
            }
            else if (Sequence != int.MinValue)
            {
                dataCache.Append("Sequence:").Append(Session).Append('_').Append(Sequence).Append(delim);
            }
            else
            {
                dataCache.Append("Session:").Append(Sequence).Append(delim);
            }
            dataCache.Append("Coroutine:").Append(CoroutineId).Append(delim).Append("Operation:").Append(Operation)
                .Append(delim);
            if (!string.IsNullOrWhiteSpace(TargetName))
            {
                dataCache.Append("Target:").Append(TargetName);
                foreach (string argument in Arguments)
                {
                    dataCache.Append('_').Append(argument);
                }
            }
            return dataCache.ToString();
        }
    }
}