using System;

namespace Norns.Urd.Http
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RouteAttribute : Attribute
    {
        public string Alias { get; set; }
    }
}