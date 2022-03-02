using Newtonsoft.Json;

if (!File.Exists("configuration.json"))
{
    Console.WriteLine("Unable to find configuration file.");
    return;
}

var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("configuration.json"));
if (configuration != null && configuration.targets.Any())
{
    BuildScreen(configuration);

    var flooder = new FlooderProcess(configuration);
    Console.CancelKeyPress += (sender, e) =>
    {
        flooder.Stop();
        e.Cancel = true;
    };

    flooder.BeginFlood();
    while (flooder.IsFlooding)
    {
        UpdateScreen(configuration);
        Thread.Sleep(100);
    }
}

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

        Console.WriteLine($"###>> Connected since: {stats.connectedEstablished}");
        Console.SetCursorPosition(Console.WindowWidth - 1, 2 + i++);
        Console.WriteLine("#");

        Console.WriteLine("##");
        Console.SetCursorPosition(Console.WindowWidth - 1, 2 + i++);
        Console.WriteLine("#");
    }
    Console.WriteLine(new string('#', Console.WindowWidth));
    Console.WriteLine(new string('#', Console.WindowWidth));
}