using System.Collections.Generic;

namespace Testflow.Data.Attributes
{
    public interface IAttributeDeclaration
    {

        /// <summary>
        /// 完整名称
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// 属性的目标名称
        /// </summary>
        string Target { get; }

        /// <summary>
        /// 属性的类型名称
        /// </summary>
        string Type { get; }

        /// <summary>
        /// 属性的参数
        /// </summary>
        IList<IAttributeArgument> Arguments { get; }
    }
}