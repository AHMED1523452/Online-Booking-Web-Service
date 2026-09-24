
using System.Linq.Expressions;

namespace Application.Common.Interfaces
{
    public interface IBackgroundJobService
    {
        void Enqueue<T>(Expression<Action<T>> methodCall);
        void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan timeSpan);
    }
}
