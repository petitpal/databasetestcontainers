namespace pipelinetest.v3.SqlServer;

public class SqlServerContainerParams
{
    public SqlServerContainerParams()
    {
        SaPassword = string.Empty;
        HostPort = string.Empty;
    }
    
    public string SaPassword {get ; set; }
    public string HostPort { get; set; }
}