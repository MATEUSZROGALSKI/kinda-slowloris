using Newtonsoft.Json;
using System.Text;

if (!File.Exists("configuration.json"))
{
    Console.WriteLine("Unable to find configuration file.");
    return;
}
var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("configuration.json"));
if (configuration != null && configuration.targets.Any())
{
    var flooder = new FlooderProcess(configuration);
    flooder.BeginFlood();
    while (flooder.IsFlooding)
    {
        Console.Clear();
        BuildScreen(configuration);
        Thread.Sleep(1000);
    }
}

void BuildScreen(Configuration configuration)
{
    var defaultFontColor = Console.ForegroundColor;
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
        Console.ForegroundColor = defaultFontColor;
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