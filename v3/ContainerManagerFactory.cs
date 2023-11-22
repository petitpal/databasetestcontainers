using pipelinetest.v3.SqlServer;

namespace pipelinetest.v3;

public class ContainerManagerFactory
{
    private readonly DockerClient _dockerClient;

    public ContainerManagerFactory(DockerClient dockerClient)
    {
        _dockerClient = dockerClient;
    }

    public async Task<IContainerManager> GetSqlServerContainerManager(SqlServerContainerParams parameters)
    {
        const string DOCKER_IMAGE = "mcr.microsoft.com/mssql/server:2022-latest";
        const string DEFAULT_SQL_PORT = "1433";

        var createResponse = await _dockerClient.Containers.CreateContainerAsync(new() {
                Image = DOCKER_IMAGE,
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true,
                Env = new[] { "ACCEPT_EULA=Y", $"SA_PASSWORD={parameters.SaPassword}" },
                ExposedPorts = new Dictionary<string, EmptyStruct>() { { DEFAULT_SQL_PORT, default(EmptyStruct) } },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>> {
                        {
                            DEFAULT_SQL_PORT, new List<PortBinding>() { new PortBinding { HostPort = parameters.HostPort } }
                        }
                    }    
                }
            });

        // could move this elsewhere
        // Thread.Sleep(15000);   // let SQL server start-up (can run a query for this later)
        Thread.Sleep(15000);   // let SQL server start-up (can run a query for this later)
        // var attach = dockerClient.Containers.AttachContainerAsync(TestContainerId, true, new ContainerAttachParameters() { Logs = "" });
        // attach.Wait();
        // var logs = await _dockerClient.Containers.GetContainerLogsAsync(createResponse.ID, true, new ContainerLogsParameters(), CancellationToken.None);
        
        return new SqlServerContainerManager(_dockerClient, createResponse.ID, parameters);
    }
}