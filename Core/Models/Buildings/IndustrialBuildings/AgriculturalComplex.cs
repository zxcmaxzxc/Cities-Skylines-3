using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Сельскохозяйственный комбинат - промышленное здание для производства сельскохозяйственной продукции
    /// Обрабатывает сырье (семена, удобрения, вода, корм) в готовую продукцию (зерно, овощи, фрукты, молоко, мясо и т.д.)
    /// </summary>
    public class AgriculturalComplex : CommercialBuilding, IConstructable<AgriculturalComplex>
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки сельскохозяйственного комбината в денежных единицах
        /// Используется при строительстве для проверки достаточности средств
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 320000m;

        /// <summary>
        /// Материалы, необходимые для строительства сельскохозяйственного комбината
        /// Используется строительной системой для проверки наличия материалов
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 8 },      // Сталь для каркаса и оборудования
                { ConstructionMaterial.Concrete, 10 },  // Бетон для фундамента и конструкций
                { ConstructionMaterial.Glass, 6 },      // Стекло для теплиц и окон
                { ConstructionMaterial.Plastic, 4 }     // Пластик для труб и упаковки
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы сельскохозяйственного сырья, используемого в производстве
        /// Каждый тип соответствует определенному виду исходного материала
        /// </summary>
        public enum AgriMaterial
        {
            Seeds,       // Семена для посадки растений
            Fertilizer,  // Удобрения для улучшения роста
            Water,       // Вода для полива и содержания животных
            AnimalFeed   // Корм для животноводства
        }

        /// <summary>
        /// Типы сельскохозяйственной продукции, производимой комбинатом
        /// Каждый тип соответствует конечному продукту для продажи или использования
        /// </summary>
        public enum AgriProduct
        {
            Wheat,          // Пшеница - основной зерновой продукт
            Vegetables,     // Овощи - различные виды овощных культур
            Fruits,         // Фрукты - плодовые культуры
            Milk,           // Молоко - продукция молочного животноводства
            Eggs,           // Яйца - птицеводческая продукция
            Meat,           // Мясо - продукция мясного животноводства
            ProcessedFood   // Переработанные продукты - готовая пищевая продукция
        }

        #endregion

        /// <summary>Количество сырья на складе - хранит текущие запасы каждого типа сырья</summary>
        public Dictionary<AgriMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<AgriMaterial, int>();

        /// <summary>Количество продукции на складе - хранит готовую продукцию для продажи</summary>
        public Dictionary<AgriProduct, int> ProductsStorage { get; private set; } = new Dictionary<AgriProduct, int>();

        /// <summary>Максимальная вместимость склада сырья - ограничивает общее количество хранимого сырья</summary>
        public int MaxMaterialStorage { get; private set; }

        /// <summary>Максимальная вместимость склада продукции - ограничивает общее количество готовой продукции</summary>
        public int MaxProductStorage { get; private set; }

        /// <summary>Производственные цеха комбината - каждый цех выполняет определенную производственную операцию</summary>
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

        /// <summary>Текущее количество рабочих - влияет на эффективность производства</summary>
        public int WorkersCount { get; private set; }

        /// <summary>Максимальное количество рабочих - ограничивает численность персонала</summary>
        public int MaxWorkers { get; private set; }

        /// <summary>
        /// Коэффициент эффективности производства в зависимости от количества рабочих
        /// Формула: 0.3 (базовая эффективность) + (рабочие/максимум) * 0.7
        /// Гарантирует что даже при минимальном количестве рабочих есть базовая производительность
        /// </summary>
        public float ProductionEfficiency => WorkersCount > 0 ? 0.3f + (WorkersCount / (float)MaxWorkers) * 0.7f : 0f;

        /// <summary>
        /// Создает новый сельскохозяйственный комбинат с начальной конфигурацией
        /// Инициализирует хранилища, цеха и стартовые материалы
        /// </summary>
        public AgriculturalComplex() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 2000;  // Вместимость склада сырья
            MaxProductStorage = 1000;   // Вместимость склада продукции
            MaxWorkers = 15;            // Максимальное количество рабочих
            WorkersCount = 0;           // Начальное количество рабочих

            InitializeWorkshops();      // Создание производственных цехов
            InitializeStartingMaterials(); // Заполнение начальными материалами
        }

        /// <summary>
        /// Инициализирует производственные цеха комбината
        /// Каждый цех настроен на определенный вид сельскохозяйственной деятельности
        /// с указанием требуемых ресурсов и выходной продукции
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех растениеводства (зерновые культуры) - производит пшеницу из семян, удобрений и воды
            var cropWorkshop = new Workshop
            {
                Name = "Цех растениеводства",
                ProductionCycleTime = 6  // Время производства в условных единицах
            };
            cropWorkshop.InputRequirements.Add("Seeds", 10);      // Требуется 10 единиц семян
            cropWorkshop.InputRequirements.Add("Fertilizer", 5);  // Требуется 5 единиц удобрений
            cropWorkshop.InputRequirements.Add("Water", 8);       // Требуется 8 единиц воды
            cropWorkshop.OutputProducts.Add("Wheat", 15);         // Производит 15 единиц пшеницы
            Workshops.Add(cropWorkshop);

            // Цех овощеводства - производит овощи с более быстрым циклом производства
            var vegetableWorkshop = new Workshop
            {
                Name = "Цех овощеводства",
                ProductionCycleTime = 5
            };
            vegetableWorkshop.InputRequirements.Add("Seeds", 8);
            vegetableWorkshop.InputRequirements.Add("Fertilizer", 4);
            vegetableWorkshop.InputRequirements.Add("Water", 6);
            vegetableWorkshop.OutputProducts.Add("Vegetables", 12);
            Workshops.Add(vegetableWorkshop);

            // Цех садоводства - производит фрукты с более длительным циклом производства
            var orchardWorkshop = new Workshop
            {
                Name = "Цех садоводства",
                ProductionCycleTime = 8
            };
            orchardWorkshop.InputRequirements.Add("Seeds", 6);
            orchardWorkshop.InputRequirements.Add("Fertilizer", 3);
            orchardWorkshop.InputRequirements.Add("Water", 5);
            orchardWorkshop.OutputProducts.Add("Fruits", 10);
            Workshops.Add(orchardWorkshop);

            // Цех животноводства (молочная продукция) - производит молоко и яйца из корма и воды
            var dairyWorkshop = new Workshop
            {
                Name = "Молочный цех",
                ProductionCycleTime = 7
            };
            dairyWorkshop.InputRequirements.Add("AnimalFeed", 12);
            dairyWorkshop.InputRequirements.Add("Water", 4);
            dairyWorkshop.OutputProducts.Add("Milk", 8);
            dairyWorkshop.OutputProducts.Add("Eggs", 6);
            Workshops.Add(dairyWorkshop);

            // Цех переработки мяса - производит мясо из корма с длительным циклом производства
            var meatWorkshop = new Workshop
            {
                Name = "Цех переработки мяса",
                ProductionCycleTime = 10
            };
            meatWorkshop.InputRequirements.Add("AnimalFeed", 15);
            meatWorkshop.InputRequirements.Add("Water", 3);
            meatWorkshop.OutputProducts.Add("Meat", 5);
            Workshops.Add(meatWorkshop);

            // Цех пищевой переработки - производит готовые продукты из другой продукции комбината
            var processingWorkshop = new Workshop
            {
                Name = "Цех пищевой переработки",
                ProductionCycleTime = 9
            };
            processingWorkshop.InputRequirements.Add("Wheat", 8);      // Использует пшеницу из растениеводства
            processingWorkshop.InputRequirements.Add("Milk", 4);       // Использует молоко из животноводства
            processingWorkshop.InputRequirements.Add("Vegetables", 6); // Использует овощи из овощеводства
            processingWorkshop.OutputProducts.Add("ProcessedFood", 10); // Производит переработанные продукты
            Workshops.Add(processingWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые материалы на складе
        /// Обеспечивает начальный запас сырья для начала производства
        /// Добавляет материалы с проверкой вместимости склада
        /// </summary>
        private void InitializeStartingMaterials()
        {
            // Очищаем хранилище перед инициализацией
            MaterialsStorage.Clear();

            // Добавляем основные материалы
            AddMaterial(AgriMaterial.Seeds, 600);
            AddMaterial(AgriMaterial.Fertilizer, 400);
            AddMaterial(AgriMaterial.Water, 800);

            // AnimalFeed добавляем только если есть место (проверка на переполнение)
            int availableSpace = MaxMaterialStorage - GetTotalMaterialStorage();
            if (availableSpace >= 300)
            {
                AddMaterial(AgriMaterial.AnimalFeed, 300);
            }
            else if (availableSpace > 0)
            {
                // Добавляем только то количество, для которого есть место
                AddMaterial(AgriMaterial.AnimalFeed, availableSpace);
            }
        }

        /// <summary>
        /// Устанавливает количество рабочих на комбинате
        /// Автоматически ограничивает значение максимальным количеством рабочих
        /// </summary>
        /// <param name="count">Желаемое количество рабочих</param>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        /// <summary>
        /// Добавляет сырье на склад комбината с проверкой вместимости
        /// Защищает от переполнения склада и отрицательных значений
        /// </summary>
        /// <param name="material">Тип добавляемого сырья</param>
        /// <param name="amount">Количество сырья для добавления</param>
        /// <returns>True если сырье успешно добавлено, false при ошибке</returns>
        public bool AddMaterial(AgriMaterial material, int amount)
        {
            // Защита от отрицательных и нулевых значений
            if (amount <= 0)
                return false;

            // Получаем текущее количество материала (0 если материала нет)
            int currentAmount = MaterialsStorage.ContainsKey(material) ? MaterialsStorage[material] : 0;

            // Проверяем не превысит ли добавление вместимость склада
            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage)
                return false;

            // Обновляем количество материала в хранилище
            MaterialsStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Получает общее количество сырья на складе
        /// Суммирует все значения в словаре материалов
        /// </summary>
        /// <returns>Общее количество сырья</returns>
        public int GetTotalMaterialStorage()
        {
            return MaterialsStorage.Values.Sum();
        }

        /// <summary>
        /// Запускает производственные циклы во всех цехах комбината
        /// Обрабатывает сырье в продукцию с учетом эффективности рабочих
        /// Не выполняется если нет рабочих или эффективность равна 0
        /// </summary>
        public void ProcessWorkshops()
        {
            // Проверка условий для производства
            if (WorkersCount == 0 || ProductionEfficiency <= 0)
                return;

            // Создаем словарь доступных ресурсов для цехов
            var availableResources = new Dictionary<object, int>();

            // Конвертируем enum материалы в строки для совместимости с Workshop
            foreach (var material in MaterialsStorage)
            {
                availableResources.Add(material.Key.ToString(), material.Value);
            }

            // Добавляем существующую продукцию как доступный ресурс для цехов переработки
            foreach (var product in ProductsStorage)
            {
                availableResources.Add(product.Key.ToString(), product.Value);
            }

            // Словарь для накопления произведенной продукции
            var producedOutputs = new Dictionary<object, int>();

            // Обрабатываем каждый цех последовательно
            foreach (var workshop in Workshops)
            {
                // Создаем копию ресурсов для каждого цеха (изолируем изменения)
                var workshopResources = new Dictionary<object, int>(availableResources);
                var workshopOutputs = new Dictionary<object, int>();

                // Запускаем обработку цеха
                if (workshop.Process(workshopResources, workshopOutputs))
                {
                    // Учитываем эффективность производства (влияние количества рабочих)
                    ApplyProductionEfficiency(workshopOutputs);

                    // Обновляем доступные ресурсы после успешной обработки
                    availableResources = workshopResources;

                    // Добавляем выходы цеха в общие выходы
                    foreach (var output in workshopOutputs)
                    {
                        if (producedOutputs.ContainsKey(output.Key))
                            producedOutputs[output.Key] += output.Value;
                        else
                            producedOutputs[output.Key] = output.Value;
                    }
                }
            }

            // Обновляем хранилище материалов после всех производственных циклов
            UpdateMaterialsStorage(availableResources);

            // Обновляем хранилище продукции с проверкой вместимости
            UpdateProductsStorage(producedOutputs);
        }

        /// <summary>
        /// Применяет коэффициент эффективности к произведенной продукции
        /// Уменьшает выход продукции если эффективность меньше 100%
        /// </summary>
        /// <param name="outputs">Словарь произведенной продукции</param>
        private void ApplyProductionEfficiency(Dictionary<object, int> outputs)
        {
            // Если эффективность 100% или больше - не применяем изменения
            if (ProductionEfficiency >= 1f)
                return;

            // Создаем копию ключей для безопасной модификации словаря
            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                // Умножаем количество на коэффициент эффективности
                outputs[key] = (int)(outputs[key] * ProductionEfficiency);

                // Удаляем продукты с нулевым или отрицательным количеством
                if (outputs[key] <= 0)
                    outputs.Remove(key);
            }
        }

        /// <summary>
        /// Обновляет хранилище материалов из результатов обработки цехов
        /// Конвертирует строковые ключи обратно в enum типы
        /// </summary>
        /// <param name="availableResources">Ресурсы после обработки всех цехов</param>
        private void UpdateMaterialsStorage(Dictionary<object, int> availableResources)
        {
            // Очищаем текущее хранилище
            MaterialsStorage.Clear();

            // Заполняем новыми значениями
            foreach (var resource in availableResources)
            {
                // Пытаемся преобразовать строковый ключ обратно в enum
                if (Enum.TryParse<AgriMaterial>(resource.Key.ToString(), out var material))
                {
                    MaterialsStorage[material] = resource.Value;
                }
            }
        }

        /// <summary>
        /// Обновляет хранилище продукции с проверкой вместимости склада
        /// Защищает от переполнения и логирует потери при недостатке места
        /// </summary>
        /// <param name="producedOutputs">Произведенная продукция</param>
        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                // Пытаемся преобразовать строковый ключ в enum продукции
                if (Enum.TryParse<AgriProduct>(output.Key.ToString(), out var product))
                {
                    // Получаем текущее количество продукта
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;

                    // Вычисляем доступное место на складе
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();

                    // Определяем сколько можно добавить без переполнения
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    // Добавляем продукцию если есть место
                    if (amountToAdd > 0)
                    {
                        ProductsStorage[product] = currentAmount + amountToAdd;
                    }

                    // Логируем потерю продукции при переполнении (для отладки)
                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - amountToAdd} единиц продукции {product}");
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет полный рабочий цикл комбината
        /// Обертка для ProcessWorkshops для единообразия API
        /// </summary>
        public void FullProductionCycle()
        {
            ProcessWorkshops();
        }

        /// <summary>
        /// Получает текущие запасы готовой продукции
        /// Возвращает копию словаря для защиты от внешних изменений
        /// </summary>
        /// <returns>Словарь с типами продукции и их количеством</returns>
        public Dictionary<AgriProduct, int> GetProductionOutput()
        {
            return new Dictionary<AgriProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество сырья на складе
        /// Возвращает копию словаря для защиты от внешних изменений
        /// </summary>
        /// <returns>Словарь с типами сырья и их количеством</returns>
        public Dictionary<AgriMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<AgriMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// Уменьшает количество продукции на складе с проверкой наличия
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false при ошибке</returns>
        public bool ConsumeProduct(AgriProduct product, int amount)
        {
            // Защита от отрицательных и нулевых значений
            if (amount <= 0)
                return false;

            // Проверка наличия достаточного количества продукции
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < amount)
                return false;

            // Уменьшаем количество продукции
            ProductsStorage[product] -= amount;

            // Удаляем запись если продукция закончилась
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        /// <summary>
        /// Получает общее количество продукции на складе
        /// Суммирует все значения в словаре продукции
        /// </summary>
        /// <returns>Общее количество продукции</returns>
        public int GetTotalProductStorage()
        {
            return ProductsStorage.Values.Sum();
        }

        /// <summary>
        /// Получает информацию о производственной эффективности комбината
        /// Содержит все ключевые показатели для мониторинга и отладки
        /// </summary>
        /// <returns>Словарь с производственной информацией</returns>
        public Dictionary<string, object> GetProductionInfo()
        {
            return new Dictionary<string, object>
            {
                { "WorkersCount", WorkersCount },                    // Текущее количество рабочих
                { "MaxWorkers", MaxWorkers },                       // Максимальное количество рабочих
                { "ProductionEfficiency", ProductionEfficiency },   // Коэффициент эффективности
                { "TotalMaterialStorage", GetTotalMaterialStorage() }, // Общее количество сырья
                { "MaxMaterialStorage", MaxMaterialStorage },       // Максимальная вместимость сырья
                { "TotalProductStorage", GetTotalProductStorage() }, // Общее количество продукции
                { "MaxProductStorage", MaxProductStorage },         // Максимальная вместимость продукции
                { "ActiveWorkshops", Workshops.Count },             // Количество активных цехов
                { "SeasonalBonus", GetSeasonalBonus() }             // Сезонный бонус к производству
            };
        }

        /// <summary>
        /// Получает сезонный бонус к производству
        /// Имитирует влияние времени года на сельскохозяйственное производство
        /// Летом +20% эффективности, зимой -20%
        /// </summary>
        /// <returns>Коэффициент сезонного бонуса</returns>
        private float GetSeasonalBonus()
        {
            // Получаем текущий месяц для определения сезона
            var month = DateTime.Now.Month;

            // Весна и лето (март-сентябрь) - благоприятный сезон
            // Осень и зима (октябрь-февраль) - неблагоприятный сезон
            return month >= 3 && month <= 9 ? 1.2f : 0.8f;
        }

        /// <summary>
        /// Вызывается при размещении здания на карте
        /// Запускает начальный производственный цикл
        /// </summary>
        public override void OnBuildingPlaced()
        {
            // Запускаем полный производственный цикл при размещении
            FullProductionCycle();
        }
    }
}
