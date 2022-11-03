using System;
using System.Linq;
using Xunit;

namespace Test.Norns.Urd.Linq
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void WhenDistinctBy()
        {
            var array = new int[] { 1, 324, 5, 1, 5 }.DistinctBy(i => i).ToArray();
            Assert.Equal(3, array.Length);
            Assert.Contains(1, array);
            Assert.Contains(324, array);
            Assert.Contains(5, array);
        }

        [Fact]
        public void WhenUnion()
        {
            var array = new int[] { 5 }.Union(new int[] { 1, 324 }).ToArray();
            Assert.Equal(3, array.Length);
            Assert.Contains(1, array);
            Assert.Contains(324, array);
            Assert.Contains(5, array);
        }

        [Fact]
        public void WhenFirstOrDefault()
        {
            var v = Array.Empty<int>().FirstOrDefault(77);
            Assert.Equal(77, v);
            v = new int[] { 66, 77 }.FirstOrDefault(66);
            Assert.Equal(66, v);
        }

        [Fact]
        public void WhenIsNullOrEmpty()
        {
            Assert.True(Array.Empty<int>().IsNullOrEmpty());
            int[] array = null;
            Assert.True(array.IsNullOrEmpty());
            Assert.False(new int[] { 55 }.IsNullOrEmpty());
        }
    }
}