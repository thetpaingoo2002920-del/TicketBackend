using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Service Configuration (Render ၏ ConnectionStrings__MongoDB ကို မှန်ကန်စွာဖတ်ရန် "MongoDB" ဟု ပြောင်းထားပါသည်)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    // Render တွင် ထည့်ထားသော ConnectionStrings__MongoDB နှင့် ကိုက်ညီစေရန် "MongoDB" ဟု ထည့်ထားပါသည်
    var connectionString = configuration.GetConnectionString("MongoDB") ?? configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("TicketDb");
});

// CORS Policy Configuration for Frontend
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
// Production (Render) တွင်ပါ Swagger ပွင့်စေရန် Environment စစ်ဆေးမှုကို ဖြုတ်ထားသည်
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticket Backend API V1");
    c.RoutePrefix = string.Empty; // ဒါလေးထည့်ထားလျှင် Root URL (https://your-url.onrender.com/) တွင် Swagger UI တန်းပေါ်လာပါမည်
});

app.UseHttpsRedirection();

// CORS must be placed before UseAuthorization and MapControllers
app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();