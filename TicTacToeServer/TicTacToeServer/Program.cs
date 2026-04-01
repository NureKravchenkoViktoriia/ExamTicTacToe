using TicTacToeServer.Services; // Додаємо using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Реєструємо як Singleton, щоб стан гри (поле 100х100) був єдиним на весь сервер
builder.Services.AddSingleton<GameEngineService>();
// Запускаємо як фоновий процес, який постійно читатиме чергу
builder.Services.AddHostedService(provider => provider.GetRequiredService<GameEngineService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
