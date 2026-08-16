using AjayIndustriesERP.Application.Services;
using AjayIndustriesERP.Infrastructure.DependencyInjection;


var builder =
    WebApplication.CreateBuilder(args);


// =========================================================
// QUEST PDF CONFIGURATION
// =========================================================

PurchaseOrderPdfService
    .ConfigureQuestPdf();


// =========================================================
// MVC SERVICES
// =========================================================

builder.Services
    .AddControllersWithViews();


// =========================================================
// INFRASTRUCTURE / APPLICATION DI
// =========================================================

var webRootPath =
    builder.Environment.WebRootPath
    ?? Path.Combine(
        builder.Environment.ContentRootPath,
        "wwwroot");


builder.Services
    .AddInfrastructure(
        builder.Configuration,
        webRootPath);


// =========================================================
// BUILD APPLICATION
// =========================================================

var app =
    builder.Build();


// =========================================================
// HTTP PIPELINE
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");


app.Run();