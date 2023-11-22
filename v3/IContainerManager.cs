namespace pipelinetest.v3;

public interface IContainerManager : IDisposable
{
    string ContainerId {get; }
    Task<IContainerManager> InitAsync();
    IContainerManager Init();
    Task<IContainerManager> TearDownAsync();
    IContainerManager TearDown();
}
