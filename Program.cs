//To Do List
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskService>(new InMemoryTaskService()); // Register Service into the DI Container. 
//AddSingleton - It refers the lifetime of the dependency

var app = builder.Build(); // Web application object - Application pipeline

var todos = new List<Todo>{};

//URL Rewrite middleware - Buit in middleware
//app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "todos/"));
var rewriteOptions = new RewriteOptions().AddRedirect("tasks/(.*)", "todos/$1");
app.UseRewriter(rewriteOptions);

//Custom Middleware - Logging Middleware(Log some informations when request comes in and response go out)
app.Use(async (context, next) => // Http Context and Request Delegate as parameter
{
    // Pre-processing logic
    Console.WriteLine($"{context.Request.Method} {context.Request.Path} started at {DateTime.UtcNow}");
    await next();
    // Post-processing logic
    Console.WriteLine($"{context.Request.Method} {context.Request.Path} finished at {DateTime.UtcNow}");
});

//app.MapGet("/", () => "Sreenath K G");

//To get all todo list
//app.MapGet("/todos", () => todos);
//Using DI
app.MapGet("/todos", (ITaskService service) => service.GetTodos());

//To get todo list by Id
//app.MapGet("/todos/{Id}", Results<Ok<Todo>, NotFound> (int Id) =>
//{
//  var targetTodo = todos.SingleOrDefault(t=>Id== t.Id);
//  return targetTodo is null ? TypedResults.NotFound() : TypedResults.Ok(targetTodo);
//});
//Using DI
app.MapGet("/todos/{Id}", Results<Ok<Todo>, NotFound> (int Id, ITaskService service) =>
{
  var targetTodo = service.GetTodoById(Id);
  return targetTodo is null ? TypedResults.NotFound() : TypedResults.Ok(targetTodo);
});

//To add into todo list
// app.MapPost("/todos", (Todo task) =>// Todo parameter is the input to handler function
// {
//     todos.Add(task);
//     return TypedResults.Created($"/todos/{task.Id}", task); // Use string interpolation
// })
// .AddEndpointFilter(async (context, next) =>
// {
//     var taskArgument = context.GetArgument<Todo>(0); // 0 means one argument passed
//     var errors = new Dictionary<string, string[]>(); // Error Dictionary

//     if (taskArgument.DueDate < DateTime.UtcNow)
//     {
//         errors.Add(nameof(Todo.DueDate), new[] { "Cannot have due date in past" });
//     }
//     if (taskArgument.IsCompleted)
//     {
//         errors.Add(nameof(Todo.IsCompleted), new[] { "Cannot add completed Todo" });
//     }
//     if (errors.Count > 0)
//     {
//         return Results.ValidationProblem(errors);
//     }
//     return await next(context);
// });
//Using DI
app.MapPost("/todos", (Todo task, ITaskService service) =>// Todo parameter is the input to handler function
{
    service.AddTodo(task);
    return TypedResults.Created($"/todos/{task.Id}", task); // Use string interpolation
})
.AddEndpointFilter(async (context, next) =>
{
    var taskArgument = context.GetArgument<Todo>(0); // 0 means one argument passed
    var errors = new Dictionary<string, string[]>(); // Error Dictionary

    if (taskArgument.DueDate < DateTime.UtcNow)
    {
        errors.Add(nameof(Todo.DueDate), new[] { "Cannot have due date in past" });
    }
    if (taskArgument.IsCompleted)
    {
        errors.Add(nameof(Todo.IsCompleted), new[] { "Cannot add completed Todo" });
    }
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }
    return await next(context);
});

//To delete todos by an Id
// app.MapDelete("/todos/{id}", (int id) =>
// {
//     todos.RemoveAll(t => id == t.Id);
//     return TypedResults.NoContent();
// });
//Using DI
app.MapDelete("/todos/{id}", (int id, ITaskService service) =>
{
    service.DeleteTodoById(id);
    return TypedResults.NoContent();
});

app.Run(); // stops the pipeline — no next middleware runs

public record Todo(int Id, string Name, DateTime DueDate, bool IsCompleted);

//Interface for Dependancy Injection - Common functionality of the Service
interface ITaskService
{
    Todo? GetTodoById(int id);
    List<Todo> GetTodos();
    void DeleteTodoById(int id);
    Todo AddTodo(Todo task);
}

//Interface concrete implementation (complete and usable implementation of a concept or abstraction)
class InMemoryTaskService : ITaskService // All data managed in memory
{
    private readonly List<Todo> _todos = new();

    public Todo AddTodo(Todo task)
    {
        _todos.Add(task);
        return task;
    }

    public void DeleteTodoById(int id)
    {
        _todos.RemoveAll(task => task.Id == id);
    }

    public Todo? GetTodoById(int id)
    {
        return _todos.SingleOrDefault(t => t.Id == id);
    }

    public List<Todo> GetTodos()
    {
        return _todos;
    }
}




