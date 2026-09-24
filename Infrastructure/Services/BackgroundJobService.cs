
using Application.Common.Interfaces;
using Hangfire;
using System.Linq.Expressions;

namespace Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobClient jobClient;

        public BackgroundJobService(IBackgroundJobClient jobClient)
        {
            this.jobClient = jobClient;
        }

        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
             jobClient.Enqueue<T>(methodCall);
        }

        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan timeSpan) { jobClient.Schedule<T>(methodCall, timeSpan); }
    }
}
