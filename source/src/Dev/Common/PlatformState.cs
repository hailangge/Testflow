namespace Testflow
{
    /// <summary>
    ///  平台的当前状态
    /// </summary>
    public enum PlatformState
    {
        /// <summary>
        /// 平台不可用
        /// </summary>
        NotAvailable = -1,

        /// <summary>
        /// 设计时状态
        /// </summary>
        Designtime = 0,

        /// <summary>
        /// 运行时状态
        /// </summary>
        Runtime = 1
    }
}