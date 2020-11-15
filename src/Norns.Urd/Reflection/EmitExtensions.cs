using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Reflection
{
    public static class EmitExtensions
    {
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

        private static bool ShouldLdtoken(Type t)
        {
            return t.IsGenericParameter || t.GetTypeInfo().IsVisible;
        }
    }
}