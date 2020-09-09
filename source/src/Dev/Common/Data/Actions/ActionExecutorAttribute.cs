using System;

namespace Testflow.Data.Actions
{
    /// <summary>
    /// Action执行器属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionExecutorAttribute : Attribute
    {
        /// <summary>
        /// 属性执行器的名称
        /// </summary>
        public string Name { get; set; }

        public ActionExecutorAttribute(string name)
        {
            this.Name = name;
        }
    }
}