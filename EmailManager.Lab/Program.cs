// See https://aka.ms/new-console-template for more information
using EmailManager.Lab;
using EmailManager.Lab.Models;

Console.WriteLine("Email");

var cancellationTokenSource = new CancellationTokenSource();

// Task for UI thread, so we can call Task.Wait wait on the main thread.
Task.Run(() =>
{
    Console.WriteLine("Press 'c' to cancel within 3 seconds after work begins.");
    Console.WriteLine("Or let the task time out by doing nothing.");
    if (Console.ReadKey(true).KeyChar == 'c')
        cancellationTokenSource.Cancel();
});

var emailInboxManager = new EmailInboxManager("pop.gmail.com", "everton.devbr@gmail.com", "nnxzxiljwlxjtyht", true);
var emailList = new List<Email>();

_ = emailInboxManager.MonitorNewEmailAsync(cancellationTokenSource.Token);

cancellationTokenSource.Dispose();

Console.WriteLine("\n===================================");
Console.WriteLine($"{emailInboxManager.Emails.Count} emails recived...");
Console.WriteLine("===================================");
foreach (var email in emailInboxManager.Emails)
{
    Console.WriteLine("-------------------------------------");
    Console.WriteLine(email);
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();