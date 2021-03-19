using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testflow.Data.Attributes;
using Testflow.Data.Sequence;
using Testflow.SlaveCore.Common;

namespace Testflow.SlaveCore.Runner.Attributes
{
    internal class AttributesProcessor
    {
        private readonly SlaveContext _context;

        public AttributesProcessor(SlaveContext context)
        {
            this._context = context;
        }

        public void ProcessPrecedingAttributes(ISequenceStep stepData)
        {
            // foreach (IStepAttribute stepAttribute in stepData.PrecedingAttributes)
            // {
            //     
            // }
        }

        public void ProcessPostAttributes(ISequenceStep stepData)
        {
            // foreach (IStepAttribute stepAttribute in stepData.PostAttributes)
            // {
            //     
            // }
        }

        private void ProcessSingleAttribute(ISequenceStep step)
        {

        }
    }
}
