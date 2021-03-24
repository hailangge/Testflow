using System;
using System.Collections;
using System.Collections.Generic;
using Testflow.Data.Sequence;
using Testflow.SequenceManager.Common;

namespace Testflow.SequenceManager.SequenceElements
{
    [Serializable]
    [GenericCollection(typeof(SequenceStep))]
    public class SequenceStepCollection : ISequenceStepCollection
    {
        private List<ISequenceStep> _innerCollection;

        public SequenceStepCollection()
        {
            this._innerCollection = null;
        }

        public IEnumerator<ISequenceStep> GetEnumerator()
        {
            if (null == _innerCollection)
            {
                return new EmptyEnumerator<ISequenceStep>();
            }
            return this._innerCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ISequenceStep item)
        {
            CreateIfCollectionIsNull();
            ModuleUtils.SetElementName(item, this);
            ModuleUtils.AddAndRefreshIndex(_innerCollection, item);
        }

        public void Clear()
        {
            _innerCollection?.Clear();
            FreeIfNotUsed();
        }

        public bool Contains(ISequenceStep item)
        {
            return _innerCollection?.Contains(item) ?? false;
        }

        public void CopyTo(ISequenceStep[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(ISequenceStep item)
        {
            bool result = null != _innerCollection && ModuleUtils.RemoveAndRefreshIndex(_innerCollection, item);
            FreeIfNotUsed();
            return result;
        }

        public int Count => _innerCollection?.Count ?? 0;
        public bool IsReadOnly => false;
        public int IndexOf(ISequenceStep item)
        {
            return _innerCollection?.IndexOf(item) ?? -1;
        }

        public void Insert(int index, ISequenceStep item)
        {
            CreateIfCollectionIsNull();
            ModuleUtils.SetElementName(item, this);
            ModuleUtils.InsertAndRefreshIndex(_innerCollection, item, index);
        }

        public void RemoveAt(int index)
        {
            if (null == _innerCollection)
            {
                return;
            }
            ModuleUtils.RemoveAtAndRefreshIndex(_innerCollection, index);
            FreeIfNotUsed();
        }

        public ISequenceStep this[int index]
        {
            get { return _innerCollection[index]; }
            set { throw new System.NotImplementedException(); }
        }


        private void CreateIfCollectionIsNull()
        {
            if (null != _innerCollection) return;
            _innerCollection = new List<ISequenceStep>(Constants.DefaultSequenceSize);
        }

        private void FreeIfNotUsed()
        {
            if (null != _innerCollection && _innerCollection.Count == 0)
            {
                _innerCollection = null;
            }
        }
    }
}