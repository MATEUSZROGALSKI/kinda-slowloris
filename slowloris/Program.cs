using Newtonsoft.Json;

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
    Console.Clear();
    try
    {
        BuildScreen(configuration);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Not enough screen space to display statistics.");
    }

    var flooder = new FlooderProcess(configuration);
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
        Console.SetCursorPosition(Console.BufferWidth - 1, 2 + i++);
        Console.WriteLine("#");
    }
    Console.WriteLine(new string('#', Console.WindowWidth));
    Console.WriteLine(new string('#', Console.WindowWidth));
}