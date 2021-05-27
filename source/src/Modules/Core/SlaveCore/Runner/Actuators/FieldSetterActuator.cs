using System;
using System.Collections.Generic;
using System.Reflection;
using Testflow.CoreCommon.Common;
using Testflow.Data;
using Testflow.Data.Sequence;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Data;
using Testflow.SlaveCore.Runner.Expression;

namespace Testflow.SlaveCore.Runner.Actuators
{
    internal class FieldSetterActuator : ActuatorBase
    {
        public FieldSetterActuator(ISequenceStep step, SlaveContext context, int sequenceIndex) : base(step, context, sequenceIndex)
        {
            _fields = new List<FieldInfo>(step.Function.Parameters.Count);
            _params = new List<object>(step.Function.Parameters.Count);
            this._fieldIndex = -1;
        }

        protected override void GenerateInvokeInfo()
        {
            BindingFlags bindingFlags = BindingFlags.Public;
            bindingFlags |= (Function.Type == FunctionType.InstanceFieldSetter)
                ? BindingFlags.Instance
                : BindingFlags.Static;
            IArgumentCollection arguments = Function.ParameterType;
            for (int i = 0; i < arguments.Count; i++)
            {
                Context.CoroutineManager.TestGenerationTrace.SetTarget(TargetOperation.FunctionGeneration, 
                    arguments[i].Name);

                if (Function.Parameters[i].ParameterType == ParameterType.NotAvailable)
                {
                    _fields.Add(null);
                    continue;
                }
                string fieldName = arguments[i].Name;
                Type classType = Context.TypeInvoker.GetType(Function.ClassType);
                _fields.Add(classType.GetField(fieldName, bindingFlags));
            }
        }

        protected override void InitializeParamsValues()
        {
            string instanceVarName = null;
            if (!string.IsNullOrWhiteSpace(Function.Instance))
            {
                instanceVarName = ModuleUtils.GetVariableNameFromParamValue(Function.Instance);
                _instanceVar = ModuleUtils.GetVariableFullName(instanceVarName, StepData, Context.SessionId);
            }
            IParameterDataCollection parameters = Function.Parameters;
            for (int i = 0; i < _fields.Count; i++)
            {
                string paramValue = parameters[i].Value;
                if (null == _fields[i])
                {
                    _params.Add(null);
                    continue;
                }
                Context.CoroutineManager.TestGenerationTrace.SetTarget(TargetOperation.ArgumentInitialization,
                    Function.ParameterType[i].Name);
                IArgument argument = Function.ParameterType[i];

                switch (parameters[i].ParameterType)
                {
                    case ParameterType.NotAvailable:
                        _params.Add(null);
                        break;
                    case ParameterType.Value:
                        _params.Add(Context.TypeInvoker.IsSimpleType(argument.Type)
                            ? Context.TypeInvoker.CastConstantValue(argument.Type, paramValue)
                            : null);
                        break;
                    case ParameterType.Variable:
                        string variableRawName = ModuleUtils.GetVariableNameFromParamValue(paramValue);
                        string varFullName = ModuleUtils.GetVariableFullName(variableRawName, StepData,
                            Context.SessionId);
                        // 将parameter的Value中，变量名称替换为运行时变量名
                        parameters[i].Value = ModuleUtils.GetFullParameterVariableName(varFullName, paramValue);
                        _params.Add(null);
                        break;
                    case ParameterType.Expression:
                        ExpressionProcessor expProcessor = Coroutine.ExpressionProcessor;
                        int expIndex = expProcessor.CompileExpression(paramValue, StepData);
                        // 在参数数据中写入表达式索引
                        parameters[i].Value = expIndex.ToString();
                        _params.Add(null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            CommonStepDataCheck(instanceVarName);
        }

        private readonly List<FieldInfo> _fields;

        private readonly List<object> _params;

        private string _instanceVar;
        private int _fieldIndex;

        public override StepResult InvokeStep(bool forceInvoke)
        {
            this._fieldIndex = -1;
            return SetFieldFromFieldIndex(forceInvoke);
        }

        public override StepResult ResumeInvoke(bool forceInvoke, StepResult resultBeforeResume)
        {
            StepResult stepResult = SetFieldFromFieldIndex(forceInvoke);
            return ModuleUtils.GetMergedStepResult(resultBeforeResume, stepResult);
        }

        private StepResult SetFieldFromFieldIndex(bool forceInvoke)
        {
            object instance = null;
            if (Function.Type == FunctionType.InstanceFieldSetter)
            {
                instance = Context.VariableMapper.GetParamValue(this._instanceVar, Function.Instance,
                    Function.ClassType);
            }

            IParameterDataCollection parameters = Function.Parameters;
            IArgumentCollection arguments = Function.ParameterType;
            // 开始计时
            StartTiming();

            int maxFieldIndex = this._fields.Count - 1;
            while (this._fieldIndex < maxFieldIndex && (forceInvoke || !Context.Cancellation.IsCancellationRequested))
            {
                this._fieldIndex++;
                if (null == this._fields[this._fieldIndex])
                {
                    continue;
                }

                if (parameters[this._fieldIndex].ParameterType == ParameterType.Variable)
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.ArgumentCalculation, arguments[this._fieldIndex].Name);
                    // 获取变量值的名称，该名称为变量的运行时名称，其值在InitializeParamValue方法里配置
                    string variableName = ModuleUtils.GetVariableNameFromParamValue(parameters[this._fieldIndex].Value);
                    // 根据ParamString和变量对应的值配置参数。
                    this._params[this._fieldIndex] = Context.VariableMapper.GetParamValue(variableName,
                        parameters[this._fieldIndex].Value,
                        arguments[this._fieldIndex].Type);
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._fieldIndex].Name);
                    this._fields[this._fieldIndex].SetValue(instance, this._params[this._fieldIndex]);
                }
                else if (parameters[this._fieldIndex].ParameterType == ParameterType.Expression)
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.ArgumentCalculation, arguments[this._fieldIndex].Name);
                    int expIndex = int.Parse(parameters[this._fieldIndex].Value);
                    ExpressionProcessor expProcessor = Coroutine.ExpressionProcessor;
                    this._params[this._fieldIndex] = expProcessor.Calculate(expIndex, arguments[this._fieldIndex].Type);
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._fieldIndex].Name);
                    this._fields[this._fieldIndex].SetValue(instance, this._params[this._fieldIndex]);
                }
                // 如果参数类型为value且参数值为null且参数配置的字符不为空且参数类型是类或结构体，则需要实时计算该属性或字段的值
                else if (parameters[this._fieldIndex].ParameterType == ParameterType.Value &&
                         null == this._params[this._fieldIndex] &&
                         !string.IsNullOrEmpty(parameters[this._fieldIndex].Value) &&
                         !Context.TypeInvoker.IsSimpleType(this._fields[this._fieldIndex].FieldType))
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._fieldIndex].Name);
                    object originalValue = this._fields[this._fieldIndex].GetValue(instance);
                    this._params[this._fieldIndex] = Context.TypeInvoker.CastConstantValue(this._fields[this._fieldIndex].FieldType,
                        parameters[this._fieldIndex].Value,
                        originalValue);
                    // 如果原始值为空，则需要配置Value，否则其参数都已经写入，无需外部更新
                    if (null == originalValue)
                    {
                        this._fields[this._fieldIndex].SetValue(instance, this._params[this._fieldIndex]);
                    }
                }
                else
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._fieldIndex].Name);
                    this._fields[this._fieldIndex].SetValue(instance, this._params[this._fieldIndex]);
                }
            }

            // 停止计时
            EndTiming();
            if (CoreUtils.IsValidVaraible(Function.Instance) && Function.Type == FunctionType.InstanceFieldSetter)
            {
                LogTraceVariable(Function.Instance, instance);
            }
            return StepResult.Pass;
        }
    }
}