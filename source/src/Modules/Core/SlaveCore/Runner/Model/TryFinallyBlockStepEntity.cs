using System;
using System.Reflection;
using Testflow.CoreCommon.Data;
using Testflow.Data;
using Testflow.Data.Sequence;
using Testflow.FlowControl;
using Testflow.Runtime;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Data;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Model
{
    internal class TryFinallyBlockStepEntity : StepTaskEntityBase
    {
        public TryFinallyBlockStepEntity(ISequenceStep step, SlaveContext context, int sequenceIndex) : base(step, context, sequenceIndex)
        {
        }

        protected override void InvokeStepSingleTime(bool forceInvoke)
        {
            // 重置计时
            Actuator.ResetTiming();
            // 调用前置监听
            OnPreListener();

            // 开始计时
            Actuator.StartTiming();
            // 停止计时
            Actuator.EndTiming();
            // 应为TryFinally块上级为空，默认为pass
            this.Result = StepResult.Pass;
            // 调用后置监听
            OnPostListener();

            StepTaskEntityBase tryBlock = SubStepRoot;
            StepTaskEntityBase finallyBlock = tryBlock.NextStep;
            ExecutionInfo errorExecutionInfo = null;
            try
            {
                tryBlock.Invoke(forceInvoke);
            }
            // 需要处理上次出错的Step的数据
            catch (TestflowAssertException ex)
            {
                errorExecutionInfo = Coroutine.TaskPointer.Clone();
                if (null != errorExecutionInfo.StepEntity && errorExecutionInfo.StepEntity.Result == StepResult.NotAvailable)
                {
                    // 停止计时
                    errorExecutionInfo.StepEntity.EndTiming();
                    errorExecutionInfo.StepEntity.Result = StepResult.Failed;
                    errorExecutionInfo.StepEntity.RecordInvocationError(ex, FailedType.AssertionFailed);
                }
                throw;
            }
            catch (TargetInvocationException ex)
            {
                errorExecutionInfo = Coroutine.TaskPointer.Clone();
                if (null != errorExecutionInfo.StepEntity && errorExecutionInfo.StepEntity.Result == StepResult.NotAvailable)
                {
                    // 停止计时
                    errorExecutionInfo.StepEntity.EndTiming();
                    errorExecutionInfo.StepEntity.RecordTargetInvocationError(ex.InnerException);
                }
                throw;
            }
            catch (TargetException ex)
            {
                errorExecutionInfo = Coroutine.TaskPointer.Clone();
                if (null != errorExecutionInfo.StepEntity && errorExecutionInfo.StepEntity.Result == StepResult.NotAvailable)
                {
                    // 停止计时
                    errorExecutionInfo.StepEntity.EndTiming();
                    errorExecutionInfo.StepEntity.Result = StepResult.Error;
                    errorExecutionInfo.StepEntity.RecordInvocationError(ex, FailedType.TargetError);
                }
                throw;
            }
            catch (TestflowInternalException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // 停止计时
                Actuator.EndTiming();
                errorExecutionInfo = Coroutine.TaskPointer.Clone();
                this.Context.LogSession.Print(LogLevel.Fatal, this.Context.SessionId, $"Error location:{errorExecutionInfo}.");

                if (null != errorExecutionInfo.StepEntity && errorExecutionInfo.StepEntity.Result == StepResult.NotAvailable)
                {
                    errorExecutionInfo.StepEntity.Result = StepResult.Error;
                    if (null != ex.InnerException)
                    {
                        errorExecutionInfo.StepEntity.RecordTargetInvocationError(ex.InnerException);
                    }
                    else
                    {
                        errorExecutionInfo.StepEntity.RecordInvocationError(ex, FailedType.TargetError);
                    }
                }
                throw;
            }
            finally
            {
                // finally模块是强制调用
                finallyBlock.Invoke(true);
                // 如果错误执行对象不为空，则在finally结束后将错误执行指针重新指向该对象
                if (null != errorExecutionInfo?.StepEntity)
                {
                    Coroutine.TaskPointer.Reset(errorExecutionInfo);
                }
            }
            if (null != StepData && StepData.RecordStatus)
            {
                RecordRuntimeStatus();
            }
        }
    }
}