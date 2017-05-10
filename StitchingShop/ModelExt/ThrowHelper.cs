using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum ExceptionArgument
{
    array,
    capacity,
    collection,
    converter,
    count,
    index,
    item,
    list,
    match,
    value,
    oldIndex,
    newIndex
}
public enum ExceptionResource
{
    Arg_ArrayPlusOffTooSmall,
    Arg_NonZeroLowerBound,
    Arg_RankMultiDimNotSupported,
    Argument_InvalidArrayType,
    Argument_InvalidOffLen,
    ArgumentOutOfRange_BiggerThanCollection,
    ArgumentOutOfRange_Count,
    ArgumentOutOfRange_Index,
    ArgumentOutOfRange_ListInsert,
    ArgumentOutOfRange_NeedNonNegNum,
    ArgumentOutOfRange_SmallCapacity,
    InvalidOperation_EnumFailedVersion,
    InvalidOperation_EnumOpCantHappen,
    NotSupported_ReadOnlyCollection
}
public static class ThrowHelper
{
    public const int MaxArrayLength = 0X7FEFFFFF;

    static Type enumType = typeof(ExceptionArgument);
    public static void ThrowArgumentNullException(ExceptionArgument expArgument)
    {
        throw new ArgumentNullException(enumType.GetEnumName(expArgument));
    }
    public static void ThrowNotSupportedException(ExceptionResource expResource)
    {
        if (expResource == ExceptionResource.NotSupported_ReadOnlyCollection)
            throw new NotSupportedException("Unable to perform this action because source is marked as readonly!");
    }
    public static void ThrowArgumentOutOfRangeException(ExceptionArgument expArgument = ExceptionArgument.index)
    {
        throw new ArgumentOutOfRangeException(enumType.GetEnumName(expArgument));
    }
    public static void ThrowArgumentException(ExceptionResource expResource)
    {
        switch (expResource)
        {
            case ExceptionResource.Arg_ArrayPlusOffTooSmall:
                throw new ArgumentException("Destination array is too small to fit all the value at destination space. Adjust starting index and/or destination array length.");
            case ExceptionResource.Arg_NonZeroLowerBound:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.Arg_RankMultiDimNotSupported:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.Argument_InvalidArrayType:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.Argument_InvalidOffLen:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.ArgumentOutOfRange_BiggerThanCollection:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.ArgumentOutOfRange_Count:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.ArgumentOutOfRange_Index:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.ArgumentOutOfRange_ListInsert:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.InvalidOperation_EnumFailedVersion:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.InvalidOperation_EnumOpCantHappen:
                throw new ArgumentException("Invalid Argument value.");
            case ExceptionResource.NotSupported_ReadOnlyCollection:
                throw new ArgumentException("Invalid Argument value.");
            default:
                throw new ArgumentException("Invalid Argument value.");
        }
    }
    public static void ThrowArgumentOutOfRangeException(ExceptionArgument expArgument, ExceptionResource expResource)
    {
        string paramName = enumType.GetEnumName(expArgument);
        switch (expResource)
        {
            case ExceptionResource.Arg_ArrayPlusOffTooSmall:
            case ExceptionResource.Arg_NonZeroLowerBound:
            case ExceptionResource.Arg_RankMultiDimNotSupported:
            case ExceptionResource.Argument_InvalidArrayType:
            case ExceptionResource.Argument_InvalidOffLen:
            case ExceptionResource.ArgumentOutOfRange_BiggerThanCollection:
            case ExceptionResource.ArgumentOutOfRange_Count:
            case ExceptionResource.ArgumentOutOfRange_Index:
            case ExceptionResource.ArgumentOutOfRange_ListInsert:
            case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
            case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
            case ExceptionResource.InvalidOperation_EnumFailedVersion:
            case ExceptionResource.InvalidOperation_EnumOpCantHappen:
            case ExceptionResource.NotSupported_ReadOnlyCollection:
            default:
                throw new ArgumentOutOfRangeException(paramName);
        }
        throw new NotImplementedException();
    }
    public static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument expArgument)
    {
        string paramName = enumType.GetEnumName(expArgument);
        if (value == null)
            throw new ArgumentNullException(paramName);
        Type t = typeof(T);
        if (!(t.IsAssignableFrom(value.GetType())))
            throw new InvalidCastException(paramName + " has invalid type, expected type name is : " + t.Name);
    }
    public static void ThrowWrongValueTypeArgumentException(object value, Type t, ExceptionArgument expArgument)
    {
        string paramName = enumType.GetEnumName(expArgument);
        if (value == null)
            throw new ArgumentNullException(paramName);
        if (!(t.IsAssignableFrom(value.GetType())))
            throw new InvalidCastException(paramName + " has invalid type, expected type name is : " + t.Name);
    }

    public static void ThrowIfNotCompatibleObject(ExceptionArgument expArgument)
    {
        throw new InvalidCastException(enumType.GetEnumName(expArgument) + " is not compatible with the colection.");
    }

    public static void ThrowInvalidOperationException(ExceptionResource expResource)
    {
        if (expResource ==  ExceptionResource.InvalidOperation_EnumFailedVersion)
            throw new InvalidOperationException("Version of the collection has changed!");
        throw new InvalidOperationException("Invalid Operation of the type : " + typeof(ExceptionResource).GetEnumName(expResource));
    }
    public sealed class FunctorComparer<T> : IComparer<T>
    {
        Comparison<T> comparison;
        public FunctorComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }
        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }

}