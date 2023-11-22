using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace pipelinetest.v3;

public class SqlServerManager
{
    private string _adminConnectionString;

    public SqlServerManager(string adminConnectionString)
    {
        _adminConnectionString = adminConnectionString;
    }


    public SqlServerManager CreateDatabase(string name) {
        var sql = $"CREATE DATABASE {name}";
        RunSql(sql);
        return this;
    }

    public SqlServerManager CreateLogon(string user, string password) {
        var sql = $"CREATE LOGIN {user} WITH PASSWORD='{password}', CHECK_EXPIRATION=OFF;";
        RunSql(sql);
        return this;
    }

    public SqlServerManager AddLogonToDatabase(string database, string logon, string role)
    {
        var sql = $"USE {database}; CREATE USER {logon} FOR LOGIN {logon}; ALTER ROLE {role} ADD MEMBER {logon};";
        RunSql(sql);
        return this;
    }

    public SqlServerManager UpgradeDatabase(string name) {
        Console.WriteLine("UpgradeDatabase");
        var sql = $"USE {name}; CREATE TABLE Foo (ID INT, BAR INT); INSERT INTO Foo (ID, BAR) VALUES (1, 0); INSERT INTO Foo (ID, BAR) VALUES (2, 0);";
        RunSql(sql);
        return this;
    }

    private void RunSql(string query) {
        using (var connection = new SqlConnection(_adminConnectionString)) {
            connection.Open();
            using (var command = new SqlCommand(query, connection)) {
                command.ExecuteNonQuery();
            }           
            connection.Close();
        }
    }
}