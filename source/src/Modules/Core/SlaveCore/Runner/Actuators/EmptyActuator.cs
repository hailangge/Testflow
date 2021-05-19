using Testflow.Data.Sequence;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Data;

namespace Testflow.SlaveCore.Runner.Actuators
{
    internal class EmptyActuator : ActuatorBase
    {
        public EmptyActuator(SlaveContext context, ISequenceStep step, int sequenceIndex) : base(step, context, sequenceIndex)
        {
        }

        protected override void GenerateInvokeInfo()
        {
            // ignore
        }

        protected override void InitializeParamsValues()
        {
            // ignore
        }

        public override StepResult InvokeStep(bool forceInvoke)
        {
            // 更新协程中当前执行目标的信息
            Coroutine.ExecuteTarget(TargetOperation.Execution, string.Empty);
            // 开始计时
            StartTiming();
            // 停止计时
            EndTiming();
            return StepResult.Pass;
        }
    }
}