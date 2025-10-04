using image_app.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ImageRepository>();

builder.Configuration
    .AddJsonFile("/etc/secrets/appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var port = Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("⚠️  No PORT environment variable found. Falling back to 8080.");
    port = "8080";
}
else
{
    Console.WriteLine($"✅ Found PORT environment variable: {port}");
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

if (builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var group = app.MapGroup("images").WithName("Images");

app.MapPost("/upload", async (
    [FromServices] ImageRepository images, 
    [FromForm] string name,
    IFormFile file) =>
{
    using MemoryStream ms = new();
    file.CopyTo(ms);
    var id = await images.AddAsync(name, ms.ToArray());
    return Results.Ok(new { id });
}).DisableAntiforgery();

app.MapGet("/metadata", async ([FromServices] ImageRepository images) =>
{
    return await images.GetMetadataAsync();
}).DisableAntiforgery();

app.MapGet("/download", async ([FromServices] ImageRepository images, [FromQuery] Guid id) =>
{
    var image = await images.GetByIdAsync(id);
    return Results.File(image.Data!, "application/octet-stream", image.Name);
}).DisableAntiforgery();

app.MapDelete("/delete/{id:guid}", async ([FromServices] ImageRepository images, [FromRoute] Guid id) =>
{
    await images.DeleteAsync(id);
    return Results.NoContent();
}).DisableAntiforgery();

app.Run();