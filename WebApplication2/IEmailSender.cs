namespace GatePass
{
    public interface IEmailSender
    {
        Task SendEMailAsync(string email, string subject, string message);
    }
}
