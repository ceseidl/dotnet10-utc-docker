using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Injeta TimeProvider/SystemClock
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IClock, SystemClock>();

var app = builder.Build();

// Health
app.MapGet("/health", () => Results.Ok("ok"));

// Endpoint que retorna UTC em ISO 8601
app.MapGet("/utc-now", (IClock clock) => Results.Ok(new { utc = clock.UtcNowOffset.ToString("O") }));

// Exemplo de conversão para local (apenas exibição; persistência deve ser sempre UTC)
app.MapGet("/local-now", (IClock clock) =>
{
    var tz = TimeZoneInfo.Local; // evitar usar em regras de negócio
    var local = TimeZoneInfo.ConvertTime(clock.UtcNow, tz);
    return Results.Ok(new { local = local.ToString("O"), timeZone = tz.Id });
});

app.Run();


// ---------------------------------------------------------
// Tipos DEVEM vir depois dos top-level statements
// ---------------------------------------------------------
public interface IClock
{
    DateTime UtcNow { get; }
    DateTimeOffset UtcNowOffset { get; }
}

public class SystemClock : IClock
{
    private readonly TimeProvider _timeProvider;
    public SystemClock(TimeProvider timeProvider) => _timeProvider = timeProvider;

    public DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
    public DateTimeOffset UtcNowOffset => _timeProvider.GetUtcNow();
}

