using CTC.Repository.IRepository;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CTC.Models;


namespace CTC.Repository.Repository
{
    public class MailService : IMailService
    {
        private readonly EmailSettings _emailSettings;
        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("hamzakanan81@gmail.com", "enhj cbsg fdmx fvsb") //this is the correct App Password
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("hamzakanan81@gmail.com"),
                Subject = subject,
                Body = message,
            };
            mailMessage.To.Add(toEmail);

             return  client.SendMailAsync(mailMessage);
        }

    }

}