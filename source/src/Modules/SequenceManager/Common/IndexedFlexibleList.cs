using System;
using System.Collections.Generic;
using System.Linq;

namespace Testflow.SequenceManager.Common
{
    [Serializable]
    public class IndexedFlexibleList<TDataType> : FlexibleList<TDataType>
    {
        public IndexedFlexibleList() : base()
        {
        }

        public IndexedFlexibleList(IEnumerable<TDataType> data) : base(data)
        {

        }

        public override void Add(TDataType item)
        {
            base.Add(item);
            ModuleUtils.UpdateIndex(this.InnerCollection ?? EmptyCollection, this.Count - 1);
        }

        public override bool Remove(TDataType item)
        {
            List<TDataType> collection = this.InnerCollection ?? EmptyCollection;
            if (!collection.Any(collectionItem => collectionItem.Equals(item)))
            {
                return false;
            }
            int index = collection.IndexOf(item);
            base.Remove(item);
            ModuleUtils.UpdateIndex(collection, index);
            return true;
        }

        public override void Insert(int index, TDataType item)
        {
            base.Insert(index, item);
            ModuleUtils.UpdateIndex(this.InnerCollection ?? EmptyCollection, index);
        }

        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            ModuleUtils.UpdateIndex(this.InnerCollection ?? EmptyCollection, index);
        }
    }
}