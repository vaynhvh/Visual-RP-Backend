//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Utils.Extensions
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    internal static class EnumerableExtensions
    {
        //[HandleExceptions]
        internal static async Task forEach<T>(this IEnumerable<T> elements, Action<T> action)
        {
            foreach (var element in elements)
            {
                action(element);
            }
        }

        //[HandleExceptions]
        internal static async Task forEachAlternativeAsync<T>(this IEnumerable<T> elements, Action<T> action)
        {
            for (int i = elements.Count() - 1; i >= 0; i--)
            {
                var element = elements.ToList()[i];

                action(element);
            }
        }

        //[HandleExceptions]
        internal static void forEachAlternative<T>(this IEnumerable<T> elements, Action<T> action)
        {
            for (int i = elements.Count() - 1; i >= 0; i--)
            {
                var element = elements.ToList()[i];

                action(element);
            }
        }

        //[HandleExceptions]
        internal static void forEachDefault<T>(this IEnumerable<T> elements, Action<T> action)
        {
            foreach (var element in elements)
            {
                action(element);
            }
        }

        //[HandleExceptions]
        internal static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
