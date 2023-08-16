using LightResolver.Logic.Input;
using LightResolver.Logic.Models;

namespace LightResolver.Logic
{
    public class LightResolverModule : ILightResolverModule
    {
        public IInputProvider InputProvider { get; }

        public LightResolverModule(IInputProvider inputProvider)
        {
            InputProvider = inputProvider;
        }

        /// <summary>
        /// Calculates optimal lighting components arrangement in given composition. Always tries to resolve cheapest arrangement of components.
        /// Building rules for components are described in wiki.
        /// https://github.com/vividdev/light-resolver-dev/wiki/Assignment#electrified-components
        /// </summary>
        /// <param name="composition"></param>
        /// <returns></returns>
        /// 

        // Set types for shelves
        private void ShelfTypeAssigner(Composition composition, double wattage)
        {
            int sectionsCount = composition.Sections.Count;

            for (int i = 0; i < sectionsCount; i++)
            {

                Section section = composition.Sections[i];


                for (int j = 0; j < section.Shelves.Count; j++)
                {
                    Shelf shelf = section.Shelves[j];
                    Shelf previousShelf = composition.ShelfAt(i - 1, j);

                    int lastSectionIndex = sectionsCount - 1;
                    int previousSectionIndex = i - 1; 

                    bool hasLight = shelf.HasLight;


                    if (i == 0) // First section
                    {
                        shelf.Type = hasLight ? ShelfType.G1 : ShelfType.E;
                    }
                    else if (i == lastSectionIndex) // Last section
                    {
                        if(wattage > 100) shelf.Type = hasLight ? ShelfType.G2 : ShelfType.E;
                        else shelf.Type = hasLight ? ShelfType.H : ShelfType.E;

                        if (previousShelf.Type == ShelfType.E) previousShelf.Type = ShelfType.EI;
                    }
                    else if (i != 0) // Middle section
                    {
                        if (hasLight)
                        {
                            shelf.Type = ShelfType.H;
                            if (previousShelf.HasLight && previousSectionIndex != 0)
                            {
                                previousShelf.Type = ShelfType.H;
                            }
                            else if (!previousShelf.HasLight && previousSectionIndex != 0) 
                            {
                                previousShelf.Type = ShelfType.EI;
                            }
                            else if (previousShelf.HasLight && previousSectionIndex == 0)
                            {
                                previousShelf.Type = ShelfType.G1;
                            }
                            else if (!previousShelf.HasLight && previousSectionIndex == 0)
                            {
                                previousShelf.Type = ShelfType.F1;
                            }
                        }
                        else
                        {
                            shelf.Type = ShelfType.E;
                        }
                    }

                }

            }
        }

        public OptimizationResult OptimizeComposition(Composition composition)
        {

            int sectionsCount = composition.Sections.Count;
            double wattage = 0;
            decimal shelfPrice = 0;

            ShelfTypeAssigner(composition, wattage);

            // Set price for shelfs and wattage

            for (int i = 0; i < sectionsCount; i++)
            {
                Section section = composition.Sections[i];

                for (int j = 0; j < section.Shelves.Count; j++)
                {
                    Shelf shelf = section.Shelves[j];
                    shelf.Width = section.Width;

                    shelfPrice += shelf.Price;
                    wattage += shelf.Wattage;

                }
            }

            // Set price for walls

            decimal wallPrice = 0;

            OuterWall leftOuterWall = composition.LeftOuterWall;
            OuterWall rightOuterWall = composition.RightOuterWall;


            if(wattage == 0.0)
            {
                leftOuterWall.Type = WallType.NotElectrified;
                leftOuterWall.WattageConsumption = 0.0;

                rightOuterWall.Type = WallType.NotElectrified;
                rightOuterWall.WattageConsumption = 0.0;

                wallPrice += leftOuterWall.Price + rightOuterWall.Price;
            }

            else if (wattage > 0.0 && wattage < 100)
            {
                leftOuterWall.Type = WallType.A1;
                leftOuterWall.WattageConsumption = wattage;

                rightOuterWall.Type = WallType.NotElectrified;
                rightOuterWall.WattageConsumption = 0.0;

                wallPrice += leftOuterWall.Price + rightOuterWall.Price;
            }

            else if (wattage > 100 && wattage < 200)
            {
                leftOuterWall.Type = WallType.A1;
                leftOuterWall.WattageConsumption = wattage;

                rightOuterWall.Type = WallType.A2;
                rightOuterWall.WattageConsumption = 0.0;

                wallPrice += leftOuterWall.Price + rightOuterWall.Price;
                ShelfTypeAssigner(composition, wattage);
            }
            
            else
            {
                // too much power used
            }

            decimal totalPrice = shelfPrice + wallPrice;


            //TODO: Create logic that calculates the optimal lighting composition with the cheapest price

            //TODO: Update the composition model objects with OuterWallTypes and ShelfTypes

            return new(composition);
        }
    }
}