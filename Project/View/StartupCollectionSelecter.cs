using Project.Collections;
using Project.Model;

namespace Project
{
    public static class StartupCollectionSelecter
    {
        public static IMyCollectionFactory<TaskItem> ChooseFactory()
        {
            List<IMyCollectionFactory<TaskItem>> factories =
                CollectionFactoryResolver.GetAllFactories<TaskItem>();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choose which data structure you want to use:");
                Console.WriteLine();

                for (int i = 0; i < factories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {factories[i].Name}");
                }

                Console.WriteLine();
                Console.Write("Enter your choice: ");

                string? input = Console.ReadLine();

                if (int.TryParse(input, out int choice) &&
                    choice >= 1 &&
                    choice <= factories.Count)
                {
                    return factories[choice - 1];
                }

                Console.WriteLine("Invalid choice. Press Enter to try again.");
                Console.ReadLine();
            }
        }
    }
}