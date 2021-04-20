using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.Helper
{
    public static class PasswordReset
    {
        public static void PasswordResetSendEmail(string link) // link

        {
            MailMessage mail = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("mail.t4h4.net");

            mail.From = new MailAddress("taha@t4h4.net");
            mail.To.Add("thyasinerkan@gmail.com");

            mail.Subject = $"www.t4h4.net::Şifre sıfırlama";
            mail.Body = "<h2>Şifrenizi yenilemek için lütfen aşağıdaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href='{link}'>şifre yenileme linki</a>";
            mail.IsBodyHtml = true; // html kodu icerdiginden dolayı true.
            smtpClient.Port = 587;
            smtpClient.Credentials = new System.Net.NetworkCredential("taha@t4h4.net", "!$6*TG.*iwVh");

            smtpClient.Send(mail);
        }
    }
}
