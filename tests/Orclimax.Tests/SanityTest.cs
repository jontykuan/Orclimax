using NUnit.Framework;

namespace Orclimax.Tests
{
    [TestFixture]
    public class SanityTest
    {
        [Test]
        public void Test_SanityCheck_Passes()
        {
            Assert.Pass("Sanity check passed.");
        }
    }
}
