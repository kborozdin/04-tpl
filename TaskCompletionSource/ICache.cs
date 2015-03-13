using System.Threading.Tasks;

namespace TaskCompletionSource
{
    public interface ICache
    {
        Task<byte[]> GetFirstBytes(string url);
    }
}