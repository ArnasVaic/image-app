using Dapper;
using image_app.Data;
using image_app.Models;
using Microsoft.AspNetCore.Mvc;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ImageRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll"); // or "FrontendOnly" if you restricted

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

if (builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseDeveloperExceptionPage();

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

app.MapGet("/my-endpoint", async (HttpContext http, IConfiguration config) =>
{
    var connStr = config.GetConnectionString("DefaultConnection");
    try
    {
        using var conn = new Npgsql.NpgsqlConnection(connStr);
        var result = await conn.QueryAsync<Image>("SELECT id FROM public.\"Images\"");
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Endpoint /my-endpoint failed: {ex}");
        return Results.Problem(ex.Message);
    }
});

app.Run();