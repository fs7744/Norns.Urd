using System;

namespace Norns.Urd.HttpClient.Attributes
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class ClientNameAttribute : Attribute
    {
        public ClientNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}