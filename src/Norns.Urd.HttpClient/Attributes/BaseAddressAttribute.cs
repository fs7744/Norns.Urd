using System;

namespace Norns.Urd.HttpClient
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class BaseAddressAttribute : Attribute
    {
        public BaseAddressAttribute(string baseAddress)
        {
            BaseAddress = new Uri(baseAddress);
        }

        public Uri BaseAddress { get; }
    }
}