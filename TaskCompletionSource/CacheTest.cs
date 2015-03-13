using System;
using System.Diagnostics;
using NUnit.Framework;

namespace TaskCompletionSource
{
    [TestFixture]
    public class CacheTest
    {
        private const string BashUrl = "http://bash.im/abyssbest/20150201";
        private Cache cache;

        [TestFixtureSetUp]
        public void Setup()
        {
            cache = new Cache();
        }

        [Test]
        public void Caching()
        {
            var sw = Stopwatch.StartNew();
            var task = cache.GetFirstBytes(BashUrl);
            var length = task.Result.Length;
            Console.WriteLine("Download finished in {0} ms. {1} chars downloaded.", sw.ElapsedMilliseconds, length);
            sw.Restart();
            task = cache.GetFirstBytes(BashUrl);
            length = task.Result.Length;
            Console.WriteLine("Download finished in {0} ms. {1} chars downloaded.", sw.ElapsedMilliseconds, length);
        }

        [Test]
        public void BadUrl()
        {
            var sw = Stopwatch.StartNew();
            var task = cache.GetFirstBytes("qqq");
            task.ContinueWith(_ => { }).Wait();
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.WriteLine(task.Exception);
        }
    }
}