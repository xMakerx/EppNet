///////////////////////////////////////////////////////
/// Filename: SocketTests.cs
/// Date: July 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Node;
using EppNet.Processes;

using System.Globalization;
using System.Text;

namespace EppNet.Tests
{

    public class TestEvent : IBufferEvent
    {

        public int Number { set; get; }

        public StringBuilder Builder = new();
        public bool Disposed { internal set; get; }

        public void Cleanup()
        {
            Builder?.Clear();
        }

        public void Dispose()
        {
            Disposed = true;
            this.Cleanup();
        }

        public void Initialize()
        {
            this.Disposed = false;
            this.Number = Random.Shared.Next(0, 100);
        }

        public void SetNumber(int number)
        {
            this.Number = number;
        }

        public bool IsDisposed() => Disposed;
    }

    public class TestEventHandler : IBufferEventHandler<TestEvent>
    {

        public bool Handle(TestEvent e)
        {
            e.Builder.Append("Your number: ");
            e.Builder.Append(e.Number);
            return true;
        }

    }

    public class TestEventHandler2 : IBufferEventHandler<TestEvent>
    {
        public bool Handle(TestEvent e)
        {

            if (e.Number < 10)
                e.Builder.Append(":)");
            else
                e.Builder.Append("...");

            e.Number = 500;

            return true;
        }
    }

    public class TestEventHandler3 : IBufferEventHandler<TestEvent>
    {

        public bool Handle(TestEvent e)
        {
            if (e.Number != 500)
                e.Builder.AppendLine("BAD!");
            else
                e.Builder.AppendLine("..Good");

            var s = e.Builder.ToString();
            Task.Run(() => Console.Write(s));
            return true;
        }

    }

    public class SocketTest
    {

        public SocketTest()
        {

            using (NetworkNode node = EppNet.CreateServer(4296, "SocketTest", Exceptions.ExceptionStrategy.LogOnly, LogLevelFlags.All))
            {
                node.TryStart();

                MultithreadedBufferBuilder<TestEvent> mtbb = new(node);
                mtbb.ThenUseHandlers(new TestEventHandler())
                    .ThenUseHandlers(new TestEventHandler2())
                    .ThenUseHandlers(new TestEventHandler3());
                MultithreadedBuffer<TestEvent> buffer = mtbb.Build();

                buffer.SetLogLevel(LogLevelFlags.All);
                buffer.Start();

                ConsoleKeyInfo info = Console.ReadKey(true);
                while (info.Key != ConsoleKey.Escape)
                {
                    info = Console.ReadKey();

                    if (info.Key == ConsoleKey.Enter)
                    {

                        for (int i = 0; i < 1 + Random.Shared.Next(5); i++)
                        {
                            buffer.CreateAndWrite();
                        }
                    }

                    Console.Beep();
                }

                buffer.Dispose();
            }

        }

        public static string GetTabs(int tabs)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < tabs; i++)
            {
                sb.Append('\u0020');
                sb.Append('\u0020');
                sb.Append('\u0020');
                sb.Append('\u0020');
            }

            return sb.ToString();
        }

        public static void Main(string[] args)
        {

            string[] primitives = { "byte", "sbyte", "ushort", "short", "uint", "int", "ulong", "long", "float", "double" };
            TextInfo info = new CultureInfo("en-US", false).TextInfo;

            using (StreamWriter writer = new StreamWriter("nextensions_generated.cs"))
            {
                writer.WriteLine("using System;");
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("namespace EppNet.Utilities");
                writer.WriteLine("{");
                writer.WriteLine(GetTabs(1) + "public static class NumberExtensions");
                writer.WriteLine(GetTabs(1) + "{");
                writer.WriteLine(" ");
                writer.WriteLine(" ");

                foreach (string primitive in primitives)
                {
                    string title = info.ToTitleCase(primitive);
                    writer.WriteLine(GetTabs(2) + $"#region {title} Conversions");

                    for (int i = 0; i < primitives.Length; i++)
                    {
                        string sourceStr = primitives[i];
                        string sourceTitle = info.ToTitleCase(sourceStr);

                        string methodName = $"_Internal_{sourceTitle}To{title}";

                        if (i - 1 >= 0)
                            writer.WriteLine();

                        writer.WriteLine(GetTabs(2) + $"private static {primitive} {methodName}({sourceStr} input)");
                        writer.WriteLine(GetTabs(2) + "{");
                        //writer.WriteLine(GetTabs(3) + $"if ({primitive}.MinValue <= input && input <= {primitive}.MaxValue)");
                        writer.WriteLine(GetTabs(3) + $"return ({primitive}) input;");
                        //writer.WriteLine();
                        //writer.WriteLine(GetTabs(3) + "throw new ArgumentOutOfRangeException(nameof(input));");
                        writer.WriteLine(GetTabs(2) + "}");
                        writer.WriteLine();
                        writer.WriteLine();
                        writer.WriteLine(GetTabs(2) + $"private static Func<{sourceStr}, {primitive}> _{sourceStr}2{title} = {methodName};");
                    }

                    writer.WriteLine(GetTabs(2) + $"#endregion");
                    writer.WriteLine();
                }

                writer.WriteLine();
                writer.WriteLine(GetTabs(2) + "private static Dictionary<Type, Dictionary<Type, object>> _lookupTable = new()");
                writer.WriteLine(GetTabs(2) + "{");

                for (int j = 0; j < primitives.Length; j++)
                {
                    string primitive = primitives[j];
                    string title = info.ToTitleCase(primitive);
                    writer.WriteLine(GetTabs(3) + "{");
                    writer.WriteLine(GetTabs(4) + $"typeof({primitive}), new()");
                    writer.WriteLine(GetTabs(4) + "{");


                    for (int i = 0; i < primitives.Length; i++)
                    {
                        string sourceStr = primitives[i];

                        writer.Write(GetTabs(5) + "{ ");
                        writer.Write($"typeof({sourceStr}), _{sourceStr}2{title} ");

                        writer.WriteLine("},");
                    }

                    writer.WriteLine(GetTabs(4) + "}");

                    if (j + 1 < primitives.Length)
                        writer.Write(GetTabs(3) + "},");
                    else
                        writer.WriteLine(GetTabs(3) + "}");

                    writer.WriteLine();
                }

                //writer.WriteLine("\t\t}");

                writer.WriteLine(GetTabs(2) + "};");
                writer.WriteLine(GetTabs(1) + "}");
                writer.WriteLine("}");
            }
        }

    }

}