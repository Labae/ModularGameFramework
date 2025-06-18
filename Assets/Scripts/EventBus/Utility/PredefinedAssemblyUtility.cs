using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventBus.Utility
{
    public static class PredefinedAssemblyUtility
    {
        static void AddTypesFromAssembly(Type[] assemblyTypes, Type interfaceType, ICollection<Type> results)
        {
            if (assemblyTypes == null) return;
            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                Type type = assemblyTypes[i];
                if (type != interfaceType && interfaceType.IsAssignableFrom(type))
                {
                    results.Add(type);
                }
            }
        }

        public static List<Type> GetTypes(Type interfaceType)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> types = new();
            for (int i = 0; i < assemblies.Length; i++)
            {
                AddTypesFromAssembly(assemblies[i].GetTypes(), interfaceType, types);
            }

            return types;
        }
    }
}