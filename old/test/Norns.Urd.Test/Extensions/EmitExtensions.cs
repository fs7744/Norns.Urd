using Norns.Urd.Extensions;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace Norns.Urd.Test.Extensions
{
    public class EmitExtensionsTests
    {
        private readonly TypeBuilder typeBuilder;

        public EmitExtensionsTests()
        {
            this.typeBuilder = AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName("Assembly"), AssemblyBuilderAccess.Run)
                .DefineDynamicModule("Module")
                .DefineType("Type", TypeAttributes.Public);
        }

        [Fact]
        public void EmitByte()
        {
            var value = (byte)1;
            var (methodName, ilGenerator) = this.GetILGenerator<byte>();

            ilGenerator.EmitByte(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<byte>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitChar()
        {
            var value = 'A';
            var (methodName, ilGenerator) = this.GetILGenerator<char>();

            ilGenerator.EmitChar(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<char>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmitBoolean(bool value)
        {
            var (methodName, ilGenerator) = this.GetILGenerator<bool>();

            ilGenerator.EmitBoolean(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<bool>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitDecimal()
        {
            var value = 1.0M;
            var (methodName, ilGenerator) = this.GetILGenerator<decimal>();

            ilGenerator.EmitDecimal(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<decimal>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitString()
        {
            var value = "hello";
            var (methodName, ilGenerator) = this.GetILGenerator<string>();

            ilGenerator.EmitString(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<string>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitArray()
        {
            var value = new int[] { 1, 2, 3 };
            var (methodName, ilGenerator) = this.GetILGenerator<int[]>();

            ilGenerator.EmitArray(value, typeof(int));
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<int[]>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitDouble()
        {
            var value = 1.0;
            var (methodName, ilGenerator) = this.GetILGenerator<double>();

            ilGenerator.EmitDouble(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<double>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitUShort()
        {
            var value = (ushort)1;
            var (methodName, ilGenerator) = this.GetILGenerator<ushort>();

            ilGenerator.EmitUShort(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<ushort>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitULong()
        {
            var value = (ulong)1;
            var (methodName, ilGenerator) = this.GetILGenerator<ulong>();

            ilGenerator.EmitULong(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<ulong>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitUInt()
        {
            var value = (uint)1;
            var (methodName, ilGenerator) = this.GetILGenerator<uint>();

            ilGenerator.EmitUInt(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<uint>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitSingle()
        {
            var value = 1.0F;
            var (methodName, ilGenerator) = this.GetILGenerator<float>();

            ilGenerator.EmitSingle(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<float>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitLong()
        {
            var value = 1L;
            var (methodName, ilGenerator) = this.GetILGenerator<long>();

            ilGenerator.EmitLong(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<long>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitDefault()
        {
            var value = default(int);
            var (methodName, ilGenerator) = this.GetILGenerator<int>();

            ilGenerator.EmitDefault(typeof(int));
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<int>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitSByte()
        {
            var value = (sbyte)'A';
            var (methodName, ilGenerator) = this.GetILGenerator<sbyte>();

            ilGenerator.EmitSByte(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<sbyte>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitShort()
        {
            var value = (short)1;
            var (methodName, ilGenerator) = this.GetILGenerator<short>();

            ilGenerator.EmitShort(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<short>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(1000)]
        public void EmitInt(int value)
        {
            var (methodName, ilGenerator) = this.GetILGenerator<int>();

            ilGenerator.EmitInt(value);
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<int>(methodName);

            Assert.Equal(value, returnValue);
        }

        [Fact]
        public void EmitNull()
        {
            var (methodName, ilGenerator) = this.GetILGenerator<object>();

            ilGenerator.EmitNull();
            ilGenerator.Emit(OpCodes.Ret);

            var returnValue = this.GetReturnValue<object>(methodName);

            Assert.Null(returnValue);
        }

        private (string, ILGenerator) GetILGenerator<T>(Type[] parameterTypes = null)
        {
            var methodName = Guid.NewGuid().ToString();
            var ilGenerator = typeBuilder.DefineMethod(methodName, MethodAttributes.Public,
                CallingConventions.Standard, typeof(T), parameterTypes)
                .GetILGenerator();
            return (methodName, ilGenerator);
        }

        private T GetReturnValue<T>(string methodName)
        {
            var type = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(type);
            return (T)type.GetMethod(methodName).Invoke(instance, null);
        }
    }
}
