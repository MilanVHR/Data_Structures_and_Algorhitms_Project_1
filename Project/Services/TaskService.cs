using Project.Collections;
using Project.Model;
using Project.Repository;

namespace Project.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IMyCollection<TaskItem> _tasks;

        public TaskService(ITaskRepository repository)
        {
            _repository = repository;
            _tasks = _repository.LoadTasks();
        }

        public IMyCollection<TaskItem> GetAllTasks() => _tasks;

        private int GetNextId()
        {
            int max = 0;
            var it = _tasks.GetIterator();
            while (it.HasNext())
            {
                var t = it.Next();
                if (t.Id > max) max = t.Id;
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
            _repository.SaveTasks(_tasks);
        }

        public void RemoveTask(int id)
        {
            var task = _tasks.FindBy(id, (t, key) => t.Id == key);
            if (task != null)
            {
                _tasks.Remove(task);
                _repository.SaveTasks(_tasks);
            }
        }

        public void ToggleTaskCompletion(int id)
        {
            var task = _tasks.FindBy(id, (t, key) => t.Id == key);
            if (task != null)
            {
                task.Completed = !task.Completed;
                _repository.SaveTasks(_tasks);
            }
        }
    }
}
