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

    public enum TaskFilterField
    {
        All,
        ToDo,
        Doing,
        ToReview,
        Done
    }

    public interface ITaskService
    {
        // Indicates whether there are unsaved changes that need to be persisted.
        bool HasUnsavedChanges { get; }

        // Saves all changes to the repository. The view calls this when the user chooses to save.
        void SaveChanges();

        // Returns all tasks currently stored in the system.
        IMyCollection<TaskItem> GetAllTasks();

        // Returns a sorted copy of all tasks based on the selected field and direction.
        IMyCollection<TaskItem> GetSortedTasks(TaskSortField sortField, bool ascending);

        // Returns a filtered copy of all tasks based on the selected filter.
        IMyCollection<TaskItem> GetFilteredTasks(TaskFilterField filterField);

        // Combines sorting and filtering in one method to avoid multiple iterations.
        IMyCollection<TaskItem> GetSortedAndFilteredTasks(TaskSortField sortField, bool ascending, TaskFilterField filterField);
        
        // Returns a task by ID, or null if not found.
        TaskItem? GetTaskById(int id);

        // Creates a new task with the given description.
        // parentTaskId is optional; when set, the task becomes a subtask.
        int AddTask(string description, string? assignedTo, int? parentTaskId = null);

        // Removes a task by its ID.
        // Returns true when removed, false when not found.
        bool RemoveTask(int id);

        // Moves a task to the selected Kanban status.
        // Returns true when moved, false when not found or blocked by unfinished subtasks.
        bool MoveTaskToStatus(int id, TaskStage status, out string? errorMessage);

        // Updates the description of a task by its ID.
        // Returns true when updated, false when not found.
        bool UpdateTaskDescription(int id, string newDescription);

        // Updates the assignee of a task by its ID.
        // Returns true when updated, false when not found.
        bool UpdateTaskAssignee(int id, string assignedTo);

        // Updates the parent relationship of a task by its ID.
        // Returns true when updated, false when not found or invalid.
        bool UpdateTaskParent(int id, int? parentTaskId, out string? errorMessage);

        // Gets the list of available assignees.
        List<string> GetAssignees();
    }
}
