using Norns.Urd.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Norns.Urd.Http
{
    public interface IQueryStringBuilder
    {
        Func<string, AspectContext, string> Build((QueryAttribute, ParameterReflector)[] queryReplacements);
    }

    public class QueryStringBuilder : IQueryStringBuilder
    {
        private readonly Dictionary<Type, Action<StringBuilder, object, string>> cache = new Dictionary<Type, Action<StringBuilder, object, string>>();
        private static readonly MethodInfo nullableToAny = typeof(QueryStringBuilder).GetMethod(nameof(QueryStringBuilder.NullableToAny));

        public QueryStringBuilder()
        {
            cache.Add(typeof(string), ObjectToString);
            cache.Add(typeof(int), ObjectToString);
            cache.Add(typeof(long), ObjectToString);
            cache.Add(typeof(double), ObjectToString);
            cache.Add(typeof(short), ObjectToString);
            cache.Add(typeof(ushort), ObjectToString);
            cache.Add(typeof(uint), ObjectToString);
            cache.Add(typeof(ulong), ObjectToString);
            cache.Add(typeof(Decimal), ObjectToString);
            cache.Add(typeof(bool), ObjectToString);
            cache.Add(typeof(float), ObjectToString);
            cache.Add(typeof(Guid), ObjectToString);
            cache.Add(typeof(char), ObjectToString);
            cache.Add(typeof(byte), ObjectToString);
            cache.Add(typeof(int?), (sb, v, name) => NullableToString(sb, (int?)v, name));
            cache.Add(typeof(long?), (sb, v, name) => NullableToString(sb, (long?)v, name));
            cache.Add(typeof(double?), (sb, v, name) => NullableToString(sb, (double?)v, name));
            cache.Add(typeof(short?), (sb, v, name) => NullableToString(sb, (short?)v, name));
            cache.Add(typeof(int?), (sb, v, name) => NullableToString(sb, (int?)v, name));
            cache.Add(typeof(uint?), (sb, v, name) => NullableToString(sb, (uint?)v, name));
            cache.Add(typeof(ulong?), (sb, v, name) => NullableToString(sb, (ulong?)v, name));
            cache.Add(typeof(ushort?), (sb, v, name) => NullableToString(sb, (ushort?)v, name));
            cache.Add(typeof(Decimal?), (sb, v, name) => NullableToString(sb, (Decimal?)v, name));
            cache.Add(typeof(bool?), (sb, v, name) => NullableToString(sb, (bool?)v, name));
            cache.Add(typeof(float?), (sb, v, name) => NullableToString(sb, (float?)v, name));
            cache.Add(typeof(Guid?), (sb, v, name) => NullableToString(sb, (Guid?)v, name));
            cache.Add(typeof(char?), (sb, v, name) => NullableToString(sb, (Guid?)v, name));
            cache.Add(typeof(byte?), (sb, v, name) => NullableToString(sb, (byte?)v, name));
        }

        public Func<string, AspectContext, string> Build((QueryAttribute, ParameterReflector)[] queryReplacements)
        {
            var querys = queryReplacements.Select<(QueryAttribute, ParameterReflector), Action<StringBuilder, AspectContext>>(i =>
            {
                var name = i.Item1.Alias ?? i.Item2.MemberInfo.Name;
                i.Item1.Alias = name;
                var index = i.Item2.MemberInfo.Position;
                Action<StringBuilder, object, string> builder = CreateBuilder(i.Item1, i.Item2.MemberInfo.ParameterType);
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

        private Action<StringBuilder, object, string> CreateBuilder(QueryAttribute options, Type parameterType)
        {
            if (options.CustomQueryStringBuilder != null)
            {
                var builder = Activator.CreateInstance(options.CustomQueryStringBuilder) as ICustomQueryStringBuilder;
                return builder.CreateConverter(options, parameterType);
            }
            else if (cache.TryGetValue(parameterType, out var builder))
            {
                return builder;
            }
            else if (parameterType.IsEnum)
            {
                return options.EnumFormatters == QueryEnumFormatters.Name
                    ? EnumToName(parameterType)
                    : ObjectToString;
            }
            else if (parameterType == typeof(DateTime))
            {
                return (sb, v, name) => ObjectToString(sb, ((DateTime)v).ToString("o", CultureInfo.InvariantCulture), name);
            }
            else if (parameterType.IsNullableType())
            {
                return (Action<StringBuilder, object, string>)nullableToAny.MakeGenericMethod(Nullable.GetUnderlyingType(parameterType)).Invoke(this, new object[] { options });
            }
            else
            {
                var properties = parameterType.GetProperties()
                    .Where(i => i.CanRead)
                    .Select(i =>
                    {
                        var q = i.GetReflector().GetCustomAttribute<QueryAttribute>() ?? new QueryAttribute();
                        var name = q.Alias = q.Alias ?? i.Name;
                        var b = CreateBuilder(q, i.PropertyType);
                        var getter = i.CreateGetter<object, object>();
                        Action<StringBuilder, object> queryBuilder = (sb, v) =>
                        {
                            b(sb, getter(v), name);
                        };
                        return queryBuilder;
                    })
                    .ToArray();
                return (sb, v, name) =>
                {
                    foreach (var p in properties)
                    {
                        p(sb, v);
                    }
                };
            }
        }

        public static Action<StringBuilder, object, string> EnumToName(Type type)
        {
            return (sb, v, name) => ObjectToString(sb, Enum.GetName(type, v), name);
        }

        public Action<StringBuilder, object, string> NullableToAny<T>(QueryAttribute options) where T : struct
        {
            var builder = CreateBuilder(options, typeof(T));

            return (sb, v, name) =>
            {
                var nv = (T?)v;
                if (nv.HasValue)
                {
                    builder(sb, nv.Value, name);
                }
            };
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