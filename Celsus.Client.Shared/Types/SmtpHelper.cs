using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public class SmtpHelper
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        readonly string mailServer = "smtp.gmail.com";
        readonly int mailServerPort = 587;
        readonly string mailServerUsername = "celsusappgit@gmail.com";
        readonly string mailServerUserPassword = "1234qqqQasd";
        readonly bool mailServerUseSsl = true;

        SmtpClient smtpClient = null;

        public SmtpHelper()
        {
            smtpClient = new SmtpClient(mailServer, mailServerPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mailServerUsername, mailServerUserPassword),
                EnableSsl = mailServerUseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

        }
        public async Task<bool> SendEMail(string subject, string body, List<string> toAddresses)
        {
            var result = false;

            MailMessage mailMessage = new MailMessage
            {
                BodyEncoding = Encoding.UTF8,
                From = new MailAddress(mailServerUsername),
                IsBodyHtml = false,
                Subject = subject,
                Body = body
            };

            foreach (var toAddress in toAddresses)
            {
                if (string.IsNullOrWhiteSpace(toAddress) == false && Regex.IsMatch(toAddress, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z"))
                {
                    mailMessage.To.Add(toAddress);
                }
            }

            if (mailMessage.To.Count == 0)
            {
                logger.Error("No to addresses");
                return result;
            }

            smtpClient.SendCompleted += (sender, e) =>
            {
                try
                {
                    if (e.Cancelled)
                    {
                        logger.Error(e.Error, "Callback cancelled");
                    }

                    if (e.Error != null)
                    {
                        logger.Error(e.Error, "Callback error");
                    }
                }
                catch (ArgumentNullException argument)
                {
                    logger.Error(argument, "Callback error");
                }
                finally
                {
                    smtpClient.Dispose();
                    mailMessage.Dispose();
                }
            };

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                result = true;
            }
            catch (ArgumentOutOfRangeException argumentOutOfRangeException)
            {
                logger.Error(argumentOutOfRangeException, "ArgumentOutOfRangeException");
            }
            catch (ConfigurationErrorsException configurationErrorsException)
            {
                logger.Error(configurationErrorsException, "ConfigurationErrorsException");
            }
            catch (ArgumentNullException argumentNullException)
            {
                logger.Error(argumentNullException, "ArgumentNullException");
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                logger.Error(objectDisposedException, "ObjectDisposedException");
            }
            catch (InvalidOperationException invalidOperationException)
            {
                logger.Error(invalidOperationException, "InvalidOperationException");
            }
            catch (SmtpFailedRecipientsException smtpFailedRecipientsException)
            {
                logger.Error(smtpFailedRecipientsException, "SmtpFailedRecipientsException");
            }
            catch (SmtpFailedRecipientException smtpFailedRecipientException)
            {
                logger.Error(smtpFailedRecipientException, "SmtpFailedRecipientException");
            }
            catch (SmtpException smtpException)
            {
                logger.Error(smtpException, "SmtpException");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }

            return result;
        }
    }
}
