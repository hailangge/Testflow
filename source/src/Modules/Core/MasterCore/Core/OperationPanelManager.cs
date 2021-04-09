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
using Testflow.Modules;
using Testflow.Runtime;
using Testflow.Runtime.Data;
using Testflow.Runtime.OperationPanel;
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

        private AutoResetEvent _blockHandle;

        public OperationPanelManager(ModuleGlobalInfo globalInfo)
        {
            this._globalInfo = globalInfo;
            this._sequenceParameters = null;
            this._isStartConfirmed = false;
            this.IsRunning = false;
            this._internalException = null;
            this._operationPanelInfo = null;
            this._eventActions = new List<Delegate>(10);
        }

        /// <summary>
        /// 返回当前OI是否已经开始运行
        /// </summary>
        public bool IsRunning { get; private set; }

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

        public void Start()
        {
            if (null == this._operationPanelInfo || string.IsNullOrWhiteSpace(this._operationPanelInfo.Parameters))
            {
                return;
            }
            RegisterEvent();
            this._blockHandle = new AutoResetEvent(false);
            this._isStartConfirmed = false;
            this._operationPanel.ConfigurationOver += OiStartSequenceConfirmed;
            ThreadPool.QueueUserWorkItem(ShowOperationPanel);
            // 设定OI配置超时时间为1小时
            bool isNotTimeout = this._blockHandle.WaitOne(new TimeSpan(0, 1, 0, 0));
            if (!isNotTimeout)
            {
                throw new TestflowRuntimeException(ModuleErrorCode.OperationTimeout,
                    this._globalInfo.I18N.GetStr("OITimeout"));
            }
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
            catch (TestflowException ex)
            {
                this._operationPanel.ShowErrorMessage(false, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                this._operationPanel.ShowErrorMessage(false, ex.Message);
                throw new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetFStr("OIParamError", ex.Message));
            }
        }

        private void ShowOperationPanel(object state)
        {
            try
            {
                object[] oiExtraParams = GetShowParameter();
                this._operationPanel.ShowPanel(this._sequenceData, oiExtraParams);
            }
            catch (TestflowException ex)
            {
                this._internalException = ex;
            }
            catch (Exception ex)
            {
                Exception innerException = ex.InnerException ?? ex;
                this._internalException = new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetFStr("OperationPanelError", innerException.Message), innerException);
            }
            finally
            {
                this._blockHandle?.Set();
            }
        }

        private object[] GetShowParameter()
        {
            List<object> oiExtraParams = new List<object>(10);
            // TestFlow平台运行时关联路径
            List<string> platformDirs = new List<string>(10);
            platformDirs.AddRange(this._globalInfo.ConfigData.GetProperty<string[]>("SequencePath"));
            platformDirs.AddRange(this._globalInfo.ConfigData.GetProperty<string[]>("WorkspaceDir"));
            platformDirs.Add(this._globalInfo.ConfigData.GetProperty<string>("PlatformLibDir"));
            platformDirs.Add(this._globalInfo.ConfigData.GetProperty<string>("TestflowHome"));
            oiExtraParams.Add(platformDirs.ToArray());

            return oiExtraParams.ToArray();
        }

        private IOperationPanel _operationPanel;
        private IOperationPanelInfo _operationPanelInfo;
        private readonly List<Delegate> _eventActions;

        public void Initialize(ISequenceFlowContainer sequenceData)
        {
            try
            {
                this._sequenceData = sequenceData;
                if (_sequenceData is ITestProject)
                {
                    // TODO TestProject暂未实现
                    throw new TestflowRuntimeException(ModuleErrorCode.IncorrectParamType, "Test Project is not supported.");
                }
                else if (this._sequenceData is ISequenceGroup)
                {
                    ISequenceGroup sequenceGroup = ((ISequenceGroup)sequenceData);
                    this._operationPanelInfo = sequenceGroup.Info.OperationPanelInfo;
                    if (IsEmptyOperationPanel())
                    {
                        this._operationPanelInfo = null;
                        return;
                    }
                    InitOperationPanel();
                }
            }
            catch (TestflowException)
            {
                throw;
            }
            catch (TargetException ex)
            {
                Exception innerException = ex.InnerException ?? ex;
                this._globalInfo.LogService.Print(LogLevel.Error, CommonConst.PlatformLogSession, ex,
                    "Target Exception occur when loading operation panel.");
                throw new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetFStr("OperationPanelError", innerException.Message), innerException);
            }
            catch (Exception ex)
            {
                this._globalInfo.LogService.Print(LogLevel.Error, CommonConst.PlatformLogSession, ex,
                    "Runtime Exception occur when loading operation panel.");
                throw new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetFStr("OperationPanelError", ex.Message), ex);
            }
        }

        private bool IsEmptyOperationPanel()
        {
            return _operationPanelInfo?.Assembly == null ||
                   string.IsNullOrWhiteSpace(this._operationPanelInfo.Assembly.Path) ||
                   null == this._operationPanelInfo.OperationPanelClass ||
                   string.IsNullOrWhiteSpace(this._operationPanelInfo.OperationPanelClass.Name);
        }

        // 异步事件
        private void OiStartSequenceConfirmed(bool isStartConfirmed, Dictionary<string, object> parameters)
        {
            this._sequenceParameters = parameters;
            this._isStartConfirmed = isStartConfirmed;
            this._blockHandle.Set();
        }

        private void InitOperationPanel()
        {
            if (_operationPanelInfo?.Assembly?.Path == null || string.IsNullOrWhiteSpace(this._operationPanelInfo.Assembly.Path))
            {
                return;
            }
            _operationPanel = null;
            Assembly assembly = Assembly.LoadFrom(_operationPanelInfo.Assembly.Path);
            Type operationPanelType = assembly.GetType(
                $"{_operationPanelInfo.OperationPanelClass.Namespace}.{_operationPanelInfo.OperationPanelClass.Name}");
            if (null == operationPanelType)
            {
                throw new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetFStr("OIClassNotFound",
                        this._operationPanelInfo.OperationPanelClass.Name));
            }
            _operationPanel = Activator.CreateInstance(operationPanelType) as IOperationPanel;
            if (null == _operationPanel)
            {
                this._globalInfo.LogService.Print(LogLevel.Error, CommonConst.PlatformLogSession,
                    $"The operation panel type {operationPanelType.Name} is not derived from {nameof(IOperationPanel)}");
                throw new TestflowRuntimeException(ModuleErrorCode.OperationPanelError,
                    this._globalInfo.I18N.GetStr("InvalidOIType"));
            }
        }


        private void RegisterEvent()
        {
            _eventActions.Clear();
            _eventActions.Add(new RuntimeDelegate.TestGenerationAction(TestGenStart));
            _eventActions.Add(new RuntimeDelegate.TestGenerationAction(TestGenOver));
            _eventActions.Add(new RuntimeDelegate.TestInstanceStatusAction(TestInstanceStart));
            _eventActions.Add(new RuntimeDelegate.SessionStatusAction(SessionStart));
            _eventActions.Add(new RuntimeDelegate.SequenceStatusAction(SequenceStarted));
            _eventActions.Add(new RuntimeDelegate.StatusReceivedAction(StatusReceived));
            _eventActions.Add(new RuntimeDelegate.SequenceStatusAction(SequenceOver));
            _eventActions.Add(new RuntimeDelegate.SessionStatusAction(SessionOver));
            _eventActions.Add(new RuntimeDelegate.TestInstanceStatusAction(TestInstanceOver));
            IEngineController engineController = _globalInfo.TestflowRunner.EngineController;
            engineController.RegisterRuntimeEvent(_eventActions[0], "TestGenerationStart", 0);
            engineController.RegisterRuntimeEvent(_eventActions[1], "TestGenerationEnd", 0);
            engineController.RegisterRuntimeEvent(_eventActions[2], "TestInstanceStart", 0);
            engineController.RegisterRuntimeEvent(_eventActions[3], "SessionStart", 0);
            engineController.RegisterRuntimeEvent(_eventActions[4], "SequenceStarted", 0);
            engineController.RegisterRuntimeEvent(_eventActions[5], "StatusReceived", 0);
            engineController.RegisterRuntimeEvent(_eventActions[6], "SequenceOver", 0);
            engineController.RegisterRuntimeEvent(_eventActions[7], "SessionOver", 0);
            engineController.RegisterRuntimeEvent(_eventActions[8], "TestInstanceOver", 0);
        }

        private void SessionOver(ITestResultCollection statistics)
        {
            _operationPanel.SessionOver(statistics);
        }

        private void SessionStart(ITestResultCollection statistics)
        {
            _operationPanel.SessionStart(statistics);
        }

        private void TestInstanceStart(IList<ITestResultCollection> statistics)
        {
            _operationPanel.TestStart(statistics);
        }

        private void TestGenStart(ITestGenerationInfo generationInfo)
        {
            _operationPanel.TestGenerationStart(generationInfo);
        }

        private void TestGenOver(ITestGenerationInfo generationInfo)
        {
            _operationPanel.TestGenerationOver(generationInfo);
        }

        private void SequenceStarted(ISequenceTestResult statistics)
        {
            _operationPanel.SequenceStart(statistics);
        }

        private void StatusReceived(IRuntimeStatusInfo statusinfo)
        {
            _operationPanel.StatusReceived(statusinfo);
        }

        private void TestInstanceOver(IList<ITestResultCollection> statistics)
        {
            _operationPanel.TestOver(statistics);
        }

        private void SequenceOver(ISequenceTestResult statistics)
        {
            _operationPanel.SequenceOver(statistics);
        }

        private int _disposedFlag = 0;
        public void Dispose()
        {
            if (_disposedFlag != 0)
            {
                return;
            }
            Thread.VolatileWrite(ref _disposedFlag, 1);
            Thread.MemoryBarrier();
            if (null == this._operationPanelInfo)
            {
                return;
            }
            IEngineController engineController = _globalInfo.TestflowRunner.EngineController;
            engineController.UnregisterRuntimeEvent(_eventActions[0], "TestGenerationStart", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[1], "TestGenerationEnd", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[2], "TestInstanceStart", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[3], "SessionStart", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[4], "SequenceStarted", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[5], "StatusReceived", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[6], "SequenceOver", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[7], "SessionOver", 0);
            engineController.UnregisterRuntimeEvent(_eventActions[8], "TestInstanceOver", 0);
            _eventActions.Clear();

            // 如果OI已经开始成功运行则不执行OI的释放操作
            if (!IsRunning)
            {
                _operationPanel?.Dispose();
            }
            if (null != this._blockHandle)
            {
                this._blockHandle.Set();
                this._blockHandle.Dispose();
                this._blockHandle = null;
            }
        }
    }
}