using System.Text;

namespace EmailManager.Lab.Models
{
    public class Email
    {
        public Email(string from, IList<string> to, IList<string> cc, string subject, string textBody, string htmlBody)
        {
            From = from;
            To = to;
            Cc = cc;
            Subject = subject;
            TextBody = textBody;
            HtmlBody = htmlBody;
        }

        public string From { get; private set; }
        public IList<string> To { get; private set; }
        public IList<string> Cc { get; private set; }
        public string Subject { get; private set; }
        public string TextBody { get; private set; }
        public string HtmlBody { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"From: {From}");
            sb.AppendLine($"To: {To}");
            sb.AppendLine($"Cc: {Cc}");
            sb.AppendLine($"Subject: {Subject}");
            sb.AppendLine($"Body: \n{TextBody}");
            return sb.ToString();
        }
    }
}