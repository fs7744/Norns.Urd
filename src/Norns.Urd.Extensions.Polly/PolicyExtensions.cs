namespace Norns.Urd.Extensions.Polly
{
    public static class PolicyExtensions
    {
        public static IAspectConfiguration EnablePolly(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.Add(new PolicyInterceptor());
            return configuration;
        }
    }
}