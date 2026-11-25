using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.Parse("8.0.46-mysql")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");

app.MapGet("/items",async (ToDoDbContext db) => await db.Items.ToListAsync());
  

app.MapPost("/items",async (Item item,ToDoDbContext  db)=>
   {item.IsComplete ??= false;
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);}
);

app.MapPut("/items/{id}",async(int id,bool IsComplete,ToDoDbContext  db)=>{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound("404");

    item.IsComplete=IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/items/{id}",async (int id,ToDoDbContext  db)=>{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound("404");

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.Run();
