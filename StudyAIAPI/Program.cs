using DotNetEnv;
using StudyAIAPI.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var groqApiKey = builder.Configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Configura Groq:ApiKey antes de iniciar la aplicación.");


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://studyai-application.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new LLMService(groqApiKey));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
