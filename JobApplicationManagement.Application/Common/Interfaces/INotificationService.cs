using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplicationManagement.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task NotifyCandiate(int applicationId);
    }
}
