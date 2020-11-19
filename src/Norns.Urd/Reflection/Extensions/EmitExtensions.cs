using Norns.Urd.DynamicProxy;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Reflection
{
    public static class EmitExtensions
    {
        public static void EmitDefault(this ILGenerator il, Type type)
        {
            if (type == typeof(void)) return;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                case TypeCode.DateTime:
                    if (type.GetTypeInfo().IsValueType)
                    {
                        // Type.GetTypeCode on an enum returns the underlying
                        // integer TypeCode, so we won't get here.
                        // This is the IL for default(T) if T is a generic type
                        // parameter, so it should work for any type. It's also
                        // the standard pattern for structs.
                        LocalBuilder lb = il.DeclareLocal(type);
                        il.Emit(OpCodes.Ldloca, lb);
                        il.Emit(OpCodes.Initobj, type);
                        il.Emit(OpCodes.Ldloc, lb);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.String:
                    il.Emit(OpCodes.Ldnull);
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    il.Emit(OpCodes.Ldc_R4, default(Single));
                    break;

                case TypeCode.Double:
                    il.Emit(OpCodes.Ldc_R8, default(Double));
                    break;

                case TypeCode.Decimal:
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Newobj, typeof(Decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(int) }));
                    break;

                case TypeCode.DBNull:
                    il.Emit(OpCodes.Ldsfld, typeof(DBNull).GetField(nameof(DBNull.Value)));
                    break;

                default:
                    throw new InvalidOperationException("Code supposed to be unreachable.");
            }
        }

        public static void EmitThis(this ILGenerator il)
        {
            il.EmitLoadArg(0);
        }

        public static void EmitLoadArg(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;

                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;

                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;

                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;

                case var _ when index <= byte.MaxValue:
                    il.Emit(OpCodes.Ldarg_S, (byte)index);
                    break;

                default:
                    il.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        public static void EmitConvertTo(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked = true)
        {
            if (typeFrom.IsEquivalentTo(typeTo))
            {
                return;
            }

            var typeFromInfo = typeFrom.GetTypeInfo();
            var typeToInfo = typeTo.GetTypeInfo();

            var nnExprType = typeFromInfo.GetNonNullableType();
            var nnType = typeToInfo.GetNonNullableType();

            if (typeFromInfo.IsInterface ||
              typeToInfo.IsInterface ||
               typeFrom == typeof(object) ||
               typeTo == typeof(object) ||
               typeFrom == typeof(Enum) ||
               typeFrom == typeof(ValueType) ||
               typeFromInfo.IsLegalExplicitVariantDelegateConversion(typeToInfo))
            {
                il.EmitCastTo(typeFromInfo, typeToInfo);
            }
            else if (typeFromInfo.IsNullableType() || typeToInfo.IsNullableType())
            {
                il.EmitNullableConversion(typeFromInfo, typeToInfo, isChecked);
            }
            else if (!(typeFromInfo.IsConvertible() && typeToInfo.IsConvertible()) // primitive runtime conversion
                     &&
                     (nnExprType.GetTypeInfo().IsAssignableFrom(nnType) || // down cast
                     nnType.GetTypeInfo().IsAssignableFrom(nnExprType))) // up cast
            {
                il.EmitCastTo(typeFromInfo, typeToInfo);
            }
            else if (typeFromInfo.IsArray && typeToInfo.IsArray)
            {
                il.EmitCastTo(typeFromInfo, typeToInfo);
            }
            else
            {
                il.EmitNumericConversion(typeFromInfo, typeToInfo, isChecked);
            }
        }

        public static void EmitConvertToObject(this ILGenerator il, Type typeFrom)
        {
            if (typeFrom.GetTypeInfo().IsGenericParameter)
            {
                il.Emit(OpCodes.Box, typeFrom);
            }
            else
            {
                il.EmitConvertTo(typeFrom, typeof(object), true);
            }
        }

        public static void EmitConvertObjectTo(this ILGenerator il, Type type)
        {
            if (type.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.EmitConvertTo(typeof(object), type, true);
            }
        }

        public static void EmitCastTo(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo)
        {
            if (!typeFrom.IsValueType && typeTo.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, typeTo.AsType());
            }
            else if (typeFrom.IsValueType && !typeTo.IsValueType)
            {
                il.Emit(OpCodes.Box, typeFrom.AsType());
                if (typeTo.AsType() != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, typeTo.AsType());
                }
            }
            else if (!typeFrom.IsValueType && !typeTo.IsValueType)
            {
                il.Emit(OpCodes.Castclass, typeTo.AsType());
            }
            else
            {
                throw new InvalidCastException($"Caanot cast {typeFrom} to {typeTo}.");
            }
        }

        private static void EmitNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            bool isTypeFromNullable = typeFrom.IsNullableType();
            bool isTypeToNullable = typeTo.IsNullableType();
            if (isTypeFromNullable && isTypeToNullable)
            {
                il.EmitNullableToNullableConversion(typeFrom, typeTo, isChecked);
            }
            else if (isTypeFromNullable)
            {
                il.EmitNullableToNonNullableConversion(typeFrom, typeTo, isChecked);
            }
            else
            {
                il.EmitNonNullableToNullableConversion(typeFrom, typeTo, isChecked);
            }
        }

        public static void EmitHasValue(this ILGenerator il, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("get_HasValue", BindingFlags.Instance | BindingFlags.Public);
            il.Emit(OpCodes.Call, mi);
        }

        public static void EmitGetValueOrDefault(this ILGenerator il, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("GetValueOrDefault", Type.EmptyTypes);
            il.Emit(OpCodes.Call, mi);
        }

        public static void EmitGetValue(this ILGenerator il, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
            il.Emit(OpCodes.Call, mi);
        }

        private static void EmitNullableToNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            LocalBuilder locFrom = il.DeclareLocal(typeFrom.AsType());
            il.Emit(OpCodes.Stloc, locFrom);

            LocalBuilder locTo = il.DeclareLocal(typeTo.AsType());
            // test for null
            il.Emit(OpCodes.Ldloca, locFrom);
            il.EmitHasValue(typeFrom.AsType());
            Label labIfNull = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, labIfNull);
            il.Emit(OpCodes.Ldloca, locFrom);
            il.EmitGetValueOrDefault(typeFrom.AsType());
            Type nnTypeFrom = typeFrom.GetNonNullableType();
            Type nnTypeTo = typeTo.GetNonNullableType();
            il.EmitConvertTo(nnTypeFrom, nnTypeTo, isChecked);
            // construct result type
            ConstructorInfo ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            il.Emit(OpCodes.Newobj, ci);
            il.Emit(OpCodes.Stloc, locTo);
            Label labEnd = il.DefineLabel();
            il.Emit(OpCodes.Br_S, labEnd);
            // if null then create a default one
            il.MarkLabel(labIfNull);
            il.Emit(OpCodes.Ldloca, locTo);
            il.Emit(OpCodes.Initobj, typeTo.AsType());
            il.MarkLabel(labEnd);
            il.Emit(OpCodes.Ldloc, locTo);
        }

        private static void EmitNullableToNonNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            if (typeTo.IsValueType)
            {
                il.EmitNullableToNonNullableStructConversion(typeFrom, typeTo, isChecked);
            }
            else
            {
                il.EmitNullableToReferenceConversion(typeFrom);
            }
        }

        private static void EmitNullableToNonNullableStructConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            var locFrom = il.DeclareLocal(typeFrom.AsType());
            il.Emit(OpCodes.Stloc, locFrom);
            il.Emit(OpCodes.Ldloca, locFrom);
            il.EmitGetValue(typeFrom.AsType());
            var nnTypeFrom = typeFrom.GetNonNullableType();
            il.EmitConvertTo(nnTypeFrom, typeTo.AsType(), isChecked);
        }

        private static void EmitNullableToReferenceConversion(this ILGenerator il, TypeInfo typeFrom)
        {
            // We've got a conversion from nullable to Object, ValueType, Enum, etc.  Just box it so that
            // we get the nullable semantics.
            il.Emit(OpCodes.Box, typeFrom.AsType());
        }

        private static void EmitNonNullableToNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            var locTo = il.DeclareLocal(typeTo.AsType());
            var nnTypeTo = typeTo.GetNonNullableType();
            il.EmitConvertTo(typeFrom.AsType(), nnTypeTo, isChecked);
            var ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            il.Emit(OpCodes.Newobj, ci);
            il.Emit(OpCodes.Stloc, locTo);
            il.Emit(OpCodes.Ldloc, locTo);
        }

        private static void EmitNumericConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            bool isFromUnsigned = typeFrom.IsUnsigned();
            bool isFromFloatingPoint = typeFrom.IsFloatingPoint();
            if (typeTo.AsType() == typeof(Single))
            {
                if (isFromUnsigned)
                    il.Emit(OpCodes.Conv_R_Un);
                il.Emit(OpCodes.Conv_R4);
            }
            else if (typeTo.AsType() == typeof(Double))
            {
                if (isFromUnsigned)
                    il.Emit(OpCodes.Conv_R_Un);
                il.Emit(OpCodes.Conv_R8);
            }
            else
            {
                TypeCode tc = Type.GetTypeCode(typeTo.AsType());
                if (isChecked)
                {
                    // Overflow checking needs to know if the source value on the IL stack is unsigned or not.
                    if (isFromUnsigned)
                    {
                        switch (tc)
                        {
                            case TypeCode.SByte:
                                il.Emit(OpCodes.Conv_Ovf_I1_Un);
                                break;

                            case TypeCode.Int16:
                                il.Emit(OpCodes.Conv_Ovf_I2_Un);
                                break;

                            case TypeCode.Int32:
                                il.Emit(OpCodes.Conv_Ovf_I4_Un);
                                break;

                            case TypeCode.Int64:
                                il.Emit(OpCodes.Conv_Ovf_I8_Un);
                                break;

                            case TypeCode.Byte:
                                il.Emit(OpCodes.Conv_Ovf_U1_Un);
                                break;

                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                il.Emit(OpCodes.Conv_Ovf_U2_Un);
                                break;

                            case TypeCode.UInt32:
                                il.Emit(OpCodes.Conv_Ovf_U4_Un);
                                break;

                            case TypeCode.UInt64:
                                il.Emit(OpCodes.Conv_Ovf_U8_Un);
                                break;

                            default:
                                throw new InvalidCastException();
                        }
                    }
                    else
                    {
                        switch (tc)
                        {
                            case TypeCode.SByte:
                                il.Emit(OpCodes.Conv_Ovf_I1);
                                break;

                            case TypeCode.Int16:
                                il.Emit(OpCodes.Conv_Ovf_I2);
                                break;

                            case TypeCode.Int32:
                                il.Emit(OpCodes.Conv_Ovf_I4);
                                break;

                            case TypeCode.Int64:
                                il.Emit(OpCodes.Conv_Ovf_I8);
                                break;

                            case TypeCode.Byte:
                                il.Emit(OpCodes.Conv_Ovf_U1);
                                break;

                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                il.Emit(OpCodes.Conv_Ovf_U2);
                                break;

                            case TypeCode.UInt32:
                                il.Emit(OpCodes.Conv_Ovf_U4);
                                break;

                            case TypeCode.UInt64:
                                il.Emit(OpCodes.Conv_Ovf_U8);
                                break;

                            default:
                                throw new InvalidCastException();
                        }
                    }
                }
                else
                {
                    switch (tc)
                    {
                        case TypeCode.SByte:
                            il.Emit(OpCodes.Conv_I1);
                            break;

                        case TypeCode.Byte:
                            il.Emit(OpCodes.Conv_U1);
                            break;

                        case TypeCode.Int16:
                            il.Emit(OpCodes.Conv_I2);
                            break;

                        case TypeCode.UInt16:
                        case TypeCode.Char:
                            il.Emit(OpCodes.Conv_U2);
                            break;

                        case TypeCode.Int32:
                            il.Emit(OpCodes.Conv_I4);
                            break;

                        case TypeCode.UInt32:
                            il.Emit(OpCodes.Conv_U4);
                            break;

                        case TypeCode.Int64:
                            if (isFromUnsigned)
                            {
                                il.Emit(OpCodes.Conv_U8);
                            }
                            else
                            {
                                il.Emit(OpCodes.Conv_I8);
                            }
                            break;

                        case TypeCode.UInt64:
                            if (isFromUnsigned || isFromFloatingPoint)
                            {
                                il.Emit(OpCodes.Conv_U8);
                            }
                            else
                            {
                                il.Emit(OpCodes.Conv_I8);
                            }
                            break;

                        default:
                            throw new InvalidCastException();
                    }
                }
            }
        }

        public static void EmitInt(this ILGenerator il, int value)
        {
            OpCode c;
            switch (value)
            {
                case -1:
                    c = OpCodes.Ldc_I4_M1;
                    break;

                case 0:
                    c = OpCodes.Ldc_I4_0;
                    break;

                case 1:
                    c = OpCodes.Ldc_I4_1;
                    break;

                case 2:
                    c = OpCodes.Ldc_I4_2;
                    break;

                case 3:
                    c = OpCodes.Ldc_I4_3;
                    break;

                case 4:
                    c = OpCodes.Ldc_I4_4;
                    break;

                case 5:
                    c = OpCodes.Ldc_I4_5;
                    break;

                case 6:
                    c = OpCodes.Ldc_I4_6;
                    break;

                case 7:
                    c = OpCodes.Ldc_I4_7;
                    break;

                case 8:
                    c = OpCodes.Ldc_I4_8;
                    break;

                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    return;
            }
            il.Emit(c);
        }

        public static void EmitLdRef(this ILGenerator il, Type type)
        {
            if (type == typeof(short))
            {
                il.Emit(OpCodes.Ldind_I1);
            }
            else if (type == typeof(Int16))
            {
                il.Emit(OpCodes.Ldind_I2);
            }
            else if (type == typeof(Int32))
            {
                il.Emit(OpCodes.Ldind_I4);
            }
            else if (type == typeof(Int64))
            {
                il.Emit(OpCodes.Ldind_I8);
            }
            else if (type == typeof(float))
            {
                il.Emit(OpCodes.Ldind_R4);
            }
            else if (type == typeof(double))
            {
                il.Emit(OpCodes.Ldind_R8);
            }
            else if (type == typeof(ushort))
            {
                il.Emit(OpCodes.Ldind_U1);
            }
            else if (type == typeof(UInt16))
            {
                il.Emit(OpCodes.Ldind_U2);
            }
            else if (type == typeof(UInt32))
            {
                il.Emit(OpCodes.Ldind_U4);
            }
            else if (type.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Ldobj);
            }
            else
            {
                il.Emit(OpCodes.Ldind_Ref);
            }
        }

        public static void EmitStRef(this ILGenerator il, Type type)
        {
            if (type == typeof(short))
            {
                il.Emit(OpCodes.Stind_I1);
            }
            else if (type == typeof(Int16))
            {
                il.Emit(OpCodes.Stind_I2);
            }
            else if (type == typeof(Int32))
            {
                il.Emit(OpCodes.Stind_I4);
            }
            else if (type == typeof(Int64))
            {
                il.Emit(OpCodes.Stind_I8);
            }
            else if (type == typeof(float))
            {
                il.Emit(OpCodes.Stind_R4);
            }
            else if (type == typeof(double))
            {
                il.Emit(OpCodes.Stind_R8);
            }
            else if (type.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Stobj);
            }
            else
            {
                il.Emit(OpCodes.Stind_Ref);
            }
        }

        public static void EmitString(this ILGenerator il, string value)
        {
            il.Emit(OpCodes.Ldstr, value);
        }

        public static void EmitArray(this ILGenerator il, Array items, Type elementType)
        {
            il.EmitInt(items.Length);
            il.Emit(OpCodes.Newarr, elementType);
            for (int i = 0; i < items.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                il.EmitConstant(items.GetValue(i), elementType);
                il.EmitStoreElement(elementType);
            }
        }

        public static void EmitStoreElement(this ILGenerator il, Type type)
        {
            if (type.GetTypeInfo().IsEnum)
            {
                il.Emit(OpCodes.Stelem, type);
                return;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    il.Emit(OpCodes.Stelem_I1);
                    break;

                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    il.Emit(OpCodes.Stelem_I2);
                    break;

                case TypeCode.Int32:
                case TypeCode.UInt32:
                    il.Emit(OpCodes.Stelem_I4);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Stelem_I8);
                    break;

                case TypeCode.Single:
                    il.Emit(OpCodes.Stelem_R4);
                    break;

                case TypeCode.Double:
                    il.Emit(OpCodes.Stelem_R8);
                    break;

                default:
                    if (type.GetTypeInfo().IsValueType)
                    {
                        il.Emit(OpCodes.Stelem, type);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                    break;
            }
        }

        public static void EmitConstant(this ILGenerator il, object value, Type valueType)
        {
            if (value == null)
            {
                EmitDefault(il, valueType);
                return;
            }

            if (il.TryEmitILConstant(value, valueType))
            {
                return;
            }

            var t = value as Type;
            if (t != null)
            {
                il.EmitType(t);
                if (valueType != typeof(Type))
                {
                    il.Emit(OpCodes.Castclass, valueType);
                }
                return;
            }

            var mb = value as MethodBase;
            if (mb != null)
            {
                il.EmitMethod((MethodInfo)mb);
                return;
            }

            if (valueType.GetTypeInfo().IsArray)
            {
                var array = (Array)value;
                il.EmitArray(array, valueType.GetElementType());
            }

            throw new InvalidOperationException("Code supposed to be unreachable.");
        }

        public static void EmitType(this ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, Constants.GetTypeFromHandle);
        }

        public static void EmitMethod(this ILGenerator il, MethodInfo method)
        {
            EmitMethod(il, method, method.DeclaringType);
        }

        public static void EmitMethod(this ILGenerator il, MethodInfo method, Type declaringType)
        {
            il.Emit(OpCodes.Ldtoken, method);
            il.Emit(OpCodes.Ldtoken, method.DeclaringType);
            il.Emit(OpCodes.Call, Constants.GetMethodFromHandle);
            il.EmitConvertTo(typeof(MethodBase), typeof(MethodInfo));
        }

        private static bool TryEmitILConstant(this ILGenerator il, object value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    il.EmitBoolean((bool)value);
                    return true;

                case TypeCode.SByte:
                    il.EmitSByte((sbyte)value);
                    return true;

                case TypeCode.Int16:
                    il.EmitShort((short)value);
                    return true;

                case TypeCode.Int32:
                    il.EmitInt((int)value);
                    return true;

                case TypeCode.Int64:
                    il.EmitLong((long)value);
                    return true;

                case TypeCode.Single:
                    il.EmitSingle((float)value);
                    return true;

                case TypeCode.Double:
                    il.EmitDouble((double)value);
                    return true;

                case TypeCode.Char:
                    il.EmitChar((char)value);
                    return true;

                case TypeCode.Byte:
                    il.EmitByte((byte)value);
                    return true;

                case TypeCode.UInt16:
                    il.EmitUShort((ushort)value);
                    return true;

                case TypeCode.UInt32:
                    il.EmitUInt((uint)value);
                    return true;

                case TypeCode.UInt64:
                    il.EmitULong((ulong)value);
                    return true;

                case TypeCode.Decimal:
                    il.EmitDecimal((decimal)value);
                    return true;

                case TypeCode.String:
                    il.EmitString((string)value);
                    return true;

                default:
                    return false;
            }
        }

        public static void EmitBoolean(this ILGenerator il, bool value)
        {
            if (value)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
        }

        public static void EmitChar(this ILGenerator il, char value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_U2);
        }

        public static void EmitByte(this ILGenerator il, byte value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_U1);
        }

        public static void EmitSByte(this ILGenerator il, sbyte value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_I1);
        }

        public static void EmitShort(this ILGenerator il, short value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_I2);
        }

        public static void EmitUShort(this ILGenerator il, ushort value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_U2);
        }

        public static void EmitUInt(this ILGenerator il, uint value)
        {
            il.EmitInt((int)value);
            il.Emit(OpCodes.Conv_U4);
        }

        public static void EmitLong(this ILGenerator il, long value)
        {
            il.Emit(OpCodes.Ldc_I8, value);

            //
            // Now, emit convert to give the constant type information.
            //
            // Otherwise, it is treated as unsigned and overflow is not
            // detected if it's used in checked ops.
            //
            il.Emit(OpCodes.Conv_I8);
        }

        public static void EmitULong(this ILGenerator il, ulong value)
        {
            il.Emit(OpCodes.Ldc_I8, (long)value);
            il.Emit(OpCodes.Conv_U8);
        }

        public static void EmitDouble(this ILGenerator il, double value)
        {
            il.Emit(OpCodes.Ldc_R8, value);
        }

        public static void EmitSingle(this ILGenerator il, float value)
        {
            il.Emit(OpCodes.Ldc_R4, value);
        }

        public static void EmitDecimal(this ILGenerator il, decimal value)
        {
            if (Decimal.Truncate(value) == value)
            {
                if (Int32.MinValue <= value && value <= Int32.MaxValue)
                {
                    int intValue = Decimal.ToInt32(value);
                    il.EmitInt(intValue);
                    il.Emit(OpCodes.Newobj, typeof(Decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(int) }));
                }
                else if (Int64.MinValue <= value && value <= Int64.MaxValue)
                {
                    long longValue = Decimal.ToInt64(value);
                    il.EmitLong(longValue);
                    il.Emit(OpCodes.Newobj, typeof(Decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(long) }));
                }
                else
                {
                    il.EmitDecimalBits(value);
                }
            }
            else
            {
                il.EmitDecimalBits(value);
            }
        }

        private static void EmitDecimalBits(this ILGenerator il, decimal value)
        {
            int[] bits = Decimal.GetBits(value);
            il.EmitInt(bits[0]);
            il.EmitInt(bits[1]);
            il.EmitInt(bits[2]);
            il.EmitBoolean((bits[3] & 0x80000000) != 0);
            il.EmitByte((byte)(bits[3] >> 16));
            il.Emit(OpCodes.Newobj, typeof(decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte) }));
        }
    }
}