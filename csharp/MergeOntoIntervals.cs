using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;

namespace MergeBy
{
    class Program
    {
        /// <summary>
        /// Copies items from one collection into a new collection, changing a date property for each item. The 
        /// date property will be set to an integral "interval" multiple that contains the DateTime.
        /// </summary>
        /// <typeparam name="T">Type of item in collection.</typeparam>
        /// <param name="data">Collection of objects to work with.</param>
        /// <param name="property">DateTime property to consider.</param>
        /// <param name="interval">Period of interval.</param>
        /// <param name="intervalStart">Starting point for interval. Items before this point in time will be ignored.</param>
        /// <param name="copyConstructorAssign">Instantiates a new item and sets the DateTime property to the parameter.</param>
        /// <returns>A collection with the changed datetimes.</returns>
        /// <remarks>
        /// using System.Reflection;
        /// using System.Linq.Expressions;
        /// </remarks>
        public static List<T> MergeOntoIntervals<T>(
            IEnumerable<T> data, 
            Expression<Func<T, DateTime>> property, 
            TimeSpan interval, 
            DateTime intervalStart, 
            Func<T, DateTime, T> copyConstructorAssign)
        {
            if (Object.ReferenceEquals(null, data))
            {
                return null;
            }

            if (object.ReferenceEquals(null, property))
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (object.ReferenceEquals(null, copyConstructorAssign))
            {
                throw new ArgumentNullException(nameof(copyConstructorAssign));
            }

            if (interval.TotalSeconds < 1)
            {
                throw new ArgumentException($"{nameof(interval)} must be >= 1 second");
            }

            var results = new List<T>();

            var intervalSeconds = interval.TotalSeconds;
            var expression = property.Body as MemberExpression;
            var prop = expression.Member as PropertyInfo;

            // Do some error checking. For the type parameter, find the property with the
            // same name as the property paramter.
            var itemProperties = (typeof(T)).GetProperties();
            var propertyCheck = itemProperties.FirstOrDefault(x => x.Name == prop.Name);

            if (object.ReferenceEquals(null, propertyCheck))
            {
                throw new MemberAccessException("Property paramter could not be found on type parameter.");
            }

            if (propertyCheck.GetGetMethod() != prop.GetGetMethod())
            {
                throw new MemberAccessException("Property paramter is not the same as property on type parameter.");
            }

            foreach (var item in data)
            {
                var itemDateTime = (DateTime)(prop.GetValue(item));

                if (intervalStart > itemDateTime)
                {
                    continue;
                }

                // This is the nth interval, truncate to an integer.
                var n = (int)((itemDateTime - intervalStart).TotalSeconds / intervalSeconds);

                // Rebuild into interval time.
                var mergedDateTime = intervalStart.AddSeconds(n * intervalSeconds);

                // Let the caller figure out how to instantiate and overwrite the date field. There are too many
                // special cases, and structs are another ambiguous case.
                var merged = copyConstructorAssign(item, mergedDateTime);

                results.Add(merged);
            }

            return results;
        }

        static void Main(string[] args)
        {
            var data = new List<Tuple<DateTime, string, int>>() {
                new Tuple<DateTime, string, int>(DateTime.Parse("8/01/2016"), "Low", 10),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/01/2017"), "Low", 10),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/01/2017"), "Med", 20),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/01/2017"), "High", 10),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/02/2017"), "Low", 20),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/02/2017"), "Med", 15),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/02/2017"), "High", 30),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/03/2017"), "Low", 15),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/03/2017"), "Med", 5),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/03/2017"), "High", 5),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/10/2017"), "Low", 10),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/10/2017"), "Med", 55),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/10/2017"), "High", 40),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/13/2017"), "Low", 20),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/13/2017"), "Med", 10),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/13/2017"), "High", 8),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/16/2017"), "Low", 9),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/16/2017"), "Med", 20),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/16/2017"), "High", 22),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/19/2017"), "Low", 25),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/19/2017"), "Med", 30),
                new Tuple<DateTime, string, int>(DateTime.Parse("8/19/2017"), "High", 12)
            };

            var merged = MergeOntoIntervals<Tuple<DateTime, string, int>>(
                data, 
                x => x.Item1, 
                TimeSpan.FromDays(7),
                DateTime.Parse("7/30/2016"), 
                (x,d) => {
                    return new Tuple<DateTime, string, int>(d, x.Item2, x.Item3);
                });

            var grouped = merged
                .GroupBy(x => new { x.Item1, x.Item2 })
                .Select(x => new Tuple<DateTime, string, int>(x.Key.Item1, x.Key.Item2, x.ToList().Select(y => y.Item3).Sum()));

            foreach (var item in grouped)
            {
                Console.WriteLine($"{item.Item1.ToShortDateString()}, {item.Item2}, {item.Item3}");
            }

            // output: 

            /*
            7/30/2016, Low, 10
            7/29/2017, Low, 45
            7/29/2017, Med, 40
            7/29/2017, High, 45
            8/5/2017, Low, 10
            8/5/2017, Med, 55
            8/5/2017, High, 40
            8/12/2017, Low, 29
            8/12/2017, Med, 30
            8/12/2017, High, 30
            8/19/2017, Low, 25
            8/19/2017, Med, 30
            8/19/2017, High, 12
            */
        }
    }
}
