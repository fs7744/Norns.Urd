﻿using Norns.Urd.Extensions;
using Norns.Urd.Utils;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Proxy
{
    public class InheritProxyGenerator : FacadeProxyGenerator
    {
        public override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;

        public override void GetServiceInstance(ProxyGeneratorContext context, ILGenerator il)
        {
            il.EmitThis();
        }

        public override void DefineFields(ProxyGeneratorContext context)
        {
            // Method intentionally left empty to avoid instance field.
        }

        public override void DefineMethod(ProxyGeneratorContext context, MethodInfo method)
        {
            if (!method.IsVisibleAndVirtual()) return;
            var baseMethodName = $"{method.Name}_Base";
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBaseBuilder = context.TypeBuilder.DefineMethod(baseMethodName, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            var il = methodBaseBuilder.GetILGenerator();
            il.EmitThis();
            for (var i = 1; i <= parameters.Length; i++)
            {
                il.EmitLoadArg(i);
            }
            il.Emit(OpCodes.Call, method);
            il.Emit(OpCodes.Ret);

            base.DefineMethod(context, method);
        }
    }
}