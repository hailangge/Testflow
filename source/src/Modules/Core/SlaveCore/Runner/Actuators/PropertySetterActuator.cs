using System;
using System.Collections.Generic;
using System.Reflection;
using Testflow.CoreCommon;
using Testflow.CoreCommon.Common;
using Testflow.Data;
using Testflow.Data.Sequence;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Data;
using Testflow.SlaveCore.Runner.Expression;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Actuators
{
    internal class PropertySetterActuator : ActuatorBase
    {
        public PropertySetterActuator(ISequenceStep step, SlaveContext context, int sequenceIndex) : base(step, context, sequenceIndex)
        {
            _properties = new List<PropertyInfo>(step.Function.Parameters.Count);
            _params = new List<object>(step.Function.Parameters.Count);
            this._propertyIndex = -1;
        }

        protected override void GenerateInvokeInfo()
        {
            BindingFlags bindingFlags = BindingFlags.Public;
            bindingFlags |= (Function.Type == FunctionType.InstancePropertySetter)
                ? BindingFlags.Instance
                : BindingFlags.Static;
            IArgumentCollection arguments = Function.ParameterType;
            IParameterDataCollection parameters = Function.Parameters;
            for (int i = 0; i < arguments.Count; i++)
            {
                Context.CoroutineManager.TestGenerationTrace.SetTarget(TargetOperation.FunctionGeneration,
                    arguments[i].Name);

                if (parameters[i].ParameterType == ParameterType.NotAvailable)
                {
                    _properties.Add(null);
                    continue;
                }
                string propertyName = arguments[i].Name;
                Type classType = Context.TypeInvoker.GetType(Function.ClassType);
                _properties.Add(classType.GetProperty(propertyName, bindingFlags));
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
            for (int i = 0; i < _properties.Count; i++)
            {
                string paramValue = parameters[i].Value;
                if (null == _properties[i])
                {
                    _params.Add(null);
                    continue;
                }
                IArgument argument = Function.ParameterType[i];
                Context.CoroutineManager.TestGenerationTrace.SetTarget(TargetOperation.ArgumentInitialization,
                    _properties[i].Name);
                switch (parameters[i].ParameterType)
                {
                    case ParameterType.NotAvailable:
                        _params.Add(null);
                        break;
                    case ParameterType.Value:
                        // 如果是简单类型，则直接转换，如果是类或结构体，则需要在运行时解析
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
                        throw new TestflowDataException(ModuleErrorCode.SequenceDataError,
                                Context.I18N.GetStr("InvalidParamVar"));
                        break;
                }
            }
            CommonStepDataCheck(instanceVarName);
        }

        private readonly List<PropertyInfo> _properties;

        private readonly List<object> _params;

        private string _instanceVar;
        private int _propertyIndex;

        public override StepResult InvokeStep(bool forceInvoke)
        {
            this._propertyIndex = -1;
            return SetPropertyFromPropertyIndex(forceInvoke);
        }

        public override StepResult ResumeInvoke(bool forceInvoke, StepResult resultBeforeResume)
        {
            StepResult stepResult = SetPropertyFromPropertyIndex(forceInvoke);
            return ModuleUtils.GetMergedStepResult(resultBeforeResume, stepResult);
        }

        private StepResult SetPropertyFromPropertyIndex(bool forceInvoke)
        {
            object instance = null;
            if (Function.Type == FunctionType.InstancePropertySetter)
            {
                instance = Context.VariableMapper.GetParamValue(this._instanceVar, Function.Instance,
                    Function.ClassType);
            }

            IParameterDataCollection parameters = Function.Parameters;
            IArgumentCollection arguments = Function.ParameterType;
            // 开始计时
            StartTiming();
            int maxPropertyIndex = this._properties.Count - 1;
            while (this._propertyIndex < maxPropertyIndex && (forceInvoke || !Context.Cancellation.IsCancellationRequested))
            {
                this._propertyIndex++;
                if (null == this._properties[this._propertyIndex])
                {
                    continue;
                }
                if (parameters[this._propertyIndex].ParameterType == ParameterType.Variable)
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.ArgumentCalculation, arguments[this._propertyIndex].Name);
                    // 获取变量值的名称，该名称为变量的运行时名称，其值在InitializeParamValue方法里配置
                    string variableName = ModuleUtils.GetVariableNameFromParamValue(parameters[this._propertyIndex].Value);
                    // 根据ParamString和变量对应的值配置参数。
                    this._params[this._propertyIndex] = Context.VariableMapper.GetParamValue(variableName, parameters[this._propertyIndex].Value,
                        arguments[this._propertyIndex].Type);
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._propertyIndex].Name);
                    this._properties[this._propertyIndex].SetValue(instance, this._params[this._propertyIndex]);
                }
                else if (parameters[this._propertyIndex].ParameterType == ParameterType.Expression)
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.ArgumentCalculation, arguments[this._propertyIndex].Name);
                    int expIndex = int.Parse(parameters[this._propertyIndex].Value);
                    ExpressionProcessor expProcessor = Coroutine.ExpressionProcessor;
                    this._params[this._propertyIndex] = expProcessor.Calculate(expIndex, arguments[this._propertyIndex].Type);

                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._propertyIndex].Name);
                    this._properties[this._propertyIndex].SetValue(instance, this._params[this._propertyIndex]);
                }
                // 如果参数类型为value且参数值为null且参数配置的字符不为空且参数类型是类或结构体，则需要实时计算该属性或字段的值
                else if (parameters[this._propertyIndex].ParameterType == ParameterType.Value && null == this._params[this._propertyIndex] &&
                         !string.IsNullOrEmpty(parameters[this._propertyIndex].Value) &&
                         !Context.TypeInvoker.IsSimpleType(this._properties[this._propertyIndex].PropertyType))
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._propertyIndex].Name);
                    object originalValue = this._properties[this._propertyIndex].GetValue(instance);
                    this._params[this._propertyIndex] = Context.TypeInvoker.CastConstantValue(this._properties[this._propertyIndex].PropertyType,
                        parameters[this._propertyIndex].Value, originalValue);
                    // 如果原始值为空，则需要配置Value，否则其参数都已经写入，无需外部更新
                    if (null == originalValue)
                    {
                        this._properties[this._propertyIndex].SetValue(instance, this._params[this._propertyIndex]);
                    }
                }
                else
                {
                    // 更新协程中当前执行目标的信息
                    Coroutine.ExecuteTarget(TargetOperation.Execution, arguments[this._propertyIndex].Name);
                    this._properties[this._propertyIndex].SetValue(instance, this._params[this._propertyIndex]);
                }
            }
            if (CoreUtils.IsValidVaraible(Function.Instance) && Function.Type == FunctionType.InstancePropertySetter)
            {
                LogTraceVariable(Function.Instance, instance);
            }
            // 停止计时
            EndTiming();
            return StepResult.Pass;
        }
    }
}