using System;

namespace Testflow.Data.Actions
{
    /// <summary>
    /// Action方法属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionMethodAttribute : Attribute
    {
        /// <summary>
        /// Action方法的名称
        /// </summary>
        public string Name { get; set; }

        public ActionMethodAttribute(string name)
        {
            this.Name = name;
        }
    }
}