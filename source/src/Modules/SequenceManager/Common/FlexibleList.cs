using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Testflow.SequenceManager.Common
{
    [Serializable]
    public class FlexibleList<TDataType> : IList<TDataType>
    {
        protected List<TDataType> InnerCollection;
        public FlexibleList(bool isReadOnly = false)
        {
            this.IsReadOnly = isReadOnly;
            InnerCollection = null;
        }

        public FlexibleList(IEnumerable<TDataType> data)
        {
            InnerCollection = new List<TDataType>(10);
            foreach (TDataType element in data)
            {
                InnerCollection.Add(element);
            }
            if (InnerCollection.Count == 0)
            {
                InnerCollection = null;
            }
        }

        public IEnumerator<TDataType> GetEnumerator()
        {
            return InnerCollection?.GetEnumerator() ?? Enumerable.Empty<TDataType>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(TDataType item)
        {
            if (null == InnerCollection)
            {
                InnerCollection = new List<TDataType>(Constants.DefaultArgumentSize);
            }
            InnerCollection.Add(item);
        }

        public void Clear()
        {
            InnerCollection?.Clear();
            InnerCollection = null;
        }

        public bool Contains(TDataType item)
        {
            return InnerCollection?.Contains(item) ?? false;
        }

        public void CopyTo(TDataType[] array, int arrayIndex)
        {
            if (null == InnerCollection)
            {
                return;
            }
            for (int i = arrayIndex; i < InnerCollection.Count; i++)
            {
                array[i] = InnerCollection[i];
            }
        }

        public virtual bool Remove(TDataType item)
        {
            return InnerCollection?.Remove(item) ?? false;
        }

        public int Count => InnerCollection?.Count ?? 0;
        public bool IsReadOnly { get; }
        public int IndexOf(TDataType item)
        {
            return InnerCollection?.IndexOf(item) ?? -1;
        }

        public virtual void Insert(int index, TDataType item)
        {
            if ((InnerCollection?.Count ?? 0) < index)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (null == InnerCollection)
            {
                InnerCollection = new List<TDataType>(Constants.DefaultArgumentSize);
            }
            InnerCollection.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            if ((InnerCollection?.Count ?? -1) < index)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            InnerCollection?.RemoveAt(index);
        }

        public TDataType this[int index]
        {
            get { return InnerCollection[index]; }
            set {
                if (IsReadOnly)
                {
                    throw new NotSupportedException("Setter is not available.");
                }
                InnerCollection[index] = value;
            }
        }
    }
}