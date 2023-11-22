// using System.Data.SqlClient;
// using Docker.DotNet;
// using Docker.DotNet.Models;

// [AttributeUsage(AttributeTargets.Class)]
// public class SqlServerIntegrationTest : Attribute
// {
//      DockerClient dockerClient;
//      protected string TestContainerId = String.Empty;

//     private string DatabaseName = String.Empty;
//     private string SaPassword = String.Empty;
//     private string UserLogon = String.Empty;
//     private string UserPassword = String.Empty;
//     private int HostPortNumber = 0;

//     public SqlServerIntegrationTest(
//         string databaseName,
//         string saPassword,
//         string userLogon,
//         string userPassword,
//         int hostPortNumber)
//     {
//         this.DatabaseName = databaseName;
//         this.SaPassword = saPassword;
//         this.UserLogon = userLogon;
//         this.UserPassword = userPassword;
//         this.HostPortNumber = hostPortNumber;
//         dockerClient = new DockerClientConfiguration().CreateClient();
//         this.Setup();
//     }

//     protected string ConnectionString {
//         get => new SqlConnectionStringBuilder {
//                 DataSource = $"127.0.0.1,{ HostPortNumber }",
//                 UserID = "sa",
//                 Password = SaPassword,
//                 TrustServerCertificate = true,
//                 ConnectTimeout = 30
//         }.ConnectionString;
//     }

//     public void Setup()
//     {
//         const string DOCKER_IMAGE = "mcr.microsoft.com/mssql/server:2022-latest";
//         const string DEFAULT_SQL_PORT = "1433";

//         // dockerClient = new DockerClientConfiguration().CreateClient();
//         var createResponse = dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters()
//         {
//             Image = DOCKER_IMAGE,
//             AttachStderr = true,
//             AttachStdin = true,
//             AttachStdout = true,
//             Env = new[] { "ACCEPT_EULA=Y", $"SA_PASSWORD={SaPassword}" },
//             ExposedPorts = new Dictionary<string, EmptyStruct>() { { DEFAULT_SQL_PORT, default(EmptyStruct) } },
//             HostConfig = new HostConfig
//             {
//                 PortBindings = new Dictionary<string, IList<PortBinding>> {
//                     {
//                         DEFAULT_SQL_PORT, new List<PortBinding>() { new PortBinding { HostPort = HostPortNumber.ToString() } }
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

//         var CreateDatabaseSQL = $"CREATE DATABASE {DatabaseName}";

//         var SetupAppServiceAccountSQL =
//             $"""
//                 CREATE LOGIN {UserLogon} WITH PASSWORD='{UserPassword}', CHECK_EXPIRATION=OFF, DEFAULT_DATABASE={DatabaseName};
//                 USE {DatabaseName} CREATE USER {UserLogon} FOR LOGIN {UserLogon} WITH DEFAULT_SCHEMA=dbo;
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