
using SecretSanta.Api.Controllers;

namespace SecretSanta.Api;

public class Program
{
    private const string SecretSantaOrigins = nameof(SecretSantaOrigins);

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        // Add services to the container.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: SecretSantaOrigins,
                policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<Allocator>();

        WebApplication app = builder.Build();

        app.UseCors(SecretSantaOrigins);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI((options) =>
            {
                options.EnableTryItOutByDefault();
            });
        }

        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapControllers();

        Allocator allocator = app.Services.GetRequiredService<Allocator>();

        if (!File.Exists(Allocator.PeopleFileName))
        {
            allocator.InitialisePeople(SecretSantaController.People);
        }

        allocator.ReadPeople(SecretSantaController.People);

        if (!File.Exists(Allocator.AllocationFileName) || !File.Exists(Allocator.PeopleFileName))
        {
            allocator.InitialiseAllocations(SecretSantaController.People);
        }

        app.Run();
    }
}
