using System.Text.Json;

namespace SecretSanta.Api;

public class Allocator
{
    public const string AllocationFileName = "allocation.json";
    public const string PeopleFileName = "people.json";

    private static readonly JsonSerializerOptions serializerOptions = new() { AllowTrailingCommas = false, WriteIndented = true };

    private readonly object _sync = new();
    private readonly Random _random = new((int)DateTime.Now.Ticks);

    public string Allocate(string name)
    {
        lock (_sync)
        {
            AllocateState state = JsonSerializer.Deserialize<AllocateState>(File.ReadAllText(AllocationFileName))!;

            int giverIndex = state.AllocatedGivers.IndexOf(name);

            // Return the name already allocated
            return state.AllocatedReceivers[giverIndex];
        }
    }

    public AllocateState InitialiseState(ICollection<string> people)
    {
        lock (_sync)
        {
            List<string> givers = [.. people];
            List<string> receivers = [.. people];

            AllocateState state = new()
            {
                AllocatedGivers = [],
                AllocatedReceivers = []
            };

            while (givers.Count > 0)
            {
                // Only 2 left?
                if (receivers.Count == 2)
                {
                    // Add second last set
                    string giver1 = givers[0];
                    string giver2 = givers[1];

                    string receiver1 = receivers[0];
                    string receiver2 = receivers[1];

                    if (giver1 != receiver1 && giver2 != receiver2)
                    {
                        // Add names to state sets
                        state.AllocatedGivers.Add(giver1);
                        state.AllocatedReceivers.Add(receiver1);

                        state.AllocatedGivers.Add(giver2);
                        state.AllocatedReceivers.Add(receiver2);
                    }
                    else
                    {
                        // Add names to state sets
                        state.AllocatedGivers.Add(giver1);
                        state.AllocatedReceivers.Add(receiver2);

                        state.AllocatedGivers.Add(giver2);
                        state.AllocatedReceivers.Add(receiver1);
                    }

                    break;
                }

                // Get random indexes
                int i = _random.Next(0, givers.Count);
                int j = _random.Next(0, receivers.Count);

                // Get names
                string giver = givers[i];
                string receiver = receivers[j];

                // Giver and receiver cannot be the same
                if (giver == receiver)
                {
                    continue;
                }

                // Add names to state sets
                state.AllocatedGivers.Add(giver);
                state.AllocatedReceivers.Add(receiver);

                // Remove names from sets
                givers = [.. givers.Where(x => x != giver)];
                receivers = [.. receivers.Where(x => x != receiver)];
            }

            return state;
        }
    }

    public void InitialiseAllocations(IDictionary<string, SecretSantaPerson> people)
    {
        AllocateState state = InitialiseState(people.Keys);
        File.WriteAllText(AllocationFileName, JsonSerializer.Serialize(state, serializerOptions));
    }

    public void InitialisePeople(IDictionary<string, SecretSantaPerson> people)
    {
        File.WriteAllText(PeopleFileName, JsonSerializer.Serialize(people.Values, serializerOptions));
    }

    public void ReadPeople(IDictionary<string, SecretSantaPerson> people)
    {
        var peopleAndCodes = JsonSerializer.Deserialize<IList<SecretSantaPerson>>(File.ReadAllText(PeopleFileName), serializerOptions)!;

        people.Clear();
        foreach(var person in peopleAndCodes)
        {
            people.Add(person.Name, person);
        }
    }
}
