using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ErrorHandling
{
    [TestFixture]
    public class ErrorHandlingTests
    {
        [Test]
        public void Wait()
        {
            var taskWithError = Task.Run(() =>
                                         {
                                             Thread.Sleep(100);
                                             throw new Exception();
                                         });

            try
            {
                taskWithError.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine(taskWithError.Status);
        }

        [Test]
        public void SimultaneousFailure()
        {
            var firstTask = Task.Run(() =>
                                     {
                                         Thread.Sleep(100);
                                         throw new ArgumentException();
                                     });
            var secondTask = Task.Run(() =>
                                     {
                                         Thread.Sleep(100);
                                         throw new NullReferenceException();
                                     });

            var aggregatedTask = Task.WhenAll(firstTask, secondTask);
            aggregatedTask.IgnoreExceptions().Wait();

            Console.WriteLine(aggregatedTask.Exception);
        }

        [Test, Ignore]
        public static void UnobservedException()
        {
            Task.Run(() =>
                     {
                         Thread.Sleep(100);
                         throw new Exception();
                     });
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
                                                     {
                                                         Console.WriteLine("Catched!");
                                                         Console.WriteLine(eventArgs.Exception);
                                                         eventArgs.SetObserved();
                                                     };

            Thread.Sleep(200);
            GC.Collect();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }


    public static class TaskExtensions
    {
        public static Task IgnoreExceptions(this Task t)
        {
            return t.ContinueWith(_ => { });
        }
    }
}