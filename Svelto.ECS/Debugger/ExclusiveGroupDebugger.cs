using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.ECS;

    public static class EGIDDebugger
    {
        static EGIDDebugger()
        {
            _idToName = new Dictionary<uint, string>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type != null && type.IsClass && type.IsSealed && type.IsAbstract) //this means only static classes
                    {
                        var fields = type.GetFields();
                        foreach(var field in fields)
                        {
                            if (field.IsStatic && typeof(ExclusiveGroup).IsAssignableFrom(field.FieldType))
                            {
                                string name  = $"{type.FullName}.{field.Name}";
                                var    group = (ExclusiveGroup)field.GetValue(null);
                                _idToName[(ExclusiveGroupStruct)group] = name;
                            }
                            
                            if (field.IsStatic && typeof(ExclusiveGroupStruct).IsAssignableFrom(field.FieldType))
                            {
                                string name  = $"{type.FullName}.{field.Name}";
                                var    group = (ExclusiveGroupStruct)field.GetValue(null);
                                _idToName[@group] = name;
                            }
                        }
                    }
                }
            }
        }
        
        public static string GetName(this in ExclusiveGroupStruct group)
        {
            if (!_idToName.TryGetValue(group, out var name)) name = $"<undefined:{group}>";

            return name;
        }
        
        public static readonly Dictionary<uint, string> _idToName;
    }

