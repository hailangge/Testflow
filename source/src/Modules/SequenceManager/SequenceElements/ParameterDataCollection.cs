using System;
using System.Collections;
using System.Collections.Generic;
using Testflow.Data.Sequence;
using Testflow.SequenceManager.Common;

namespace Testflow.SequenceManager.SequenceElements
{
    [Serializable]
    [GenericCollection(typeof(ParameterData))]
    public class ParameterDataCollection : IndexedFlexibleList<IParameterData>, IParameterDataCollection
    {
    }
}