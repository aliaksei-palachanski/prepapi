using Moq;
using PrepApi.Data;
using PrepApi.Models;
using PrepApi.Services;

namespace PrepApi.Tests;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repo = new();
    private readonly TodoService _sut;

    public TodoServiceTests()
    {
        _sut = new TodoService(_repo.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidTitle_NormalizesAddsAndSaves()
    {
        _repo.Setup(r => r.ExistsWithTitleAsync("Buy milk", null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

        var result = await _sut.CreateAsync("  Buy   milk ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy milk", result.Todo!.Title);
        _repo.Verify(r => r.Add(It.Is<Todo>(t => t.Title == "Buy milk")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateTitle_ReturnsDuplicateAndDoesNotSave()
    {
        _repo.Setup(r => r.ExistsWithTitleAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var result = await _sut.CreateAsync("Buy milk");

        Assert.False(result.IsSuccess);
        Assert.Equal(TodoErrorKind.Duplicate, result.Error!.Kind);
        _repo.Verify(r => r.Add(It.IsAny<Todo>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidTitle_DoesNotTouchRepository()
    {
        var result = await _sut.CreateAsync("   ");

        Assert.Equal(TodoErrorKind.Validation, result.Error!.Kind);
        _repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Todo?)null);

        var deleted = await _sut.DeleteAsync(42);

        Assert.False(deleted);
        _repo.Verify(r => r.Remove(It.IsAny<Todo>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Existing_RemovesAndSaves()
    {
        var todo = new Todo { Id = 1, Title = "Buy milk" };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(todo);

        var deleted = await _sut.DeleteAsync(1);

        Assert.True(deleted);
        _repo.Verify(r => r.Remove(todo), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}