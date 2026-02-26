using Project.Collections;
using Project.Model;

namespace Project.Repository
{
    public interface ITaskRepository
    {
        IMyCollection<TaskItem> LoadTasks();
        void SaveTasks(IMyCollection<TaskItem> tasks);
    }
}
