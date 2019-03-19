How to run demo from video:

You need Unity 2018.2.12f1 or higher. 

Please do all step 1 by 1. It will save your time and confusion. We will clean everything more (prefabs mostly) but now scene works properly. This is it's first release.

1) Import Vegetation Studio PRO (In 1.0.1 + version or higher)
https://assetstore.unity.com/packages/tools/terrain/vegetation-studio-pro-131835?aid=1011lGkb

2) Setup project with VS proje like it's metioned here (Setting up  unity part only) : https://www.awesometech.no/index.php/setup-guide/

3) Setup project to linear color space in player settings

4) Setup project deferred rendering in Graphics Settings (not necessary but it improve speed) 

5) Import Post Process Stack 2.0 into your project from Window ->Package Manager -> Post Processing (click update or instal)

6) Import R.A.M into your project (water shading, road and river splines, ground textures)
https://assetstore.unity.com/packages/tools/terrain/r-a-m-river-auto-material-101205?aid=1011lGkb
IMPORTANT INFO: We used more advanced r.a.m versions in this project so you could expect some missings. 
We will push this new version online as fast as we can. So you will be able to modify river and lakes.

7) Import CTS - Coplete Terrain Shader to your project (terrain shader - not necessary but setup was made with CTS)
https://assetstore.unity.com/packages/tools/terrain/cts-complete-terrain-shader-91938?aid=1011lGkb
If you don't use cts after scene import change material at terrain object back to standard shader.

8) Import Screen Space Shadow pack into your project (not necessary but nice and cheap shadows on grass are welcome)
https://assetstore.unity.com/packages/vfx/shaders/directx-11/se-screen-space-shadows-77836?aid=1011lGkb

9) Import Meadow Environment Pack to your project. It will change most standard shaders which doesn't support VS PRO instanced indirect
into our shaders which support it.
https://assetstore.unity.com/packages/3d/vegetation/meadow-environment-dynamic-nature-132195?aid=1011lGkb

10) Find folder called Hills Demo Scene in NatureManufacture Assets folder.

11) Find file called "Hills Demo Meadow Environment & VS PRO" and import it. It will add materials and profiles and setup for all assets.

12) Open scene called "Meadow Biome"

13) Click Play - you could fly and check demo scene.

14) Low FPS - > for low end gpu turn off screen space reflections from post processing object (it's expensive)
Make note that current scene setup spawn all foliage at  RUNTIME to the camera. You could bake it this will improve
fps too. Check point "C"

15) Optional: Bake ambient light in window->rendering -> lighting settings 
(It will break reflections a bit probably but nothing special - check point "D" in more usefull options part).

More usefull options about visual effects and optimisation: 

A) You could turn on movie track by disable Extended Flycam script on camera and turn on Playable director and Animatior.

B) You could extend scene by drag and drop adding forest. Simply in biome profiles you will find "Small Forest" and "Small Forest Line" prefabs. 
Drag and drop them and modify shape to get new forest objects. At the end you have to bake texture splatmap. 

C) You could expand foliage rendering distance in VegetationSystemPro objects in Vegetation Settings. (we use 400 for grass and additional 300 for trees at video)
If foliage distance will be high you probably should bake vegetation to spawn it smoothly. 
In presisten Vegetation Storage part simply click Bake All vegetation items from all vegetation packages/biomes. Before that check what foliage we 
spawn at runtime (not all objects are enabled) because we use few of them only for specific places and we paint it by hand. 
So remove them from storage after bake -> storage vegetation and clear instances for : 
- prefab_flower_bouncing_bet_01_detailed_1
- prefab_flower_goldenrod_01_detailed_1
- prefab_flower_sunroot_01_detailed_2 (optional)

D) Reflections - make note that when you rebake reflection probe you have to see area around probe because vs pro instance foliage only for camera.
 We bake all probes with high view for foliage.

E) You could adjust shadow distance to 1350 like we did and improve shadow resolution to Very High Resolution.

What else? ENJOY and play with it, build a game or nice video! 
All best from NatureManufacture team and thanks for supporting us!
