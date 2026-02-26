using System.IO;
using System.Text.Json;
using Project.Collections;
using Project.Model;

namespace Project.Repository
{
    public class JsonTaskRepository : ITaskRepository
    {
        private readonly string _filePath;

        public JsonTaskRepository(string filePath)
        {
            _filePath = filePath;
        }

        public IMyCollection<TaskItem> LoadTasks()
        {
            if (!File.Exists(_filePath))
                return new ArrayCollection<TaskItem>();

            string json = File.ReadAllText(_filePath);
            var arr = JsonSerializer.Deserialize<TaskItem[]>(json);
            return arr == null ? new ArrayCollection<TaskItem>() : ArrayCollection<TaskItem>.FromArray(arr);
        }

        public void SaveTasks(IMyCollection<TaskItem> tasks)
        {
            var temp = new ArrayCollection<TaskItem>();
            var it = tasks.GetIterator();
            while (it.HasNext())
                temp.Add(it.Next());

            string json = JsonSerializer.Serialize(temp.ToArray(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
