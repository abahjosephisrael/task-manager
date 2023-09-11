using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using TaskManager.Application;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Seeds;
using TaskManager.Infrastructure.Shared;
using TaskManager.Presentation.Api.Extensions;
using TaskManager.Presentation.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerExtension();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();


builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var backg = builder.Services.BuildServiceProvider().GetRequiredService<IBackgroundTask>();

var recurringJobManager = builder.Services.BuildServiceProvider().GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate(
    "Send Due Notification", 
    () => backg.SendDueNotification(), 
    Cron.HourInterval(5));


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    try
    {
        var userManager = (UserManager<User>)scope.ServiceProvider.GetService(typeof(UserManager<User>));
        var roleManager = (RoleManager<IdentityRole>)scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>));

        await DefaultRoles.SeedAsync(roleManager);
        await DefaultAdmin.SeedAsync(userManager);

        Console.WriteLine("Finished Seeding Default Data");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension();
}

app.UseErrorHandlingMiddleware();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.UseHangfireDashboard("/background", new DashboardOptions
{
    IsReadOnlyFunc = (DashboardContext context) => false
});

app.Run();
