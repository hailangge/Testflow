using System.Collections;
using System.Collections.Generic;

namespace Testflow.SequenceManager.SequenceElements
{
    public class EmptyEnumerator<TDataType> : IEnumerator<TDataType> where TDataType : class
    {
        public void Dispose()
        {
            // ignore
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
            //ignore;
        }

        public TDataType Current => null;

        object IEnumerator.Current => Current;
    }
}