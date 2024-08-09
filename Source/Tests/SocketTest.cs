///////////////////////////////////////////////////////
/// Filename: SocketTests.cs
/// Date: SocketTest.cs
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Node;
using EppNet.Processes;

using System.Text;

namespace EppNet.Tests
{

    public class TestEvent : IBufferEvent
    {
        public bool ShouldContinue { set; get; }

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
            this.ShouldContinue = true;
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
                            buffer.CreateAndWrite(null);
                        }
                    }

                    Console.Beep();
                }

                buffer.Dispose();
            }

        }

        public static void Main(string[] args)
        {
            new SocketTest();
        }

    }

}