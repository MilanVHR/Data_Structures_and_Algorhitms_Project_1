using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Project.Collections;

namespace Project
{
    public static class CollectionFactoryResolver
    {
        public static List<IMyCollectionFactory<T>> GetAllFactories<T>()
        {
            Type factoryInterface = typeof(IMyCollectionFactory<T>);

            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    !t.IsInterface &&
                    !t.IsAbstract &&
                    factoryInterface.IsAssignableFrom(t))
                .Select(t => (IMyCollectionFactory<T>)Activator.CreateInstance(t)!)
                .ToList();
        }

        public static IMyCollectionFactory<T> ResolveByName<T>(string name)
        {
            IMyCollectionFactory<T>? factory = GetAllFactories<T>()
                .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (factory == null)
            {
                throw new ArgumentException($"No collection factory found with name '{name}'.");
            }

            return factory;
        }
    }
}