using Newtonsoft.Json;
using System.Diagnostics;

// check for configuration file
if (!File.Exists("configuration.json"))
{
    // if configuration is not found - just return
    Console.WriteLine("Unable to find configuration file.");
    return;
}

// load configuration
var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("configuration.json"));
if (configuration != null && configuration.targets.Any())
{
    // if statistics are enabled - show them
    if (args.Contains("--statistics") || args.Contains("-s"))
    try
    {
        BuildScreen(configuration);
    }
    catch
    {
        Console.WriteLine("Not enough screen space to display statistics.");
    }

    // create instance of flooder process for current configuration
    var flooder = new FlooderProcess(configuration);
    // register cancel key to stop flooding
    Console.CancelKeyPress += (sender, e) =>
    {
        flooder.Stop();
        Console.Clear();
        e.Cancel = true;
    };

    flooder.BeginFlood();
    while (flooder.IsFlooding)
    {
        try
        {
            // if statistics are enabled - show them
            if (args.Contains("--statistics") || args.Contains("-s"))
                UpdateScreen(configuration);
        }
        catch { }
        Thread.Sleep(100);
    }
}

// just visuals below,
// this might throw some exceptions on smaller screens
// this can be replaces with any UI ( WinForms, WPF, WinAPI, etc. )
// no impact on the application itself

void UpdateScreen(Configuration configuration)
{
    var connectedFontColor = ConsoleColor.Green;
    var disconnectedFontColor = ConsoleColor.Red;
    int i = 3;
    var enumerator = configuration.targets.GetEnumerator();
    while (enumerator.MoveNext())
    {
        var target = enumerator.Current;
        HostStatistics stats = Statistics.Get(target.host);

        Console.SetCursorPosition("##>> of ".Length, i++);
        Console.ForegroundColor = stats.isConnected ? connectedFontColor : disconnectedFontColor;
        Console.Write($"{target.host}:");
        Console.ResetColor();

        Console.SetCursorPosition("###>> Total bytes sent: ".Length, i++);
        Console.Write(stats.bytesSent);

        Console.SetCursorPosition("###>> Connected since: ".Length, i++);
        Console.Write(stats.connectedEstablished);
        i++;
    }
}

void BuildScreen(Configuration configuration)
{
    var connectedFontColor = ConsoleColor.Green;
    var disconnectedFontColor = ConsoleColor.Red;

    Console.Clear();

    try
    {
        Console.SetWindowSize(64, 5 + configuration.targets.Count());
    }
    catch { }

    Console.WriteLine(new string('#', Console.WindowWidth));
    Console.WriteLine(new string('#', Console.WindowWidth));
    Console.WriteLine("#>> Statistics:".PadRight(Console.WindowWidth - 1) + "#");
    int i = 1;
    var enumerator = configuration.targets.GetEnumerator();
    while (enumerator.MoveNext())
    {
        var target = enumerator.Current;
        HostStatistics stats = Statistics.Get(target.host);

        Console.Write("##>> of ");
        Console.ForegroundColor = stats.isConnected ? connectedFontColor : disconnectedFontColor;
        Console.Write($"{target.host}:");
        Console.ResetColor();
        Console.SetCursorPosition(Console.WindowWidth - 1, 2 + i++);
        Console.WriteLine("#");

        Console.Write($"###>> Total bytes sent: {stats.bytesSent}");
        Console.SetCursorPosition(Console.WindowWidth - 1, 2 + i++);
        Console.WriteLine("#");

        Console.Write($"###>> Connected since: {stats.connectedEstablished}");
        Console.SetCursorPosition(Console.WindowWidth - 1, 2 + i++);
        Console.WriteLine("#");

        Console.Write("##");
        Console.SetCursorPosition(Console.WindowWidth - 1, 2 + i++);
        Console.WriteLine("#");
    }
    Console.WriteLine(new string('#', Console.WindowWidth));
    Console.WriteLine(new string('#', Console.WindowWidth));

}