using System.Net.Mail;

namespace Net_Core_Identity_App.Helper
{
    public class EmailConfirmation
    {
        public static void SendEmail(string link, string email)

        {
            MailMessage mail = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("mail.t4h4.net");

            mail.From = new MailAddress("taha@t4h4.net");
            mail.To.Add(email);

            mail.Subject = $"www.t4h4.net::Email Doğrulama";
            mail.Body = "<h2>Mail adresinizi doğrulamak için lütfen aşağıdaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href='{link}'>email doğrulama linki</a>";
            mail.IsBodyHtml = true;
            smtpClient.Port = 587;
            smtpClient.Credentials = new System.Net.NetworkCredential("taha@t4h4.net", "!$6*TG.*iwVh");

            smtpClient.Send(mail);
        }
    }
}
