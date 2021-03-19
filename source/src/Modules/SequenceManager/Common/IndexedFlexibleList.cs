using System;
using System.Collections.Generic;

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
            ModuleUtils.AddAndRefreshIndex(this.InnerCollection ?? EmptyCollection, item);
        }

        public override bool Remove(TDataType item)
        {
            base.Remove(item);
            return ModuleUtils.RemoveAndRefreshIndex(this.InnerCollection ?? EmptyCollection, item);
        }

        public override void Insert(int index, TDataType item)
        {
            base.Insert(index, item);
            ModuleUtils.InsertAndRefreshIndex(this.InnerCollection ?? EmptyCollection, item, index);
        }

        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            ModuleUtils.RemoveAtAndRefreshIndex(this.InnerCollection ?? EmptyCollection, index);
        }
    }
}