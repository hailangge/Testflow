namespace Testflow.SlaveCore.Data
{
    /// <summary>
    /// 目标的操作类型
    /// </summary>
    internal enum TargetOperation
    {
        /// <summary>
        /// 无任何操作
        /// </summary>
        None = 0,

        /// <summary>
        /// 变量初始化
        /// </summary>
        VariableInitialization = 1,

        /// <summary>
        /// 参数初始化
        /// </summary>
        ArgumentInitialization = 2,

        /// <summary>
        /// Action初始化
        /// </summary>
        ActionInitialization = 3,

        /// <summary>
        /// Attribute初始化
        /// </summary>
        AttributeInitialization = 4,

        /// <summary>
        /// 执行目标初始化
        /// </summary>
        TargetGeneration = 5,

        /// <summary>
        /// 参数计算
        /// </summary>
        ArgumentCalculation = 6,

        /// <summary>
        /// Action执行
        /// </summary>
        ActionExecution = 7,

        /// <summary>
        /// Attribute计算
        /// </summary>
        AttributeCalculation = 8,

        /// <summary>
        /// 执行方法
        /// </summary>
        FunctionExecution = 9,

        /// <summary>
        /// 目标执行结束
        /// </summary>
        Over = 20
    }
}