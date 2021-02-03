using System;
using Testflow.Runtime.Data;

namespace Testflow.SlaveCore.Runner.Model.Execution
{
    internal struct ExecutionResult
    {
        public StepResult Result { get; set; }

        public DateTime StartTime { get; set; }

        public long ExecutionTicks { get; set; }


    }
}