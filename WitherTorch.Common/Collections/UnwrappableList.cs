﻿using InlineIL;

using InlineMethod;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;

namespace WitherTorch.Common.Collections
{
    public static class UnwrappableList
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static UnwrappableList<T> FromExistArray<T>(T[] array) => new UnwrappableList<T>(array);

        [Inline(InlineBehavior.Keep, export: true)]
        public static UnwrappableList<T> FromExistArray<T>(T[] array, int initialCount) => new UnwrappableList<T>(array, initialCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnwrappableList<T> FromExistArray<T>(T[] array, bool trimNullItems) where T : class
        {
            int count = array.Length;
            if (trimNullItems)
            {
                if (count > 0)
                {
                    if (array[0] is null)
                        return new UnwrappableList<T>(array, initialCount: 0);
                    for (; count > 0; count--)
                    {
                        if (array[count - 1] is null)
                            continue;
                        return new UnwrappableList<T>(array, count);
                    }
                }
            }
            return new UnwrappableList<T>(array, count);
        }
    }

    public sealed class UnwrappableList<T> : CustomListBase<T>
    {
        public UnwrappableList(T[] array) : base(array) { }

        public UnwrappableList(T[] array, int initialCount) : base(array, initialCount) { }

        public UnwrappableList(int capacity = 4) : base(CreateArray(capacity), initialCount: 0) { }

        public UnwrappableList(IEnumerable<T> items) : base(CreateArrayFromItems(items), initialCount: 0) { }

        [Inline(InlineBehavior.Remove)]
        private static T[] CreateArray(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity == 0)
                return Array.Empty<T>();
            return new T[capacity];
        }

        [Inline(InlineBehavior.Remove)]
        private static T[] CreateArrayFromItems(IEnumerable<T> items)
            => items switch
            {
                T[] _array => CreateArrayFromItems(_array),
                UnwrappableList<T> _list => CreateArrayFromItems(_list.Unwrap()),
                ICollection<T> _items => CreateArrayFromItems(_items),
                _ => CreateArrayFromItemsCore(items),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T[] CreateArrayFromItemsCore(IEnumerable<T> items)
        {
            int count = items.Count();
            if (count < 0)
                return Array.Empty<T>();
            T[] result = new T[count];
            IEnumerator<T> enumerator = items.GetEnumerator();
            for (int i = 0; i < count && enumerator.MoveNext(); i++)
                result[i] = enumerator.Current;
            enumerator.Dispose();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T[] CreateArrayFromItems(ICollection<T> items)
        {
            int count = items.Count;
            if (count < 0)
                return Array.Empty<T>();
            T[] result = new T[count];
            items.CopyTo(result, 0);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T[] CreateArrayFromItems(T[] items)
        {
            int length = items.Length;
            if (length < 0)
                return Array.Empty<T>();
            T[] result = new T[length];
            Array.Copy(items, result, length);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void EnsureCapacity()
        {
            int oldLength = _array.Length;
            int count = _count;
            if (oldLength < count)
            {
                int newLength;
                if (oldLength >= Limits.MaxArrayLength / 2)
                    newLength = Limits.MaxArrayLength;
                else
                    newLength = MathHelper.Max(oldLength * 2, count);
                T[] newArray = new T[newLength];
                Array.Copy(_array, newArray, oldLength);
                _array = newArray;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Unwrap() => _array;
    }

}
