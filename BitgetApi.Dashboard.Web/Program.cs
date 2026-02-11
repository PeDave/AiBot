using BitgetApi.WebSocket;
using BitgetApi.Dashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// BitgetApi services - Singleton because WebSocket connections should persist
builder.Services.AddSingleton<BitgetWebSocketClient>();
builder.Services.AddSingleton<PriceTrackerService>();
builder.Services.AddSingleton<OrderBookService>();
builder.Services.AddSingleton<TradeStreamService>();

var app = builder.Build();

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
