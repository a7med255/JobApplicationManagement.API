using Hangfire;
using JobApplicationManagement.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JobApplicationManagement.Infrastucture.Services
{
    public class HangfireBackgroundJob : IBackgroundJob
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireBackgroundJob(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
            _backgroundJobClient.Enqueue<T>(methodCall);
        }
    }
}
