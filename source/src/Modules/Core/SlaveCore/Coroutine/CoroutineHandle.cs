using System;
using System.Diagnostics;
using System.Threading;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Data;
using Testflow.SlaveCore.Debugger;
using Testflow.SlaveCore.Runner.Expression;
using Testflow.SlaveCore.Runner.Model;

namespace Testflow.SlaveCore.Coroutine
{
    internal class CoroutineHandle : IDisposable
    {
        private int _stateValue;

        /// <summary>
        /// TODO 当前执行信息，后续通过内部操作。目前使用外部更新处理
        /// </summary>
        public ExecutionInfo ExecutionInfo { get; }

        public CoroutineState State
        {
            get { return (CoroutineState) _stateValue; }
            set
            {
                int newStateValue = (int)value;
                // 除了新旧状态都是运行态以外，协程的状态只能从前向后
                if (newStateValue <= _stateValue && !IsRunState(_stateValue) && !IsRunState(newStateValue))
                {
                    return;
                }
                Thread.VolatileWrite(ref _stateValue, newStateValue);
            }
        }

        public int Id { get; }
        private readonly AutoResetEvent _blockEvent;

        public ExecutionTrack ExecutionTracker { get; }

        /// <summary>
        /// 表达式解析器
        /// </summary>
        public ExpressionProcessor ExpressionProcessor { get; }

        public event Action<StepTaskEntityBase> PreListener;
        public event Action<StepTaskEntityBase> PostListener;

        /// <summary>
        /// 协程开始执行时间
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// 协程执行结束时间
        /// </summary>
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// 协程执行时间
        /// </summary>
        public long ElapsedTicks { get; private set; }

        /// <summary>
        /// 用于计算协程全局执行时间的计时器
        /// </summary>
        private readonly Stopwatch _activeTimingTimer;

        /// <summary>
        /// 用于计算单个Step单次执行时间的计时器
        /// </summary>
        private readonly Stopwatch _executionTimer;

        /// <summary>
        /// 时钟频率
        /// </summary>
        private readonly long _timerFrequency;

        public CoroutineHandle(SlaveContext slaveContext, int id)
        {
            this.State = CoroutineState.Idle;
            this.Id = id;
            this._blockEvent = new AutoResetEvent(false);
//            this.ExecutionTracker = new ExecutionTrack(Constants.ExecutionTrackerSize);
            this.StartTime = DateTime.MinValue;
            this.EndTime = DateTime.MinValue;
            this._activeTimingTimer = new Stopwatch();
            this._executionTimer = new Stopwatch();
            this._timerFrequency = Stopwatch.Frequency;
            this.ElapsedTicks = -1;
            ExpressionProcessor = new ExpressionProcessor(slaveContext, id);
            ExecutionInfo = new ExecutionInfo(slaveContext.SessionId, id);
        }

        public void SequenceGenerationEnd()
        {
            ExpressionProcessor.TrimExpressionCache();
        }

        #region 全局控制

        public void Start()
        {
            this.State = CoroutineState.Running;
            this.StartTime = DateTime.Now;
            this.ElapsedTicks = -1;
            this._activeTimingTimer.Reset();
            this._activeTimingTimer.Start();
        }

        public void Pause()
        {
            this.State = CoroutineState.Blocked;
            this._activeTimingTimer.Stop();
        }

        public void Continue()
        {
            this.State = CoroutineState.Running;
            this._activeTimingTimer.Start();
        }

        public void Stop()
        {
            this._activeTimingTimer.Stop();
            this.ElapsedTicks = this._activeTimingTimer.ElapsedTicks;
            this.EndTime = DateTime.Now;
            this.State = CoroutineState.Over;
        }

        public void WaitSignal()
        {
            Pause();
            _blockEvent.WaitOne();
            Continue();
        }

        public void SetSignal()
        {
            _blockEvent.Set();
        }

        #endregion

        #region 执行目标更新

        public void SequenceStart(int sequenceIndex)
        {
            this.ExecutionInfo.Initialize(sequenceIndex);
        }

        public void StepStart(StepTaskEntityBase step)
        {
            this.ExecutionInfo.Initialize(step);
        }

        public void StepOver(StepTaskEntityBase step)
        {
            this.ExecutionInfo.TargetOver(step);
        }

        public void SequenceOver(int sequenceIndex)
        {
            this.ExecutionInfo.SequenceOver();
        }

        public void ExecuteTarget(TargetOperation target, string targetName, params string[] arguments)
        {
            this.ExecutionInfo.SetTarget(target, targetName, arguments);
        }
        
        public void StartTiming()
        {
            this._executionTimer.Reset();
            this._executionTimer.Start();
        }

        public long EndTiming()
        {
            this._executionTimer.Stop();
            // 计算精确到微秒级别的数据
            return (long)((double)this._executionTimer.ElapsedTicks * 1E6 / this._timerFrequency );
        }

        #endregion

        #region 监听器

        public void OnPreListener(StepTaskEntityBase stepEntity)
        {
            PreListener?.Invoke(stepEntity);
        }

        public void OnPostListener(StepTaskEntityBase stepEntity)
        {
            PostListener?.Invoke(stepEntity);
        }

        #endregion

        private int _disposedFlag = 0;
        public void Dispose()
        {
            if (this._disposedFlag != 0)
            {
                return;
            }
            Thread.VolatileWrite(ref this._disposedFlag, 1);
            Thread.MemoryBarrier();
            if (IsRunState(_stateValue))
            {
                Stop();
            }
            _blockEvent?.Dispose();
            ExecutionTracker?.Dispose();
        }

        private static bool IsRunState(int stateValue)
        {
            return stateValue == (int) CoroutineState.Running || stateValue == (int) CoroutineState.Blocked;
        }

    }
}