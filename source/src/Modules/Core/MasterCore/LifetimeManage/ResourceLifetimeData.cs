using System;

namespace Testflow.MasterCore.LifetimeManage
{
    internal class ResourceLifetimeData
    {
        public IDisposable Resource { get; }

        public LifeTimeType Type { get; }

        /// <summary>
        /// 生命周期计数，如果是固定次数则
        /// </summary>
        public int Count { get; set; }

        public ResourceLifetimeData(IDisposable resource, LifeTimeType type, int count = -1)
        {
            this.Resource = resource;
            this.Type = type;
            this.Count = count;
        }
    }
}