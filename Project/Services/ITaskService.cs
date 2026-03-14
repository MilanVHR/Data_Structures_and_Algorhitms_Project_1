// The View (UI) interacts ONLY with this interface, never directly with the repository
// or the data structures. This keeps the architecture clean and modular.

using Project.Collections;
using Project.Model;

namespace Project.Services
{
    public enum TaskSortField
    {
        Id,
        Description,
        Status,
        CreatedAt
    }

    public interface ITaskService
    {
        // Returns all tasks currently stored in the system.
        IMyCollection<TaskItem> GetAllTasks();

        // Returns a sorted copy of all tasks based on the selected field and direction.
        IMyCollection<TaskItem> GetSortedTasks(TaskSortField sortField, bool ascending);

        // Returns a task by ID, or null if not found.
        TaskItem? GetTaskById(int id);

        // Creates a new task with the given description.
        void AddTask(string description);

        // Removes a task by its ID.
        // Returns true when removed, false when not found.
        bool RemoveTask(int id);

        // Toggles the completion state of a task (true -> false, false -> true).
        // Returns true when toggled, false when not found.
        bool ToggleTaskCompletion(int id);

        // Updates the description of a task by its ID.
        // Returns true when updated, false when not found.
        bool UpdateTaskDescription(int id, string newDescription);
    }
}
