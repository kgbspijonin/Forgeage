using UnityEngine;
using System.Collections.Generic;

namespace Forgeage
{
    namespace Extensions
    {
        public interface IExecutable {
            void Execute();
        }

        public delegate void BasicDelegate();
        public delegate void KeyPressDelegate(ActionStatus status);
        public delegate void ActionDelegate();
        public delegate void AbilityDelegate(GameObject manager);
        public delegate void ManagedActionDelegate(GameObject manager);

        public enum ActionStatus
        {
            PRESSED,
            HELD,
            RELEASED
        }

        public static class ExtensionMethods
        {
            public static TValue GetOrAddValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
            {
                TValue current;
                if (dict.ContainsKey(key))
                {
                    dict.TryGetValue(key, out current);
                    return current;
                }
                else
                {
                    dict[key] = value;
                    return value;
                }
            }
        }
    }
}
