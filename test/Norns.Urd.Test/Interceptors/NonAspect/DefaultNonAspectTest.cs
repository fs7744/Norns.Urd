using Norns.Urd;
using Norns.Urd.Interceptors;
using Xunit;

namespace Test.Norns.Urd.Interceptors.NonAspect
{
    public class DefaultNonAspectTest
    {
        private readonly NonAspectTypePredicate defaultTypePredicate;
        private readonly NonAspectMethodPredicate defaultMethodPredicate;

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