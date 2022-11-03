using System.Reflection;

namespace Norns.Urd.Http
{
    public abstract class ParameterRequestMessageSettingsAttribute : HttpRequestMessageSettingsAttribute
    {
        public ParameterInfo Parameter { get; internal set; }
    }
}