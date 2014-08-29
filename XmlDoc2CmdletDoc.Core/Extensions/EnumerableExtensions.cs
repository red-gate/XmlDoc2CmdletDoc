using System;
using System.Collections.Generic;
using System.Linq;

namespace XmlDoc2CmdletDoc.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns elements having distinct values of a specified property.
        /// </summary>
        /// <typeparam name="TElement">The types of the element in the sequence.</typeparam>
        /// <typeparam name="TProperty">The type of the property selected from each element.</typeparam>
        /// <param name="enumerable">The sequence of elements.</param>
        /// <param name="selector">The function used to select the property from each item in the sequence.</param>
        /// <returns>Those items in the sequence which have a unique property. If multiple items share the same property, only the
        /// first item is returned.</returns>
        public static IEnumerable<TElement> Distinct<TElement, TProperty>(this IEnumerable<TElement> enumerable, Func<TElement, TProperty> selector)
        {
            var hashSet = new HashSet<TProperty>();
            return enumerable.Where(item => hashSet.Add(selector(item)));
        }
    }
}