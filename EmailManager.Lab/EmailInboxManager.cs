using EAGetMail;
using EmailManager.Lab.Models;

namespace EmailManager.Lab
{
    public class EmailInboxManager
    {
        private readonly MailServer _mailServer;
        private readonly MailClient _mailClient;

        private const int TIMEOUT_SECONDS = 60;

        public List<Email> Emails { get; private set; }

        public EmailInboxManager(string server, string email, string password, bool useSsl = false)
        {
            ArgumentNullException.ThrowIfNull(server, nameof(server));
            ArgumentNullException.ThrowIfNull(email, nameof(email));
            ArgumentNullException.ThrowIfNull(password, nameof(password));

            // Mail Server
            _mailServer = new MailServer(server, email, password, ServerProtocol.Imap4);

            // Enable SSL/TLS connection, most modern email server require SSL/TLS by default
            _mailServer.SSLConnection = useSsl;

            // IMAP4 port is 143,  IMAP4 SSL port is 993.
            _mailServer.Port = (useSsl) ? 993 : 143;

            _mailClient = new MailClient("TryIt");

            // Register all events
            _mailClient.OnAuthorized += new MailClient.OnAuthorizedEventHandler(OnAuthorized);
            _mailClient.OnConnected += new MailClient.OnConnectedEventHandler(OnConnected);
            _mailClient.OnIdle += new MailClient.OnIdleEventHandler(OnIdle);
            _mailClient.OnSecuring += new MailClient.OnSecuringEventHandler(OnSecuring);
            _mailClient.OnReceivingDataStream += new MailClient.OnReceivingDataStreamEventHandler(OnReceivingDataStream);
            _mailClient.OnSendCommand += new MailClient.OnSendCommandEventHandler(OnSendCommand);
            _mailClient.OnReceiveResponse += new MailClient.OnReceiveResponseEventHandler(OnReceiveResponse);

            this.Emails = new List<Email>();
        }

        public async Task MonitorNewEmailAsync(CancellationToken cancellationToken)
        {
            try
            {
                _mailClient.Connect(_mailServer);

                await RefreshMailAsync(_mailClient);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    Console.WriteLine("\n----------------------------------------");
                    Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} | Waiting for new emails for {TIMEOUT_SECONDS} seconds...");
                    Console.WriteLine("----------------------------------------\n");
                    if (_mailClient.WaitNewEmail(TIMEOUT_SECONDS))
                    {
                        // New email arrived, Retrieve emails again
                        await RefreshMailAsync(_mailClient);
                        await ReceiveUnreadMailAsync();
                    }
                    else
                    {
                        // No new email, cancel waiting
                        _mailClient.CancelWaitEmail();
                    }
                    Console.WriteLine("----------------------------------------\n");
                }

                Console.WriteLine("Disconnecting ...");
                _mailClient.Logout();

                Console.WriteLine("Completed!");
            }
            catch
            {
                throw;
            }
        }

        public async Task ReceiveUnreadMailAsync()
        {
            _mailClient.GetMailInfosParam.Reset();
            _mailClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

            Console.WriteLine("Retreiving email list ...");
            MailInfo[] infos = _mailClient.GetMailInfos();

            Console.WriteLine("Total {0} unread email(s)", infos.Length);

            for (int i = 0; i < infos.Length; i++)
            {
                Console.WriteLine("Checking {0}/{1} ...", i + 1, infos.Length);
                MailInfo info = infos[i];

                var oMail = _mailClient.GetMail(info);
                var to = Array.ConvertAll<MailAddress, string>(oMail.To, new Converter<MailAddress, string>(c => c.ToString()));
                var cc = Array.ConvertAll<MailAddress, string>(oMail.Cc, new Converter<MailAddress, string>(c => c.ToString()));
                var newEmail = new Email
                (
                    from:oMail.From.ToString(),
                    to:to,
                    cc:cc,
                    subject:oMail.Subject,
                    textBody:oMail.TextBody,
                    htmlBody:oMail.HtmlBody
                );
                Console.WriteLine("* 1 email was stored.");
                this.Emails.Add(newEmail);

                // mark unread email as read, next time this email won't be retrieved again
                if (!info.Read)
                {
                    Console.WriteLine("Mark email as read\r\n");
                    _mailClient.MarkAsRead(info, true);
                }
            }

            await Task.CompletedTask;
        }

        private async Task RefreshMailAsync(MailClient oClient)
        {
            // Create a folder named "inbox" under current directory
            // to save the email retrieved.
            string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
            // If the folder is not existed, create it.
            if (!Directory.Exists(localInbox))
            {
                Directory.CreateDirectory(localInbox);
            }

            oClient.RefreshMailInfos();

            Console.WriteLine("Retrieving email list ...");
            MailInfo[] infos = oClient.GetMailInfos();
            Console.WriteLine("Total {0} email(s)", infos.Length);

            await Task.CompletedTask;
        }

        private void OnConnected(object sender, ref bool cancel) => Console.WriteLine("Connected");

        private void OnQuit(object sender, ref bool cancel) => Console.WriteLine("Quit");

        private void OnReceivingDataStream(object sender, MailInfo info, int received, int total, ref bool cancel) =>
            Console.WriteLine(String.Format("Receiving {0}, {1}/{2}...", info.Index, received, total));

        private void OnIdle(object sender, ref bool cancel)
        {
        }

        private void OnAuthorized(object sender, ref bool cancel) => Console.WriteLine("Authorized");

        private void OnSecuring(object sender, ref bool cancel) => Console.WriteLine("Securing");

        private void OnSendCommand(object sender, byte[] data, bool dataStream, ref bool cancel)
        {
            if (!dataStream)
            {
                Console.Write(System.Text.Encoding.ASCII.GetString(data));
            }
        }

        private void OnReceiveResponse(object sender,byte[] data,bool dataStream, ref bool cancel)
        {
            if (!dataStream)
            {
                Console.Write(System.Text.Encoding.ASCII.GetString(data));
            }
        }
    }
}