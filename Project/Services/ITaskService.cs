// The View (UI) interacts ONLY with this interface, never directly with the repository
// or the data structures. This keeps the architecture clean and modular.

using Project.Collections;
using Project.Model;

namespace Project.Services
{
    public interface ITaskService
    {
        // Returns all tasks currently stored in the system.
        IMyCollection<TaskItem> GetAllTasks();

        // Creates a new task with the given description.
        void AddTask(string description);

        // Removes a task by its ID.
        void RemoveTask(int id);

        // Toggles the completion state of a task (true -> false, false -> true).
        void ToggleTaskCompletion(int id);

        // Updates the description of a task by its ID.
        void UpdateTaskDescription(int id, string newDescription);
    }
}
