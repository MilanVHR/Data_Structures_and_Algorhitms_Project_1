using Project.Repository;
using Project.Services;
using Project.View;

namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var repo = new JsonTaskRepository("tasks.json");
            var service = new TaskService(repo);
            var view = new ConsoleTaskView(service);

            view.Run();
        }
    }
}
