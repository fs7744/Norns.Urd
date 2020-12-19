using System;
using Client = System.Net.Http.HttpClient;

namespace Norns.Urd.HttpClient
{
    public abstract class ClientSettingsAttribute : Attribute
    {
        public abstract void SetClient(Client client, AspectContext context);
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class BaseAddressAttribute : ClientSettingsAttribute
    {
        private readonly Uri baseAddress;

        public BaseAddressAttribute(string baseAddress)
        {
            this.baseAddress = new Uri(baseAddress);
        }

        public override void SetClient(Client client, AspectContext context)
        {
            client.BaseAddress = baseAddress;
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class MaxResponseContentBufferSizeAttribute : ClientSettingsAttribute
    {
        private readonly long maxResponseContentBufferSize;

        public MaxResponseContentBufferSizeAttribute(long maxResponseContentBufferSize)
        {
            this.maxResponseContentBufferSize = maxResponseContentBufferSize;
        }

        public override void SetClient(Client client, AspectContext context)
        {
            client.MaxResponseContentBufferSize = maxResponseContentBufferSize;
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class TimeoutAttribute : ClientSettingsAttribute
    {
        private readonly TimeSpan timeout;

        public TimeoutAttribute(string timeout)
        {
            this.timeout = TimeSpan.Parse(timeout);
        }

        public override void SetClient(Client client, AspectContext context)
        {
            client.Timeout = timeout;
        }
    }
}