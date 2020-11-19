using System;

namespace Norns.Urd
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class InjectAttribute : Attribute
    {
    }
}