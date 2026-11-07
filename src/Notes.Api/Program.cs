using Notes.Api;
using Notes.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddNotes();

var app = builder.Build();

app.UseExceptionHandler();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapNoteEndpoints();

await app.RunAsync();

public partial class Program;
