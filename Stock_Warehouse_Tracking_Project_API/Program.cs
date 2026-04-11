using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;
using Stock_Warehouse_Tracking_Project_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// SAP Client (config flag ile Mock/RFC seçimi)
if (builder.Configuration.GetValue<bool>("SapClient:UseMock"))
    builder.Services.AddScoped<ISapClient, MockSapClient>();
else
    builder.Services.AddScoped<ISapClient, RfcSapClient>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<ISapStockClient, MockSapStockClient>();
builder.Services.AddScoped<IStockService, StockService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapHealthChecks("/health");

app.Run();
