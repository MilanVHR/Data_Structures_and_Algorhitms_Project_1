using Project.Collections;

namespace Project
{

public static class FactorySelector<T>
{
    public static IMyCollectionFactory<T> GetCollectionFactory()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Choose a collection type:");
            Console.WriteLine("1. Array");
            Console.WriteLine("2. Linked List");
            //Console.WriteLine("3. Binary Search Tree");
            Console.Write("Enter your choice: ");
            string option = Console.ReadLine() ?? "";

            switch (option)
            {
                case "1":
                    return new ArrayCollectionFactory<T>();
                case "2":
                    return new LinkedListCollectionFactory<T>();
                //case "3":
                    //return new BinarySearchTreeCollectionFactory<T>();
                default:
                    Console.WriteLine("Invalid choice. Press Enter to try again.");
                    Console.ReadLine();
                    continue;
            }
        }
    }
}
}