using Project.Collections;
using Project.Model;

namespace Project.Services
{
    public interface ITaskService
    {
        IMyCollection<TaskItem> GetAllTasks();
        void AddTask(string description);
        void RemoveTask(int id);
        void ToggleTaskCompletion(int id);
    }
}
