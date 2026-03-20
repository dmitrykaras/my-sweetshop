using Microsoft.EntityFrameworkCore;
using MySweetShop.Api.Entities;

namespace MySweetShop.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<EmailChangeCode> EmailChangeCodes => Set<EmailChangeCode>();
   
    
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<UserFavorite> UserFavorites { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Категории
        var dessertId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var maleCakesId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var femaleCakesId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var kidsCakesId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var weddingCakesId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var setsId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var drinksId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = dessertId, Name = "Десерты" },
            new Category { Id = maleCakesId, Name = "Мужские торты" },
            new Category { Id = femaleCakesId, Name = "Женские торты" },
            new Category { Id = kidsCakesId, Name = "Детские торты" },
            new Category { Id = weddingCakesId, Name = "Свадебные торты" },
            new Category { Id = setsId, Name = "Наборы" },
            new Category { Id = drinksId, Name = "Напитки" }
        );

        // Продукты
        modelBuilder.Entity<Product>().HasData(
            // Десерты
            new Product { Id = Guid.Parse("aaaa1111-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Вишня-фисташка", CategoryId = dessertId, Description = "Миндальный бисквит + хрустящий слой + начинка из вишни и малины + фисташковый мусс" },
            new Product { Id = Guid.Parse("aaaa1112-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Два шоколада", CategoryId = dessertId, Description = "Шоколадный бисквит + хрустящий слой + ганаш + шоколадный мусс" },
            new Product { Id = Guid.Parse("aaaa1113-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Дорблю-грецкий орех", CategoryId = dessertId, Description = "Бисквит из грецкого ореха + крем с сыром дорблю + начинка из груши + мусс с белым шоколадом" },
            new Product { Id = Guid.Parse("aaaa1114-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Манго-шоколад-маракуйя", CategoryId = dessertId, Description = "Шоколадный брауни + креме с молочным шоколадом + мусс с манго и маракуйей"   },
            new Product { Id = Guid.Parse("aaaa1115-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Наполеон", CategoryId = dessertId, Description = "Авторский Наполеон с нежным сливочным кремом на белом бельгийском шоколаде"},
            new Product { Id = Guid.Parse("aaaa1116-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Чизкейк чёрная смородина", CategoryId = dessertId, Description = "Основа из песочного печенья + крем чиз с добавлением смородины + смородина в украшении сверху" },
            new Product { Id = Guid.Parse("aaaa1117-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Красный бархат", CategoryId = dessertId, Description = "Бисквит красный бархат + хрустящий слой + йогуртовый мусс" },
            new Product { Id = Guid.Parse("aaaa1118-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Павлова", CategoryId = dessertId, Description = "Нежнейшее безе + взбитые сливки + ягоды по сезону" },
            new Product { Id = Guid.Parse("aaaa1119-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Тарт с орехами", CategoryId = dessertId, Description = "Миндальный тарт + солёная карамель + микс орехов" },
            new Product { Id = Guid.Parse("aaaa1120-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Тарт с ягодами", CategoryId = dessertId, Description = "Миндальный тарт + крем чиз + ягоды" },
            new Product { Id = Guid.Parse("aaaa1121-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Трубочка", CategoryId = dessertId, Description = "Хрустящая трубочка + нежный крем с солёной карамелью" },
            new Product { Id = Guid.Parse("aaaa1122-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Картошка в ассортименте", CategoryId = dessertId, Description = "Вкус: Ванильная, Шоколадкая, Фисташковая" },
            new Product { Id = Guid.Parse("aaaa1123-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Эклер в ассортименте", CategoryId = dessertId, Description = "Нежнейший эклер с заварным кремом. Вкус: Ванильный, Шоколадный, Карамельный, Фисташковый" },
            new Product { Id = Guid.Parse("aaaa1124-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Батончик ореховый", CategoryId = dessertId, Description = "Три вида орехов с воздушными рисовыми шариками, хрустящей вафлей в бельгийском шоколаде" },
            new Product { Id = Guid.Parse("aaaa1125-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Макарон в ассортименте", CategoryId = dessertId },
            new Product { Id = Guid.Parse("aaaa1126-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Ириска", CategoryId = dessertId },
            new Product { Id = Guid.Parse("aaaa1127-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Пончик", CategoryId = dessertId },
            new Product { Id = Guid.Parse("aaaa1128-aaaa-1111-aaaa-aaaaaaaaaaaa"), Name = "Эстерхази", CategoryId = dessertId },

            // Мужские торты
            new Product { Id = Guid.Parse("bbbb1111-bbbb-1111-bbbb-bbbbbbbbbbbb"), Name = "Муссовый торт", CategoryId = maleCakesId, Description = "Начинки муссовых тортов: Зайкина радость, Новый медовик, Бейлиз, Два шоколада, Шоколад-манго-маракуйя, Клубника-персик-сливки, Груша-карамель, Фундук-карамель, Вишнёвый или клубничный йогурт" },
            new Product { Id = Guid.Parse("bbbb1112-bbbb-1111-bbbb-bbbbbbbbbbbb"), Name = "Бисквитный торт", CategoryId = maleCakesId, Description = "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло" },

            // Женские торты
            new Product { Id = Guid.Parse("cccc1111-cccc-1111-cccc-cccccccccccc"), Name = "Муссовый торт", CategoryId = femaleCakesId, Description = "Начинки муссовых тортов: Зайкина радость, Новый медовик, Бейлиз, Два шоколада, Шоколад-манго-маракуйя, Клубника-персик-сливки, Груша-карамель, Фундук-карамель, Вишнёвый или клубничный йогурт" },
            new Product { Id = Guid.Parse("cccc1112-cccc-1111-cccc-cccccccccccc"), Name = "Бисквитный торт", CategoryId = femaleCakesId, Description = "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло" },

            // Детские торты
            new Product { Id = Guid.Parse("dddd1111-dddd-1111-dddd-dddddddddddd"), Name = "Муссовый торт", CategoryId = kidsCakesId, Description = "Начинки муссовых тортов: Зайкина радость, Новый медовик, Бейлиз, Два шоколада, Шоколад-манго-маракуйя, Клубника-персик-сливки, Груша-карамель, Фундук-карамель, Вишнёвый или клубничный йогурт" },
            new Product { Id = Guid.Parse("dddd1112-dddd-1111-dddd-dddddddddddd"), Name = "Бисквитный торт", CategoryId = kidsCakesId, Description = "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло" },

            // Свадебные торты
            new Product { Id = Guid.Parse("eeee1111-eeee-1111-eeee-eeeeeeeeeeee"), Name = "Свадебный торт 2 яруса", CategoryId = weddingCakesId, Description = "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло" },
            new Product { Id = Guid.Parse("eeee1112-eeee-1111-eeee-eeeeeeeeeeee"), Name = "Свадебный торт 3 яруса", CategoryId = weddingCakesId, Description = "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло" },
            new Product { Id = Guid.Parse("eeee1113-eeee-1111-eeee-eeeeeeeeeeee"), Name = "Свадебный торт + капкейки", CategoryId = weddingCakesId, Description = "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло" },

            // Наборы
            new Product { Id = Guid.Parse("ffff1111-ffff-1111-ffff-ffffffffffff"), Name = "Трюфель 9шт", CategoryId = setsId },
            new Product { Id = Guid.Parse("ffff1112-ffff-1111-ffff-ffffffffffff"), Name = "Ассорти из 4-х пирожных", CategoryId = setsId, Description = "4 любых пирожных на ваш выбор в одном наборе" },
            new Product { Id = Guid.Parse("ffff1113-ffff-1111-ffff-ffffffffffff"), Name = "Макарон 4шт + цветы", CategoryId = setsId, Description = "Макарон в ассортименте и свежие цветы" },
            new Product { Id = Guid.Parse("ffff1114-ffff-1111-ffff-ffffffffffff"), Name = "Макарон 6шт + цветы", CategoryId = setsId, Description = "Макарон 6шт в ассортименте и свежайшие цветы" },
            new Product { Id = Guid.Parse("ffff1115-ffff-1111-ffff-ffffffffffff"), Name = "Макарон 6шт + зефир 9шт + цветы", CategoryId = setsId, Description = "Макарон и зефир в ассортименте и цветы на выбор" },
            new Product { Id = Guid.Parse("ffff1116-ffff-1111-ffff-ffffffffffff"), Name = "Макарон 8шт + зефир 8шт + цветы", CategoryId = setsId, Description = "Макарон в ассортименте + вкуснейший зефир + цветы" },
            new Product { Id = Guid.Parse("ffff1117-ffff-1111-ffff-ffffffffffff"), Name = "Капкейк 4шт", CategoryId = setsId, Description = "Внешний вид может отличаться" },
            new Product { Id = Guid.Parse("ffff1118-ffff-1111-ffff-ffffffffffff"), Name = "Капкейки 6шт", CategoryId = setsId },
            new Product { Id = Guid.Parse("ffff1119-ffff-1111-ffff-ffffffffffff"), Name = "Капкейки 9шт + макарон 6шт", CategoryId = setsId, Description = "Внешний вид может отличаться" },
            new Product { Id = Guid.Parse("ffff1120-ffff-1111-ffff-ffffffffffff"), Name = "Капкейк 12шт", CategoryId = setsId, Description = "Внешний вид может отличаться" },

            // Напитки
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111111"), Name = "Эспрессо 30 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111112"), Name = "Двойной эспрессо", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111113"), Name = "Американо 130 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111114"), Name = "Капучино 250/300 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111115"), Name = "Латте 250/300 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111116"), Name = "Латте макиато 250/300 мл.", CategoryId = drinksId    },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111117"), Name = "Флэт Уайт 250/300 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111118"), Name = "Чай в ассортименте 350 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111119"), Name = "Фреш 300 мл.", CategoryId = drinksId, Description = "Свежевыжатый сок апельсина или грейпфрута" },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111120"), Name = "Молочный коктейль 350 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111121"), Name = "Горячий шоколад 250 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111122"), Name = "Сок с трубочкой 300 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111123"), Name = "Добрый апельсин 300 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111124"), Name = "Добрый кола 300 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111125"), Name = "Спрайт 500 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111126"), Name = "Pulpy 500 мл.", CategoryId = drinksId },
            new Product { Id = Guid.Parse("88888888-1111-1111-8888-111111111127"), Name = "BonAqua 500 мл.", CategoryId = drinksId }

        );


    modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.FirstName).HasMaxLength(100);
            e.Property(x => x.LastName).HasMaxLength(100);
        });

    modelBuilder.Entity<EmailVerificationCode>(e =>
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Email).IsRequired().HasMaxLength(256);
        e.Property(x => x.CodeHash).IsRequired().HasMaxLength(256);

        e.HasIndex(x => x.Email);
    });

    modelBuilder.Entity<UserFavorite>()
        .HasOne(f => f.User)
        .WithMany()
        .HasForeignKey(f => f.UserId);

    modelBuilder.Entity<UserFavorite>()
        .HasOne(f => f.Product)
        .WithMany()
        .HasForeignKey(f => f.ProductId);
    }
}