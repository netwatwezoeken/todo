namespace UnitTest;

public class UnitTests
{
    [Fact]
    public void a_new_todo_item_is_not_completed()
    {
        // Arrange/Act
        var todo = new Todo.Api.Todos.Todo();
        
        // Assert
        Assert.False(todo.IsComplete);
    }
    
    [Fact]
    public void a_todo_item_that_gets_completed_is_completed()
    {
        // Arrange
        var todo = new Todo.Api.Todos.Todo();
        
        // Act
        todo.Complete();
        
        // Assert
        Assert.True(todo.IsComplete);
    }
    
    [Fact]
    public void a_todo_item_that_gets_opened_is_not_completed()
    {
        // Arrange
        var todo = new Todo.Api.Todos.Todo();
        todo.IsComplete = true;
        
        // Act
        todo.Open();
        
        // Assert
        Assert.False(todo.IsComplete);
    }
}
