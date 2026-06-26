# Buffet Patron Simulator

Play: https://yhlu.itch.io/buffet-patron-simulator

A Unity game where you are at a buffet and can take food.

Explore options and build out a plate.

## Roadmap

Subject to change

- [x] Demo features
    - [x] Plate
    - [x] Rice
    - [x] Steak
    - [x] Mesh deformation of food when placed
    - [x] Camera control when placing food
    - [x] Menu scene transitions into placement scene (and back)

- [x] MVP features
    - [x] Movable character
    - [x] Plate exchange
    - [x] ~10 foods with different structural properties

- [ ] Extra features
    - [ ] Decoration
        - [ ] Add a table or something to the placement scene
    - [x] Drinks & liquids
    - [ ] Calorie counter
    - [ ] "Eating"

## Credits

### Software

- Unity 6
- Visual Studio Code
- ~~MagicaVoxel~~
- Blender

### Third Party Assets

- Skybox: [Joburg Central Sunset](https://polyhaven.com/a/sunset_jhbcentral) from Polyhaven under [CC0](https://creativecommons.org/publicdomain/zero/1.0/)
- Plate exchange sound effect: ["Plates stack on top of each other, clatter, version 3"](https://www.zapsplat.com/music/plates-stack-on-top-of-each-other-clatter-version-3) from Zapsplat under the [standard license for free users](https://www.zapsplat.com/license-type/standard-license/)
- Glass exchange sound effect: ["Empty drinking glasses, beer jugs clink together, cheers, full sides touch, medium"](https://www.zapsplat.com/music/empty-drinking-glasses-beer-jugs-clink-together-cheers-full-sides-touch-medium/) from Zapsplat under the [standard license for free users](https://www.zapsplat.com/license-type/standard-license/)
- Soda fountain dispensing sound effect: ["FillUpDrinkInCafeteria.wav"](https://freesound.org/people/ViaTorci/sounds/66791/) from Freesound under [CC0](https://creativecommons.org/publicdomain/zero/1.0/)
- Food placing sound effect: ["Toast place down on surface"](https://www.zapsplat.com/music/toast-place-down-on-surface/) from Zapsplat under the [standard license for free users](https://www.zapsplat.com/license-type/standard-license/)
- Comic strip poster: https://www.xkcd.com/1795; use permitted by https://xkcd.com/about/

## AI Disclosure

- I direct the game
- I make art assets (aside from those credited above)
- LLMs used for technical questions
- There may be AI generated or architected code - I aim to understand, adapt, and tweak such code

## Self Notes

- ~~For the purposes of this project, 1 voxel = 1x1x1 cm. Therefore, my models need to be scaled by 0.1 in Unity.~~
- Models need many evenly distributed vertices for deformations to look good.
- Voxel model textures don't work well with Blender remesh. I'll see if I can paint the textures directly in Blender instead.

### How to Add Food Items

1. Make the model and texture. 
    - Name each element (e.g., material) descriptively so they will be easier to find in Unity.
    - Ensure it has enough vertices for mesh deformation to work.
    - The pivot of the model should be at its bottom
    - Export settings: selected objects, apply transform
3. After importing to Unity, enable Read/Write on the model
4. Create a prefab with a MeshFilter, MeshCollider, and the FoodDeformer script
5. Add to Assets/Resources/FoodItems/
6. Done.
