using Microsoft.Win32;
using System.CommandLine;
using System.Diagnostics;
using ThFnsc.Edgent;

namespace ThFnsc.RemoteControl;

public class Program
{
    public static readonly string Name = Process.GetCurrentProcess().ProcessName;
    private static readonly Lazy<RegistryKey> _runRegistryKey = new(() =>
    {
        var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        return key ?? throw new InvalidOperationException("Run registry key not found");
    });

    private static void KillOtherInstances(ILogger logger)
    {
        foreach (var process in Process.GetProcessesByName(Name).Where(p => p.Id != Environment.ProcessId))
        {
            process.Kill();
            logger.LogInformation("Ended process with Id {ProcessId}", process.Id);
        }
    }

    private static void SpawnIndependentInstance(ILogger logger)
    {
        Process.Start(
            startInfo: new ProcessStartInfo(
                fileName: Environment.ProcessPath!,
                arguments: "watch")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        logger.LogInformation("Instance started");
    }

    public static async Task Main(string[] args)
    {
        using var host = EdgeNukerHost.Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        var startCommand = new Command("watch", "Starts the service to keep watch");
        var hiddenOption = new Option<bool>("--hidden", "When hidden, the terminal will not be shown");

        startCommand.AddOption(hiddenOption);
        startCommand.SetHandler(hidden =>
        {
            if (!hidden)
                return host.RunAsync();
            SpawnIndependentInstance(logger);
            return Task.CompletedTask;
        }, hiddenOption);

        var runOnceCommand = new Command("clean", "Runs the cleaning algorithm once and exits");
        runOnceCommand.SetHandler(() =>
        {
            var worker = host.Services
                .GetRequiredService<IEnumerable<IHostedService>>()
                .OfType<EdgeNukerWorker>()
                .Single();
            worker.SeekAndDestroy();
        });

        var installCommand = new Command("install", "Installs the service and spins up an instance in the background");
        installCommand.SetHandler(() =>
        {
            KillOtherInstances(logger);
            _runRegistryKey.Value.SetValue(Name, $"{Environment.ProcessPath} watch --hidden");
            logger.LogInformation("Registry key set to run at startup");
            SpawnIndependentInstance(logger);
        });

        var uninstallCommand = new Command("uninstall", "Uninstalls the service and kills all instances");
        uninstallCommand.SetHandler(() =>
        {
            if (_runRegistryKey.Value.GetValue(Name) is null)
                logger.LogInformation("App was not configured to run at startup, ignoring step");
            else
            {
                _runRegistryKey.Value.DeleteValue(Name);
                logger.LogInformation("Removed registry key to run at startup");
            }
            KillOtherInstances(logger);
        });

        var root = new RootCommand()
        {
            startCommand,
            runOnceCommand,
            installCommand,
            uninstallCommand
        };

        await root.InvokeAsync(args);
    }
}
