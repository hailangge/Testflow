using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Testflow.CoreCommon;
using Testflow.CoreCommon.Common;
using Testflow.Data.Sequence;
using Testflow.MasterCore.Common;
using Testflow.Usr;
using Testflow.Utility.Utils;

namespace Testflow.MasterCore.Core
{
    internal class OperationPanelManager : IDisposable
    {
        private readonly ModuleGlobalInfo _globalInfo;

        private volatile ISequenceFlowContainer _sequenceData;

        private volatile Dictionary<string, object> _sequenceParameters;

        private volatile bool _isStartConfirmed;

        private volatile Exception _internalException;

        // 显示OI面板的委托
        private Action _showPanel;
        private Action<bool, string> _showError;

        private AutoResetEvent _blockHandle;

        private IDisposable _oiInstance;

        public OperationPanelManager(ModuleGlobalInfo globalInfo)
        {
            this._globalInfo = globalInfo;
            this._showError = null;
            this._showPanel = null;
            this._sequenceParameters = null;
            this._isStartConfirmed = false;
            this.IsRunning = false;
            this._internalException = null;
        }

        /// <summary>
        /// 返回当前OI是否已经开始运行
        /// </summary>
        public bool IsRunning { get; private set; }

        public void Initialize(ISequenceFlowContainer sequenceData)
        {
            this._sequenceData = sequenceData;
            if (this._sequenceData is ISequenceGroup)
            {
                IOperationPanelInfo panelInfo = ((ISequenceGroup)this._sequenceData).Info.OperationPanelInfo;
                if (panelInfo?.Assembly == null || null == panelInfo.OperationPanelClass)
                {
                    return;
                }
                this._blockHandle = new AutoResetEvent(false);
                this._isStartConfirmed = false;
                ThreadPool.QueueUserWorkItem(StartRunOperationPanel);
                // 设定OI配置超时时间为1小时
                this._blockHandle.WaitOne(new TimeSpan(0, 1, 0, 0));
                if (null != this._internalException)
                {
                    throw this._internalException;
                }

                try
                {
                    if (!this._isStartConfirmed)
                    {
                        throw new TestflowRuntimeException(ModuleErrorCode.UserCancelled,
                            this._globalInfo.I18N.GetStr("UserCancel"));
                    }
                    FillSequenceParameters();
                }
                catch (Exception ex)
                {
                    this._showError(false, ex.Message);
                    throw;
                }
            }
        }

        private void FillSequenceParameters()
        {
            if (null == this._sequenceParameters || this._sequenceParameters.Count == 0)
            {
                return;
            }
            IVariableCollection variables = ((ISequenceGroup)this._sequenceData).Variables;
            foreach (KeyValuePair<string, object> paramPair in this._sequenceParameters)
            {
                IVariable variable = variables.FirstOrDefault(item => item.Name.Equals(paramPair.Key));
                if (null == variable)
                {
                    throw new TestflowRuntimeException(ModuleErrorCode.SequenceDataError,
                        this._globalInfo.I18N.GetFStr("ParamVariableNotExist", paramPair.Key));
                }
                string value = this._globalInfo.Serializer.Serialize(paramPair.Value);
                variable.Value = value;
            }
        }

        // 异步事件
        private void OiStartSequenceConfirmed(bool isStartConfirmed, Dictionary<string, object> parameters)
        {
            this._sequenceParameters = parameters;
            this._isStartConfirmed = isStartConfirmed;
            this._blockHandle.Set();
        }

        private EventInfo GetOIEventInfo(Type oiClassType, string eventName)
        {
            EventInfo startSequenceEvent = oiClassType.GetEvent(eventName,
                BindingFlags.Public | BindingFlags.Instance);
            if (null == startSequenceEvent)
            {
                throw new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetFStr("OIClassEventNotExist", oiClassType.Name,
                        eventName));
            }
            return startSequenceEvent;
        }

        private MethodInfo GetOIMethodInfo(Type oiClassType, string methodName, params Type[] paramTypes)
        {
            MethodInfo showPanelMethod = oiClassType.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public, null, paramTypes, null);
            if (null == showPanelMethod)
            {
                throw new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetFStr("OIClassMethodNotExist", oiClassType.Name,
                        methodName));
            }

            return showPanelMethod;
        }

        private void Start()
        {

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
            // 如果OI已经开始成功运行则不执行OI的释放操作
            if (!IsRunning)
            {
                this._oiInstance.Dispose();
            }

            if (null != this._blockHandle)
            {
                this._blockHandle.Set();
                this._blockHandle.Dispose();
            }
        }
    }
}