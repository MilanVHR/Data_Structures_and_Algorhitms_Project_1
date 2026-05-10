namespace Project.View
{
    public interface ITaskView
    {
        void Run(Func<bool>? shutdownRequested = null);
    }
}