var runtimeConfig = new RuntimeConfiguration(
    args.Contains("--statistics") || args.Contains("-s"));

var slowloris = new Slowloris(runtimeConfig);
slowloris.Start();

Console.CancelKeyPress += (sender, e) =>
{
    slowloris.Stop();
    Console.Clear();
    e.Cancel = true;
};
while (slowloris.IsRunning)
{
    Thread.Sleep(100);
}