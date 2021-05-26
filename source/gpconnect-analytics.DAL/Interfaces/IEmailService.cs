using System;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IEmailService
    {
        Task SendProcessErrorEmail(Exception exc);
    }
}
