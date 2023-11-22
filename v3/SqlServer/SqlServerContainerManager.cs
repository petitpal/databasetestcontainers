using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace pipelinetest.v3.SqlServer;

public class SqlServerContainerManager : IContainerManager
{
    private const string DOCKER_IMAGE = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string DEFAULT_SQL_PORT = "1433";


    private readonly DockerClient _dockerClient;

    private bool _disposedValue = false;
    private bool _tornDown = false;

    public string ContainerId { get; private set; }
    public SqlServerContainerParams Parameters { get; private set; }

    public SqlServerContainerManager(DockerClient dockerClient, string containerId, SqlServerContainerParams sqlParameters)
    {
        _dockerClient = dockerClient;
        Parameters = sqlParameters;
        ContainerId = containerId;
    }

    public IContainerManager Init()
    {
        InitAsync().Wait();
        return this;
    }
    
    public async Task<IContainerManager> InitAsync()
    {
        await _dockerClient.Containers.StartContainerAsync(ContainerId, new());
   
        Thread.Sleep(15000);   // let SQL server start-up (can run a query for this later)
        // // var attach = dockerClient.Containers.AttachContainerAsync(TestContainerId, true, new ContainerAttachParameters() { Logs = "" });
        // // attach.Wait();
        return this;
    }

    public IContainerManager TearDown()
    {
        TearDownAsync().Wait();
        return this;
    }

    public async Task<IContainerManager> TearDownAsync()
    {
        if (_tornDown || _dockerClient==null) return this;

        await _dockerClient.Containers.StopContainerAsync(ContainerId, new() { WaitBeforeKillSeconds = 30 }, CancellationToken.None);
        await _dockerClient.Containers.RemoveContainerAsync(ContainerId, new() { Force = true }, CancellationToken.None);
        _tornDown=true;
        return this;
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!_tornDown) TearDown();
                _dockerClient?.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}