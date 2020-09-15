using System;
using Testflow.Runtime;

namespace Testflow.ExtensionBase.Actions
{
    /// <summary>
    /// Action执行结果
    /// </summary>
    public class ActionResult
    {
        /// <summary>
        /// Action执行是否通过
        /// </summary>
        public bool IsPassed { get; set; }

        /// <summary>
        /// 失败类型
        /// </summary>
        public FailedType FailedType { get; set; }

        /// <summary>
        /// 错误异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }
    }
}