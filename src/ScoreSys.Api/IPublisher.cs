using System.Threading.Tasks;

namespace ScoreSys.Api
{
    public interface IPublisher<T>
    {
        Task<bool> Publish(T data);
    }
}
