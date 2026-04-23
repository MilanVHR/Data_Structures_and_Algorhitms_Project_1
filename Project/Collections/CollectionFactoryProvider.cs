using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Project.Collections;

namespace Project
{
    public static class CollectionFactoryProvider
    {
        public static List<IMyCollectionFactory<T>> GetAllFactories<T>()
        {
            Type targetInterface = typeof(IMyCollectionFactory<T>);

            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t =>
                {
                    if (t.IsGenericTypeDefinition)
                    {
                        try
                        {
                            Type closedType = t.MakeGenericType(typeof(T));
                            if (targetInterface.IsAssignableFrom(closedType))
                                return Activator.CreateInstance(closedType) as IMyCollectionFactory<T>;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (targetInterface.IsAssignableFrom(t))
                            return Activator.CreateInstance(t) as IMyCollectionFactory<T>;
                    }

                    return null;
                })
                .Where(f => f != null)
                .Cast<IMyCollectionFactory<T>>()
                .ToList();
        }
    }
}