namespace Testflow.Data.Description
{
    /// <summary>
    /// Action方法描述信息
    /// </summary>
    public interface IActionMethodDescription : IFuncInterfaceDescription
    {
        /// <summary>
        /// Action根名称
        /// </summary>
        string RootName { get; set; }

        /// <summary>
        /// Action名称
        /// </summary>
        string ActionName { get; set; }

        /// <summary>
        /// Action完整名称
        /// </summary>
        string ActionFullName { get; set; }
    }
}