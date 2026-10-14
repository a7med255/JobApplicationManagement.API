using JobApplicationManagement.Application.Features.Jobs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobDtoFactoryTests
{
    [Fact]
    public void Hidden_Salary_Is_Not_Exposed()
    {
        var job = new Job
        {
            Id = 1,
            Title = "Developer",
            Company = new Company { Id = 2, Name = "Acme" },
            MinimumSalary = 1000,
            MaximumSalary = 2000,
            Currency = "USD",
            SalaryPeriod = SalaryPeriod.Monthly,
            IsSalaryVisible = false,
            Status = JobStatus.Published
        };

        var response = JobDtoFactory.ToSummary(job);

        Assert.Null(response.MinimumSalary);
        Assert.Null(response.MaximumSalary);
        Assert.Null(response.Currency);
        Assert.Null(response.SalaryPeriod);
    }
}
