using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для рыбодобывающего комбината
    /// </summary>
    [TestClass]
    public sealed class FisheryComplexTests
    {
        /// <summary>
        /// Тест создания рыбкомбината
        /// </summary>
        [TestMethod]
        public void TestFisheryComplexCreation()
        {
            var complex = new FisheryComplex();

            // Проверка статических свойств строительства
            Assert.AreEqual(350000m, FisheryComplex.BuildCost);
            Assert.AreEqual(4, FisheryComplex.RequiredMaterials.Count);
            Assert.AreEqual(12, FisheryComplex.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(8, FisheryComplex.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(3, FisheryComplex.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(8, FisheryComplex.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(1500, complex.MaxMaterialStorage);
            Assert.AreEqual(800, complex.MaxProductStorage);
            Assert.AreEqual(12, complex.MaxWorkers);
            Assert.AreEqual(0, complex.WorkersCount);
            Assert.AreEqual(5, complex.Workshops.Count);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var complex = new FisheryComplex();

            var materials = complex.GetMaterialStorage();

            Assert.AreEqual(500, materials[FisheryComplex.FisheryMaterial.Fuel]);
            Assert.AreEqual(300, materials[FisheryComplex.FisheryMaterial.FishingGear]);
            Assert.AreEqual(400, materials[FisheryComplex.FisheryMaterial.Ice]);
            Assert.AreEqual(200, materials[FisheryComplex.FisheryMaterial.Salt]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var complex = new FisheryComplex();

            // Установка количества рабочих
            complex.SetWorkersCount(6);
            Assert.AreEqual(6, complex.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            complex.SetWorkersCount(15);
            Assert.AreEqual(12, complex.WorkersCount); // Должно ограничиться MaxWorkers

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
            var complex = new FisheryComplex();

            // Получаем начальное состояние
            var initialMaterials = complex.GetMaterialStorage();
            int initialFuel = initialMaterials[FisheryComplex.FisheryMaterial.Fuel];
            int initialTotal = complex.GetTotalMaterialStorage();

            // Вычисляем сколько можно добавить без превышения лимита
            int availableSpace = complex.MaxMaterialStorage - initialTotal;
            int fuelToAdd = System.Math.Min(100, availableSpace);

            // Успешное добавление топлива
            bool addedFuel = complex.AddMaterial(FisheryComplex.FisheryMaterial.Fuel, fuelToAdd);
            Assert.IsTrue(addedFuel, "Добавление топлива должно быть успешным");

            var materialsAfter = complex.GetMaterialStorage();
            Assert.AreEqual(initialFuel + fuelToAdd, materialsAfter[FisheryComplex.FisheryMaterial.Fuel]);
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddMaterialsExceedingCapacity()
        {
            var complex = new FisheryComplex();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = complex.AddMaterial(FisheryComplex.FisheryMaterial.Fuel, 1200);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 500 + 1200 > 1500

            var materials = complex.GetMaterialStorage();
            Assert.AreEqual(500, materials[FisheryComplex.FisheryMaterial.Fuel]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutWorkers()
        {
            var complex = new FisheryComplex();

            var initialMaterials = complex.GetMaterialStorage();
            var initialProducts = complex.GetProductionOutput();

            // Запуск производства без рабочих
            complex.ProcessWorkshops();

            var finalMaterials = complex.GetMaterialStorage();
            var finalProducts = complex.GetProductionOutput();

            // Материалы и продукция не должны измениться
            Assert.AreEqual(initialMaterials[FisheryComplex.FisheryMaterial.Fuel],
                          finalMaterials[FisheryComplex.FisheryMaterial.Fuel]);
            Assert.AreEqual(initialMaterials[FisheryComplex.FisheryMaterial.FishingGear],
                          finalMaterials[FisheryComplex.FisheryMaterial.FishingGear]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12); // Максимальная эффективность

            var initialMaterials = complex.GetMaterialStorage();
            var initialFuel = initialMaterials[FisheryComplex.FisheryMaterial.Fuel];
            var initialFishingGear = initialMaterials[FisheryComplex.FisheryMaterial.FishingGear];
            var initialIce = initialMaterials[FisheryComplex.FisheryMaterial.Ice];

            // Запуск производства
            complex.ProcessWorkshops();

            var finalMaterials = complex.GetMaterialStorage();
            var finalProducts = complex.GetProductionOutput();

            // Материалы должны быть израсходованы
            Assert.IsTrue(finalMaterials[FisheryComplex.FisheryMaterial.Fuel] < initialFuel);
            Assert.IsTrue(finalMaterials[FisheryComplex.FisheryMaterial.FishingGear] < initialFishingGear);
            Assert.IsTrue(finalMaterials[FisheryComplex.FisheryMaterial.Ice] < initialIce);

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
            var complex = new FisheryComplex();

            // Проверка эффективности при разном количестве рабочих
            complex.SetWorkersCount(0);
            Assert.AreEqual(0f, complex.ProductionEfficiency);

            complex.SetWorkersCount(6);
            Assert.AreEqual(System.Math.Round(0.7f, 3), System.Math.Round(complex.ProductionEfficiency, 3)); // 0.4 + (6/12)*0.6 = 0.7

            complex.SetWorkersCount(12);
            Assert.AreEqual(1.0f, complex.ProductionEfficiency); // 0.4 + (12/12)*0.6 = 1.0
        }

        /// <summary>
        /// Тест потребления продукции
        /// </summary>
        [TestMethod]
        public void TestProductConsumption()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);
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
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);
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
            var complex = new FisheryComplex();
            complex.SetWorkersCount(8);

            var info = complex.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(8, info["WorkersCount"]);
            Assert.AreEqual(12, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.AreEqual(1500, info["MaxMaterialStorage"]);
            Assert.AreEqual(800, info["MaxProductStorage"]);
            Assert.AreEqual(5, info["ActiveWorkshops"]);
            Assert.IsNotNull(info["FleetEfficiency"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);

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
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);

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
            var complex = new FisheryComplex();

            // Вычисляем доступное место
            int availableSpace = complex.MaxMaterialStorage - complex.GetTotalMaterialStorage();

            // Добавляем материалы до полного заполнения
            bool added = complex.AddMaterial(FisheryComplex.FisheryMaterial.Fuel, availableSpace);
            Assert.IsTrue(added);
            Assert.AreEqual(complex.MaxMaterialStorage, complex.GetTotalMaterialStorage());

            // Попытка добавить еще должно вернуть false
            bool notAdded = complex.AddMaterial(FisheryComplex.FisheryMaterial.FishingGear, 1);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(complex.MaxMaterialStorage, complex.GetTotalMaterialStorage());
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var complex = new FisheryComplex();

            Assert.AreEqual(5, complex.Workshops.Count);

            // Проверка цеха добычи рыбы
            var fishingWorkshop = complex.Workshops[0];
            Assert.AreEqual("Цех добычи рыбы", fishingWorkshop.Name);
            Assert.AreEqual(8, fishingWorkshop.ProductionCycleTime);
            Assert.AreEqual(2, fishingWorkshop.InputRequirements.Count);
            Assert.AreEqual(1, fishingWorkshop.OutputProducts.Count);

            // Проверка цеха заморозки рыбы
            var freezingWorkshop = complex.Workshops[1];
            Assert.AreEqual("Цех заморозки рыбы", freezingWorkshop.Name);
            Assert.AreEqual(6, freezingWorkshop.ProductionCycleTime);

            // Проверка консервного цеха
            var canningWorkshop = complex.Workshops[2];
            Assert.AreEqual("Консервный цех", canningWorkshop.Name);
            Assert.AreEqual(10, canningWorkshop.ProductionCycleTime);

            // Проверка цеха разделки рыбы
            var processingWorkshop = complex.Workshops[3];
            Assert.AreEqual("Цех разделки рыбы", processingWorkshop.Name);
            Assert.AreEqual(7, processingWorkshop.ProductionCycleTime);

            // Проверка цеха переработки отходов
            var byproductWorkshop = complex.Workshops[4];
            Assert.AreEqual("Цех переработки отходов", byproductWorkshop.Name);
            Assert.AreEqual(9, byproductWorkshop.ProductionCycleTime);
        }

        /// <summary>
        /// Тест получения общего количества материалов
        /// </summary>
        [TestMethod]
        public void TestGetTotalMaterialStorage()
        {
            var complex = new FisheryComplex();

            // Добавляем дополнительные материалы
            complex.AddMaterial(FisheryComplex.FisheryMaterial.Fuel, 100);
            complex.AddMaterial(FisheryComplex.FisheryMaterial.FishingGear, 50);
            complex.AddMaterial(FisheryComplex.FisheryMaterial.Ice, 30);

            int total = complex.GetTotalMaterialStorage();

            // 500 + 300 + 400 + 200 + 100 + 50 + 30 = 1580, но ограничено MaxMaterialStorage = 1500
            Assert.AreEqual(1500, total);
        }

        /// <summary>
        /// Тест получения общего количества продукции
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);
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
            Assert.AreEqual(0, (int)FisheryComplex.FisheryMaterial.Fuel);
            Assert.AreEqual(1, (int)FisheryComplex.FisheryMaterial.FishingGear);
            Assert.AreEqual(2, (int)FisheryComplex.FisheryMaterial.Ice);
            Assert.AreEqual(3, (int)FisheryComplex.FisheryMaterial.Salt);

            // Проверка enum продукции
            Assert.AreEqual(0, (int)FisheryComplex.FisheryProduct.FreshFish);
            Assert.AreEqual(1, (int)FisheryComplex.FisheryProduct.FrozenFish);
            Assert.AreEqual(2, (int)FisheryComplex.FisheryProduct.CannedFish);
            Assert.AreEqual(3, (int)FisheryComplex.FisheryProduct.FishFillets);
            Assert.AreEqual(4, (int)FisheryComplex.FisheryProduct.FishOil);
            Assert.AreEqual(5, (int)FisheryComplex.FisheryProduct.FishMeal);
            Assert.AreEqual(6, (int)FisheryComplex.FisheryProduct.Seafood);
        }

        /// <summary>
        /// Тест производства рыбьего жира и рыбной муки
        /// </summary>
        [TestMethod]
        public void TestByproductProduction()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                complex.ProcessWorkshops();
            }

            var products = complex.GetProductionOutput();

            // Должны производиться побочные продукты
            Assert.IsTrue(products.ContainsKey(FisheryComplex.FisheryProduct.FishOil) ||
                         products.ContainsKey(FisheryComplex.FisheryProduct.FishMeal));
        }

        /// <summary>
        /// Тест многоступенчатого производства (свежая рыба -> консервы/филе)
        /// </summary>
        [TestMethod]
        public void TestMultiStageProduction()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                complex.ProcessWorkshops();
            }

            var products = complex.GetProductionOutput();

            // Если производится свежая рыба, то должны производиться производные продукты
            if (products.ContainsKey(FisheryComplex.FisheryProduct.FreshFish))
            {
                Assert.IsTrue(products.ContainsKey(FisheryComplex.FisheryProduct.FrozenFish) ||
                             products.ContainsKey(FisheryComplex.FisheryProduct.CannedFish) ||
                             products.ContainsKey(FisheryComplex.FisheryProduct.FishFillets));
            }
        }

        /// <summary>
        /// Тест эффективности флота
        /// </summary>
        [TestMethod]
        public void TestFleetEfficiency()
        {
            var complex = new FisheryComplex();

            var info = complex.GetProductionInfo();
            var fleetEfficiency = (float)info["FleetEfficiency"];

            // Эффективность флота должна быть в допустимом диапазоне
            Assert.IsTrue(fleetEfficiency >= 0.7f && fleetEfficiency <= 1.0f);
        }

        /// <summary>
        /// Тест производства морепродуктов
        /// </summary>
        [TestMethod]
        public void TestSeafoodProduction()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                complex.ProcessWorkshops();
            }

            var products = complex.GetProductionOutput();

            // Должны производиться морепродукты
            Assert.IsTrue(products.ContainsKey(FisheryComplex.FisheryProduct.Seafood));
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);

            var productsAfterFirstCycle = complex.GetProductionOutput().Values.Sum();
            complex.ProcessWorkshops();
            var productsAfterSecondCycle = complex.GetProductionOutput().Values.Sum();

            // После второго цикла должно быть больше продукции
            Assert.IsTrue(productsAfterSecondCycle > productsAfterFirstCycle);
        }

        /// <summary>
        /// Тест производства всех видов рыбной продукции
        /// </summary>
        [TestMethod]
        public void TestAllFishProductsProduction()
        {
            var complex = new FisheryComplex();
            complex.SetWorkersCount(12);

            // Запускаем несколько циклов для производства всех видов продукции
            for (int i = 0; i < 5; i++)
            {
                complex.ProcessWorkshops();
            }

            var products = complex.GetProductionOutput();

            // Должны производиться различные виды рыбной продукции
            Assert.IsTrue(products.Count >= 3); // Как минимум 3 разных продукта
        }
    }
}
