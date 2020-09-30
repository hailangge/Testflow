using System;
using System.Collections;
using System.Collections.Generic;
using Testflow.Data.Attributes;
using Testflow.SequenceManager.Common;

namespace Testflow.SequenceManager.SequenceElements
{
    [Serializable]
    [GenericCollection(typeof(StepAttribute))]
    public class StepAttributeCollection : IStepAttributeCollection
    {
        private const int Capacity = 5;
        private List<IStepAttribute> _innerCollection;
        public StepAttributeCollection()
        {
            this._innerCollection = null;
        }

        public IEnumerator<IStepAttribute> GetEnumerator()
        {
            if (null == _innerCollection)
            {
                return new EmptyEnumerator<IStepAttribute>();
            }
            return _innerCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IStepAttribute item)
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

        public bool Contains(IStepAttribute item)
        {
            return _innerCollection?.Contains(item) ?? false;
        }

        public void CopyTo(IStepAttribute[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(IStepAttribute item)
        {
            bool result =  null != _innerCollection && ModuleUtils.RemoveAndRefreshIndex(_innerCollection, item);
            FreeIfNotUsed();
            return result;
        }

        public int Count => _innerCollection?.Count ?? 0;
        public bool IsReadOnly => false;
        public int IndexOf(IStepAttribute item)
        {
            return _innerCollection?.IndexOf(item) ?? -1;
        }

        public void Insert(int index, IStepAttribute item)
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

        public IStepAttribute this[int index]
        {
            get { return _innerCollection[index]; }
            set { throw new System.NotImplementedException(); }
        }

        private void CreateIfCollectionIsNull()
        {
            if (null != _innerCollection) return;
            _innerCollection = new List<IStepAttribute>(Capacity);
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