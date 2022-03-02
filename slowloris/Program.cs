using Newtonsoft.Json;

if (!File.Exists("configuration.json"))
{
    Console.WriteLine("Unable to find configuration file.");
    return;
}

var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("configuration.json"));
if (configuration != null && configuration.targets.Any())
{
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

        Console.SetCursorPosition("##>> of ".Length, Math.Min(i++, Console.BufferHeight));
        Console.ForegroundColor = stats.isConnected ? connectedFontColor : disconnectedFontColor;
        Console.Write($"{target.host}:");
        Console.ResetColor();

        Console.SetCursorPosition("###>> Total bytes sent: ".Length, Math.Min(i++, Console.BufferHeight));
        Console.Write(stats.bytesSent);

        Console.SetCursorPosition("###>> Connected since: ".Length, Math.Min(i++, Console.BufferHeight));
        Console.Write(stats.connectedEstablished);
        i++;
    }
}

void BuildScreen(Configuration configuration)
{
    var connectedFontColor = ConsoleColor.Green;
    var disconnectedFontColor = ConsoleColor.Red;

    Console.WriteLine(new string('#', Console.BufferWidth));
    Console.WriteLine(new string('#', Console.BufferWidth));
    Console.WriteLine("#>> Statistics:".PadRight(Console.BufferWidth - 1) + "#");
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
        Console.SetCursorPosition(Console.BufferWidth - 1, Math.Min(2 + i++, Console.BufferHeight));
        Console.WriteLine("#");

        Console.Write($"###>> Total bytes sent: {stats.bytesSent}");
        Console.SetCursorPosition(Console.BufferWidth - 1, Math.Min(2 + i++, Console.BufferHeight));
        Console.WriteLine("#");

        Console.Write($"###>> Connected since: {stats.connectedEstablished}");
        Console.SetCursorPosition(Console.BufferWidth - 1, Math.Min(2 + i++, Console.BufferHeight));
        Console.WriteLine("#");

        Console.Write("##");
        Console.SetCursorPosition(Console.BufferWidth - 1, Math.Min(2 + i++, Console.BufferHeight));
        Console.WriteLine("#");
    }
    Console.WriteLine(new string('#', Console.BufferWidth));
    Console.WriteLine(new string('#', Console.BufferWidth));
}