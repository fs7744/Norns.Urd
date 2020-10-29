using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd
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

                default:
                    if (index <= byte.MaxValue) il.Emit(OpCodes.Ldarg_S, (byte)index);
                    else il.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        public static void EmitThis(this ILGenerator il)
        {
            il.EmitLoadArg(0);
        }

        public static void EmitLoadElement(this ILGenerator ilGenerator, Type type)
        {
            if (!type.GetTypeInfo().IsValueType)
            {
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                ilGenerator.Emit(OpCodes.Ldelem, type);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                        ilGenerator.Emit(OpCodes.Ldelem_I1);
                        break;
                    case TypeCode.Byte:
                        ilGenerator.Emit(OpCodes.Ldelem_U1);
                        break;
                    case TypeCode.Int16:
                        ilGenerator.Emit(OpCodes.Ldelem_I2);
                        break;
                    case TypeCode.Char:
                    case TypeCode.UInt16:
                        ilGenerator.Emit(OpCodes.Ldelem_U2);
                        break;
                    case TypeCode.Int32:
                        ilGenerator.Emit(OpCodes.Ldelem_I4);
                        break;
                    case TypeCode.UInt32:
                        ilGenerator.Emit(OpCodes.Ldelem_U4);
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        ilGenerator.Emit(OpCodes.Ldelem_I8);
                        break;
                    case TypeCode.Single:
                        ilGenerator.Emit(OpCodes.Ldelem_R4);
                        break;
                    case TypeCode.Double:
                        ilGenerator.Emit(OpCodes.Ldelem_R8);
                        break;
                    default:
                        ilGenerator.Emit(OpCodes.Ldelem, type);
                        break;
                }
            }
        }

        public static void EmitInt(this ILGenerator ilGenerator, int value)
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
                        ilGenerator.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        ilGenerator.Emit(OpCodes.Ldc_I4, value);
                    }
                    return;
            }
            ilGenerator.Emit(c);
        }
    }
}