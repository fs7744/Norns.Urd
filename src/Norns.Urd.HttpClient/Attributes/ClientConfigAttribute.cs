using System;
using Client = System.Net.Http.HttpClient;

namespace Norns.Urd.HttpClient
{
    public abstract class ClientConfigAttribute : Attribute
    {
        public abstract void SetClient(Client client);
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class BaseAddressAttribute : ClientConfigAttribute
    {
        private readonly Uri baseAddress;

        public BaseAddressAttribute(string baseAddress)
        {
            this.baseAddress = new Uri(baseAddress);
        }

        public override void SetClient(Client client)
        {
            client.BaseAddress = baseAddress;
        }
    }
}