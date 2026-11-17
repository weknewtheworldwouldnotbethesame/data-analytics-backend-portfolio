using Microsoft.EntityFrameworkCore;
using OrderSync.Api.Data;
using OrderSync.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Orders")));
builder.Services.AddSingleton<WriteGate>();
builder.Services.AddScoped<SyncService>();
builder.Services.AddHttpClient("receiver", client => client.Timeout = TimeSpan.FromSeconds(3));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
          .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDb>();
    db.Database.Migrate();
    // Only the first launch adds the fixed fictional records.
    if (builder.Configuration.GetValue<bool>("SeedData"))
        SeedOrders.AddIfEmpty(db);
    // A stopped send has an unknown outcome. Keep it visible for review.
    foreach (var order in db.Orders.Where(order => order.Status == "Sent"))
    {
        order.Status = "Failed";
        order.LastError = "Application stopped during send. Check the receiver before retrying.";
        db.Events.Add(new() { OrderId = order.Id, Step = "Failed", Message = order.LastError });
    }
    db.SaveChanges();
}
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.Run();
