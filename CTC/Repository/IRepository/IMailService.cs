
namespace CTC.Repository.IRepository
{
    public interface IMailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);

    }
}
