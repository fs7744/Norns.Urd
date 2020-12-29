using System;
using System.Text;

namespace Norns.Urd.Http
{
    public interface ICustomQueryStringBuilder
    {
        Action<StringBuilder, object, string> CreateConverter(QueryAttribute options, Type parameterType);
    }
}