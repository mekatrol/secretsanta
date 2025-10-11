using Microsoft.AspNetCore.Mvc;

namespace SecretSanta.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class SecretSantaController(ILogger<SecretSantaController> logger, Allocator allocator) : ControllerBase
{
    public static readonly IDictionary<string, SecretSantaPerson> People = new Dictionary<string, SecretSantaPerson>(StringComparer.OrdinalIgnoreCase)
    {
        { "Bob",    new SecretSantaPerson { Name = "Bob",   Code = "1" }},
        { "Mary",    new SecretSantaPerson { Name = "Mary",   Code = "2" }},
        { "Peter",  new SecretSantaPerson { Name = "Peter",   Code = "3" }},
        { "Jane",    new SecretSantaPerson { Name = "Jane",   Code = "4" }},
        { "Simon",  new SecretSantaPerson { Name = "Simon", Code = "5" }},
        { "Claire",  new SecretSantaPerson { Name = "Claire", Code = "6" }},
        { "Mark",  new SecretSantaPerson { Name = "Mark", Code = "7" }}
    };

    private readonly ILogger<SecretSantaController> _logger = logger;
    private readonly Allocator _allocator = allocator;

    [HttpPost(Name = "allocate")]
    public SecretSantaResponse Allocate(SecretSantaRequest selection)
    {
        if (string.IsNullOrWhiteSpace(selection.Giver))
        {
            throw new BadHttpRequestException("You must enter your name");
        }

        // Trim the name
        selection.Giver = selection.Giver.Trim();

        if (string.IsNullOrWhiteSpace(selection.Code))
        {
            throw new BadHttpRequestException("You must enter your code");
        }

        // Trim the name
        selection.Code = selection.Code.Trim();

        SecretSantaPerson selectedPerson = People.Values.SingleOrDefault(x => x.Name.ToLower().Trim() == selection.Giver.ToLower().Trim())
            ?? throw new BadHttpRequestException("You entered your name wrong. Please try again...");

        if (selection.Code != selectedPerson.Code)
        {
            throw new BadHttpRequestException("You entered your code wrong. Please try again...");
        }

        return new SecretSantaResponse
        {
            Giver = selectedPerson.Name,
            Receiver = _allocator.Allocate(selectedPerson.Name)
        };
    }
}
