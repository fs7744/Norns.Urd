using System;
using Xunit;

namespace Test.Norns.Urd
{
    public class StringExtensionsTest
    {
        [Fact]
        public void MatchesTest()
        {
            Assert.True("Microsoft".Matches("Microsoft"));
            Assert.True("Microsoft.AspNetCore.Hosting.IWebHostEnvironment".Matches("Microsoft.*"));
        }
    }
}