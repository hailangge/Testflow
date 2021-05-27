﻿using System;
using Newtonsoft.Json;
using Testflow.CoreCommon;
using Testflow.CoreCommon.Data;
using Testflow.Data;
using Testflow.Data.Sequence;
using Testflow.Runtime;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Coroutine;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Actuators
{
    internal abstract class ActuatorBase
    {
        public static ActuatorBase GetActuator(ISequenceStep stepData, SlaveContext context, int sequenceIndex)
        {
            if (stepData?.Function == null)
            {
                return new EmptyActuator(context, stepData, sequenceIndex);
            }
            switch (stepData.Function.Type)
            {
                case FunctionType.Constructor:
                case FunctionType.StructConstructor:
                case FunctionType.InstanceFunction:
                case FunctionType.StaticFunction:
                    return new FunctionActuator(stepData, context, sequenceIndex);
                    break;
                case FunctionType.Assertion:
                    return new AssertActuator(stepData, context, sequenceIndex);
                    break;
                case FunctionType.CallBack:
                    return new CallBackActuator(stepData, context, sequenceIndex);
                    break;
                case FunctionType.StaticPropertySetter:
                case FunctionType.InstancePropertySetter:
                    return new PropertySetterActuator(stepData, context, sequenceIndex);
                case FunctionType.StaticFieldSetter:
                case FunctionType.InstanceFieldSetter:
                    return new FieldSetterActuator(stepData, context, sequenceIndex);
                    break;
                default:
                    throw new InvalidOperationException();
                    break;
            }
        }

        protected ActuatorBase(ISequenceStep step, SlaveContext context, int sequenceIndex)
        {
            this.Context = context;
            this.Function = step?.Function;
            this.StepData = step;
            this.SequenceIndex = sequenceIndex;
            this.Return = null;
            this.ExecutionTime = DateTime.MaxValue;
            this.ExecutionTicks = -1;
        }

        protected SlaveContext Context { get; }

        protected IFunctionData Function { get; }

        protected int SequenceIndex { get; }

        protected ISequenceStep StepData { get; }

        public object Return { get; protected set; }

        public DateTime ExecutionTime { get; protected set; }
        
        public long ExecutionTicks { get; protected set; }

        /// <summary>
        /// 当前运行所在运行器的协程。
        /// </summary>
        protected CoroutineHandle Coroutine { get; private set; }

        #region 测试生成相关接口

        /// <summary>
        /// 生成调用信息
        /// </summary>
        protected abstract void GenerateInvokeInfo();

        /// <summary>
        /// 初始化常数参数值
        /// </summary>
        protected abstract void InitializeParamsValues();

        /// <summary>
        /// 序列参数公共检查方法
        /// </summary>
        /// <param name="instanceVar"></param>
        protected void CommonStepDataCheck(string instanceVar)
        {
            // 判断实例方法的实例是否配置
            if (StepData?.Function != null)
            {
                FunctionType functionType = StepData.Function.Type;
                if (string.IsNullOrWhiteSpace(instanceVar) && (functionType == FunctionType.InstanceFunction ||
                                                               functionType == FunctionType.InstancePropertySetter ||
                                                               functionType == FunctionType.InstanceFieldSetter))
                {
                    string currentStack = CallStack.GetStack(Context.SessionId, StepData).ToString();
                    Context.LogSession.Print(LogLevel.Error, Context.SessionId,
                        $"The instance variable of step <{currentStack}> is empty.");
                    throw new TestflowDataException(ModuleErrorCode.SequenceDataError,
                        Context.I18N.GetStr("InvalidParamVar"));
                }
            }
        }

        /// <summary>
        /// 生成运行器
        /// </summary>
        public void Generate(CoroutineHandle coroutine)
        {
            this.Coroutine = coroutine;
            this.GenerateInvokeInfo();
            this.InitializeParamsValues();
        }

        #endregion


        #region 运行相关接口

        /// <summary>
        /// 开启运行计时
        /// </summary>
        public void StartTiming()
        {
            // 如果当前时间不为最小值则判定已经开始计时，保持原计时时间
            if (DateTime.MinValue != this.ExecutionTime)
            {
                return;
            }
            this.ExecutionTicks = -1;
            this.ExecutionTime = DateTime.Now;
            Coroutine.StartTiming();
        }

        /// <summary>
        /// 停止运行计时。为了保证性能，该方法还会在外部捕获异常的代码里执行
        /// </summary>
        public void EndTiming()
        {
            long ticks = Coroutine.EndTiming();
            if (this.ExecutionTicks <= -1)
            {
                this.ExecutionTicks = ticks;
            }
        }

        /// <summary>
        /// 重置计时，用于在未真正执行前重置运行时间，以防止出现非法的时间记录
        /// </summary>
        public void ResetTiming()
        {
            this.ExecutionTicks = -1;
            this.ExecutionTime = DateTime.MinValue;
        }

        /// <summary>
        /// 调用序列的执行代码
        /// </summary>
        public abstract StepResult InvokeStep(bool forceInvoke);

        /// <summary>
        /// 恢复当前步骤的执行
        /// </summary>
        public virtual StepResult ResumeInvoke(bool forceInvoke, StepResult resultBeforeResume)
        {
            // 仅执行一个操作的Actuator中ResumeInvoke不执行任何行为。执行多个操作的Actuator需要手动实现
            return resultBeforeResume;
        }

        protected ICallStack GetStack()
        {
            return CallStack.GetStack(Context.SessionId, StepData);
        }

        #endregion

        #region 执行后续处理

        /// <summary>
        /// 根据变量参数原始名称获取变量并检查是否需要将值写入日志
        /// </summary>
        protected void LogTraceVariable(string varString, object value)
        {
            string variableName = ModuleUtils.GetVariableNameFromParamValue(varString);
            IVariable variable = ModuleUtils.GetVaraibleByRawVarName(variableName, StepData);
            if (variable.LogRecordLevel == RecordLevel.Trace || variable.LogRecordLevel == RecordLevel.FullTrace)
            {
                LogTraceVariable(variable, value);
            }
        }

        /// <summary>
        /// 将指定变量的值写入日志
        /// </summary>
        protected void LogTraceVariable(IVariable variable, object value)
        {
            if (Context.LogSession.LogLevel > LogLevel.Info)
            {
                return;
            }
            const string variableLogFormat = "[Variable Trace] Name:{0}, Stack:{1}, Value: {2}.";
            string stackStr = GetStack().ToString();
            string varValueStr = Context.Convertor.SerializeToString(value);
            string printStr = string.Format(variableLogFormat, variable.Name, stackStr, varValueStr);
            Context.LogSession.Print(LogLevel.Info, Context.SessionId, printStr);
        }

        #endregion
    }
}