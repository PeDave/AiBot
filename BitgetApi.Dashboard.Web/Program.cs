using Microsoft.EntityFrameworkCore;
using BitgetApi.WebSocket;
using BitgetApi.Dashboard.Web.Services;
using BitgetApi.Dashboard.Web.Data;
using BitgetApi.Dashboard.Web.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Database
builder.Services.AddDbContext<MarketDataContext>(options =>
    options.UseSqlite("Data Source=marketdata.db"));

// Repositories
builder.Services.AddScoped<ICandleRepository, CandleRepository>();

// BitgetApi services - Singleton because WebSocket connections should persist
builder.Services.AddSingleton<BitgetWebSocketClient>();
builder.Services.AddSingleton<PriceTrackerService>();
builder.Services.AddSingleton<OrderBookService>();
builder.Services.AddSingleton<TradeStreamService>();

// Market data services
builder.Services.AddHttpClient<BitgetMarketDataService>();
builder.Services.AddScoped<HistoricalDataService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketDataContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
