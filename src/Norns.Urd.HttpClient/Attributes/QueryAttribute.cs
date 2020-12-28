using System;

namespace Norns.Urd.Http
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class QueryAttribute : Attribute
    {
        public string Alias { get; set; }
    }
}