using Norns.Urd.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Norns.Urd.Test.Interceptors.NonAspect
{
    public class DefaultNonAspectTest
    {
        NonAspectTypePredicate defaultTypePredicate;
        NonAspectMethodPredicate defaultMethodPredicate;
        public DefaultNonAspectTest()
        {
            var c = new AspectConfiguration();
            defaultTypePredicate = c.NonPredicates.BuildNonAspectTypePredicate();
            defaultMethodPredicate = c.NonPredicates.BuildNonAspectMethodPredicate();
        }

        [Fact]
        public void WhenMicrosoftShouldTrue()
        {
            Assert.True(defaultTypePredicate(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)));
        }
    }
}
