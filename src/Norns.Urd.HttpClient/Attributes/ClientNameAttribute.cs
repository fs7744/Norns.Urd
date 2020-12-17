using Microsoft.Extensions.Options;
using System;

namespace Norns.Urd.HttpClient
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class ClientNameAttribute : Attribute
    {
        public ClientNameAttribute(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? Options.DefaultName : name;
        }

        public string Name { get; }
    }
}