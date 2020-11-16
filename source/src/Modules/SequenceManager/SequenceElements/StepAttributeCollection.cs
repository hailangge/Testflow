using System;
using System.Collections;
using System.Collections.Generic;
using Testflow.Data.Attributes;
using Testflow.SequenceManager.Common;

namespace Testflow.SequenceManager.SequenceElements
{
    [Serializable]
    [GenericCollection(typeof(StepAttribute))]
    public class StepAttributeCollection : IndexedFlexibleList<IStepAttribute>, IStepAttributeCollection
    {
    }
}