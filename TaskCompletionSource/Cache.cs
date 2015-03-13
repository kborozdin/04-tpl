using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace TaskCompletionSource
{
    public class Cache : ICache
    {
        private const int FirstBytesCount = 100;
        private readonly ConcurrentDictionary<string, byte[]> cache = new ConcurrentDictionary<string, byte[]>();

        public Task<byte[]> GetFirstBytes(string url)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            byte[] result;
            Uri uri;

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                tcs.SetException(new UriFormatException(url));
            else if (cache.TryGetValue(url, out result))
                tcs.SetResult(result);
            else
                using (var wc = new WebClient())
                {
                    wc.OpenReadTaskAsync(uri)
                        .ContinueWith(streamTask =>
                                      {
                                          if (streamTask.IsFaulted)
                                          {
                                              tcs.SetException(streamTask.Exception.InnerExceptions);
                                              return;
                                          }

                                          result = new byte[FirstBytesCount];
                                          streamTask.Result.ReadAsync(result, 0, result.Length).Wait();
                                          cache.TryAdd(url, result);
                                          tcs.SetResult(result);
                                      });
                }

            return tcs.Task;
        }
    }
}