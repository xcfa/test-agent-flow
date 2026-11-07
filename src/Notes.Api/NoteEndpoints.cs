using Notes.Application;
using Notes.Contracts;
using Notes.Domain;

namespace Notes.Api;

public static class NoteEndpoints
{
    public static IEndpointRouteBuilder MapNoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notes");

        group.MapGet("/", async (NoteService service, CancellationToken ct) =>
            (await service.ListAsync(ct)).Select(ToDto));

        group.MapGet("/{id:guid}", async (Guid id, NoteService service, CancellationToken ct) =>
            await service.GetAsync(id, ct) is { } note ? Results.Ok(ToDto(note)) : Results.NotFound());

        group.MapPost("/", (SaveNoteRequest request, NoteService service, CancellationToken ct) =>
            Validated(async () =>
            {
                var note = await service.CreateAsync(request.Title, request.Body, ct);
                return Results.Created($"/api/notes/{note.Id}", ToDto(note));
            }));

        group.MapPut("/{id:guid}", (Guid id, SaveNoteRequest request, NoteService service, CancellationToken ct) =>
            Validated(async () =>
                await service.UpdateAsync(id, request.Title, request.Body, ct) is { } note
                    ? Results.Ok(ToDto(note))
                    : Results.NotFound()));

        group.MapDelete("/{id:guid}", async (Guid id, NoteService service, CancellationToken ct) =>
            await service.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

        return app;
    }

    private static NoteDto ToDto(Note note) =>
        new(note.Id, note.Title, note.Body, note.CreatedAt, note.UpdatedAt);

    private static async Task<IResult> Validated(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "request"] = [ex.Message],
            });
        }
    }
}
