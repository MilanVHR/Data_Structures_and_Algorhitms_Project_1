// Responsibilities:
// - Add, remove, and toggle tasks
// - Generate unique IDs
// - Communicate with the repository to load/save tasks
// - Use the custom ArrayCollection to store tasks in memory
using Project.Collections;
using Project.Model;
using Project.Repository;
using System.Collections.Generic;

namespace Project.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;     // Handles loading/saving tasks
        private readonly IMyCollectionFactory<TaskItem> _collectionFactory;
        private readonly IMyCollection<TaskItem> _tasks;  // In-memory storage of tasks
        private int _nextId;
        private List<string> _assignees;

        public TaskService(ITaskRepository repository, IMyCollectionFactory<TaskItem> collectionFactory)
        {
            _repository = repository;
            _collectionFactory = collectionFactory;

            // Load tasks from the repository (JSON file).
            // The repository returns the configured collection implementation.
            _tasks = _repository.LoadTasks();

            EnsureStatusValues();
            InitializeNextId();
            EnsureCreatedAtValues();
            _assignees = _repository.LoadAssignees();
        }

        public IMyCollection<TaskItem> GetAllTasks()
        {
            return _tasks;
        }

        public IMyCollection<TaskItem> GetSortedTasks(TaskSortField sortField, bool ascending)
        {
            IMyCollection<TaskItem> sorted = _collectionFactory.Create(_tasks.Count > 0 ? _tasks.Count : 8);

            var it = _tasks.GetIterator();
            while (it.HasNext())
                sorted.Add(it.Next());

            sorted.Sort((a, b) => CompareByField(a, b, sortField, ascending));
            return sorted;
        }

        public IMyCollection<TaskItem> GetFilteredTasks(TaskFilterField filterField)
        {
            return filterField switch
            {
                TaskFilterField.ToDo => _tasks.Filter(t => t.Status == TaskStage.ToDo),
                TaskFilterField.Doing => _tasks.Filter(t => t.Status == TaskStage.Doing),
                TaskFilterField.ToReview => _tasks.Filter(t => t.Status == TaskStage.ToReview),
                TaskFilterField.Done => _tasks.Filter(t => t.Status == TaskStage.Done),
                _ => _tasks.Filter(t => true) // All
            };
        }
        
        // Combines sorting and filtering in one method to avoid multiple iterations.
        // First filters the tasks, then sorts the filtered list.
        public IMyCollection<TaskItem> GetSortedAndFilteredTasks(TaskSortField sortField, bool ascending, TaskFilterField filterField)
        {
            var filtered = GetFilteredTasks(filterField);
            var sorted = _collectionFactory.Create(filtered.Count > 0 ? filtered.Count : 8);

            var it = filtered.GetIterator();
            while (it.HasNext())
                sorted.Add(it.Next()); // Copy filtered tasks to a new collection for sorting.

            sorted.Sort((a, b) => CompareByField(a, b, sortField, ascending));
            return sorted;
        }

        // This method abstracts away the search logic and keeps it consistent across the service.
        public TaskItem? GetTaskById(int id)
        {
            return _tasks.Find(t => t.Id == id);
        }

        // Generates the next unique ID using a monotonic counter.
        // IDs are never reused within the lifetime of the service.
        private int GetNextId()
        {
            return _nextId++;
        }

        private void InitializeNextId()
        {
            // Scan existing tasks for the highest ID.
            int max = 0;

            var it = _tasks.GetIterator();
            while (it.HasNext())
            {
                var t = it.Next();
                if (t.Id > max)
                    max = t.Id;
            }

            // Take the greater of the persisted counter and the scanned max.
            // This handles both fresh starts and legacy data with no meta file.
            int persisted = _repository.LoadNextId();
            _nextId = Math.Max(persisted, max + 1);
        }

        public int AddTask(string description, string? assignedTo, int? parentTaskId = null)
        {
            int newTaskId = GetNextId();

            if (!ValidateParentAssignment(newTaskId, parentTaskId, out string? validationError))
                throw new InvalidOperationException(validationError ?? "Invalid parent task.");

            var task = new TaskItem
            {
                Id = newTaskId,
                Description = description,
                AssignedTo = assignedTo,
                Status = TaskStage.ToDo,
                Completed = null,
                CreatedAt = DateTime.UtcNow,
                ParentTaskId = parentTaskId
            };

            _tasks.Add(task);

            // If assignedTo is new, add to assignees list
            if (!string.IsNullOrEmpty(assignedTo) && !_assignees.Contains(assignedTo))
            {
                _assignees.Add(assignedTo);
                _repository.SaveAssignees(_assignees);
            }

            // Save updated list and persist the new ID counter value.
            _repository.SaveTasks(_tasks);
            _repository.SaveNextId(_nextId);

            return newTaskId;
        }

        public bool RemoveTask(int id)
        {
            // Find the task by ID using the collection predicate-based Find method.
            var task = GetTaskById(id);

            if (task != null)
            {
                // Promote direct subtasks to top-level tasks when their parent is removed.
                var it = _tasks.GetIterator();
                while (it.HasNext())
                {
                    var candidate = it.Next();
                    if (candidate.ParentTaskId == id)
                        candidate.ParentTaskId = null;
                }

                _tasks.Remove(task);
                _repository.SaveTasks(_tasks);
                return true;
            }

            return false;
        }

        public bool MoveTaskToStatus(int id, TaskStage status, out string? errorMessage)
        {
            errorMessage = null;
            var task = GetTaskById(id);

            if (task == null)
            {
                errorMessage = $"Task with id {id} was not found.";
                return false;
            }

            if (HasIncompleteDescendants(task.Id, new HashSet<int>()))
            {
                errorMessage = "This parent task cannot be moved until all child tasks are done.";
                return false;
            }

            task.Status = status;
            task.Completed = null;

            _repository.SaveTasks(_tasks);
            return true;
        }

        private bool HasIncompleteDescendants(int parentTaskId, HashSet<int> visitedTaskIds)
        {
            if (!visitedTaskIds.Add(parentTaskId))
                return false;

            var it = _tasks.GetIterator();
            while (it.HasNext())
            {
                var candidate = it.Next();
                if (candidate.ParentTaskId != parentTaskId)
                    continue;

                if (candidate.Status != TaskStage.Done)
                    return true;

                if (HasIncompleteDescendants(candidate.Id, visitedTaskIds))
                    return true;
            }

            return false;
        }

        public bool UpdateTaskDescription(int id, string newDescription)
        {
            var task = GetTaskById(id);

            if (task != null)
            {
                task.Description = newDescription;
                _repository.SaveTasks(_tasks);
                return true;
            }

            return false;
        }

        public bool UpdateTaskAssignee(int id, string assignedTo)
        {
            var task = GetTaskById(id);

            if (task != null)
            {
                task.AssignedTo = assignedTo;

                if (!string.IsNullOrEmpty(assignedTo) && !_assignees.Contains(assignedTo))
                {
                    _assignees.Add(assignedTo);
                    _repository.SaveAssignees(_assignees);
                }

                _repository.SaveTasks(_tasks);
                return true;
            }

            return false;
        }

        public bool UpdateTaskParent(int id, int? parentTaskId, out string? errorMessage)
        {
            errorMessage = null;
            var task = GetTaskById(id);

            if (task == null)
            {
                errorMessage = $"Task with id {id} was not found.";
                return false;
            }

            if (!ValidateParentAssignment(id, parentTaskId, out errorMessage))
                return false;

            task.ParentTaskId = parentTaskId;
            _repository.SaveTasks(_tasks);
            return true;
        }

        private bool ValidateParentAssignment(int taskId, int? parentTaskId, out string? errorMessage)
        {
            errorMessage = null;

            if (!parentTaskId.HasValue)
                return true;

            if (parentTaskId.Value == taskId)
            {
                errorMessage = "A task cannot depend on itself.";
                return false;
            }

            var parent = GetTaskById(parentTaskId.Value);
            if (parent == null)
            {
                errorMessage = $"Parent task with id {parentTaskId.Value} does not exist.";
                return false;
            }

            if (WouldCreateCycle(taskId, parentTaskId.Value))
            {
                errorMessage = "Circular dependencies are not allowed.";
                return false;
            }

            return true;
        }

        private bool WouldCreateCycle(int taskId, int parentTaskId)
        {
            var visited = new HashSet<int>();
            int? current = parentTaskId;

            while (current.HasValue)
            {
                if (current.Value == taskId)
                    return true;

                if (!visited.Add(current.Value))
                    return true;

                var currentTask = GetTaskById(current.Value);
                if (currentTask == null)
                    return false;

                current = currentTask.ParentTaskId;
            }

            return false;
        }

        // Helper method to compare two tasks based on the selected sort field and direction.
        private int CompareByField(TaskItem left, TaskItem right, TaskSortField sortField, bool ascending)
        {
            int result;

            switch (sortField)
            {
                case TaskSortField.Description:
                    result = string.Compare(left.Description, right.Description, StringComparison.OrdinalIgnoreCase);
                    break;

                case TaskSortField.Status:
                    result = left.Status.CompareTo(right.Status);
                    break;

                case TaskSortField.CreatedAt:
                    result = left.CreatedAt.CompareTo(right.CreatedAt);
                    break;

                default:
                    result = left.Id.CompareTo(right.Id);
                    break;
            }

            return ascending ? result : -result;
        }
        // This method ensures that all tasks have their Status field correctly set based on the legacy Completed flag.
        // If Completed is true, Status is set to Done. Then Completed is cleared (set to null) for all tasks. 
        // If any changes were made, the updated tasks are saved back to the repository.
        private void EnsureStatusValues()
        {
            bool hasChanges = false;
            var it = _tasks.GetIterator();

            while (it.HasNext())
            {
                var task = it.Next();

                if (task.Completed == true && task.Status != TaskStage.Done)
                {
                    task.Status = TaskStage.Done;
                    hasChanges = true;
                }

                if (task.Completed.HasValue)
                {
                    task.Completed = null;
                    hasChanges = true;
                }
            }

            if (hasChanges)
                _repository.SaveTasks(_tasks);
        }

// This method ensures that all tasks have a valid CreatedAt timestamp.
// If any task has CreatedAt set to the default value (DateTime.MinValue), 
// it is updated to the current UTC time.
        private void EnsureCreatedAtValues()
        {
            bool hasChanges = false;
            var it = _tasks.GetIterator();

            while (it.HasNext())
            {
                var task = it.Next();

                if (task.CreatedAt == default)
                {
                    task.CreatedAt = DateTime.UtcNow;
                    hasChanges = true;
                }
            }

            if (hasChanges)
                _repository.SaveTasks(_tasks);
        }

        public List<string> GetAssignees()
        {
            return _assignees;
        }
    }
}
