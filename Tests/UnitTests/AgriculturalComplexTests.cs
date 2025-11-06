using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для сельскохозяйственного комбината
    /// </summary>
    [TestClass]
    public sealed class AgriculturalComplexTests
    {
        /// <summary>
        /// Тест создания агрокомбината
        /// </summary>
        [TestMethod]
        public void TestAgriculturalComplexCreation()
        {
            var complex = new AgriculturalComplex();

            // Проверка статических свойств строительства
            Assert.AreEqual(320000m, AgriculturalComplex.BuildCost);
            Assert.AreEqual(4, AgriculturalComplex.RequiredMaterials.Count);
            Assert.AreEqual(8, AgriculturalComplex.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(10, AgriculturalComplex.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(6, AgriculturalComplex.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(4, AgriculturalComplex.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(2000, complex.MaxMaterialStorage);
            Assert.AreEqual(1000, complex.MaxProductStorage);
            Assert.AreEqual(15, complex.MaxWorkers);
            Assert.AreEqual(0, complex.WorkersCount);
            Assert.AreEqual(6, complex.Workshops.Count);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var complex = new AgriculturalComplex();

            var materials = complex.GetMaterialStorage();

            Assert.AreEqual(600, materials[AgriculturalComplex.AgriMaterial.Seeds]);
            Assert.AreEqual(400, materials[AgriculturalComplex.AgriMaterial.Fertilizer]);
            Assert.AreEqual(800, materials[AgriculturalComplex.AgriMaterial.Water]);
            Assert.AreEqual(300, materials[AgriculturalComplex.AgriMaterial.AnimalFeed]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var complex = new AgriculturalComplex();

            // Установка количества рабочих
            complex.SetWorkersCount(8);
            Assert.AreEqual(8, complex.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            complex.SetWorkersCount(20);
            Assert.AreEqual(15, complex.WorkersCount); // Должно ограничиться MaxWorkers

            // Установка нуля рабочих
            complex.SetWorkersCount(0);
            Assert.AreEqual(0, complex.WorkersCount);
        }

        /// <summary>
        /// Тест добавления сырья
        /// </summary>
        [TestMethod]
        public void TestAddMaterials()
        {
            var complex = new AgriculturalComplex();

            // Получаем начальное состояние
            var initialMaterials = complex.GetMaterialStorage();
            int initialSeeds = initialMaterials[AgriculturalComplex.AgriMaterial.Seeds];
            int initialTotal = complex.GetTotalMaterialStorage();

            // Вычисляем сколько можно добавить без превышения лимита
            int availableSpace = complex.MaxMaterialStorage - initialTotal;
            int seedsToAdd = System.Math.Min(100, availableSpace);

            // Успешное добавление семян
            bool addedSeeds = complex.AddMaterial(AgriculturalComplex.AgriMaterial.Seeds, seedsToAdd);
            Assert.IsTrue(addedSeeds, "Добавление семян должно быть успешным");

            var materialsAfter = complex.GetMaterialStorage();
            Assert.AreEqual(initialSeeds + seedsToAdd, materialsAfter[AgriculturalComplex.AgriMaterial.Seeds]);
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddMaterialsExceedingCapacity()
        {
            var complex = new AgriculturalComplex();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = complex.AddMaterial(AgriculturalComplex.AgriMaterial.Seeds, 1500);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 600 + 1500 > 2000

            var materials = complex.GetMaterialStorage();
            Assert.AreEqual(600, materials[AgriculturalComplex.AgriMaterial.Seeds]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutWorkers()
        {
            var complex = new AgriculturalComplex();

            var initialMaterials = complex.GetMaterialStorage();
            var initialProducts = complex.GetProductionOutput();

            // Запуск производства без рабочих
            complex.ProcessWorkshops();

            var finalMaterials = complex.GetMaterialStorage();
            var finalProducts = complex.GetProductionOutput();

            // Материалы и продукция не должны измениться
            Assert.AreEqual(initialMaterials[AgriculturalComplex.AgriMaterial.Seeds],
                          finalMaterials[AgriculturalComplex.AgriMaterial.Seeds]);
            Assert.AreEqual(initialMaterials[AgriculturalComplex.AgriMaterial.Fertilizer],
                          finalMaterials[AgriculturalComplex.AgriMaterial.Fertilizer]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15); // Максимальная эффективность

            var initialMaterials = complex.GetMaterialStorage();
            var initialSeeds = initialMaterials[AgriculturalComplex.AgriMaterial.Seeds];
            var initialFertilizer = initialMaterials[AgriculturalComplex.AgriMaterial.Fertilizer];
            var initialWater = initialMaterials[AgriculturalComplex.AgriMaterial.Water];

            // Запуск производства
            complex.ProcessWorkshops();

            var finalMaterials = complex.GetMaterialStorage();
            var finalProducts = complex.GetProductionOutput();

            // Материалы должны быть израсходованы
            Assert.IsTrue(finalMaterials[AgriculturalComplex.AgriMaterial.Seeds] < initialSeeds);
            Assert.IsTrue(finalMaterials[AgriculturalComplex.AgriMaterial.Fertilizer] < initialFertilizer);
            Assert.IsTrue(finalMaterials[AgriculturalComplex.AgriMaterial.Water] < initialWater);

            // Должна быть произведена продукция
            Assert.IsTrue(finalProducts.Count > 0);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест эффективности производства
        /// </summary>
        [TestMethod]
        public void TestProductionEfficiency()
        {
            var complex = new AgriculturalComplex();

            // Проверка эффективности при разном количестве рабочих
            complex.SetWorkersCount(0);
            Assert.AreEqual(0f, complex.ProductionEfficiency);

            complex.SetWorkersCount(8);
            Assert.AreEqual(System.Math.Round(0.673f, 3), System.Math.Round(complex.ProductionEfficiency, 3)); // 0.3 + (8/15)*0.7 ≈ 0.673

            complex.SetWorkersCount(15);
            Assert.AreEqual(1.0f, complex.ProductionEfficiency); // 0.3 + (15/15)*0.7 = 1.0
        }

        /// <summary>
        /// Тест потребления продукции
        /// </summary>
        [TestMethod]
        public void TestProductConsumption()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15);
            complex.ProcessWorkshops(); // Производим продукцию

            var initialProducts = complex.GetProductionOutput();

            if (initialProducts.Count > 0)
            {
                var productType = initialProducts.Keys.First();
                var initialAmount = initialProducts[productType];

                // Успешное потребление
                bool consumed = complex.ConsumeProduct(productType, 1);
                Assert.IsTrue(consumed);

                var finalProducts = complex.GetProductionOutput();
                Assert.AreEqual(initialAmount - 1, finalProducts[productType]);
            }
        }

        /// <summary>
        /// Тест потребления недостаточного количества продукции
        /// </summary>
        [TestMethod]
        public void TestInsufficientProductConsumption()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15);
            complex.ProcessWorkshops();

            var products = complex.GetProductionOutput();

            if (products.Count > 0)
            {
                var productType = products.Keys.First();

                // Попытка потребить больше, чем есть
                bool notConsumed = complex.ConsumeProduct(productType, 1000);
                Assert.IsFalse(notConsumed);
            }
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(10);

            var info = complex.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(10, info["WorkersCount"]);
            Assert.AreEqual(15, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.AreEqual(2000, info["MaxMaterialStorage"]);
            Assert.AreEqual(1000, info["MaxProductStorage"]);
            Assert.AreEqual(6, info["ActiveWorkshops"]);
            Assert.IsNotNull(info["SeasonalBonus"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15);

            var initialProducts = complex.GetProductionOutput().Values.Sum();

            complex.FullProductionCycle();

            var finalProducts = complex.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест размещения здания
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15);

            var initialProducts = complex.GetProductionOutput().Values.Sum();

            complex.OnBuildingPlaced();

            var finalProducts = complex.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест ограничения вместимости хранилища материалов
        /// </summary>
        [TestMethod]
        public void TestMaterialStorageCapacity()
        {
            var complex = new AgriculturalComplex();

            // Вычисляем доступное место
            int availableSpace = complex.MaxMaterialStorage - complex.GetTotalMaterialStorage();

            // Добавляем материалы до полного заполнения
            bool added = complex.AddMaterial(AgriculturalComplex.AgriMaterial.Seeds, availableSpace);
            Assert.IsTrue(added);
            Assert.AreEqual(complex.MaxMaterialStorage, complex.GetTotalMaterialStorage());

            // Попытка добавить еще должно вернуть false
            bool notAdded = complex.AddMaterial(AgriculturalComplex.AgriMaterial.Fertilizer, 1);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(complex.MaxMaterialStorage, complex.GetTotalMaterialStorage());
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var complex = new AgriculturalComplex();

            Assert.AreEqual(6, complex.Workshops.Count);

            // Проверка цеха растениеводства
            var cropWorkshop = complex.Workshops[0];
            Assert.AreEqual("Цех растениеводства", cropWorkshop.Name);
            Assert.AreEqual(6, cropWorkshop.ProductionCycleTime);
            Assert.AreEqual(3, cropWorkshop.InputRequirements.Count);
            Assert.AreEqual(1, cropWorkshop.OutputProducts.Count);

            // Проверка цеха овощеводства
            var vegetableWorkshop = complex.Workshops[1];
            Assert.AreEqual("Цех овощеводства", vegetableWorkshop.Name);
            Assert.AreEqual(5, vegetableWorkshop.ProductionCycleTime);

            // Проверка цеха садоводства
            var orchardWorkshop = complex.Workshops[2];
            Assert.AreEqual("Цех садоводства", orchardWorkshop.Name);
            Assert.AreEqual(8, orchardWorkshop.ProductionCycleTime);

            // Проверка молочного цеха
            var dairyWorkshop = complex.Workshops[3];
            Assert.AreEqual("Молочный цех", dairyWorkshop.Name);
            Assert.AreEqual(7, dairyWorkshop.ProductionCycleTime);

            // Проверка цеха переработки мяса
            var meatWorkshop = complex.Workshops[4];
            Assert.AreEqual("Цех переработки мяса", meatWorkshop.Name);
            Assert.AreEqual(10, meatWorkshop.ProductionCycleTime);

            // Проверка цеха пищевой переработки
            var processingWorkshop = complex.Workshops[5];
            Assert.AreEqual("Цех пищевой переработки", processingWorkshop.Name);
            Assert.AreEqual(9, processingWorkshop.ProductionCycleTime);
        }

        /// <summary>
        /// Тест получения общего количества материалов
        /// </summary>
        [TestMethod]
        public void TestGetTotalMaterialStorage()
        {
            var complex = new AgriculturalComplex();

            // Добавляем дополнительные материалы
            complex.AddMaterial(AgriculturalComplex.AgriMaterial.Seeds, 100);
            complex.AddMaterial(AgriculturalComplex.AgriMaterial.Fertilizer, 50);
            complex.AddMaterial(AgriculturalComplex.AgriMaterial.Water, 30);

            int total = complex.GetTotalMaterialStorage();

            // 600 + 400 + 800 + 300 + 100 + 50 + 30 = 2280, но ограничено MaxMaterialStorage = 2000
            Assert.AreEqual(2000, total);
        }

        /// <summary>
        /// Тест получения общего количества продукции
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15);
            complex.ProcessWorkshops();

            int total = complex.GetTotalProductStorage();

            Assert.IsTrue(total >= 0);
            Assert.IsTrue(total <= complex.MaxProductStorage);
        }

        /// <summary>
        /// Тест конфигурации сырья и продукции
        /// </summary>
        [TestMethod]
        public void TestMaterialAndProductConfiguration()
        {
            // Проверка enum сырья
            Assert.AreEqual(0, (int)AgriculturalComplex.AgriMaterial.Seeds);
            Assert.AreEqual(1, (int)AgriculturalComplex.AgriMaterial.Fertilizer);
            Assert.AreEqual(2, (int)AgriculturalComplex.AgriMaterial.Water);
            Assert.AreEqual(3, (int)AgriculturalComplex.AgriMaterial.AnimalFeed);

            // Проверка enum продукции
            Assert.AreEqual(0, (int)AgriculturalComplex.AgriProduct.Wheat);
            Assert.AreEqual(1, (int)AgriculturalComplex.AgriProduct.Vegetables);
            Assert.AreEqual(2, (int)AgriculturalComplex.AgriProduct.Fruits);
            Assert.AreEqual(3, (int)AgriculturalComplex.AgriProduct.Milk);
            Assert.AreEqual(4, (int)AgriculturalComplex.AgriProduct.Eggs);
            Assert.AreEqual(5, (int)AgriculturalComplex.AgriProduct.Meat);
            Assert.AreEqual(6, (int)AgriculturalComplex.AgriProduct.ProcessedFood);
        }

        /// <summary>
        /// Тест производства переработанной пищи
        /// </summary>
        [TestMethod]
        public void TestProcessedFoodProduction()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                complex.ProcessWorkshops();
            }

            var products = complex.GetProductionOutput();

            // Должна производиться переработанная еда
            Assert.IsTrue(products.ContainsKey(AgriculturalComplex.AgriProduct.ProcessedFood));
        }

        /// <summary>
        /// Тест многоступенчатого производства (сырье -> переработанная еда)
        /// </summary>
        [TestMethod]
        public void TestMultiStageProduction()
        {
            var complex = new AgriculturalComplex();
            complex.SetWorkersCount(15);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                complex.ProcessWorkshops();
            }

            var products = complex.GetProductionOutput();

            // Если производится пшеница и молоко, то должна производиться переработанная еда
            if (products.ContainsKey(AgriculturalComplex.AgriProduct.Wheat) &&
                products.ContainsKey(AgriculturalComplex.AgriProduct.Milk))
            {
                Assert.IsTrue(products.ContainsKey(AgriculturalComplex.AgriProduct.ProcessedFood));
            }
        }

        /// <summary>
        /// Тест сезонного бонуса
        /// </summary>
        [TestMethod]
        public void TestSeasonalBonus()
        {
            var complex = new AgriculturalComplex();

            var info = complex.GetProductionInfo();
            var seasonalBonus = (float)info["SeasonalBonus"];

            // Бонус должен быть в допустимом диапазоне
            Assert.IsTrue(seasonalBonus >= 0.8f && seasonalBonus <= 1.2f);
        }
    }
}
