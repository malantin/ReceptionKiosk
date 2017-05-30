using System;
using System.Collections.Generic;

namespace ReceptionKiosk.Helpers
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// ForEach Implementierung für IEnumerable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="enumeration">Enumeration</param>
        /// <param name="action">Action</param>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}
