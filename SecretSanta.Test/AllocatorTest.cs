using SecretSanta.Api;

namespace SecretSanta.Test;

[TestClass]
public class AllocatorTest
{
    [TestMethod]
    public void RunManyTest()
    {
        Allocator allocator = new();

        string[] names = ["Peter", "Bob", "Mary", "Jane", "Fred"];

        for (int j = 0; j < 1000000; j++)
        {
            AllocateState state = allocator.InitialiseState(names);

            Assert.AreEqual(names.Length, state.AllocatedGivers.Count);
            Assert.AreEqual(names.Length, state.AllocatedReceivers.Count);
            Assert.AreEqual(names.Length, state.AllocatedGivers.Distinct().Count());
            Assert.AreEqual(names.Length, state.AllocatedReceivers.Distinct().Count());

            for (int i = 0; i < names.Length; i++)
            {
                Assert.AreNotEqual(state.AllocatedGivers[i], state.AllocatedReceivers[i]);
            }
        }
    }
}