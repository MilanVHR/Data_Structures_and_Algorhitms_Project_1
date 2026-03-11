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
    public class JsonTaskRepository : ITaskRepository
    {
        // Path to the JSON file where tasks are stored.
        private readonly string _filePath;

        public JsonTaskRepository(string filePath)
        {
            _filePath = filePath;
        }

        public IMyCollection<TaskItem> LoadTasks()
        {
            // If the file does not exist, return an empty collection.
            if (!File.Exists(_filePath))
                return new ArrayCollection<TaskItem>();

            // Read the JSON file as text.
            string json = File.ReadAllText(_filePath);

            // Convert JSON into an array of TaskItem objects.
            var tasks = JsonSerializer.Deserialize<TaskItem[]>(json);

            // If deserialization fails, return an empty collection.
            if (tasks == null)
                return new ArrayCollection<TaskItem>();

            // Convert the array into your custom ArrayCollection.
            return ArrayCollection<TaskItem>.FromArray(tasks);
        }

        public void SaveTasks(IMyCollection<TaskItem> tasks)
        {
            // Convert the custom collection into a simple array.
            // This is needed because JSON serialization works best with arrays/lists.
            var temp = new ArrayCollection<TaskItem>();
            var it = tasks.GetIterator();

            while (it.HasNext())
                temp.Add(it.Next());

            TaskItem[] data = temp.ToArray();

            // Serialize the array into JSON with indentation for readability.
            string json = JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions { WriteIndented = true }
            );

            // Write the JSON to disk.
            File.WriteAllText(_filePath, json);
        }
    }
}
