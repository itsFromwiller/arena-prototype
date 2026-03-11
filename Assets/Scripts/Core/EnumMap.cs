using System;
using System.Collections.Generic;

namespace Arena.Core
{
    public static class EnumMap<T> where T : Enum
    {
        private static readonly Dictionary<string, T> NameToValueMap;
        private static readonly Dictionary<T, string> ValueToNameMap;

        static EnumMap()
        {
            NameToValueMap = new Dictionary<string, T>();
            ValueToNameMap = new Dictionary<T, string>();

            var values = (T[])Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                string name = value.ToString();
                NameToValueMap[name] = value;
                ValueToNameMap[value] = name;
            }
        }

        /// <summary>
        /// Gets the enum value for a given name.
        /// </summary>
        public static T GetValue(string name)
        {
            if (NameToValueMap.TryGetValue(name, out T value))
            {
                return value;
            }
            throw new ArgumentException($"Name '{name}' not found in enum '{typeof(T).Name}'.");
        }

        /// <summary>
        /// Gets the name for a given enum value.
        /// </summary>
        public static string GetName(T value)
        {
            if (ValueToNameMap.TryGetValue(value, out string name))
            {
                return name;
            }
            throw new ArgumentException($"Value '{value}' not found in enum '{typeof(T).Name}'.");
        }
    }
}