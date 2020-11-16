﻿using System;
using Testflow.Usr;
using Testflow.Data;
using Testflow.Data.Attributes;
using Testflow.Data.Description;
using Testflow.Data.Expression;
using Testflow.Data.Sequence;

namespace Testflow.Modules
{
    /// <summary>
    /// 序列持久化模块
    /// </summary>
    public interface ISequenceManager : IController
    {
        /// <summary>
        /// 序列的版本号
        /// </summary>
        string Version { get; set; }

        #region 创建序列元素

        /// <summary>
        /// 创建空白的测试工程
        /// </summary>
        /// <returns>返回创建的测试工程</returns>
        ITestProject CreateTestProject();

        /// <summary>
        /// 创建空白的序列组
        /// </summary>
        /// <returns>返回创建的空白测试组</returns>
        ISequenceGroup CreateSequenceGroup();

        /// <summary>
        /// 创建空白的序列
        /// </summary>
        /// <returns></returns>
        ISequence CreateSequence();

        /// <summary>
        /// 创建空白的序列Step
        /// </summary>
        /// <returns></returns>
        ISequenceStep CreateSequenceStep(bool createSubStepCollection = false);

        /// <summary>
        /// 对指定的序列步骤创建空白的子序列步骤集合，如果当前Step已经包含子序列步骤集合则不执行任何操作
        /// </summary>
        /// <returns></returns>
        void AddSubStepCollection(ISequenceStep parent);

        /// <summary>
        /// 根据类型创建的序列Step
        /// </summary>
        /// <returns></returns>
        ISequenceStep CreateNonExecutionStep(SequenceStepType stepType);

        /// <summary>
        /// 创建空白的Argument
        /// </summary>
        /// <returns></returns>
        IArgument CreateArugment();

        /// <summary>
        /// 创建空白的FunctionData
        /// </summary>
        /// <returns></returns>
        IFunctionData CreateFunctionData(IFuncInterfaceDescription funcInterface);

        /// <summary>
        /// 创建空白的StepAction
        /// </summary>
        /// <returns></returns>
        IStepAction CreateActionData(IFuncInterfaceDescription funcInterface);

        /// <summary>
        /// 创建步骤属性，如果入参为null则创建空白的对象，否则给根据模板创建对应的Attribute
        /// </summary>
        /// <param name="attributeDeclaration">创建Step属性的定义模板</param>
        IStepAttribute CreateStepAttribute(IAttributeDeclaration attributeDeclaration = null);

        /// <summary>
        /// 创建空白的LoopCounter
        /// </summary>
        /// <returns></returns>
        ILoopCounter CreateLoopCounter();

        /// <summary>
        /// 创建空白的RetryCounter
        /// </summary>
        /// <returns></returns>
        IRetryCounter CreateRetryCounter();

        /// <summary>
        /// 创建空白的SequenceGroupParameter
        /// </summary>
        /// <returns></returns>
        ISequenceGroupParameter CreateSequenceGroupParameter();

        /// <summary>
        /// 创建空白的SequenceParameter
        /// </summary>
        /// <returns></returns>
        ISequenceParameter CreateSequenceParameter();

        /// <summary>
        /// 创建空白的SequenceStepParameter
        /// </summary>
        /// <returns></returns>
        ISequenceStepParameter CreateSequenceStepParameter();

        /// <summary>
        /// 创建空白的ParameterData
        /// </summary>
        /// <param name="argument">该ParameterData对应的Argument对象</param>
        /// <returns></returns>
        IParameterData CreateParameterData(IArgument argument);

        /// <summary>
        /// 创建空白的TypeData
        /// </summary>
        /// <returns></returns>
        ITypeData CreateTypeData(ITypeDescription typeDescription);

        /// <summary>
        /// 创建空白的Variable
        /// </summary>
        IVariable CreateVariable();

        /// <summary>
        /// 创建空白的Variable
        /// </summary>
        [Obsolete]
        IVariable CreateVarialbe();


        
        /// <summary>
        /// 创建空白的AssemblyInfo
        /// </summary>
        /// <returns></returns>
        IAssemblyInfo CreateAssemblyInfo();

        /// <summary>
        /// 使用字符串获取表达式数据结构
        /// </summary>
        /// <param name="step">表达式所在的Step</param>
        /// <param name="expressionValue">表达式的值</param>
        /// <returns></returns>
        IExpressionData GetExpressionData(ISequenceStep step, string expressionValue);

        /// <summary>
        /// 判断某个参数的配置是否为表达式，该值不能是常量
        /// </summary>
        /// <param name="parameterValue">参数值</param>
        bool IsExpression(string parameterValue);
        
        #endregion

        #region 序列化相关

        /// <summary>
        /// 序列化测试工程
        /// </summary>
        /// <param name="project">待序列化的工程</param>
        /// <param name="target">序列化的目标</param>
        /// <param name="param">额外参数</param>
        void Serialize(ITestProject project, SerializationTarget target, params string[] param);

        /// <summary>
        /// 序列化测试序列组
        /// </summary>
        /// <param name="sequenceGroup">待序列化的测试序列组</param>
        /// <param name="target">序列化的目标</param>
        /// <param name="param">额外参数</param>
        void Serialize(ISequenceGroup sequenceGroup, SerializationTarget target, params string[] param);
        
        /// <summary>
        /// 加载测试工程
        /// </summary>
        /// <param name="source">反序列化的源</param>
        /// <param name="param">额外参数</param>
        ITestProject LoadTestProject(SerializationTarget source, params string[] param);

        /// <summary>
        /// 加载测试序列组
        /// </summary>
        /// <param name="source">反序列化的源</param>
        /// <param name="param">额外参数，如果是文件需要传入文件路径</param>
        ISequenceGroup LoadSequenceGroup(SerializationTarget source, params string[] param);

        /// <summary>
        /// 加载参数配置文件
        /// </summary>
        /// <param name="sequenceGroup">加载参数配置的目标序列组</param>
        /// <param name="forceLoad">是否强制加载，false时如果hash比对不通过会报错</param>
        /// <param name="param">额外参数，如果是文件需要传入文件路径</param>
        void LoadParameter(ISequenceGroup sequenceGroup, bool forceLoad, params string[] param);

        /// <summary>
        /// 运行时序列化
        /// </summary>
        /// <param name="testProject">待序列化的TestProject</param>
        string RuntimeSerialize(ITestProject testProject);

        /// <summary>
        /// 运行时序列化
        /// </summary>
        /// <param name="sequenceGroup">待序列化的SequenceGroup</param>
        string RuntimeSerialize(ISequenceGroup sequenceGroup);

        /// <summary>
        /// 运行时反序列化
        /// </summary>
        /// <param name="testProjectStr">待反序列化的TestProject</param>
        ITestProject RuntimeDeserializeTestProject(string testProjectStr);

        /// <summary>
        /// 运行时反序列化
        /// </summary>
        /// <param name="sequenceGroupStr">待反序列化的SequecneGroup</param>
        ISequenceGroup RuntimeDeserializeSequenceGroup(string sequenceGroupStr);

        #endregion

        #region 序列数据操作

        /// <summary>
        /// 生效序列数据，处理所有连带配置
        /// </summary>
        void ValidateSequenceData(ITestProject testProject);

        /// <summary>
        /// 生效序列数据，处理所有连带配置
        /// </summary>
        void ValidateSequenceData(ISequenceGroup sequenceGroup, ITestProject parent = null);

        /// <summary>
        /// 校验序列数据的结构，如果失败抛出TestflowDataExceptoin
        /// </summary>
        void CheckSequenceData(ISequenceFlowContainer sequenceData);

        #endregion


    }
}