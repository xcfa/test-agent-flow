using System.Net;
using System.Net.Http.Json;
using Notes.Contracts;

namespace Notes.Client;

public sealed class NotesClient(HttpClient httpClient)
{
    private const string BasePath = "api/notes";

    public async Task<IReadOnlyList<NoteDto>> ListAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<NoteDto>>(BasePath, cancellationToken) ?? [];

    public async Task<NoteDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(ItemUri(id), cancellationToken);
        return await ReadOrNullAsync(response, cancellationToken);
    }

    public async Task<NoteDto> CreateAsync(SaveNoteRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(BasePath, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<NoteDto>(cancellationToken))!;
    }

    public async Task<NoteDto?> UpdateAsync(Guid id, SaveNoteRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(ItemUri(id), request, cancellationToken);
        return await ReadOrNullAsync(response, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(ItemUri(id), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    private static Uri ItemUri(Guid id) => new($"{BasePath}/{id}", UriKind.Relative);

    private static async Task<NoteDto?> ReadOrNullAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<NoteDto>(cancellationToken);
    }
}
