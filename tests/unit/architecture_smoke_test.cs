using NUnit.Framework;

namespace Game.Tests.Unit
{
    public sealed class ArchitectureSmokeTests
    {
        [Test]
        public void TestInfrastructure_IsConfigured()
        {
            Assert.Pass("Root unit test evidence path is configured. Unity-discovered tests live under client/Assets/Tests.");
        }
    }
}
