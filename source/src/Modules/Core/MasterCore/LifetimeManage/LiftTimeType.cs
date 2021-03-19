namespace Testflow.MasterCore.LifetimeManage
{
    public enum LifeTimeType
    {
        /// <summary>
        /// 全生命周期，和TestFlow运行时平台同周期
        /// </summary>
        WholeLifeCycle = 0,

        /// <summary>
        /// 下个运行实例执行前
        /// </summary>
        BeforeInstance = 1,

        /// <summary>
        /// 下个运行实例执行前
        /// </summary>
        DuringInstance = 1 << 1,

        /// <summary>
        /// 实例执行结束后
        /// </summary>
        InstanceOver = 1 << 2,

        /// <summary>
        /// 固定时间，单位为秒
        /// </summary>
        SpecifiedTime = 1 << 15,
    }
}