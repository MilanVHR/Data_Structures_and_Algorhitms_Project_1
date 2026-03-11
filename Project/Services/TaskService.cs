// Responsibilities:
// - Add, remove, and toggle tasks
// - Generate unique IDs
// - Communicate with the repository to load/save tasks
// - Use the custom ArrayCollection to store tasks in memory
//
// This class does NOT:
// - Handle user input (View layer)
// - Handle JSON or file operations (Repository layer)
// - Know anything about Spectre.Console or UI

using Project.Collections;
using Project.Model;
using Project.Repository;

namespace Project.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;     // Handles loading/saving tasks
        private readonly IMyCollection<TaskItem> _tasks;  // In-memory storage of tasks

        public TaskService(ITaskRepository repository)
        {
            _repository = repository;

            // Load tasks from the repository (JSON file).
            // The repository returns an ArrayCollection<TaskItem>.
            _tasks = _repository.LoadTasks();
        }

        public IMyCollection<TaskItem> GetAllTasks()
        {
            return _tasks;
        }

        public TaskItem? GetTaskById(int id)
        {
            return _tasks.FindBy(id, (t, key) => t.Id == key);
        }

        // Generates the next available ID by scanning all tasks.
        // Example: if IDs are 1, 2, 5 → next ID = 6.
        private int GetNextId()
        {
            int max = 0;

            var it = _tasks.GetIterator();
            while (it.HasNext())
            {
                var t = it.Next();
                if (t.Id > max)
                    max = t.Id;
            }

            return max + 1;
        }

        public void AddTask(string description)
        {
            var task = new TaskItem
            {
                Id = GetNextId(),
                Description = description,
                Completed = false
            };

            _tasks.Add(task);

            // Save updated list to JSON.
            _repository.SaveTasks(_tasks);
        }

        public bool RemoveTask(int id)
        {
            // Find the task by ID using the custom FindBy method.
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
    }
}
