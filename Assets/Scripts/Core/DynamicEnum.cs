using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Core
{
    // Allows for strings to register for an id
    // that can be used for faster look-up in other
    // data structures

    public static class DynamicEnum
    {
        public static Dictionary<string, int> EnumNameMap = new Dictionary<string, int>();
        public static Dictionary<int, string> EnumValueMap = new Dictionary<int, string>();
        private static int nextValue = 1;

        public static int GetEnum(string name)
        {
            if (EnumNameMap.TryGetValue(name, out int value))
            {
                return value;
            }
            EnumNameMap.Add(name, nextValue);
            nextValue++;
            return nextValue - 1;
        }

        public static string GetEnum(int value)
        {
            if (EnumValueMap.TryGetValue(value, out string name))
            {
                return name;
            }
            return null;
        }
    }
}