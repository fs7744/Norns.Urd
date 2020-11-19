using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
