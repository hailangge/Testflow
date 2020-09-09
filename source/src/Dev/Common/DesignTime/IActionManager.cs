using System.Collections.Generic;
using Testflow.Data;
using Testflow.Data.Sequence;

namespace Testflow.DesignTime
{
    /// <summary>
    /// Action管理器
    /// </summary>
    public interface IActionManager
    {
        /// <summary>
        /// Action根名称
        /// </summary>
        IList<string> ActionRoots { get; }

        /// <summary>
        /// 获取指定Root下所有可用的Action名称
        /// </summary>
        /// <param name="rootName"></param>
        /// <returns></returns>
        IList<string> GetActionNamesUnderRoot(string rootName);

        /// <summary>
        /// 基于action名称创建action对象
        /// </summary>
        IStepAction CreateAction(string root, string actionName);

        /// <summary>
        /// 基于action名称创建action对象
        /// </summary>
        IStepAction CreateAction(string actionFullName);

        /// <summary>
        /// 获取指定Action的参数列表
        /// </summary>
        IArgumentCollection GetParameterTypes(string root, string actionName);

        /// <summary>
        /// 获取指定Action的参数列表
        /// </summary>
        IArgumentCollection GetParameterTypes(string actionFullName);

        /// <summary>
        /// 获取某个指定位置参数的可用值，如果当前参数的值是非枚举项，则返回null
        /// </summary>
        IList<string> GetParameterValueSources(IStepAction action, int paramIndex);
    }
}