using Reqnroll;

namespace UnitTests;

[Binding]
public class TodoBindings
{
    [Given("a todo item")]
    public void GivenATodoItem()
    {
        Todo = new Todo.Api.Todos.Todo();
    }

    public Todo.Api.Todos.Todo? Todo { get ; set ; }

    [Then("the item is no complete")]
    public void ThenTheItemIsNoComplete()
    {
        Assert.False(Todo?.IsComplete);
    }

    [Then("the item is complete")]
    public void ThenTheItemIsComplete()
    {
        Assert.True(Todo?.IsComplete);
    }

    [When("the todo is complete")]
    public void WhenTheTodoIsComplete()
    {
        Todo?.Complete();
    }
}