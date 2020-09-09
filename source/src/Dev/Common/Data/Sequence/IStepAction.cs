using Testflow.Usr;

namespace Testflow.Data.Sequence
{
    /// <summary>
    /// 步骤内部的行为
    /// </summary>
    public interface IStepAction : ICloneableClass<IStepAction>, ISequenceElement
    {
        /// <summary>
        /// Action的名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Action的描述信息
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Action的类型
        /// </summary>
        string ActionType { get; set; }

        /// <summary>
        /// Action的参数
        /// </summary>
        IParameterDataCollection Parameters { get; set; }
    }
}