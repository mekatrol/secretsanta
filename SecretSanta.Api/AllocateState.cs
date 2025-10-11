namespace SecretSanta.Api;

public class AllocateState
{
    public List<string> AllocatedGivers { get; set; } = [];
    public List<string> AllocatedReceivers { get; set; } = [];
}
