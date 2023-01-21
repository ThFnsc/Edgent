namespace ThFnsc.Edgent;

public class EdgeNukerWorker : BackgroundService
{
    private const string _pattern = "Microsoft Edge.lnk";
    private readonly IReadOnlyList<string> _folders = new[]
    {
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
    };
    private readonly ILogger<EdgeNukerWorker> _logger;
    private readonly List<FileSystemWatcher> _watchers = new();
    private CancellationToken _cancellationToken;
    
    public EdgeNukerWorker(ILogger<EdgeNukerWorker> logger)
    {
        _logger = logger;
        foreach (var folder in _folders)
        {
            var watcher = new FileSystemWatcher(folder, _pattern)
            {
                EnableRaisingEvents=true,
                IncludeSubdirectories=false
            };
            watcher.Created += FileEvent;
            watcher.Renamed += FileEvent;
            
            _watchers.Add(watcher);
        }
    }

    private async void FileEvent(object sender, FileSystemEventArgs args)
    {
        try
        {
            /* Wait to - hopefully - allow the process that created 
             * the file to end writing its content */
            await Task.Delay(100, _cancellationToken);
            SeekAndDestroy();
        }
        catch (Exception ex)
        {
            /* Probably a good thing to have this, since this is 
             * not only an event callback but an async void.
             * Ensures any inner exception won't crash the whole app. */
            _logger.LogError(ex, "Error handling file event");
        }
    }

    public void SeekAndDestroy()
    {
        var results = _folders.SelectMany(f => Directory.EnumerateFiles(f, _pattern));
        foreach (var result in results)
        {
            try
            {
                File.Delete(result);
                _logger.LogInformation("Not on my watch, Microsoft! Edge nuked at {Path}", result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "You've won the battle, Microsoft. But not the war! Failed to nuke edge at {Path}", result);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancellationToken = stoppingToken;
        while (!stoppingToken.IsCancellationRequested)
        {
            SeekAndDestroy();
            await Task.Delay(60000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var watcher in _watchers)
        {
            watcher.Created -= FileEvent;
            watcher.Renamed -= FileEvent;
            watcher.Dispose();
        }
        _watchers.Clear();
        base.Dispose();
    }
}
