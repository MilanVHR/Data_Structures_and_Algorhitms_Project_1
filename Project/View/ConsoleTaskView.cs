using System;
using Project.Services;
using Project.Model;
using Project.Collections;

namespace Project.View
{
    public class ConsoleTaskView : ITaskView
    {
        private readonly ITaskService _service;

        public ConsoleTaskView(ITaskService service)
        {
            _service = service;
        }

        private void DisplayTasks()
        {
            Console.Clear();
            Console.WriteLine("==== To-Do List ====\n");

            var it = _service.GetAllTasks().GetIterator();
            while (it.HasNext())
                Console.WriteLine(it.Next());

            Console.WriteLine("\n====================\n");
        }

        private string Prompt(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine() ?? "";
        }

        public void Run()
        {
            while (true)
            {
                DisplayTasks();
                Console.WriteLine("1. Add Task");
                Console.WriteLine("2. Remove Task");
                Console.WriteLine("3. Toggle Task Completion");
                Console.WriteLine("4. Exit");

                string option = Prompt("Choose: ");

                switch (option)
                {
                    case "1":
                        string desc = Prompt("Description: ");
                        _service.AddTask(desc);
                        break;

                    case "2":
                        if (int.TryParse(Prompt("Task ID: "), out int rid))
                            _service.RemoveTask(rid);
                        break;

                    case "3":
                        if (int.TryParse(Prompt("Task ID: "), out int tid))
                            _service.ToggleTaskCompletion(tid);
                        break;

                    case "4":
                        return;

                    default:
                        Console.WriteLine("Invalid option.");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
