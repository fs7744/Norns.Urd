namespace Norns.Urd.Interceptors.Features
{
    public static class FeaturesInterceptorExtensions
    {
        public static IAspectConfiguration AddParameterInject(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.Add(new ParameterInjectInterceptor());
            return configuration;
        }
    }
}