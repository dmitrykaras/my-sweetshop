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
            var storage = scope.ServiceProvider.GetRequiredService<IObjectStorage>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // Список ваших данных (я добавил несколько для примера, вставьте сюда все остальные)
            var seedData = new List<(Guid Id, string Path)>
            {
            // Десерты
            (Guid.Parse("aaaa1111-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/cherry_pistachio.jpeg"),
            (Guid.Parse("aaaa1112-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/two_chocolates.jpeg"),
            (Guid.Parse("aaaa1113-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/door_blue_walnut.jpeg"),
            (Guid.Parse("aaaa1114-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/mango_chocolate_passion_fruit.jpeg"),
            (Guid.Parse("aaaa1115-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/napoleon.jpg"),
            (Guid.Parse("aaaa1116-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/blackcurrant_cheesecake.jpg"),
            (Guid.Parse("aaaa1117-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/red_velvet.jpeg"),
            (Guid.Parse("aaaa1118-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/pavlova.jpg"),
            (Guid.Parse("aaaa1119-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/cake_with_nuts.jpg"),
            (Guid.Parse("aaaa1120-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/cake_with_berries.jpg"),
            (Guid.Parse("aaaa1121-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/straw.jpg"),
            (Guid.Parse("aaaa1122-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/potatoes.jpg"),
            (Guid.Parse("aaaa1123-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/eclair.jpg"),
            (Guid.Parse("aaaa1124-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/no_image.png"),
            (Guid.Parse("aaaa1125-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/macaron.jpeg"),
            (Guid.Parse("aaaa1126-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/no_image.png"),
            (Guid.Parse("aaaa1127-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/donut.jpg"),
            (Guid.Parse("aaaa1128-aaaa-1111-aaaa-aaaaaaaaaaaa"), "SeedImages/dessert/esterhazy.jpg"),

            // Торты
            (Guid.Parse("bbbb1111-bbbb-1111-bbbb-bbbbbbbbbbbb"), "SeedImages/mens_cakes/m_mousse_cake.jpg"),
            (Guid.Parse("bbbb1112-bbbb-1111-bbbb-bbbbbbbbbbbb"), "SeedImages/mens_cakes/m_sponge_cake.jpg"),
            (Guid.Parse("cccc1111-cccc-1111-cccc-cccccccccccc"), "SeedImages/womens_cakes/w_mousse_cake.jpg"),
            (Guid.Parse("cccc1112-cccc-1111-cccc-cccccccccccc"), "SeedImages/womens_cakes/w_sponge_cake.jpg"),
            (Guid.Parse("dddd1111-dddd-1111-dddd-dddddddddddd"), "SeedImages/childrens_cakes/c_mousse_cake.jpg"),
            (Guid.Parse("dddd1112-dddd-1111-dddd-dddddddddddd"), "SeedImages/childrens_cakes/c_sponge_cake.jpg"),
            (Guid.Parse("eeee1111-eeee-1111-eeee-eeeeeeeeeeee"), "SeedImages/wedding_cake/wedding_cake_with_two_tiers.jpeg"),
            (Guid.Parse("eeee1112-eeee-1111-eeee-eeeeeeeeeeee"), "SeedImages/wedding_cake/wedding_cake_has_three_tiers.jpeg"),
            (Guid.Parse("eeee1113-eeee-1111-eeee-eeeeeeeeeeee"), "SeedImages/wedding_cake/wedding_cake_cupcakes.jpeg"),

            // Наборы
            (Guid.Parse("ffff1111-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/truffle_9pcs.jpg"),
            (Guid.Parse("ffff1112-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/assorted_4_cakes.jpg"),
            (Guid.Parse("ffff1113-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/pieces_4_of_pasta_flowers.jpeg"),
            (Guid.Parse("ffff1114-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/pieces_6_of_pasta_flowers.jpg"),
            (Guid.Parse("ffff1115-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/pcs6_macaroni_9pcs_marshmallows_flowers.jpg"),
            (Guid.Parse("ffff1116-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/pcs8_macaroni_8pcs_marshmallows_flowers.jpeg"),
            (Guid.Parse("ffff1117-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/pcs_4_cupcake.jpg"),
            (Guid.Parse("ffff1118-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/pcs6_cupcakes.jpg"),
            (Guid.Parse("ffff1119-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/pcs9_cupcakes_6pcs_pasta.jpg"),
            (Guid.Parse("ffff1120-ffff-1111-ffff-ffffffffffff"), "SeedImages/Sets/cupcake_12pcs.jpg"),

            // Напитки
            (Guid.Parse("88888888-1111-1111-8888-111111111111"), "SeedImages/Drinks/espresso.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111112"), "SeedImages/Drinks/espresso.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111113"), "SeedImages/Drinks/latte.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111114"), "SeedImages/Drinks/americano.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111115"), "SeedImages/Drinks/americano.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111116"), "SeedImages/Drinks/americano.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111117"), "SeedImages/Drinks/americano.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111118"), "SeedImages/Drinks/americano.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111119"), "SeedImages/Drinks/fresh.jpeg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111120"), "SeedImages/Drinks/milkshake.jpeg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111121"), "SeedImages/Drinks/hot_chocolate.png"),
            (Guid.Parse("88888888-1111-1111-8888-111111111122"), "SeedImages/Drinks/juice.jpeg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111123"), "SeedImages/Drinks/good_orange.jpeg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111124"), "SeedImages/Drinks/good_cola.jpeg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111125"), "SeedImages/Drinks/sprite.jpg"),
            (Guid.Parse("88888888-1111-1111-8888-111111111126"), "SeedImages/Drinks/pulpy.png"),
            (Guid.Parse("88888888-1111-1111-8888-111111111127"), "SeedImages/Drinks/bonaqua.jpeg")
        };

            Console.WriteLine("== Seed изображений начат ==");

            foreach (var item in seedData)
            {
                var product = await db.Products.FindAsync(item.Id);
                if (product == null)
                    continue;

                var fullPath = Path.Combine(env.ContentRootPath, item.Path); // ✅ Path
                if (!File.Exists(fullPath))
                    continue;

                // не перезатираем существующие ключи
                if (!string.IsNullOrEmpty(product.ImageKey))
                    continue;

                await using var stream = File.OpenRead(fullPath);

                var formFile = new FormFile(
                    stream,
                    0,
                    stream.Length,
                    "file",
                    Path.GetFileName(fullPath)
                )
                {
                    Headers = new HeaderDictionary(),
                    ContentType = GetContentType(fullPath)
                };

                // upload в storage
                var (imageKey, _) = await storage.UploadAsync(formFile, $"products/{item.Id}");

                // запись в БД
                product.ImageKey = imageKey;
            }

            await db.SaveChangesAsync();

            Console.WriteLine("== Seed изображений завершён ==");
        }

        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}