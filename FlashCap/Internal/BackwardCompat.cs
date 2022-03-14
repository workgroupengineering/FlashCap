﻿////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

#if NET20
namespace System.Runtime.CompilerServices
{
    internal sealed class ExtensionAttribute : Attribute
    {
    }
}

namespace System
{
    internal delegate TR Func<T0, TR>(T0 arg0);
}

namespace System.Linq
{
    internal static partial class Enumerable
    {
        public static IEnumerable<int> Range(int begin, int count)
        {
            for (var index = 0; index < count; index++)
            {
                yield return begin + index;
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, IEnumerable<T> next)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }
            foreach (var item in next)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> Where<T>(
            this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            foreach (var value in enumerable)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<U> Select<T, U>(
            this IEnumerable<T> enumerable, Func<T, U> selector)
        {
            foreach (var value in enumerable)
            {
                yield return selector(value);
            }
        }

        public static IEnumerable<U> SelectMany<T, U>(
            this IEnumerable<T> enumerable, Func<T, IEnumerable<U>> binder)
        {
            foreach (var value in enumerable)
            {
                foreach (var innerValue in binder(value))
                {
                    yield return innerValue;
                }
            }
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> enumerable)
        {
            foreach (var value in enumerable)
            {
                return value;
            }

            return default(T)!;
        }

        public static T[] ToArray<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T> coll)
            {
                var arr = new T[coll.Count];
                coll.CopyTo(arr, 0);
                return arr;
            }
            else
            {
                var list = new List<T>();
                foreach (var item in enumerable)
                {
                    list.Add(item);
                }
                return list.ToArray();
            };
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> enumerable)
        {
            var dict = new Dictionary<T, T>();
            foreach (var item in enumerable)
            {
                if (!dict.ContainsKey(item))
                {
                    dict.Add(item, item);
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> OrderByDescending<T, U>(
            this IEnumerable<T> enumerable, Func<T, U> keySelector)
            where U : IComparable<U>
        {
            var list = new List<T>(enumerable);
            list.Sort((a, b) => 0 - keySelector(a).CompareTo(keySelector(b)));
            return list;
        }
    }
}
#endif

#if NET20 || NET35 || NET40 || NET45
namespace System
{
    internal static class ArrayEx
    {
        private static class EmptyArray<T>
        {
            public static readonly T[] Empty = new T[0];
        }

        public static T[] Empty<T>() =>
            EmptyArray<T>.Empty;
    }
}
#else
namespace System
{
    internal static class ArrayEx
    {
        public static T[] Empty<T>() =>
            Array.Empty<T>();
    }
}
#endif

namespace System.Linq
{
    internal static partial class EnumerableExtension
    {
        public static IEnumerable<U> Collect<T, U>(
            this IEnumerable<T> enumerable, Func<T, U?> selector)
        {
            foreach (var value in enumerable)
            {
                if (selector(value) is { } mapped)
                {
                    yield return mapped;
                }
            }
        }
    }
}
