using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Toolbox
{
    public static class Helper
    {
        public static IEnumerable<T> DefaultRange<T>(int start, int count)
        {
            foreach (var x in Enumerable.Range(start, count))
            {
                yield return default(T);
            }
        }
    }
}
