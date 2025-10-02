using image_app.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ImageRepository>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();

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

app.MapDelete("/delete/{id:guid}", async ([FromServices] ImageRepository images, [FromRoute] Guid id) =>
{
    await images.DeleteAsync(id);
    return Results.NoContent();
}).DisableAntiforgery();

app.Run();
