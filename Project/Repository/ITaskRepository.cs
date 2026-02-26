// The Service layer depends on this interface, not on the concrete implementation.
// This allows you to replace the JSON repository with another storage method
// without changing the rest of the application.

using Project.Collections;
using Project.Model;

namespace Project.Repository
{
    public interface ITaskRepository
    {
        // Loads all tasks from the storage medium (JSON file in this project).
        // Returns a custom collection containing TaskItem objects.
        IMyCollection<TaskItem> LoadTasks();

        // Saves all tasks to the storage medium.
        // The service layer calls this after every change.
        void SaveTasks(IMyCollection<TaskItem> tasks);
    }
}
