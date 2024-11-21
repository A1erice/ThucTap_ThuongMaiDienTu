namespace ThucTap_ThuongMaiDienTu.Helper;
using System.Net;
using System.Net.Mail;

public static class EmailHelper
{
    public static void SendEmail(string toEmail, string subject, string body)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var smtpServer = config["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(config["EmailSettings:SmtpPort"]);
        var senderEmail = config["EmailSettings:SenderEmail"];
        var senderPassword = config["EmailSettings:SenderPassword"];

        var smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,  // Optional, use HTML formatting for the email
        };
        mailMessage.To.Add(toEmail);

        smtpClient.Send(mailMessage);
    }
}
