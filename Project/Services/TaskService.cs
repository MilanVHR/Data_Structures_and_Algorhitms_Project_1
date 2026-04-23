// Responsibilities:
// - Add, remove, and toggle tasks
// - Generate unique IDs
// - Communicate with the repository to load/save tasks
// - Use the custom ArrayCollection to store tasks in memory
using Project.Collections;
using Project.Model;
using Project.Repository;

namespace Project.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;     // Handles loading/saving tasks
        private readonly IMyCollectionFactory<TaskItem> _collectionFactory;
        private readonly IMyCollection<TaskItem> _tasks;  // In-memory storage of tasks
        private int _nextId;

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
        }

        public IMyCollection<TaskItem> GetAllTasks()
        {
            return _tasks;
        }

        public IMyCollection<TaskItem> GetSortedTasks(TaskSortField sortField, bool ascending)
        {
            IMyCollection<TaskItem> sorted = _collectionFactory.Create();

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
            var sorted = _collectionFactory.Create();

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

        public void AddTask(string description)
        {
            var task = new TaskItem
            {
                Id = GetNextId(),
                Description = description,
                Status = TaskStage.ToDo,
                Completed = null,
                CreatedAt = DateTime.UtcNow
            };

            _tasks.Add(task);

            // Save updated list and persist the new ID counter value.
            _repository.SaveTasks(_tasks);
            _repository.SaveNextId(_nextId);
        }

        public bool RemoveTask(int id)
        {
            // Find the task by ID using the collection predicate-based Find method.
            var task = GetTaskById(id);

            if (task != null)
            {
                _tasks.Remove(task);
                _repository.SaveTasks(_tasks);
                return true;
            }

            return false;
        }

        public bool MoveTaskToStatus(int id, TaskStage status)
        {
            var task = GetTaskById(id);

            if (task != null)
            {
                task.Status = status;
                task.Completed = null;

                _repository.SaveTasks(_tasks);
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
    }
}
