using System.ComponentModel;
using System.Data.SqlClient;

namespace pipelinetest.v3;

public class TestContainerId {

    IContainerManager containerManager;

    // these details only used in this test suite, so not meant to be secure
    const string saPassword = "%NkGkPL6nR!@AMg*DayN";
    
    const string userName = "Snoopy";
    const string userPassword = "sdfsdf£$23PL6nR!@A";
    const string databaseName = "AppDatabase";

    DockerClient _dockerClient;
    ContainerManagerFactory _containerFactory;

    public TestContainerId() {
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _containerFactory = new ContainerManagerFactory(_dockerClient);
    }


    [SetUp]
    public async void Setup() {

        containerManager = await _containerFactory.GetSqlServerContainerManager(new() {
            HostPort = "14333",
            SaPassword = saPassword,
        });
        
        containerManager.Init();

        var adminConnStr = new SqlConnectionStringBuilder {
                DataSource = $"127.0.0.1,14333",
                UserID = "sa",
                Password = saPassword,
                TrustServerCertificate = true,
                ConnectTimeout = 30
        }.ConnectionString;

        var databaseManager = new SqlServerManager(adminConnStr);
        databaseManager
            .CreateDatabase(databaseName)
            .CreateLogon(userName, userPassword)
            .AddLogonToDatabase(databaseName, userName, "db_owner")
            .UpgradeDatabase(databaseName);
    }


    [TearDown]
    public void TearDown() {
        containerManager?.TearDown();
    }


    [Test]
    public void Something() {

        var connstr = new SqlConnectionStringBuilder {DataSource = $"127.0.0.1,{ 14333 }", UserID = userName, Password = userPassword, TrustServerCertificate = true, ConnectTimeout = 30, InitialCatalog = databaseName}.ConnectionString;
        using (var connection = new SqlConnection(connstr)) {
            connection.Open();
            using (var command = new SqlCommand("UPDATE Foo SET BAR=1", connection)) {
                var updates = command.ExecuteNonQuery();
                Assert.That(updates==2);
            }           
            connection.Close();
        }
    }

}