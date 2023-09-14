///////////////////////////////////////////////////////
/// Filename: NetworkObjTests.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using EppNet.Registers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Diagnostics;
using System.Reflection;

namespace EppNet.Tests
{

    [TestClass]
    public class NetworkObjTests
    {

        private ObjectRegistration<TestNetworkObj> _test_reg;
        private TestNetworkObj obj;

        private int Runs = 10000;

        public NetworkObjTests()
        {

            _test_reg = new ObjectRegistration<TestNetworkObj>();
            _test_reg.Compile();

            obj = (TestNetworkObj)_test_reg.NewInstance(7);
        }

        [TestMethod]
        public void TestObjectGenAndMsg()
        {
            TestNetworkObj obj = (TestNetworkObj) _test_reg.NewInstance(7);

            MethodInfo method = obj.GetType().GetMethod("Say");
            _test_reg._method_activators.TryGetValue("Say", out var caller);

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 10000; i++)
            {
                caller.Invoke(obj, "Stevie wonder hit me.", 69);
            }

            watch.Stop();

            long expTime = watch.ElapsedMilliseconds;

            Console.WriteLine($"Invoking with LINQ Expressions 10000 Times: {expTime} ms");

            watch.Start();

            for (int i = 0; i < 10000; i++)
            {
                method.Invoke(obj, new object[] { "Stevie wonder hit me.", 69 });
            }

            watch.Stop();
            Console.WriteLine($"Invoking with MethodInfo 10000 Times: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Averages: Expression: {expTime / 10000f}, MethodInfo: {watch.ElapsedMilliseconds / 10000f}");
        }

        [Benchmark]
        public void RunMethodInfoInvocations()
        {
            MethodInfo method = obj.GetType().GetMethod("Say");

            for (int i = 0; i < Runs; i++)
            {
                method.Invoke(obj, new object[] { "Stevie wonder hit me.", 69 });
            }
        }

        [Benchmark]
        public void RunExpressionInvocations()
        {
            _test_reg._method_activators.TryGetValue("Say", out var caller);
            for (int i = 0; i < Runs; i++)
            {
                caller.Invoke(obj, "Stevie wonder hit me.", 69);
            }
        }

        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<NetworkObjTests>();
        }

    }
}