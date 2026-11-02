using System.Net;
using System.Net.Http.Json;
using PrepApi.Models;

namespace PrepApi.IntegrationTests;

public class TodosApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string UniqueTitle(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    [Fact]
    public async Task Post_ValidTodo_Returns201AndCanBeFetched()
    {
        var title = UniqueTitle("Create");

        var response = await _client.PostAsJsonAsync("/todos", new { title, isDone = false });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        var fetched = await _client.GetFromJsonAsync<Todo>($"/todos/{created.Id}");
        Assert.Equal(title, fetched!.Title);
    }

    [Fact]
    public async Task Post_DuplicateTitleDifferentCase_Returns409()
    {
        var title = UniqueTitle("Case");
        await _client.PostAsJsonAsync("/todos", new { title, isDone = false });

        var response = await _client.PostAsJsonAsync("/todos", new { title = title.ToUpperInvariant(), isDone = false });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_EmptyTitle_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/todos", new { title = "   ", isDone = false });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExisting_Returns404()
    {
        var response = await _client.DeleteAsync("/todos/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ExistingTodo_UpdatesIt()
    {
        var create = await _client.PostAsJsonAsync("/todos", new { title = UniqueTitle("Update"), isDone = false });
        var created = await create.Content.ReadFromJsonAsync<Todo>();
        var newTitle = UniqueTitle("Updated");

        var response = await _client.PutAsJsonAsync($"/todos/{created!.Id}", new { title = newTitle, isDone = true });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<Todo>($"/todos/{created.Id}");
        Assert.Equal(newTitle, fetched!.Title);
        Assert.True(fetched.IsDone);
    }
}