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
            ModuleUtils.AddAndRefreshIndex(this.InnerCollection, item);
        }

        public override bool Remove(TDataType item)
        {
            return ModuleUtils.RemoveAndRefreshIndex(this.InnerCollection, item);
        }

        public override void Insert(int index, TDataType item)
        {
            ModuleUtils.InsertAndRefreshIndex(InnerCollection, item, index);
        }

        public override void RemoveAt(int index)
        {
            ModuleUtils.RemoveAtAndRefreshIndex(InnerCollection, index);
        }
    }
}