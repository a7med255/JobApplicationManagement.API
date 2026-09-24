using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JobApplicationManagement.Application.Common.Interfaces
{
    public interface IBackgroundJob
    {
        void Enqueue<T>(Expression<Action<T>> methodCall);
    }
}
