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

        public OptimizationResult OptimizeComposition(Composition composition)
        {
            int Wattage = 0;
            int Price = 0;
                 


            for (int i = 0; i < composition.Sections.Count; i++)
            {

                Section section = composition.Sections[i];
                

                for(int j = 0; j < section.Shelves.Count; j++)
                {
                    Shelf shelf = section.Shelves[j];
                    Shelf previousShelf = composition.ShelfAt(i - 1, j);

                    int lastSectionIndex = composition.Sections.Count - 1;

                    bool hasLight = shelf.HasLight;

                    //power left only

                    if (i == 0) // First section
                    {
                        shelf.Type = hasLight ? ShelfType.G1 : ShelfType.E;
                    }
                    else if (i == lastSectionIndex) // Last section
                    {
                        shelf.Type = hasLight ? ShelfType.H : ShelfType.E;
                        if (previousShelf.Type == ShelfType.E) previousShelf.Type = ShelfType.EI;
                    }
                    else if(i != 0) // Middle section
                    {
                        if(hasLight)
                        {
                            shelf.Type = ShelfType.H;
                            if(previousShelf.HasLight && i - 1 != 0)
                            {
                                previousShelf.Type = ShelfType.H;
                            }
                            else if(previousShelf.HasLight && i - 1 == 0)
                            {
                                previousShelf.Type = ShelfType.G1;
                            }
                            else if (!previousShelf.HasLight && i - 1 == 0)
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
            
            //TODO: Create logic that calculates the optimal lighting composition with the cheapest price

            //TODO: Update the composition model objects with OuterWallTypes and ShelfTypes

            return new(composition);
        }
    }
}