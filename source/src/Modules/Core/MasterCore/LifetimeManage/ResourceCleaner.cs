using System;
using System.Collections.Generic;
using System.Threading;
using Testflow.MasterCore.Common;

namespace Testflow.MasterCore.LifetimeManage
{
    internal class ResourceCleaner : IDisposable
    {
        private ModuleGlobalInfo _globalInfo;
        private List<ResourceLifetimeData> _legacyResources;
        internal ResourceCleaner(ModuleGlobalInfo globalInfo)
        {
            this._globalInfo = globalInfo;
            this._legacyResources = new List<ResourceLifetimeData>(500);
        }

        private int _disposedFlag = 0;
        public void Dispose()
        {
            if (this._disposedFlag != 0)
            {
                return;
            }
            Thread.VolatileWrite(ref this._disposedFlag, 1);
            Thread.MemoryBarrier();
            foreach (IDisposable legacyResource in this._legacyResources)
            {
                try
                {
                    legacyResource?.Dispose();
                }
                catch (Exception ex)
                {
                    this._globalInfo.ExceptionManager?.Append(ex);
                }
            }
            this._legacyResources.Clear();
        }
    }
}