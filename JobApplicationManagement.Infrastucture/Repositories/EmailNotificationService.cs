using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace JobApplicationManagement.Infrastucture.Repositories
{
    public class EmailNotificationService : INotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly IGenericRepository<JobApplication> _repository;
        public EmailNotificationService(IGenericRepository<JobApplication> jobCandidateApplicationRepository, ILogger<EmailNotificationService> logger)
        {
            _repository = jobCandidateApplicationRepository;
            _logger = logger;
        }
        public async Task NotifyCandiate(int applicationId)
        {
            var application= await _repository.GetByIdAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("application {applicationId}is not found ", applicationId);
                return;
            }

            _logger.LogInformation("Send Email :  cadidate {CandidateId} has applied to {JobId} and applicationId is {applicationId}",
                application.CandidateId, application.JobId, applicationId);

        }
    }
}
