using System.Net.Mail;
using System.Net;

namespace GatePass
{
    public class EmailSender :IEmailSender
    {
        public Task SendEMailAsync(string email, string subject, string message)
        {
            var mail = "nipunasnapdragon@gmail.com";
            var pw = "byrknhhxskdqvfvn";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)

            };
            return client.SendMailAsync(
                new MailMessage(from: mail,
                                to: email,
                                subject,
                                message
                                )
                );
        }
    }
}
