// Responsibilities:
// - Create the repository (data storage)
// - Create the service (business logic)
// - Create the view (user interface)
// - Start the application by calling view.Run()
//
// This file wires all layers together, but contains no logic itself.
// It is the only place where the layers are allowed to know about each other.

using Project.Repository;
using Project.Services;
using Project.View;
using Project.Collections;
using Project.Model;

namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Path to the JSON file where tasks will be saved.
            string filePath = "Data/tasks.json";

            // Let the user choose the collection implementation.
            IMyCollectionFactory<TaskItem> collectionFactory = StartupCollectionSelecter.ChooseFactory();

            // Create the repository (data persistence layer).
            ITaskRepository repository = new JsonTaskRepository(filePath, collectionFactory);

            // Create the service (business logic layer).
            ITaskService service = new TaskService(repository, collectionFactory);

            // Create the view (user interface layer).
            ITaskView view = new ConsoleTaskView(service);

            // Start the application.
            view.Run();
        }
    }
}