using System.Reflection;

namespace Norns.Urd.Http
{
    public abstract class ParameterResponseMessageSettingsAttribute : HttpResponseMessageSettingsAttribute
    {
        public ParameterInfo Parameter { get; internal set; }
    }
}