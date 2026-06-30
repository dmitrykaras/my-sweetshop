using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MySweetShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("59f62526-63d1-448e-86e3-c37aa0214d64"), "Мужские торты" },
                    { new Guid("5a88b88d-bdb3-4286-b426-0c63a4f0ca79"), "Женские торты" },
                    { new Guid("88c4f058-26eb-40b7-bce6-e1e9e249b7a3"), "Свадебные торты" },
                    { new Guid("9b212ba2-45ca-4d17-b327-279ee3f6bfca"), "Детские торты" },
                    { new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Десерты" },
                    { new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Наборы" },
                    { new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), "Напитки" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("0928c0ce-06e4-4381-88e0-637e6780cf29"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Бисквит из грецкого ореха + крем с сыром дорблю + начинка из груши + мусс с белым шоколадом", "/SeedImages/Desserts/door_blue_walnut.webp", "Дорблю-грецкий орех", 520m },
                    { new Guid("0bc88550-948d-43de-bbae-6d7611b29b50"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Внешний вид может отличаться", "/SeedImages/Sets/pcs_4_cupcake.jpg", "Капкейк 4шт", 800m },
                    { new Guid("197c0beb-cc06-4f5c-b17c-d26e86130b81"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/americano.jpg", "Капучино 250/300 мл.", 250m },
                    { new Guid("1a197f94-2036-4350-b1d3-864ca9179ccc"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/sprite.jpg", "Спрайт 500 мл.", 250m },
                    { new Guid("1a8de910-ed4f-412b-82ae-d64e946b24a6"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Шоколадный бисквит + хрустящий слой + ганаш + шоколадный мусс", "/SeedImages/Desserts/two_chocolates.webp", "Два шоколада", 480m },
                    { new Guid("1d01c9c4-c5b1-4a5b-afe7-bd972c85db63"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Макарон 6шт в ассортименте и свежайшие цветы", "/SeedImages/Sets/pieces_6_of_pasta_flowers.jpg", "Макарон 6шт + цветы", 1300m },
                    { new Guid("1e2b7ea9-d412-4f96-962c-fc9c5889735d"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Три вида орехов с воздушными рисовыми шариками, хрустящей вафлей в бельгийском шоколаде", "/SeedImages/Desserts/no_image.png", "Батончик ореховый", 200m },
                    { new Guid("202f916c-e07f-405d-a96b-2e719a914173"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/good_cola.webp", "Добрый кола 300 мл.", 180m },
                    { new Guid("230df748-3ddd-4dc1-9320-7bd1284fea91"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), null, "/SeedImages/Desserts/donut.jpg", "Пончик", 120m },
                    { new Guid("2880b0c5-8e26-43e3-8149-50b23fe956cc"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/americano.jpg", "Латте макиато 250/300 мл.", 150m },
                    { new Guid("2a3addcb-cf42-4d42-9f1c-d19a9bc62adb"), new Guid("9b212ba2-45ca-4d17-b327-279ee3f6bfca"), "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло", "/SeedImages/KidsCakes/c_sponge_cake.jpg", "Бисквитный торт", 2000m },
                    { new Guid("2a7d4cf1-5ea2-4181-bd3a-8c38029b9b3c"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Основа из песочного печенья + крем чиз с добавлением смородины + смородина в украшении сверху", "/SeedImages/Desserts/blackcurrant_cheesecake.jpg", "Чизкейк чёрная смородина", 450m },
                    { new Guid("39265bc0-da28-44cc-93f7-67f688e8642c"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Миндальный бисквит + хрустящий слой + начинка из вишни и малины + фисташковый мусс", "/SeedImages/Desserts/cherry_pistachio.webp", "Вишня-фисташка", 500m },
                    { new Guid("3cf53091-7f36-4249-af0b-47a7b76a945c"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/americano.jpg", "Чай в ассортименте 350 мл.", 180m },
                    { new Guid("3d047e04-0319-4d0a-8cd1-6887c27b585c"), new Guid("88c4f058-26eb-40b7-bce6-e1e9e249b7a3"), "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло", "/SeedImages/WeddingCakes/wedding_cake_with_two_tiers.webp", "Свадебный торт 2 яруса", 6000m },
                    { new Guid("3d7ca1c1-36a0-4c98-86ff-9032725ee57d"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Авторский Наполеон с нежным сливочным кремом на белом бельгийском шоколаде", "/SeedImages/Desserts/napoleon.jpg", "Наполеон", 400m },
                    { new Guid("44439eb4-a693-4f1e-a866-1eb1192a8f8d"), new Guid("59f62526-63d1-448e-86e3-c37aa0214d64"), "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло", "/SeedImages/MaleCakes/m_sponge_cake.jpg", "Бисквитный торт", 2000m },
                    { new Guid("49ec7689-4c5b-494f-bacc-0e6c189d7630"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Внешний вид может отличаться", "/SeedImages/Sets/cupcake_12pcs.jpg", "Капкейк 12шт", 2400m },
                    { new Guid("4f2ecf76-1d2a-4984-ae0d-fd91bbc9480f"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Макарон в ассортименте и свежие цветы", "/SeedImages/Sets/pieces_4_of_pasta_flowers.webp", "Макарон 4шт + цветы", 900m },
                    { new Guid("52d9f701-1deb-41b8-9b50-c0d3a6380f87"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "4 любых пирожных на ваш выбор в одном наборе", "/SeedImages/Sets/assorted_4_cakes.jpg", "Ассорти из 4-х пирожных", 1000m },
                    { new Guid("55cd13ee-e572-4c85-86b7-5cb3632cfeb7"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Миндальный тарт + крем чиз + ягоды", "/SeedImages/Desserts/cake_with_berries.jpg", "Тарт с ягодами", 460m },
                    { new Guid("587a7fc8-780d-4ac0-bf69-ab0b858af719"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Макарон в ассортименте + вкуснейший зефир + цветы", "/SeedImages/Sets/pcs8_macaroni_8pcs_marshmallows_flowers.webp", "Макарон 8шт + зефир 8шт + цветы", 1600m },
                    { new Guid("599c837c-2a3f-4856-b94e-85a9e019f27c"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Нежнейшее безе + взбитые сливки + ягоды по сезону", "/SeedImages/Desserts/pavlova.jpg", "Павлова", 490m },
                    { new Guid("5a61abd6-3eb1-4fac-b811-71c91a89f3cc"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Вкус: Ванильная, Шоколадкая, Фисташковая", "/SeedImages/Desserts/potatoes.jpg", "Картошка в ассортименте", 280m },
                    { new Guid("61983990-d51f-40eb-ac7a-5113df5ff9b0"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Внешний вид может отличаться", "/SeedImages/Sets/pcs9_cupcakes_6pcs_pasta.jpg", "Капкейки 9шт + макарон 6шт", 1800m },
                    { new Guid("6c29aef2-bac2-4261-8ec6-953052b20974"), new Guid("88c4f058-26eb-40b7-bce6-e1e9e249b7a3"), "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло", "/SeedImages/WeddingCakes/wedding_cake_has_three_tiers.webp", "Свадебный торт 3 яруса", 7500m },
                    { new Guid("6f982e19-4bde-4c0c-abc0-23c488caeb4e"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/pulpy.png", "Pulpy 500 мл.", 270m },
                    { new Guid("7879e98e-f4d4-4d43-b6ac-9393e4ad215e"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/americano.jpg", "Флэт Уайт 250/300 мл.", 200m },
                    { new Guid("798a5085-bd6f-4bfc-a390-b9cf4ddc21fe"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), null, "/SeedImages/Desserts/no_image.png", "Ириска", 150m },
                    { new Guid("7ad095bb-12ca-49b8-87c2-f49639668e8b"), new Guid("9b212ba2-45ca-4d17-b327-279ee3f6bfca"), "Начинки муссовых тортов: Зайкина радость, Новый медовик, Бейлиз, Два шоколада, Шоколад-манго-маракуйя, Клубника-персик-сливки, Груша-карамель, Фундук-карамель, Вишнёвый или клубничный йогурт", "/SeedImages/KidsCakes/c_mousse_cake.jpg", "Муссовый торт", 2500m },
                    { new Guid("80057b8c-fb1d-4b3b-97fa-96b2c74f0452"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), null, "/SeedImages/Sets/truffle_9pcs.jpg", "Трюфель 9шт", 1200m },
                    { new Guid("80bec1aa-7bb1-4aa5-9c90-6cc06f0aaff6"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), null, "/SeedImages/Desserts/macaron.webp", "Макарон в ассортименте", 250m },
                    { new Guid("8365e5ef-5d8b-4607-b8a8-c0ffb27e390b"), new Guid("88c4f058-26eb-40b7-bce6-e1e9e249b7a3"), "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло", "/SeedImages/WeddingCakes/wedding_cake_cupcakes.webp", "Свадебный торт + капкейки", 8000m },
                    { new Guid("8a7c1027-4dde-4924-b36c-3a8c94bca3fe"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Нежнейший эклер с заварным кремом. Вкус: Ванильный, Шоколадный, Карамельный, Фисташковый", "/SeedImages/Desserts/eclair.jpg", "Эклер в ассортименте", 300m },
                    { new Guid("8b26f5d3-9fd6-475b-9876-a136bae19ba4"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/americano.jpg", "Латте 250/300 мл.", 270m },
                    { new Guid("94320477-c17a-44ad-a46a-a7ef184ac0a5"), new Guid("5a88b88d-bdb3-4286-b426-0c63a4f0ca79"), "Начинки бисквитных тортов: Фруктовый, Красный бархат, Сникерс, Рафаэлло", "/SeedImages/FemaleCakes/w_sponge_cake.jpg", "Бисквитный торт", 2000m },
                    { new Guid("9c790c9b-dbe7-4e40-b069-ac092a754c0f"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/latte.jpg", "Американо 130 мл.", 180m },
                    { new Guid("9f17bafc-c96d-452f-833c-b3ca88d76e62"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Миндальный тарт + солёная карамель + микс орехов", "/SeedImages/Desserts/cake_with_nuts.jpg", "Тарт с орехами", 460m },
                    { new Guid("a3293e44-d291-4521-984d-64aaf3def3aa"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/bonaqua.webp", "BonAqua 500 мл.", 270m },
                    { new Guid("a420f339-36f8-4486-a6da-98ec7fd8cb99"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Шоколадный брауни + креме с молочным шоколадом + мусс с манго и маракуйей", "/SeedImages/Desserts/mango_chocolate_passion_fruit.webp", "Манго-шоколад-маракуйя", 550m },
                    { new Guid("aae92bee-88e7-4159-ae02-74a69a289fbd"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/milkshake.webp", "Молочный коктейль 350 мл.", 270m },
                    { new Guid("abfabb8d-da45-46f6-b9c0-90ef499939b1"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Бисквит красный бархат + хрустящий слой + йогуртовый мусс", "/SeedImages/Desserts/red_velvet.webp", "Красный бархат", 470m },
                    { new Guid("ade06f3c-9e1d-4129-b813-22ed21a445b4"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/espresso.jpg", "Двойной эспрессо", 200m },
                    { new Guid("b53760e1-5345-4e65-b881-8ab826300046"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/hot_chocolate.png", "Горячий шоколад 250 мл.", 270m },
                    { new Guid("c1a920a7-825d-4838-8d48-f89c489a5343"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/espresso.jpg", "Эспрессо 30 мл.", 150m },
                    { new Guid("c49ac54b-e054-4f97-b94c-dd1ac1445e9e"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), "Свежевыжатый сок апельсина или грейпфрута", "/SeedImages/Drinks/fresh.webp", "Фреш 300 мл.", 250m },
                    { new Guid("d1237e2b-6cb0-4d41-8e55-7577d9912ec6"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), null, "/SeedImages/Sets/pcs6_cupcakes.jpg", "Капкейки 6шт", 1200m },
                    { new Guid("d14b8fb8-753f-49db-972c-c0cc871ef01f"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), "Хрустящая трубочка + нежный крем с солёной карамелью", "/SeedImages/Desserts/straw.jpg", "Трубочка", 320m },
                    { new Guid("d7b5e313-8b2c-4175-b252-f98852b5e5f6"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/good_orange.webp", "Добрый апельсин 300 мл.", 200m },
                    { new Guid("d9e16f9d-80dc-4967-a2cc-8bb2e5d8a56d"), new Guid("59f62526-63d1-448e-86e3-c37aa0214d64"), "Начинки муссовых тортов: Зайкина радость, Новый медовик, Бейлиз, Два шоколада, Шоколад-манго-маракуйя, Клубника-персик-сливки, Груша-карамель, Фундук-карамель, Вишнёвый или клубничный йогурт", "/SeedImages/MaleCakes/m_mousse_cake.jpg", "Муссовый торт", 2500m },
                    { new Guid("da7edcce-410d-461a-b978-c97d5df1e5f6"), new Guid("f623d1e6-d01e-4865-946a-06af84847c67"), null, "/SeedImages/Drinks/juice.webp", "Сок с трубочкой 300 мл.", 150m },
                    { new Guid("e56a9c41-1985-48c0-95bf-3d69c4c508f9"), new Guid("d6619797-6f1b-45c8-9ba2-653f03a1ef37"), "Макарон и зефир в ассортименте и цветы на выбор", "/SeedImages/Sets/pcs6_macaroni_9pcs_marshmallows_flowers.jpg", "Макарон 6шт + зефир 9шт + цветы", 1500m },
                    { new Guid("f1eef214-5522-46f1-9216-fde6094fd7a0"), new Guid("a56b0d31-28c5-4ed6-873c-3c9cc857691a"), null, "/SeedImages/Desserts/esterhazy.jpg", "Эстерхази", 480m },
                    { new Guid("fe7be9d0-4f53-4af4-8a8b-7ccde9b7ea2b"), new Guid("5a88b88d-bdb3-4286-b426-0c63a4f0ca79"), "Начинки муссовых тортов: Зайкина радость, Новый медовик, Бейлиз, Два шоколада, Шоколад-манго-маракуйя, Клубника-персик-сливки, Груша-карамель, Фундук-карамель, Вишнёвый или клубничный йогурт", "/SeedImages/FemaleCakes/w_mousse_cake.jpg", "Муссовый торт", 2500m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0928c0ce-06e4-4381-88e0-637e6780cf29"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0bc88550-948d-43de-bbae-6d7611b29b50"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("197c0beb-cc06-4f5c-b17c-d26e86130b81"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1a197f94-2036-4350-b1d3-864ca9179ccc"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1a8de910-ed4f-412b-82ae-d64e946b24a6"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1d01c9c4-c5b1-4a5b-afe7-bd972c85db63"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1e2b7ea9-d412-4f96-962c-fc9c5889735d"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("202f916c-e07f-405d-a96b-2e719a914173"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("230df748-3ddd-4dc1-9320-7bd1284fea91"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2880b0c5-8e26-43e3-8149-50b23fe956cc"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2a3addcb-cf42-4d42-9f1c-d19a9bc62adb"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2a7d4cf1-5ea2-4181-bd3a-8c38029b9b3c"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("39265bc0-da28-44cc-93f7-67f688e8642c"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3cf53091-7f36-4249-af0b-47a7b76a945c"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3d047e04-0319-4d0a-8cd1-6887c27b585c"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3d7ca1c1-36a0-4c98-86ff-9032725ee57d"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44439eb4-a693-4f1e-a866-1eb1192a8f8d"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("49ec7689-4c5b-494f-bacc-0e6c189d7630"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("4f2ecf76-1d2a-4984-ae0d-fd91bbc9480f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("52d9f701-1deb-41b8-9b50-c0d3a6380f87"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("55cd13ee-e572-4c85-86b7-5cb3632cfeb7"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("587a7fc8-780d-4ac0-bf69-ab0b858af719"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("599c837c-2a3f-4856-b94e-85a9e019f27c"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("5a61abd6-3eb1-4fac-b811-71c91a89f3cc"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("61983990-d51f-40eb-ac7a-5113df5ff9b0"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("6c29aef2-bac2-4261-8ec6-953052b20974"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("6f982e19-4bde-4c0c-abc0-23c488caeb4e"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("7879e98e-f4d4-4d43-b6ac-9393e4ad215e"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("798a5085-bd6f-4bfc-a390-b9cf4ddc21fe"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("7ad095bb-12ca-49b8-87c2-f49639668e8b"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("80057b8c-fb1d-4b3b-97fa-96b2c74f0452"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("80bec1aa-7bb1-4aa5-9c90-6cc06f0aaff6"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8365e5ef-5d8b-4607-b8a8-c0ffb27e390b"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8a7c1027-4dde-4924-b36c-3a8c94bca3fe"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8b26f5d3-9fd6-475b-9876-a136bae19ba4"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("94320477-c17a-44ad-a46a-a7ef184ac0a5"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("9c790c9b-dbe7-4e40-b069-ac092a754c0f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("9f17bafc-c96d-452f-833c-b3ca88d76e62"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a3293e44-d291-4521-984d-64aaf3def3aa"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a420f339-36f8-4486-a6da-98ec7fd8cb99"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aae92bee-88e7-4159-ae02-74a69a289fbd"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("abfabb8d-da45-46f6-b9c0-90ef499939b1"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ade06f3c-9e1d-4129-b813-22ed21a445b4"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("b53760e1-5345-4e65-b881-8ab826300046"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c1a920a7-825d-4838-8d48-f89c489a5343"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c49ac54b-e054-4f97-b94c-dd1ac1445e9e"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d1237e2b-6cb0-4d41-8e55-7577d9912ec6"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d14b8fb8-753f-49db-972c-c0cc871ef01f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d7b5e313-8b2c-4175-b252-f98852b5e5f6"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d9e16f9d-80dc-4967-a2cc-8bb2e5d8a56d"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("da7edcce-410d-461a-b978-c97d5df1e5f6"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("e56a9c41-1985-48c0-95bf-3d69c4c508f9"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("f1eef214-5522-46f1-9216-fde6094fd7a0"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("fe7be9d0-4f53-4af4-8a8b-7ccde9b7ea2b"));

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
