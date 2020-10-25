using System;
using System.IO;
using System.Reflection;
using Testflow.Usr;

namespace Testflow.External.RunnerInvoker
{
    /// <summary>
    /// TestFlow平台外部调用类
    /// </summary>
    public static class TestFlowRunnerInvoker
    {
        /// <summary>
        /// 创建TestFlow平台实例
        /// </summary>
        /// <param name="options">TestFlow平台创建参数</param>
        /// <returns>TestFlow平台入口类</returns>
        public static TestflowRunner CreateInstance(TestflowRunnerOptions options)
        {
            string testflowHome = Environment.GetEnvironmentVariable("TESTFLOW_HOME");
            if (string.IsNullOrWhiteSpace(testflowHome))
            {
                throw new TestflowException(-1, "TestFlow platform cannot be found.");
            }
            if (!testflowHome.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                testflowHome += Path.DirectorySeparatorChar;
            }
            string activatorPath = $"{testflowHome}TestflowLauncher.dll";
            Assembly activatorAssembly = Assembly.LoadFrom(activatorPath);
            Type launcherType = activatorAssembly.GetType("Testflow.Loader.TestflowActivator", true);
            Type[] argumentTypes = new Type[] { typeof(TestflowRunnerOptions) };
            MethodInfo createMethod = launcherType.GetMethod("CreateRunner", BindingFlags.Public | BindingFlags.Static,
                null, argumentTypes, null);
            if (null == createMethod)
            {
                throw new TestflowException(-1, "TestFlow platform cannot be found.");
            }
            return (TestflowRunner) createMethod.Invoke(null, new object[] {options});
        }
    }
}
