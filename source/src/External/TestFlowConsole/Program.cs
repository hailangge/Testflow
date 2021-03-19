using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testflow;
using Testflow.Data.Sequence;
using Testflow.External.RunnerInvoker;
using Testflow.Usr;

namespace TestFlowConsole
{
    class Program
    {
        private static bool _raiseException;
        private static bool _quietStop;

        /// <summary>
        /// 运行TestFlow序列
        /// 参数说明：
        /// [-q] : 执行结束后自动停止
        /// [-e] : 出现错误时抛出异常
        /// sequenceFile: 待运行序列所在路径
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("TestFlow command line should specify the sequence file to run.");
            }

            List<string> argList = new List<string>(args.Length);
            argList.AddRange(args.Take(args.Length - 1));
            _raiseException = argList.Any(item => item.Trim().Equals("-e", StringComparison.CurrentCultureIgnoreCase));
            _quietStop = argList.Any(item => item.Trim().Equals("-q", StringComparison.CurrentCultureIgnoreCase));

            string sequenceFile = args[args.Length - 1];
            if (!File.Exists(sequenceFile))
            {
                ReturnError("Sequence file not found.");
                return;
            }

            int dirLength = sequenceFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            string directory = sequenceFile.Substring(0, dirLength);
            TestflowRunnerOptions runnerOptions = new TestflowRunnerOptions()
            {
                Mode = RunMode.Minimal,
                WorkDirectory = directory
            };
            TestflowRunner testflowRunner = TestFlowRunnerInvoker.CreateInstance(runnerOptions);
            testflowRunner.Initialize();
            testflowRunner.RuntimeInitialize();
            ISequenceGroup sequenceGroup =
                testflowRunner.SequenceManager.LoadSequenceGroup(SerializationTarget.File, sequenceFile);
            testflowRunner.RuntimeService.Load(sequenceGroup);
            testflowRunner.EngineController.ExceptionRaised += EngineControllerOnExceptionRaised;
            testflowRunner.EngineController.Start();
            if (!_quietStop)
            {
                Console.WriteLine("Execution over. Press any key to exist.");
                Console.ReadKey();
            }
        }

        private static void EngineControllerOnExceptionRaised(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        private static void ReturnError(string message)
        {
            if (_raiseException)
            {
                throw new ArgumentException(message);
            }
            Console.WriteLine(message);
        }
    }
}
