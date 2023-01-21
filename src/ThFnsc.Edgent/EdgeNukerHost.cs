using ThFnsc.RemoteControl;

namespace ThFnsc.Edgent;

public static class EdgeNukerHost
{
    public static IHost Build()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureLogging(builder =>
                builder.AddEventLog(conf => 
                    conf.SourceName = Program.Name))
            .ConfigureServices(services =>
                services.AddHostedService<EdgeNukerWorker>())
            .Build();
        
        return host;
    }
}