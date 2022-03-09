internal class ConsoleScreen
{
    private readonly Dictionary<string, int> _statsToLine;
    const int SCREEN_WIDTH = 92;
    const int SCREEN_HEIGHT = 64; // TODO!
    volatile bool _isUpdating = false;

    public ConsoleScreen()
    {
        Statistics.Instance.OnStatisticsUpdated += StatisticsUpdated;
        _statsToLine = new();
    }

    private void StatisticsUpdated(object? sender, StatisticsUpdateEventArgs e)
    {
        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        UpdateScreen();
        _isUpdating = false;
    }

    private void UpdateScreen()
    {
        var connectedFontColor = ConsoleColor.Green;
        var disconnectedFontColor = ConsoleColor.Red;
        foreach (string host in _statsToLine.Keys)
        {
            var stats = Statistics.Instance.GetStatisticsOf(host);
            var line = _statsToLine[host];

            Console.SetCursorPosition(0, line);
            Console.Write("# >> ");
            Console.Write($" {stats.bytesSent,16} | total bytes sent to ");
            Console.ForegroundColor = stats.isConnected ? connectedFontColor : disconnectedFontColor;
            Console.Write($"{host}");
            Console.ResetColor();
            Console.SetCursorPosition(SCREEN_WIDTH - 1, 1 + line);
            Console.WriteLine("#");
            Console.SetCursorPosition(0, 0);
        }
    }

    public void BuildScreen(Configuration configuration)
    {
        var connectedFontColor = ConsoleColor.Green;
        var disconnectedFontColor = ConsoleColor.Red;

        Console.Clear();
        Console.CursorVisible = false;

        Console.WriteLine(new string('#', SCREEN_WIDTH));
        int i = 0;
        foreach(var target in configuration.targets)
        {
            HostStatistics stats = Statistics.Instance.GetStatisticsOf(target.host);
            _statsToLine.Add(target.host, 1 + i);
            Console.Write("# >> ");
            Console.Write($" {stats.bytesSent,16} | total bytes sent to ");
            Console.ForegroundColor = stats.isConnected ? connectedFontColor : disconnectedFontColor;
            Console.Write($"{target.host}");
            Console.ResetColor();
            Console.SetCursorPosition(SCREEN_WIDTH - 1, 1 + i);
            Console.WriteLine("#");
            i++;
        }
        Console.WriteLine(new string('#', SCREEN_WIDTH));

    }
}