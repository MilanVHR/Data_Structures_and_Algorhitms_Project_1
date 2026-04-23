using System.Reflection;
using Project.Collections;

namespace Project
{
    public static class CollectionFactoryFinder
    {   
        public static List<IMyCollectionFactory<T>> GetAllFactories<T>()
        {
            var factoryType = typeof(IMyCollectionFactory<T>);

            return AppDomain.CurrentDomain.GetAssemblies() // Search all loaded assemblies
                .SelectMany(s => s.GetTypes())
                .Where(t => factoryType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => 
                {
                    try 
                    {
                        return (IMyCollectionFactory<T>)Activator.CreateInstance(t)!;
                    }
                    catch 
                    {
                        return null;
                    }
                })
                .Where(f => f != null)
                .ToList()!;
        }
    }
}

        //         public static IMyCollectionFactory<T> ResolveByName<T>(string name)
        //         {
        //             IMyCollectionFactory<T>? factory = GetAllFactories<T>()
        //                 .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        //             if (factory == null)
        //             {
        //                 throw new ArgumentException($"No collection factory found with name '{name}'.");
        //             }

        //             return factory;
        //         }
        //     }
        // }