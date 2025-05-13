using System.Threading.RateLimiting;
using ScrumPokerApp.Hubs;
using ScrumPokerApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddHostedService<SessionCleanupService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Replace with your client’s URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add rate limiting (adjusted for SignalR)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("SignalR", context =>
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.Id,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10, // Increased to allow more requests
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 2
            });
    });
});


var app = builder.Build();
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});




app.UseCors("AllowVueDev");
app.UseRouting();
app.MapHub<ScrumPokerHub>("/scrumPokerHub");
app.Run();