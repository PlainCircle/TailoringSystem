using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum ExceptionResource
{
    Arg_RankMultiDimNotSupported,
    Arg_InvalidArrayType,
    Arg_InvalidOffLen
}
public static class ThrowHelper
{
    public static void ThrowArgumentOutOfRangeException()
    {
        throw new IndexOutOfRangeException("Invalid value of the index!");
    }

    internal static void ThrowInvalidOperationExceptionEnumFailedVersion()
    {
        throw new InvalidOperationException("Collection changed!");
    }

    internal static void ThrowInvalidOperationExceptionInvalidState()
    {
        throw new InvalidOperationException("Invalid state reached!");
    }

    internal static void ThrowArgumentException(ExceptionResource exp_type)
    {
        switch (exp_type)
        {
            case ExceptionResource.Arg_RankMultiDimNotSupported:
                throw new AggregateException("Invalid Array!");
            default:
                throw new AggregateException("Invalid Aggregate!");
        }
    }
}
