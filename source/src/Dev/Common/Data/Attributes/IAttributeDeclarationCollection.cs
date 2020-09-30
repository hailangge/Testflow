using System.Collections.Generic;

namespace Testflow.Data.Attributes
{
    /// <summary>
    /// 所有定义属性描述的集合
    /// </summary>
    public interface IAttributeDeclarationCollection : IDictionary<string, IAttributeDeclaration>
    {
        /// <summary>
        /// 获取定义的所有属性目标
        /// </summary>
        IList<string> GetDeclaredTargets();

        /// <summary>
        /// 获取指定目标下所有的属性定义
        /// </summary>
        /// <param name="target">目标名称</param>
        IList<string> GetDeclaredTypes(string target);

        /// <summary>
        /// 获取指定的属性对象的声明信息
        /// </summary>
        /// <param name="target">目标名称</param>
        /// <param name="type">属性类型</param>
        IAttributeDeclaration GetAttributeDeclaration(string target, string type);
    }
}