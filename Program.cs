var builder = WebApplication.CreateBuilder(args);

// ✅ Register services BEFORE Build()
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("all", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// (yahan baad me JWT, DB, etc add kar sakte ho)

var app = builder.Build();

// ✅ Middleware AFTER Build()
app.UseCors("all");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();