﻿namespace UnitTests.Soils
{
    using Models.Core;
    using Models.Soils;
    using Models.Soils.Nutrients;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class StandardiserTests
    {
        /// <summary>Ensure layer mapping works.</summary>
        [Test]
        public void LayerMappingWorks()
        {
            var soil = new Soil
            {
                Children = new List<IModel>()
                {
                    new Physical()
                    {
                        Thickness = new double[] { 100, 300, 300 },
                        BD = new double[] { 1.36, 1.216, 1.24 },
                        AirDry = new double[] { 0.135, 0.214, 0.261 },
                        LL15 = new double[] { 0.27, 0.267, 0.261 },
                        DUL = new double[] { 0.365, 0.461, 0.43 },
                        SAT = new double[] { 0.400, 0.481, 0.45 },

                        Children = new List<IModel>()
                        {
                            new SoilCrop
                            {
                                Name = "Wheat",
                                KL = new double[] { 0.06, 0.060, 0.060 },
                                LL = new double[] { 0.27, 0.267, 0.261 }
                            }
                        }
                    },
                    new Models.WaterModel.WaterBalance(),
                    new CERESSoilTemperature(),
                    new Organic
                    {
                        Thickness = new double[] { 100, 300 },
                        Carbon = new double[] { 2, 1 }
                    },
                    new Solute
                    {
                        Name = "NO3",
                        Thickness = new double[] { 100, 200 },
                        InitialValues = new double[] { 27, 10 },
                        InitialValuesUnits = Solute.UnitsEnum.kgha
                    },
                    new Solute
                    {
                        Name = "CL",
                        Thickness = new double[] { 100, 200 },
                        InitialValues = new double[] { 38, double.NaN },
                        InitialValuesUnits = Solute.UnitsEnum.ppm
                    },
                    new Water
                    {
                        Thickness = new double[] { 500 },
                        InitialValues = new double[] { 0.103 },
                    }
                }
            };
            Utilities.InitialiseModel(soil);

            soil.Standardise();

            var physical = soil.FindChild<Physical>();
            var soilOrganicMatter = soil.FindChild<Organic>();
            var water = soil.FindChild<Water>();

            // Make sure layer structures have been standardised.
            var targetThickness = new double[] { 100, 300, 300 };
            Assert.AreEqual(physical.Thickness, targetThickness);
            Assert.AreEqual(soilOrganicMatter.Thickness, targetThickness);
            Assert.AreEqual(water.Thickness, targetThickness);
        }

        /// <summary>Ensure a LayerStructure is used for mapping.</summary>
        [Test]
        public void LayerStructureIsUsedForMapping()
        {
            var soil = new Soil
            {
                Children = new List<IModel>()
                {
                    new Physical()
                    {
                        Thickness = new double[] { 100, 300, 300 },
                        BD = new double[] { 1.36, 1.216, 1.24 },
                        AirDry = new double[] { 0.135, 0.214, 0.261 },
                        LL15 = new double[] { 0.27, 0.267, 0.261 },
                        DUL = new double[] { 0.365, 0.461, 0.43 },
                        SAT = new double[] { 0.400, 0.481, 0.45 },

                        Children = new List<IModel>()
                        {
                            new SoilCrop
                            {
                                Name = "WheatSoil",
                                KL = new double[] { 0.06, 0.060, 0.060 },
                                LL = new double[] { 0.27, 0.267, 0.261 }
                            }
                        }
                    },
                    new Models.WaterModel.WaterBalance(),
                    new CERESSoilTemperature(),
                    new Organic
                    {
                        Thickness = new double[] { 100, 300 },
                        Carbon = new double[] { 2, 1 }
                    },
                    new Solute
                    {
                        Name = "NO3",
                        Thickness = new double[] { 100, 200 },
                        InitialValues = new double[] { 27, 6 },
                        InitialValuesUnits = Solute.UnitsEnum.kgha
                    },
                    new Solute
                    {
                        Name = "CL",
                        Thickness = new double[] { 100, 200 },
                        InitialValues = new double[] { 38, double.NaN },
                        InitialValuesUnits = Solute.UnitsEnum.ppm
                    },
                    new Water
                    {
                        Thickness = new double[] { 500 },
                        InitialValues = new double[] { 0.103 }
                    },
                    new LayerStructure
                    {
                        Thickness = new double[] { 100, 300 }
                    }
                }
            };
            Utilities.InitialiseModel(soil);

            soil.Standardise();

            var physical = soil.FindChild<Physical>();
            var soilOrganicMatter = soil.FindChild<Organic>();
            var water = soil.FindChild<Water>();

            // Make sure layer structures have been standardised.
            var targetThickness = new double[] { 100, 300 };
            Assert.AreEqual(physical.Thickness, targetThickness);
            Assert.AreEqual(soilOrganicMatter.Thickness, targetThickness);
            Assert.AreEqual(water.Thickness, targetThickness);
        }

        /// <summary>Ensure a single initial conditions sample is created.</summary>
        [Test]
        public void InitialConditionsIsCreated()
        {
            Soil soil = CreateSimpleSoil();
            Utilities.InitialiseModel(soil);

            soil.Standardise();

            var chemical = soil.FindChild<Chemical>();
            var organic = soil.FindChild<Organic>();
            var water = soil.FindChild<Water>();
            var solutes = soil.FindAllChildren<Solute>().ToArray();

            Assert.AreEqual(soil.FindAllChildren<Water>().Count(), 1);
            Assert.AreEqual(water.Name, "Water");
            Assert.AreEqual(water.Volumetric, new double[] { 0.1, 0.2 } );
            Assert.AreEqual(organic.Carbon, new double[] { 2.0, 0.9 });
            Assert.AreEqual(chemical.PH, new double[] { 6.65, 7.0 });
            Assert.AreEqual(chemical.EC, new double[] { 150, 200 });

            Assert.AreEqual(solutes[0].InitialValues, new double[] { 21.5, 1.0 });  // NO3 kg/ha
            Assert.AreEqual(solutes[1].InitialValues, new double[] { 1.1, 0.2 }); // NH4 kg/ha
        }

        [Test]
        public void DontStandardiseDisabledSoils()
        {
            Soil soil = CreateSimpleSoil();
            Utilities.InitialiseModel(soil);
            Physical phys = soil.FindChild<Physical>();

            // Remove a layer from BD - this will cause standardisation to fail.
            phys.BD = new double[phys.BD.Length - 1];

            // Now disable the soil so it doesn't get standardised.
            soil.Enabled = false;

            // Chuck the soil in a simulation.
            Simulations sims = Utilities.GetRunnableSim();
            Zone paddock = sims.FindDescendant<Zone>();
            paddock.Children.Add(soil);
            soil.Parent = paddock;

            // Run the simulation - this shouldn't fail, because the soil is disabled.
            var runner = new Models.Core.Run.Runner(sims);
            List<Exception> errors = runner.Run();
            Assert.AreEqual(0, errors.Count, "There should be no errors - the faulty soil is disabled");
        }

        private Soil CreateSimpleSoil()
        {
            return new Soil
            {
                Children = new List<IModel>()
                {
                    new Physical()
                    {
                        Thickness = new double[] { 100, 200 },
                        BD = new double[] { 1.36, 1.216 },
                        AirDry = new double[] { 0.135, 0.214 },
                        LL15 = new double[] { 0.27, 0.267 },
                        DUL = new double[] { 0.365, 0.461 },
                        SAT = new double[] { 0.400, 0.481 },
                    },
                    new Models.WaterModel.WaterBalance(),
                    new CERESSoilTemperature(),
                    new Organic
                    {
                        Thickness = new double[] { 100, 200 },
                        Carbon = new double[] { 2, 0.9 },
                        FBiom = new double[] { 1, 2 }
                    },
                    new Chemical
                    {
                        Thickness = new double[] { 50, 50 },
                        PH = new double[] { 6.4, 6.9 },
                        EC = new double[] { 100, 200 }
                    },
                    new Solute
                    {
                        Name = "NO3",
                        Thickness = new double[] { 50, 50 },
                        InitialValues = new double[] { 27, 16 },
                        InitialValuesUnits = Solute.UnitsEnum.ppm
                    },
                    new Solute
                    {
                        Name = "NH4",
                        Thickness = new double[] { 50, 50 },
                        InitialValues = new double[] { 2, double.NaN },
                        InitialValuesUnits = Solute.UnitsEnum.ppm
                    },
                    new Water
                    {
                        Thickness = new double[] { 100, 200 },
                        InitialValues = new double[] { 0.1, 0.2 },
                    }
                }
            };
        }
    }
}
