using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Toolhouse.Monitoring.Dependencies
{
    public class SmtpDependency : IDependency
    {
        public SmtpDependency(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }

        public DependencyStatus Check()
        {
            // TODO: A more sophisticated test here would be opening up a socket and actually attempting to speak SMTP.
            using (var client = new System.Net.Mail.SmtpClient())
            {
                var message = new MailMessage("test@example.org", "smtpdependencycheck@toolhouse.com", "SMTP test", "");
                client.Send(message);
            }

            return new DependencyStatus(this, true, "");
        }
    }
}
