using CardsServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.WithOrigins("https://servercards-production.up.railway.app")
         .AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader());
});

// Get Railway DATABASE_URL
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(dbUrl))
    throw new Exception("DATABASE_URL env variable missing");

// Convert URL to Npgsql format
string connectionString = ConvertDatabaseUrl(dbUrl);

builder.Services.AddDbContext<AppDb>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowAll");

// Auto-create tables
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();
}

// ============ ENTERENCE ENDPOINTS ============

// POST /enter — increase counter
app.MapPost("/enter", async (AppDb db, HttpContext ctx) =>
{
    string userIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    string userAgent = ctx.Request.Headers["User-Agent"].ToString();

    var record = await db.Enterence.FirstOrDefaultAsync();

    string entryText =
        $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] IP: {userIp}, Agent: {userAgent}\n";

    if (record == null)
    {
        record = new Enterence
        {
            Counter = 1,
            LastEnetered = DateTime.UtcNow,
            Text = entryText
        };
        db.Enterence.Add(record);
    }
    else
    {
        record.Counter++;
        record.LastEnetered = DateTime.UtcNow;
        record.Text = string.Empty;
    }

    var newRecord = new Enterence
    {
        Counter = -1,
        LastEnetered = DateTime.UtcNow,
        Text = entryText
    };
    db.Enterence.Add(record);

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        counter = record.Counter,
        last = record.LastEnetered
    });
});

// GET /enter — return counter info
app.MapGet("/enter", async (AppDb db) =>
{
    var records = await db.Enterence.ToListAsync();

    if (records == null)
        return Results.Ok(new { counter = 0, last = (DateTime?)null });

    return Results.Ok(records);
});


// ============ REVIEWS ENDPOINTS ============

// POST /reviews — add a review
app.MapPost("/reviews", async (ReviewDto dto, AppDb db) =>
{
    var review = new Review
    {
        Phone = dto.Phone,
        Text = dto.Text,
        CreatedAt = DateTime.UtcNow
    };

    db.Reviews.Add(review);
    await db.SaveChangesAsync();

    return Results.Ok(new { success = true });
});

// GET /reviews — get all reviews
app.MapGet("/reviews", async (AppDb db) =>
{
    var reviews = await db.Reviews
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    return Results.Ok(reviews);
});

app.Run();


// ===================
// Convert FUNCTION
// ===================
static string ConvertDatabaseUrl(string rawUrl)
{
    // railway example:
    // postgres://user:pass@host:port/dbname

    if (!rawUrl.StartsWith("postgres://") && !rawUrl.StartsWith("postgresql://"))
        throw new Exception("Invalid DATABASE_URL format");

    // Remove protocol
    string url = rawUrl.Replace("postgres://", "").Replace("postgresql://", "");

    // Split user:pass@host:port/db
    var atIndex = url.IndexOf("@");
    var userPass = url.Substring(0, atIndex);
    var hostPortDb = url.Substring(atIndex + 1);

    // user + pass
    var user = userPass.Split(':')[0];
    var pass = userPass.Split(':')[1];

    // host + port + db
    var parts = hostPortDb.Split('/');
    var hostPort = parts[0];
    var db = parts[1];

    string host;
    string port;

    if (hostPort.Contains(":"))
    {
        host = hostPort.Split(':')[0];
        port = hostPort.Split(':')[1];
    }
    else
    {
        host = hostPort;
        port = "5432"; // default fallback
    }

    return
        $"Host={host};Port={port};Username={user};Password={pass};Database={db};SSL Mode=Require;Trust Server Certificate=true;";
}


