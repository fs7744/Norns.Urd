using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Norns.Urd.Http
{
    public interface IQueryStringBuilder
    {
        Func<string, AspectContext, string> Build((string, ParameterReflector)[] queryReplacements);
    }

    public class QueryStringBuilder : IQueryStringBuilder
    {
        private readonly ConcurrentDictionary<Type, Action<StringBuilder, object, string>> cache = new ConcurrentDictionary<Type, Action<StringBuilder, object, string>>();

        public QueryStringBuilder()
        {
            cache.AddOrUpdate(typeof(string), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(int), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(long), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(double), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(short), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(ushort), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(uint), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(ulong), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(Decimal), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(bool), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(float), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(Guid), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(char), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(byte), ObjectToString, (x, y) => y);
            cache.AddOrUpdate(typeof(int?), (sb, v, name) => NullableToString(sb,(int?) v , name), (x, y) => y);
            cache.AddOrUpdate(typeof(long?), (sb, v, name) => NullableToString(sb, (long?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(double?), (sb, v, name) => NullableToString(sb, (double?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(short?), (sb, v, name) => NullableToString(sb, (short?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(int?), (sb, v, name) => NullableToString(sb, (int?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(uint?), (sb, v, name) => NullableToString(sb, (uint?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(ulong?), (sb, v, name) => NullableToString(sb, (ulong?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(ushort?), (sb, v, name) => NullableToString(sb, (ushort?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(Decimal?), (sb, v, name) => NullableToString(sb, (Decimal?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(bool?), (sb, v, name) => NullableToString(sb, (bool?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(float?), (sb, v, name) => NullableToString(sb, (float?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(Guid?), (sb, v, name) => NullableToString(sb, (Guid?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(char?), (sb, v, name) => NullableToString(sb, (Guid?)v, name), (x, y) => y);
            cache.AddOrUpdate(typeof(byte?), (sb, v, name) => NullableToString(sb, (byte?)v, name), (x, y) => y);
        }

        public Func<string, AspectContext, string> Build((string, ParameterReflector)[] queryReplacements)
        {
            var querys = queryReplacements.Select<(string, ParameterReflector),Action <StringBuilder, AspectContext>>(i =>
            {
                var name = i.Item1;
                var index = i.Item2.MemberInfo.Position;
                Action<StringBuilder, object, string> builder = CreateBuilder(i.Item2.MemberInfo.ParameterType);
                return (sb, context) =>
                {
                    var v = context.Parameters[index];
                    if (v != null)
                    {
                        builder(sb, v, name);
                        
                    }
                };
            }).ToArray();

            return (path, context) =>
            {
                var p = path ?? string.Empty;
                var sb = new StringBuilder();
                foreach (var item in querys)
                {
                    item(sb, context);
                }
                if (!p.Contains('?') && sb.Length > 0)
                {
                    sb.Remove(0, 1);
                    sb.Insert(0, '?');
                }
                sb.Insert(0, p);
                return sb.ToString();
            };
        }

        private Action<StringBuilder, object, string> CreateBuilder(Type parameterType)
        {
            return cache.GetOrAdd(parameterType, InternalCreateBuilder);
        }

        private Action<StringBuilder, object, string> InternalCreateBuilder(Type type)
        {
            return null;
        }

        public static void ObjectToString(StringBuilder sb, object o, string name)
        {
            sb.Append('&');
            sb.Append(name);
            sb.Append('=');
            sb.Append(o);
        }

        public static void NullableToString<T>(StringBuilder sb, T? o, string name) where T : struct
        {
            if (o.HasValue)
            {
                ObjectToString(sb, o.Value, name);
            }
        }
        public static void DateTimeToString<T>(StringBuilder sb, T? o, string name) where T : struct
        {
            if (o.HasValue)
            {
                ObjectToString(sb, o.Value, name);
            }
        }
    }
}