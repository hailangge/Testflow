using Testflow.Runtime.Data;
using Testflow.SlaveCore.Runner.Model;

namespace Testflow.SlaveCore.Data
{
    internal class StepExecutionResult
    {
        public StepResult StepResult { get; set; }

        public StepTaskEntityBase StepEntity { get; }

        public StepExecutionResult(StepTaskEntityBase stepEntity, StepResult result)
        {
            this.StepEntity = stepEntity;
            this.StepResult = result;
        }
    }
}