var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelDb>(options => 
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/Hotels", async (HotelDb db) => await db.hotels.ToListAsync()); // Вывод всех отелей

app.MapGet("/Hotels/{id}",async (int id, HotelDb db) => // Вывод отеля по айди
    await db.hotels.FirstOrDefaultAsync(h => h.Id == id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound()
);

app.MapPost("/Hotels", async ([FromBody] Hotel hotel, HotelDb db) => // Добавить отель
{
    db.hotels.Add(hotel);
    await db.SaveChangesAsync();
    return Results.Created($"/Hotels/{hotel.Id}", hotel);
});

app.MapPut("/Hotels", async ([FromBody] Hotel hotel, HotelDb db) => // Изменение отеля
{ 
    var hotelFromDb = await db.hotels.FindAsync(new object[] {hotel.Id});

    if (hotelFromDb == null) return Results.NotFound();

    hotelFromDb.Name = hotel.Name;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("Hotels/{id}", async (int id, HotelDb db) => // Удаление отеля
{ 
    var hotelFromDb = await db.hotels.FindAsync(new object[] {id});

    if (hotelFromDb == null) return Results.NotFound();

    db.hotels.Remove(hotelFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// app.UseHttpsRedirection();

app.Run();

public class HotelDb : DbContext
{
    public HotelDb(DbContextOptions<HotelDb> options) : base(options) { }
    
    public DbSet<Hotel> hotels => Set<Hotel>();
}