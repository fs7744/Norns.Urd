using BenchmarkDotNet.Running;

namespace SimpleBenchmark
{
    internal static class Program
    {
        private static void Main()
        {
            Check();
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }

        private static void Check()
        {
            new AopTest().Check();
        }
    }
}