using System;

namespace Norns.Urd.Http
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class QueryAttribute : Attribute
    {
        public string Alias { get; set; }

        public QueryEnumFormatters EnumFormatters { get; set; }

        public Type CustomQueryStringBuilder { get; set; }
    }

    public enum QueryEnumFormatters
    {
        Name,
        Value
    }
}