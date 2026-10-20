using InsaneDreams.Combat;
using NUnit.Framework;
namespace InsaneDreams.Tests
{
    public sealed class SuppressionRulesTests
    {
        [Test] public void AccumulationCapsThenDecaysAfterQuietGapWithoutTicking()
        {
            var meter=new SuppressionMeter();Assert.AreEqual(0,meter.Sample(10,18,.65f));
            Assert.AreEqual(28,meter.Add(28,10,100,18,.65f));
            Assert.AreEqual(56,meter.Add(28,10.1f,100,18,.65f));
            Assert.AreEqual(100,meter.Add(100,10.2f,100,18,.65f));
            Assert.AreEqual(100,meter.Sample(10.8f,18,.65f),.001f);
            Assert.AreEqual(82,meter.Sample(11.85f,18,.65f),.001f);
            Assert.AreEqual(0,meter.Sample(20,18,.65f));
            Assert.AreEqual(5,meter.Add(5,20,100,18,.65f),.001f);
        }
    }
}
