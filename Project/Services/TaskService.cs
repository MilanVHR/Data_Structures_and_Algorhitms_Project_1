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
        private readonly IMyCollection<TaskItem> _tasks;  // In-memory storage of tasks
        private int _nextId;

        public TaskService(ITaskRepository repository)
        {
            _repository = repository;

            // Load tasks from the repository (JSON file).
            // The repository returns an ArrayCollection<TaskItem>.
            _tasks = _repository.LoadTasks();

            InitializeNextId();
            EnsureCreatedAtValues();
        }

        public IMyCollection<TaskItem> GetAllTasks()
        {
            return _tasks;
        }

        public IMyCollection<TaskItem> GetSortedTasks(TaskSortField sortField, bool ascending)
        {
            var sorted = new ArrayCollection<TaskItem>(_tasks.Count > 0 ? _tasks.Count : 8);

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
                TaskFilterField.Completed => _tasks.Filter(t => t.Completed),
                TaskFilterField.Pending => _tasks.Filter(t => !t.Completed),
                _ => _tasks.Filter(t => true) // All
            };
        }

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
            int max = 0;

            var it = _tasks.GetIterator();
            while (it.HasNext())
            {
                var t = it.Next();
                if (t.Id > max)
                    max = t.Id;
            }

            _nextId = max + 1;
        }

        public void AddTask(string description)
        {
            var task = new TaskItem
            {
                Id = GetNextId(),
                Description = description,
                Completed = false,
                CreatedAt = DateTime.UtcNow
            };

            _tasks.Add(task);

            // Save updated list to JSON.
            _repository.SaveTasks(_tasks);
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

        public bool ToggleTaskCompletion(int id)
        {
            var task = GetTaskById(id);

            if (task != null)
            {
                // Flip the boolean value.
                task.Completed = !task.Completed;

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

        private int CompareByField(TaskItem left, TaskItem right, TaskSortField sortField, bool ascending)
        {
            int result;

            switch (sortField)
            {
                case TaskSortField.Description:
                    result = string.Compare(left.Description, right.Description, StringComparison.OrdinalIgnoreCase);
                    break;

                case TaskSortField.Status:
                    result = left.Completed.CompareTo(right.Completed);
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
