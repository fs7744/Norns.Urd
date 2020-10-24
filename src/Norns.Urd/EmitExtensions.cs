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
    }
}