// The Service layer contains all business logic. It decides *how* tasks behave,
// while the Repository layer decides *where* tasks are stored.
//
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
    }
}
