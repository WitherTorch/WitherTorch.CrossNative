﻿using System;
using System.Numerics;

using InlineMethod;

#if NET472_OR_GREATER
using LocalsInit;
#else
using System.Runtime.CompilerServices;
#endif

namespace WitherTorch.Common.Helpers
{
    partial class SequenceHelper
    {
#pragma warning disable CS0162
        unsafe partial class Core
        {
            [Inline(InlineBehavior.Remove)]
            public static bool Equals(IntPtr* ptr, IntPtr* ptrEnd, IntPtr* ptr2)
            {
                switch (UnsafeHelper.PointerSizeConstant)
                {
                    case sizeof(int):
                        return Core<int>.Equals((int*)ptr, (int*)ptrEnd, (int*)ptr2);
                    case sizeof(long):
                        return Core<long>.Equals((long*)ptr, (long*)ptrEnd, (long*)ptr2);
                    case UnsafeHelper.PointerSizeConstant_Indeterminate:
                        switch (UnsafeHelper.PointerSize)
                        {
                            case sizeof(int):
                                return Core<int>.Equals((int*)ptr, (int*)ptrEnd, (int*)ptr2);
                            case sizeof(long):
                                return Core<long>.Equals((long*)ptr, (long*)ptrEnd, (long*)ptr2);
                            default:
                                break;
                        }
                        goto default;
                    default:
                        throw new NotImplementedException();
                }
            }

            [Inline(InlineBehavior.Remove)]
            public static bool Equals(UIntPtr* ptr, UIntPtr* ptrEnd, UIntPtr* ptr2)
            {
                switch (UnsafeHelper.PointerSizeConstant)
                {
                    case sizeof(int):
                        return Core<int>.Equals((int*)ptr, (int*)ptrEnd, (int*)ptr2);
                    case sizeof(long):
                        return Core<long>.Equals((long*)ptr, (long*)ptrEnd, (long*)ptr2);
                    case UnsafeHelper.PointerSizeConstant_Indeterminate:
                        switch (UnsafeHelper.PointerSize)
                        {
                            case sizeof(int):
                                return Core<int>.Equals((int*)ptr, (int*)ptrEnd, (int*)ptr2);
                            case sizeof(long):
                                return Core<long>.Equals((long*)ptr, (long*)ptrEnd, (long*)ptr2);
                            default:
                                break;
                        }
                        goto default;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
#pragma warning restore CS0162

        unsafe partial class Core<T>
        {
            public static bool Equals(T* ptr, T* ptrEnd, T* ptr2)
            {
                if (CheckTypeCanBeVectorized() && Vector.IsHardwareAccelerated)
                {
                    T* ptrLimit = ptrEnd - Vector<T>.Count;
                    if (ptr < ptrLimit)
                    {
                        do
                        {
                            Vector<T> valueVector = UnsafeHelper.Read<Vector<T>>(ptr);
                            Vector<T> valueVector2 = UnsafeHelper.Read<Vector<T>>(ptr2);
                            if (!valueVector.Equals(valueVector2))
                                return false;
                        } while ((ptr += Vector<T>.Count) < ptrLimit);
                        if (ptr == ptrEnd)
                            return true;
                    }
                }
                if (IsPrimitiveType())
                {
                    for (; ptr < ptrEnd; ptr++, ptr2++)
                    {
                        if (UnsafeHelper.NotEquals(*ptr, *ptr2))
                            return false;
                    }
                }
                else
                {
                    ComparerImpl comparer = ComparerImpl.Instance;
                    for (; ptr < ptrEnd; ptr++, ptr2++)
                    {
                        if (!comparer.Equals(*ptr, *ptr2))
                            return false;
                    }
                }
                return true;
            }
        }
    }
}
