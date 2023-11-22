// using Docker.DotNet;
// using Docker.DotNet.Models;
// using System.Data.SqlClient;

// namespace pipelinetest.tests;

// public class IntegrationUnitTestBase : IDisposable
// {
//     public IntegrationUnitTestBase(int hostPortId) {
//         RunningHostPort = hostPortId <= 0 ? DEFAULT_SQL_HOSTPORT : hostPortId;
//     }

//     DockerClient dockerClient;
//     protected string TestContainerId = String.Empty;
//     private const string DOCKER_IMAGE = "mcr.microsoft.com/mssql/server:2022-latest";
//     private const string DEFAULT_SA_PASSWORD = "%NkGkPL6nR!@AMg*DayN";
//     private const string DEFAULT_APPUSER_USERNAME = "AppServiceAccount";
//     private const string DEFAULT_APPUSER_PASSWORD = "%NkGkPL6nR!@AMg*DayN";
//     private const string DEFAULT_SQL_PORT = "1433";
//     private readonly int DEFAULT_SQL_HOSTPORT = 1433;
//     private const string DEFAULT_DATABASE_NAME = "FooApp";
//     protected int RunningHostPort { get; }

//     protected string ConnectionString {
//         get => new SqlConnectionStringBuilder {
//                 DataSource = $"127.0.0.1,{ RunningHostPort }",
//                 UserID = "sa",
//                 Password = DEFAULT_SA_PASSWORD,
//                 TrustServerCertificate = true,
//                 ConnectTimeout = 30
//         }.ConnectionString;
//     }
     

//     [SetUp]
//     public void Setup()
//     {
//         dockerClient = new DockerClientConfiguration().CreateClient();
//         var createResponse = dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters()
//         {
//             Image = DOCKER_IMAGE,
//             AttachStderr = true,
//             AttachStdin = true,
//             AttachStdout = true,
//             Env = new[] { "ACCEPT_EULA=Y", $"SA_PASSWORD={DEFAULT_SA_PASSWORD}" },
//             ExposedPorts = new Dictionary<string, EmptyStruct>() { { DEFAULT_SQL_PORT, default(EmptyStruct) } },
//             HostConfig = new HostConfig
//             {
//                 PortBindings = new Dictionary<string, IList<PortBinding>> {
//                     {
//                         DEFAULT_SQL_PORT, new List<PortBinding>() { new PortBinding { HostPort = RunningHostPort.ToString() } }
//                     }
//                 }    
//             }
            
//         });
//         createResponse.Wait();
//         TestContainerId = createResponse.Result.ID;

//         dockerClient.Containers.StartContainerAsync(TestContainerId, new()).Wait();
   
//         Thread.Sleep(15000);   // let SQL server start-up (can run a query for this later)
//         // var attach = dockerClient.Containers.AttachContainerAsync(TestContainerId, true, new ContainerAttachParameters() { Logs = "" });
//         // attach.Wait();

//         var CreateDatabaseSQL = $"CREATE DATABASE {DEFAULT_DATABASE_NAME}";

//         var SetupAppServiceAccountSQL =
//             $"""
//                 CREATE LOGIN {DEFAULT_APPUSER_USERNAME} WITH PASSWORD='{DEFAULT_APPUSER_PASSWORD}', CHECK_EXPIRATION=OFF, DEFAULT_DATABASE={DEFAULT_DATABASE_NAME};
//                 USE {DEFAULT_DATABASE_NAME} CREATE USER {DEFAULT_APPUSER_USERNAME} FOR LOGIN {DEFAULT_APPUSER_USERNAME} WITH DEFAULT_SCHEMA=dbo;
//             """;

//         using (var connection = new SqlConnection(ConnectionString)) {
//             connection.Open();
//             using (var command = new SqlCommand(CreateDatabaseSQL, connection)) {
//                 command.ExecuteNonQuery();
//             }
//             using (var command = new SqlCommand(SetupAppServiceAccountSQL, connection)) {
//                 command.ExecuteNonQuery();
//             }
            
//             connection.Close();
//         }
//     }


//     private bool _tornDown = false;
//     private bool disposedValue = false;

//     [TearDown]
//     public void TearDown() {
//         dockerClient?.Containers.StopContainerAsync(TestContainerId, new() { WaitBeforeKillSeconds = 30 }, CancellationToken.None).Wait();
//         dockerClient?.Containers.RemoveContainerAsync(TestContainerId, new() { Force = true }, CancellationToken.None).Wait();       
//         _tornDown=true;
//     }

//     protected virtual void Dispose(bool disposing)
//     {
//         if (!disposedValue)
//         {
//             if (disposing)
//             {
//                 if (!_tornDown) TearDown();
//                 dockerClient.Dispose();
//             }
//             disposedValue = true;
//         }
//     }

//     public void Dispose()
//     {
//         Dispose(disposing: true);
//         GC.SuppressFinalize(this);
//     }
// }