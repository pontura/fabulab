using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class SmtpMailSender { 
    void SendEmail(string attachPath) {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress("pkpk@pkpk.gob.ar");
        mail.To.Add("info@yaguar.xyz");
        mail.Subject = "Inventar el mundo pkpk";
        mail.Body = "obra";
        mail.Attachments.Add(new Attachment(attachPath));
        // you can use others too.
        SmtpClient smtpServer = new SmtpClient("smtp.hostinger.com.ar");
        smtpServer.Port = 587;
        smtpServer.Credentials = new System.Net.NetworkCredential("info@yaguar.xyz", "=XLFNy=4") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
        smtpServer.Send(mail);
    }
}
