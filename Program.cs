var builder = WebApplication.CreateBuilder(args);

// ✅ Register services BEFORE Build()
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("all", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// (yahan baad me JWT, DB, etc add kar sakte ho)

var app = builder.Build();

if (app.Environment.IsDevelopment() || true)  // 🔓 force enable on Render
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();