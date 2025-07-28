using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlotting.Extensions
{
    internal static class ListExtensions
    {
        public static void Apply<T>(this IList<T> list, Func<T, T> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = action(list[i]);
            }
        }
    }
}
