﻿using System;
using System.Net.Http;

namespace Norns.Urd.HttpClient
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class HttpCompletionOptionAttribute : Attribute
    {
        public HttpCompletionOptionAttribute(HttpCompletionOption option)
        {
            Option = option;
        }

        public HttpCompletionOption Option { get; }
    }
}