using System;
using System.Threading;
using Testflow.CoreCommon.Data;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Runner.Model;

namespace Testflow.SlaveCore.Data
{
    internal class ExecutionTrack : IDisposable
    {
        private OverLapBuffer<StepExecutionResult> _stepExecutionInfos;
        public ExecutionTrack(int capacity)
        {
            _stepExecutionInfos = new OverLapBuffer<StepExecutionResult>(capacity);
        }

        public void Enqueue(StepTaskEntityBase stepEntity)
        {
            _stepExecutionInfos.Enqueue(new StepExecutionResult(stepEntity, StepResult.NotAvailable));
        }

        public StepExecutionResult GetLastStep(int offset)
        {
            return _stepExecutionInfos.GetLastElement(offset);
        }

        public StepExecutionResult GetLastNotAvailableStep()
        {
            StepExecutionResult stepInfo;
            int offset = 1;
            do
            {
                stepInfo = _stepExecutionInfos.GetLastElement(offset);
            } while (null != stepInfo && stepInfo.StepResult != StepResult.NotAvailable);
            return stepInfo;
        }

        private int _diposedFlag = 0;
        public void Dispose()
        {
            if (_diposedFlag != 0)
            {
                return;
            }
            Thread.VolatileWrite(ref _diposedFlag, 1);
            Thread.MemoryBarrier();
            _stepExecutionInfos.Dispose();
        }
    }
}