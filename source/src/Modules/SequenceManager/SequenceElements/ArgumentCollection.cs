using System;
using System.Collections;
using System.Collections.Generic;
using Testflow.Data.Sequence;
using Testflow.SequenceManager.Common;

namespace Testflow.SequenceManager.SequenceElements
{
    [Serializable]
    [GenericCollection(typeof(Argument))]
    public class ArgumentCollection : FlexibleList<IArgument>, IArgumentCollection
    {
        public ArgumentCollection() : base(true)
        {
        }
    }
}