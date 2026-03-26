// Responsibilities:
// - Convert TaskItem objects to JSON
// - Convert JSON back into TaskItem objects
// - Ensure the application always works even if the file does not exist

using System.IO;
using System.Text.Json;
using Project.Collections;
using Project.Model;

namespace Project.Repository
{
    // Small model used to persist the ID counter.
    file class RepositoryMeta
    {
        public int NextId { get; set; }
    }

    public class JsonTaskRepository : ITaskRepository
    {
        // Path to the JSON file where tasks are stored.
        private readonly string _filePath;
        private readonly IMyCollectionFactory<TaskItem> _collectionFactory;

        // Path to the metadata file that persists the ID counter.
        private string MetaFilePath =>
            Path.Combine(Path.GetDirectoryName(_filePath) ?? ".", "meta.json");

        public JsonTaskRepository(string filePath, IMyCollectionFactory<TaskItem> collectionFactory)
        {
            _filePath = filePath;
            _collectionFactory = collectionFactory;
        }

        public IMyCollection<TaskItem> LoadTasks()
        {
            // If the file does not exist, return an empty collection.
            if (!File.Exists(_filePath))
                return _collectionFactory.Create();

            // Read the JSON file as text.
            string json = File.ReadAllText(_filePath);

            // Convert JSON into an array of TaskItem objects.
            var tasks = JsonSerializer.Deserialize<TaskItem[]>(json);

            // If deserialization fails, return an empty collection.
            if (tasks == null)
                return _collectionFactory.Create();

            // Convert the array into your custom collection implementation.
            return _collectionFactory.CreateFromArray(tasks);
        }

        public void SaveTasks(IMyCollection<TaskItem> tasks)
        {
            // Convert the custom collection into a simple array for JSON serialization.
            TaskItem[] data = new TaskItem[tasks.Count];
            int index = 0;
            var it = tasks.GetIterator();

            while (it.HasNext())
                data[index++] = it.Next();

            // Serialize the array into JSON with indentation for readability.
            string json = JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions { WriteIndented = true }
            );

            // Write the JSON to disk.
            File.WriteAllText(_filePath, json);
        }

        public int LoadNextId()
        {
            if (!File.Exists(MetaFilePath))
                return 1;

            string json = File.ReadAllText(MetaFilePath);
            var meta = JsonSerializer.Deserialize<RepositoryMeta>(json);
            return meta?.NextId ?? 1;
        }

        public void SaveNextId(int nextId)
        {
            string? dir = Path.GetDirectoryName(MetaFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(
                new RepositoryMeta { NextId = nextId },
                new JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(MetaFilePath, json);
        }
    }
}
