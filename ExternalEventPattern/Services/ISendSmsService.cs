using System.Threading.Tasks;
using ExternalEventPattern.Models;

namespace ExternalEventPattern.Services
{
    public interface ISendSmsService
    {
        Task<bool> SendAsync(SendSmsRequest request);
    }
}