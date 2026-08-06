using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MySweetShop.Api.Data;
using MySweetShop.Api.Services;

namespace MySweetShop.Api.IScript
{
    public static class ISeedProductImages
    {
        public static async Task SeedAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // Список ваших данных (я добавил несколько для примера, вставьте сюда все остальные)
            var seedData = new List<(Guid Id, string Path)>
            {
            // Десерты
            (Guid.Parse("aaaa1111-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/cherry_pistachio.webp"),
            (Guid.Parse("aaaa1112-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/two_chocolates.webp"),
            (Guid.Parse("aaaa1113-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/door_blue_walnut.webp"),
            (Guid.Parse("aaaa1114-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/mango_chocolate_passion_fruit.webp"),
            (Guid.Parse("aaaa1115-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/napoleon.webp"),
            (Guid.Parse("aaaa1116-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/blackcurrant_cheesecake.webp"),
            (Guid.Parse("aaaa1117-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/red_velvet.webp"),
            (Guid.Parse("aaaa1118-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/pavlova.webp"),
            (Guid.Parse("aaaa1119-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/cake_with_nuts.webp"),
            (Guid.Parse("aaaa1120-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/cake_with_berries.webp"),
            (Guid.Parse("aaaa1121-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/straw.webp"),
            (Guid.Parse("aaaa1122-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/potatoes.webp"),
            (Guid.Parse("aaaa1123-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/eclair.webp"),
            (Guid.Parse("aaaa1124-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/no_image2.webp"),
            (Guid.Parse("aaaa1125-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/macaron.webp"),
            (Guid.Parse("aaaa1126-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/no_image1.webp"),
            (Guid.Parse("aaaa1127-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/donut.webp"),
            (Guid.Parse("aaaa1128-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/esterhazy.webp"),

            // Торты
            (Guid.Parse("bbbb1111-bbbb-1111-bbbb-bbbbbbbbbbbb"), "SeedImages/mens_cakes/m_mousse_cake.webp"),
            (Guid.Parse("bbbb1112-bbbb-1111-bbbb-bbbbbbbbbbbb"), "SeedImages/mens_cakes/m_sponge_cake.webp"),
            (Guid.Parse("cccc1111-cccc-1111-cccc-cccccccccccc"), "SeedImages/womens_cakes/w_mousse_cake.webp"),
            (Guid.Parse("cccc1112-cccc-1111-cccc-cccccccccccc"), "SeedImages/womens_cakes/w_sponge_cake.webp"),
            (Guid.Parse("dddd1111-dddd-1111-dddd-dddddddddddd"), "SeedImages/childrens_cakes/c_mousse_cake.webp"),
            (Guid.Parse("dddd1112-dddd-1111-dddd-dddddddddddd"), "SeedImages/childrens_cakes/c_sponge_cake.webp"),
            (Guid.Parse("eeee1111-eeee-1111-eeee-eeeeeeeeeeee"), "SeedImages/wedding_cake/wedding_cake_with_two_tiers.webp"),
            (Guid.Parse("eeee1112-eeee-1111-eeee-eeeeeeeeeeee"), "SeedImages/wedding_cake/wedding_cake_has_three_tiers.webp"),
            (Guid.Parse("eeee1113-eeee-1111-eeee-eeeeeeeeeeee"), "SeedImages/wedding_cake/wedding_cake_cupcakes.webp"),

            // Наборы
            (Guid.Parse("ffff1111-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/truffle_9pcs.webp"),
            (Guid.Parse("ffff1112-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/assorted_4_cakes.webp"),
            (Guid.Parse("ffff1113-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/pieces_4_of_pasta_flowers.webp"),
            (Guid.Parse("ffff1114-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/pieces_6_of_pasta_flowers.webp"),
            (Guid.Parse("ffff1115-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/pcs6_macaroni_9pcs_marshmallows_flowers.webp"),
            (Guid.Parse("ffff1116-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/pcs8_macaroni_8pcs_marshmallows_flowers.webp"),
            (Guid.Parse("ffff1117-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/pcs_4_cupcake.webp"),
            (Guid.Parse("ffff1118-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/pcs6_cupcakes.webp"),
            (Guid.Parse("ffff1119-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/pcs9_cupcakes_6pcs_pasta.webp"),
            (Guid.Parse("ffff1120-ffff-1111-ffff-ffffffffffff"), "SeedImages/sets/cupcake_12pcs.webp"),

            // Напитки
            (Guid.Parse("88888888-1111-1111-8888-111111111111"), "SeedImages/drinks/espresso.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111112"), "SeedImages/drinks/espresso.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111113"), "SeedImages/drinks/latte.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111114"), "SeedImages/drinks/americano.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111115"), "SeedImages/drinks/americano.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111116"), "SeedImages/drinks/americano.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111117"), "SeedImages/drinks/americano.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111118"), "SeedImages/drinks/americano.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111119"), "SeedImages/drinks/fresh.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111120"), "SeedImages/drinks/milkshake.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111121"), "SeedImages/drinks/hot_chocolate.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111122"), "SeedImages/drinks/juice.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111123"), "SeedImages/drinks/good_orange.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111124"), "SeedImages/drinks/good_cola.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111125"), "SeedImages/drinks/sprite.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111126"), "SeedImages/drinks/pulpy.webp"),
            (Guid.Parse("88888888-1111-1111-8888-111111111127"), "SeedImages/drinks/bonaqua.webp")
        };

            Console.WriteLine("== Seed изображений начат ==");

            foreach (var item in seedData)
            {
                var product = await db.Products.FindAsync(item.Id);
                if (product == null)
                    continue;

                var fullSrcPath = Path.Combine(env.ContentRootPath, item.Path);

                // Убираем возможные лишние пробелы в путях, если они есть
                fullSrcPath = fullSrcPath.Trim();

                if (!File.Exists(fullSrcPath))
                {
                    // Выведем в лог, что именно мы ищем и как оно выглядит в байтах
                    var bytes = System.Text.Encoding.UTF8.GetBytes(fullSrcPath);
                    Console.WriteLine($"[DEBUG] Пытаюсь найти: {fullSrcPath}");
                    Console.WriteLine($"[DEBUG] Байты пути: {BitConverter.ToString(bytes)}");

                    // Проверим, существует ли родительская папка
                    var parent = Path.GetDirectoryName(fullSrcPath);
                    Console.WriteLine($"[DEBUG] Родительская папка существует: {Directory.Exists(parent)}");

                    continue;
                }

                // Определяем целевую структуру папок: wwwroot/uploads/products/{id}/
                var relativeFolder = Path.Combine("products", item.Id.ToString());
                var baseWebRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
                var absoluteFolder = Path.Combine(baseWebRoot, "uploads", relativeFolder);

                var fileName = Path.GetFileName(fullSrcPath);
                var absoluteDestPath = Path.Combine(absoluteFolder, fileName);
                var relativePath = Path.Combine(relativeFolder, fileName).Replace('\\', '/');
                // Пропускаем, только если БД знает путь И файл реально существует на диске
                if (!string.IsNullOrEmpty(product.ImageKey) && File.Exists(absoluteDestPath))
                    continue;

                // Если папки нет - создаем
                if (!Directory.Exists(absoluteFolder))
                {
                    Directory.CreateDirectory(absoluteFolder);
                }

                // Оборачиваем копирование в try-catch для защиты от параллельного доступа во время тестов
                try
                {
                    File.Copy(fullSrcPath, absoluteDestPath, overwrite: true);
                }
                catch (IOException)
                {
                    // Игнорируем блокировку файла, если другой поток/тест уже копирует этот файл
                }

                // Запись пути в БД
                product.ImageKey = relativePath;
            }

            await db.SaveChangesAsync();
            Console.WriteLine("== Seed изображений завершён ==");
        }
    }
}