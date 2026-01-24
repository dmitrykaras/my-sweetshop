using System.Collections.ObjectModel;
using System.Windows.Input;
using my_sweetshop.Models;
using my_sweetshop.Views.Catalog;

namespace my_sweetshop.ViewModels
{
    public class CatalogViewModel
    {
        public ObservableCollection<Product> Desserts { get; } = new();
        public ObservableCollection<Product> MaleCakes { get; } = new();
        public ObservableCollection<Product> FemaleCakes { get; } = new();
        public ObservableCollection<Product> KidsCakes { get; } = new();
        public ObservableCollection<Product> WeddingCakes { get; } = new();
        public ObservableCollection<Product> Sets { get; } = new();
        public ObservableCollection<Product> Drinks { get; } = new();

        public ICommand OpenProductCommand { get; }

        public CatalogViewModel()
        {
            OpenProductCommand = new Command<Product>(OpenProduct);

            LoadDesserts();
            LoadMaleCakes();
            LoadFemaleCakes();
            LoadKidsCakes();
            LoadWeddingCakes();
            LoadSets();
            LoadDrinks();
        }

        async void OpenProduct(Product product)
        {
            await Shell.Current.GoToAsync(
                nameof(ProductPage),
                new Dictionary<string, object>
                {
                    ["Product"] = product
                });
        }

        void LoadDesserts()
        {
            Desserts.Add(new Product { Name = "Вишня-фисташка", Image = "cherry_pistachio.jpeg", Description = "Миндальный бисквит + хрустящий слой + начинка из вишни и малины + фисташковый мусс" });
            Desserts.Add(new Product { Name = "Два шоколада", Image = "two_chocolates.jpeg", Description = "Шоколадный бисквит + хрустящий слой + ганаш + шоколадным мусс" });
            Desserts.Add(new Product { Name = "Дорблю-грецкий орех", Image = "door_blue_walnut.jpeg", Description = "Бисквит из грецкого ореха + крем с сыром дорблю + начинка из груши + мусс с белым шоколадом" });
            Desserts.Add(new Product { Name = "Манго-шоколад-маракуйя", Image = "mango_chocolate_passion_fruit.jpeg", Description = "Шоколадный брауни + креме с молочным шоколадом + мусс с манго и маракуйей" });
            Desserts.Add(new Product { Name = "Наполеон", Image = "napoleon.jpg", Description = "Авторский Наполеон с нежным сливочным кремом на белом бельгийском шоколаде" });
            Desserts.Add(new Product { Name = "Чизкейк чёрная смородина", Image = "blackcurrant_cheesecake.jpg", Description = "Основа из песочного печенья + крем чиз с добавлением смородины + смородина в украшении сверху" });
            Desserts.Add(new Product { Name = "Красный бархат", Image = "red_velvet.jpeg", Description = "Бисквит красный бархат + хрустящий слой + йогуртовый мусс" });
            Desserts.Add(new Product { Name = "Павлова", Image = "pavlova.jpg", Description = "Нежнейшее безе + взбитые сливки + ягоды по сезону" });
            Desserts.Add(new Product { Name = "Тарт с орехами", Image = "cake_with_nuts.jpg", Description = "Миндальный тарт + солёная карамель + микс орехов" });
            Desserts.Add(new Product { Name = "Тарт с ягодами", Image = "cake_with_berries.jpg", Description = "Миндальный тарт + крем чиз + ягоды" });
            Desserts.Add(new Product { Name = "Трубочка", Image = "straw.jpg", Description = "Хрустящая трубочка + нежный крем с солёной карамелью" });
            Desserts.Add(new Product { Name = "Картошка в ассортименте", Image = "potatoes.jpg", Description = "Вкус: Ванильная, Шоколадкая, Фисташковая" });
            Desserts.Add(new Product { Name = "Эклер в ассортименте", Image = "eclair.jpg", Description = "Нежнейший эклер с заварным кремом. Вкус: Ванильный, Шоколадный, Карамельный, Фисташковый" });
            Desserts.Add(new Product { Name = "Батончик ореховый", Image = "no_image.png", Description = "Три вида орехов с воздушными рисовыми шариками, хрустящей вафлей в бельгийском шоколаде" });
            Desserts.Add(new Product { Name = "Макарон в ассортименте", Image = "macaron.jpeg" });
            Desserts.Add(new Product { Name = "Ириска", Image = "no_image.png" });
            Desserts.Add(new Product { Name = "Пончик", Image = "donut.jpg" });
            Desserts.Add(new Product { Name = "Эстерхази", Image = "esterhazy.jpg" });
        }

        void LoadMaleCakes()
        {
            MaleCakes.Add(new Product { Name = "Муссовый торт", Image = "m_mousse_cake.jpg", Description = "Начинки муссовых тортов:\r\n1.    Зайкина радость: пряный бисквит с морковью, корицей, цедрой апельсина и грецкими орехами + хрустящий слой + мусс на маскарпоне\r\n2.    Новый медовик: нежнейшие медовые коржи + хрустящий слой + сметанно-сливочный мусс с мёдом\r\n3.    Бейлиз: шоколадный бисквит + крем с тёмным шоколадом и ликёром Бейлиз + воздушный мусс на основе белого шоколада\r\n4.    Два шоколада: шоколадный бисквит + хрустящий слой из бельгийской вафли + муссы на тёмном и белом шоколаде\r\n5.    Шоколад-манго-маракуйя: шоколадный бисквит-суфле + хрустящий слой = крем с пюре манго + мусс на бельгийском шоколаде с соком маракуйи\r\n6.    Клубника-персик-сливки: лёгкий заварной бисквит + клубничный центр + персиковый крем + нежнейший сливочный мусс\r\n7.    Груша-карамель: пряный бисквит с кусочками груши, корицы, орехами + солёная карамель + грушевое кули + нежный сырный мусс на маскарпоне\r\n8.    Фундук-карамель: ореховый бисквит + хрустящий слой на молочном шоколаде + карамельный крем + мусс с фундучным пралине\r\n9.    Вишнёвый или клубничный йогурт: бисквит Джоконда + вишнёвое/клубничное кули + хрустящий слой + йогуртовый мусс\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
            MaleCakes.Add(new Product { Name = "Бисквитный торт", Image = "m_sponge_cake.jpg", Description = "Начинки бисквитных тортов:\r\n1.    Фруктовый: белый/шоколадный бисквит + нежный сливочный мусс + ягоды по сезону\r\n2.    Красный бархат: Нежный шифоновый бисквит с шоколадным послевкусием + крем чиз на маскарпоне\r\n3.    Сникерс: шоколадный бисквит + карамель + орехи + карамельный крем\r\n4.    Рафаэлло: нежный белый бисквит + крем рафаэлло\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
        }

        void LoadFemaleCakes()
        {
            FemaleCakes.Add(new Product { Name = "Муссовый торт", Image = "w_mousse_cake.jpg", Description = "Начинки муссовых тортов:\r\n1.    Зайкина радость: пряный бисквит с морковью, корицей, цедрой апельсина и грецкими орехами + хрустящий слой + мусс на маскарпоне\r\n2.    Новый медовик: нежнейшие медовые коржи + хрустящий слой + сметанно-сливочный мусс с мёдом\r\n3.    Бейлиз: шоколадный бисквит + крем с тёмным шоколадом и ликёром Бейлиз + воздушный мусс на основе белого шоколада\r\n4.    Два шоколада: шоколадный бисквит + хрустящий слой из бельгийской вафли + муссы на тёмном и белом шоколаде\r\n5.    Шоколад-манго-маракуйя: шоколадный бисквит-суфле + хрустящий слой = крем с пюре манго + мусс на бельгийском шоколаде с соком маракуйи\r\n6.    Клубника-персик-сливки: лёгкий заварной бисквит + клубничный центр + персиковый крем + нежнейший сливочный мусс\r\n7.    Груша-карамель: пряный бисквит с кусочками груши, корицы, орехами + солёная карамель + грушевое кули + нежный сырный мусс на маскарпоне\r\n8.    Фундук-карамель: ореховый бисквит + хрустящий слой на молочном шоколаде + карамельный крем + мусс с фундучным пралине\r\n9.    Вишнёвый или клубничный йогурт: бисквит Джоконда + вишнёвое/клубничное кули + хрустящий слой + йогуртовый мусс\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
            FemaleCakes.Add(new Product { Name = "Бисквитный торт", Image = "w_sponge_cake.jpg", Description = "Начинки бисквитных тортов:\r\n1.    Фруктовый: белый/шоколадный бисквит + нежный сливочный мусс + ягоды по сезону\r\n2.    Красный бархат: Нежный шифоновый бисквит с шоколадным послевкусием + крем чиз на маскарпоне\r\n3.    Сникерс: шоколадный бисквит + карамель + орехи + карамельный крем\r\n4.    Рафаэлло: нежный белый бисквит + крем рафаэлло\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
        }

        void LoadKidsCakes()
        {
            KidsCakes.Add(new Product { Name = "Муссовый торт", Image = "c_mousse_cake.jpg", Description = "Начинки муссовых тортов:\r\n1.    Зайкина радость: пряный бисквит с морковью, корицей, цедрой апельсина и грецкими орехами + хрустящий слой + мусс на маскарпоне\r\n2.    Новый медовик: нежнейшие медовые коржи + хрустящий слой + сметанно-сливочный мусс с мёдом\r\n3.    Бейлиз: шоколадный бисквит + крем с тёмным шоколадом и ликёром Бейлиз + воздушный мусс на основе белого шоколада\r\n4.    Два шоколада: шоколадный бисквит + хрустящий слой из бельгийской вафли + муссы на тёмном и белом шоколаде\r\n5.    Шоколад-манго-маракуйя: шоколадный бисквит-суфле + хрустящий слой = крем с пюре манго + мусс на бельгийском шоколаде с соком маракуйи\r\n6.    Клубника-персик-сливки: лёгкий заварной бисквит + клубничный центр + персиковый крем + нежнейший сливочный мусс\r\n7.    Груша-карамель: пряный бисквит с кусочками груши, корицы, орехами + солёная карамель + грушевое кули + нежный сырный мусс на маскарпоне\r\n8.    Фундук-карамель: ореховый бисквит + хрустящий слой на молочном шоколаде + карамельный крем + мусс с фундучным пралине\r\n9.    Вишнёвый или клубничный йогурт: бисквит Джоконда + вишнёвое/клубничное кули + хрустящий слой + йогуртовый мусс\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ\r\n2 500р." });
            KidsCakes.Add(new Product { Name = "Бисквитный торт", Image = "c_sponge_cake.jpg", Description = "Начинки бисквитных тортов:\r\n1.    Фруктовый: белый/шоколадный бисквит + нежный сливочный мусс + ягоды по сезону\r\n2.    Красный бархат: Нежный шифоновый бисквит с шоколадным послевкусием + крем чиз на маскарпоне\r\n3.    Сникерс: шоколадный бисквит + карамель + орехи + карамельный крем\r\n4.    Рафаэлло: нежный белый бисквит + крем рафаэлло\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
        }

        void LoadWeddingCakes()
        {
            WeddingCakes.Add(new Product { Name = "Свадебный торт 2 яруса", Image = "wedding_cake_with_two_tiers.jpeg", Description = "Начинки бисквитных тортов:\r\n1.    Фруктовый: белый/шоколадный бисквит + нежный сливочный мусс + ягоды по сезону\r\n2.    Красный бархат: Нежный шифоновый бисквит с шоколадным послевкусием + крем чиз на маскарпоне\r\n3.    Сникерс: шоколадный бисквит + карамель + орехи + карамельный крем\r\n4.    Рафаэлло: нежный белый бисквит + крем рафаэлло\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
            WeddingCakes.Add(new Product { Name = "Свадебный торт 3 яруса", Image = "wedding_cake_has_three_tiers.jpeg", Description = "Начинки бисквитных тортов:\r\n1.    Фруктовый: белый/шоколадный бисквит + нежный сливочный мусс + ягоды по сезону\r\n2.    Красный бархат: Нежный шифоновый бисквит с шоколадным послевкусием + крем чиз на маскарпоне\r\n3.    Сникерс: шоколадный бисквит + карамель + орехи + карамельный крем\r\n4.    Рафаэлло: нежный белый бисквит + крем рафаэлло\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
            WeddingCakes.Add(new Product { Name = "Свадебный торт + капкейки", Image = "wedding_cake_cupcakes.jpeg", Description = "Начинки бисквитных тортов:\r\n1.    Фруктовый: белый/шоколадный бисквит + нежный сливочный мусс + ягоды по сезону\r\n2.    Красный бархат: Нежный шифоновый бисквит с шоколадным послевкусием + крем чиз на маскарпоне\r\n3.    Сникерс: шоколадный бисквит + карамель + орехи + карамельный крем\r\n4.    Рафаэлло: нежный белый бисквит + крем рафаэлло\r\n\r\nВНЕШНИЙ ВИД ТОРТА ОБСУЖДАЕТСЯ ПОСЛЕ ОФОРМЛЕНИЯ" });
        }

        void LoadSets()
        {
            Sets.Add(new Product { Name = "Трюфель 9шт", Image = "truffle_9pcs.jpg" });
            Sets.Add(new Product { Name = "Ассорти из 4-х пирожных", Image = "assorted_4_cakes.jpg", Description = "4 любых пирожных на ваш выбор в одном наборе" });
            Sets.Add(new Product { Name = "Макарон 4шт + цветы", Image = "pieces_4_of_pasta_flowers.jpeg", Description = "Макарон в ассортименте и свежие цветы" });
            Sets.Add(new Product { Name = "Макарон 6шт + цветы", Image = "pieces_6_of_pasta_flowers.jpg", Description = "Макарон 6шт в ассортименте и свежайшие цветы" });
            Sets.Add(new Product { Name = "Макарон 6шт + зефир 9шт + цветы", Image = "pcs6_macaroni_9pcs_marshmallows_flowers.jpg", Description = "Макарон и зефир в ассортименте и цветы на выбор" });
            Sets.Add(new Product { Name = "Макарон 8шт + зефир 8шт + цветы", Image = "pcs8_macaroni_8pcs_marshmallows_flowers.jpeg", Description = "Макарон в ассортименте + вкуснейший зефир + цветы" });
            Sets.Add(new Product { Name = "Капкейк 4шт", Image = "pcs_4_cupcake.jpg", Description = "Внешний вид может отличаться" });
            Sets.Add(new Product { Name = "Капкейки 6шт", Image = "pcs6_cupcakes.jpg" });
            Sets.Add(new Product { Name = "Капкейки 9шт + макарон 6шт", Image = "pcs9_cupcakes_6pcs_pasta.jpg", Description = "Внешний вид может отличаться" });
            Sets.Add(new Product { Name = "Капкейк 12шт", Image = "cupcake_12pcs.jpg", Description = "Внешний вид может отличаться" });
        }

        void LoadDrinks()
        {
            Drinks.Add(new Product { Name = "Эспрессо 30 мл.", Image = "espresso.jpg" });
            Drinks.Add(new Product { Name = "Двойной эспрессо", Image = "espresso.jpg" });
            Drinks.Add(new Product { Name = "Американо 130 мл.", Image = "latte.jpg" });
            Drinks.Add(new Product { Name = "Капучино 250/300 мл.", Image = "americano.jpg" });
            Drinks.Add(new Product { Name = "Латте 250/300 мл.", Image = "americano.jpg" });
            Drinks.Add(new Product { Name = "Латте макиато 250/300 мл.", Image = "americano.jpg" });
            Drinks.Add(new Product { Name = "Флэт Уайт 250/300 мл.", Image = "americano.jpg" });
            Drinks.Add(new Product { Name = "Чай в ассортименте 350 мл.", Image = "americano.jpg" });
            Drinks.Add(new Product { Name = "Фреш 300 мл.", Image = "fresh.jpeg", Description = "Свежевыжатый сок апельсина или грейпфрута" });
            Drinks.Add(new Product { Name = "Молочный коктейль 350 мл.", Image = "milkshake.jpeg" });
            Drinks.Add(new Product { Name = "Горячий шоколад 250 мл.", Image = "hot_chocolate.png" });
            Drinks.Add(new Product { Name = "Сок с трубочкой 300 мл.", Image = "juice.jpeg" });
            Drinks.Add(new Product { Name = "Добрый апельсин 300 мл.", Image = "good_orange.jpeg" });
            Drinks.Add(new Product { Name = "Добрый кола 300 мл.", Image = "good_cola.jpeg" });
            Drinks.Add(new Product { Name = "Спрайт 500 мл.", Image = "sprite.jpg" });
            Drinks.Add(new Product { Name = "Pulpy 500 мл.", Image = "pulpy.png" });
            Drinks.Add(new Product { Name = "BonAqua 500 мл.", Image = "bonaqua.jpeg" });
        }
    }
}
