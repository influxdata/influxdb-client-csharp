using NUnit.Framework;

namespace Flux.Tests
{
    public class FirstTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
        }

        [Test]
        public void FirstDummyTest()
        {
            System.Console.WriteLine("First test!");
            Assert.True(true);
        }
    }
}