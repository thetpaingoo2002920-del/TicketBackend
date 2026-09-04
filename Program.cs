using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Service Configuration (MongoDB.Driver)
// appsettings.json ထဲမှ ConnectionString (သို့မဟုတ်) Environment Variable ကို ချိတ်ဆက်ပေးခြင်း
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    // Database နာမည်ကို သင့်ပရောဂျက်အတွက် လိုအပ်သလို ပေးနိုင်ပါသည် (ဥပမာ - TicketDb)
    return client.GetDatabase("TicketDb");
});

// CORS Policy Configuration for Vue.js Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin() 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS must be placed before UseAuthorization and MapControllers
app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();