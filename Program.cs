using CEBVehicleTracker.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient<IWialonService, WialonService>(client =>
{
    client.BaseAddress = new Uri("https://hst-api.wialon.com/");
});
builder.Services.AddSingleton<List<string>>(_ => new List<string>
{
    "Kan2_LL-0478", "Kan1_LL-5494", "TM_PF-2732",
    "Kan1_LC-7860", "Mat1_42-0590", "Mat1_42-7944",
    "Mat2_42-0367", "Mat2_41-6956"
});
builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<VehicleHub>("/vehicleHub");

app.Run();