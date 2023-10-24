using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Ballistics;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framesaver;

namespace TYR_DeClutterer
{
    [BepInPlugin("com.TYR.DeClutter", "TYR_DeClutter", "1.1.0")]
    public class DeClutter : BaseUnityPlugin
    {
        private static GameWorld gameWorld;
        public static bool MapLoaded() => Singleton<GameWorld>.Instantiated;
        public static List<GameObject> savedClutterObjects = new List<GameObject>();
        public static Player Player;
        private bool deCluttered = false;
        public static ConfigEntry<bool> declutterEnabledConfig;
        public static ConfigEntry<bool> declutterGarbageEnabledConfig;
        public static ConfigEntry<bool> declutterHeapsEnabledConfig;
        public static ConfigEntry<bool> declutterSpentCartridgesEnabledConfig;
        public static ConfigEntry<bool> declutterFakeFoodEnabledConfig;
        public static ConfigEntry<bool> declutterDecalsEnabledConfig;
        public static ConfigEntry<bool> declutterPuddlesEnabledConfig;
        public static ConfigEntry<bool> declutterShardsEnabledConfig;
        public static ConfigEntry<float> declutterScaleOffsetConfig;
        public static ConfigEntry<bool> framesaverEnabledConfig;
        public static ConfigEntry<bool> framesaverPhysicsEnabledConfig;
        public static ConfigEntry<bool> framesaverParticlesEnabledConfig;
        public static ConfigEntry<bool> framesaverShellChangesEnabledConfig;
        public static ConfigEntry<bool> framesaverSoftVegetationEnabledConfig;
        public static ConfigEntry<bool> framesaverReflectionsEnabledConfig;
        public static ConfigEntry<bool> framesaverLightingShadowsEnabledConfig;
        public static ConfigEntry<bool> framesaverLightingShadowCascadesEnabledConfig;
        public static ConfigEntry<bool> framesaverWeatherUpdatesEnabledConfig;
        public static ConfigEntry<bool> framesaverTexturesEnabledConfig;
        public static ConfigEntry<bool> framesaverLODEnabledConfig;
        public static ConfigEntry<int> framesaverParticleBudgetDividerConfig;
        public static ConfigEntry<int> framesaverPixelLightDividerConfig;
        public static ConfigEntry<int> framesaverShadowDividerConfig;
        public static ConfigEntry<int> framesaverTextureSizeConfig;
        public static ConfigEntry<float> framesaverLODBiasConfig;
        public static bool applyDeclutter = false;
        public static bool applyFramesaver = false;
        public static bool defaultsoftParticles = QualitySettings.softParticles;
        public static int defaultparticleRaycastBudget = QualitySettings.particleRaycastBudget;
        public static bool defaultsoftVegetation = QualitySettings.softVegetation;
        public static bool defaultrealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
        public static int defaultpixelLightCount = QualitySettings.pixelLightCount;
        public static ShadowQuality defaultShadows = QualitySettings.shadows;
        public static int defaultshadowCascades = QualitySettings.shadowCascades;
        public static float defaultshadowCascade2Split = QualitySettings.shadowCascade2Split;
        public static Vector3 defaultshadowCascade4Split = QualitySettings.shadowCascade4Split;
        public static int defaultmasterTextureLimit = QualitySettings.masterTextureLimit;
        public static float defaultlodBias = QualitySettings.lodBias;

        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            declutterEnabledConfig = Config.Bind("A - De-Clutter Enabler", "A - De-Clutterer Enabled", true, "Enables the De-Clutterer");
            applyDeclutter = declutterEnabledConfig.Value;
            declutterScaleOffsetConfig = Config.Bind<float>("A - De-Clutter Enabler", "B - De-Clutterer Scaler", 1f, new BepInEx.Configuration.ConfigDescription("Larger Scale = Larger the Clutter Removed.", new BepInEx.Configuration.AcceptableValueRange<float>(0.5f, 2f)));
            declutterGarbageEnabledConfig = Config.Bind("B - De-Clutter Settings", "A - Garbage & Litter De-Clutter", true, "De-Clutters things labeled 'garbage' or similar. Smaller garbage piles.");
            declutterHeapsEnabledConfig = Config.Bind("B - De-Clutter Settings", "B - Heaps & Piles De-Clutter", true, "De-Clutters things labeled 'heaps' or similar. Larger garbage piles.");
            declutterSpentCartridgesEnabledConfig = Config.Bind("B - De-Clutter Settings", "C - Spent Cartridges De-Clutter", true, "De-Clutters pre-generated spent ammunition on floor.");
            declutterFakeFoodEnabledConfig = Config.Bind("B - De-Clutter Settings", "D - Fake Food De-Clutter", true, "De-Clutters fake 'food' items.");
            declutterDecalsEnabledConfig = Config.Bind("B - De-Clutter Settings", "E - Decal De-Clutter", true, "De-Clutters decals (Blood, grafiti, etc.)");
            declutterPuddlesEnabledConfig = Config.Bind("B - De-Clutter Settings", "F - Puddle De-Clutter", true, "De-Clutters fake reflective puddles.");
            declutterShardsEnabledConfig = Config.Bind("B - De-Clutter Settings", "G - Glass & Tile Shards", true, "De-Clutters things labeled 'shards' or similar. The things you can step on that make noise.");
            framesaverEnabledConfig = Config.Bind("C - Framesaver Enabler", "A - Framesaver Enabled", false, "Enables Ari's Framesaver methods, with some of my additions.");
            framesaverPhysicsEnabledConfig = Config.Bind("C - Framesaver Enabler", "B - Physics Changes", false, "Experimental physics optimization, uses default Unity physics on FixedUpdate rather than BSG's physics implementations.");
            framesaverShellChangesEnabledConfig = Config.Bind("C - Framesaver Enabler", "C - Shell Spawn Changes", false, "Stops spent cartride shells from spawning.");
            framesaverParticlesEnabledConfig = Config.Bind("C - Framesaver Enabler", "D - Particle Changes", false, "Enables particle changes.");
            framesaverSoftVegetationEnabledConfig = Config.Bind("C - Framesaver Enabler", "E - Vegetation Changes", false, "Enables vegetation changes.");
            framesaverReflectionsEnabledConfig = Config.Bind("C - Framesaver Enabler", "F - Reflection Changes", false, "Enables reflection changes.");
            framesaverLightingShadowsEnabledConfig = Config.Bind("C - Framesaver Enabler", "G - Lighting & Shadow Changes", false, "Enables lighting & shadow changes.");
            framesaverLightingShadowCascadesEnabledConfig = Config.Bind("C - Framesaver Enabler", "H - Shadow Cascade Changes", false, "Enables shadow cascade changes.");
            framesaverWeatherUpdatesEnabledConfig = Config.Bind("C - Framesaver Enabler", "I - Cloud & Weather Changes", false, "Enables Cloud Shadow & Weather changes.");
            framesaverTexturesEnabledConfig = Config.Bind("C - Framesaver Enabler", "J - Texture Changes", false, "Enables texture changes.");
            framesaverLODEnabledConfig = Config.Bind("C - Framesaver Enabler", "K - LOD Changes", false, "Enables LOD changes.");
            framesaverParticleBudgetDividerConfig = Config.Bind<int>("D - Framesaver Settings", "A - Particle Quality Divider", 1, new BepInEx.Configuration.ConfigDescription("1 is default, Higher number = Lower Particle Quality.", new BepInEx.Configuration.AcceptableValueRange<int>(1, 4)));
            framesaverPixelLightDividerConfig = Config.Bind<int>("D - Framesaver Settings", "B - Lighting Quality Divider", 1, new BepInEx.Configuration.ConfigDescription("1 is default, Higher number = Lower Lighting Quality.", new BepInEx.Configuration.AcceptableValueRange<int>(1, 4)));
            framesaverShadowDividerConfig = Config.Bind<int>("D - Framesaver Settings", "C - Shadow Quality Divider", 1, new BepInEx.Configuration.ConfigDescription("1 is default, Higher number = Lower Shadow Quality.", new BepInEx.Configuration.AcceptableValueRange<int>(1, 4)));
            framesaverTextureSizeConfig = Config.Bind<int>("D - Framesaver Settings", "D - Texture Size Divider", 1, new BepInEx.Configuration.ConfigDescription("1 is default, Higher number = Lower Texture Quality.", new BepInEx.Configuration.AcceptableValueRange<int>(1, 4)));
            framesaverLODBiasConfig = Config.Bind<float>("D - Framesaver Settings", "E - LOD Bias Reducer", 0.0f, new BepInEx.Configuration.ConfigDescription("0 is default, Higher number = Lower Model Quality.", new BepInEx.Configuration.AcceptableValueRange<float>(0.0f, 1.0f)));
            applyFramesaver = framesaverEnabledConfig.Value;
            InitializeClutterNames();

            // Register the SettingChanged event
            declutterEnabledConfig.SettingChanged += OnApplyDeclutterSettingChanged;
            framesaverEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverPhysicsEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverShellChangesEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverParticlesEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverSoftVegetationEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverReflectionsEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverLightingShadowsEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverWeatherUpdatesEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverTexturesEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverLODEnabledConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverParticleBudgetDividerConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverPixelLightDividerConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverShadowDividerConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverTextureSizeConfig.SettingChanged += OnApplyFramesaverSettingChanged;
            framesaverLODBiasConfig.SettingChanged += OnApplyFramesaverSettingChanged;
        }
        // Framesaver information and patches brought to you by Ari.
        private void OnApplyFramesaverSettingChanged(object sender, EventArgs e)
        {
            applyFramesaver = framesaverEnabledConfig.Value;
            if (deCluttered)
            {
                if (applyFramesaver)
                {
                    if (framesaverPhysicsEnabledConfig.Value)
                    {
                        new PhysicsUpdatePatch().Enable();
                        new PhysicsFixedUpdatePatch().Enable();
                    }
                    if (framesaverParticlesEnabledConfig.Value)
                    {
                        QualitySettings.softParticles = false;
                        QualitySettings.particleRaycastBudget = defaultparticleRaycastBudget / framesaverParticleBudgetDividerConfig.Value;
                    }
                    if (framesaverSoftVegetationEnabledConfig.Value)
                    {
                        QualitySettings.softVegetation = false;
                    }
                    if (framesaverReflectionsEnabledConfig.Value)
                    {
                        QualitySettings.realtimeReflectionProbes = false;
                    }
                    if (framesaverLightingShadowsEnabledConfig.Value)
                    {
                        new AmbientLightOptimizeRenderingPatch().Enable();
                        new AmbientLightDisableUpdatesPatch().Enable();
                        new AmbientLightDisableLateUpdatesPatch().Enable();
                    }
                    if (framesaverLightingShadowCascadesEnabledConfig.Value)
                    {
                        QualitySettings.pixelLightCount = defaultpixelLightCount / framesaverPixelLightDividerConfig.Value;
                        QualitySettings.shadows = ShadowQuality.HardOnly;
                        QualitySettings.shadowCascades = defaultshadowCascades / framesaverShadowDividerConfig.Value;
                        QualitySettings.shadowCascade2Split = defaultshadowCascade2Split * framesaverShadowDividerConfig.Value;
                        QualitySettings.shadowCascade4Split = defaultshadowCascade4Split * framesaverShadowDividerConfig.Value;
                    }
                    if (framesaverTexturesEnabledConfig.Value)
                    {
                        QualitySettings.masterTextureLimit = defaultmasterTextureLimit * framesaverTextureSizeConfig.Value;
                    }
                    if (framesaverLODEnabledConfig.Value)
                    {
                        QualitySettings.lodBias = defaultlodBias - framesaverLODBiasConfig.Value;
                    }
                    if (framesaverShellChangesEnabledConfig.Value)
                    {
                        new DontSpawnShellsFiringPatch().Enable();
                        new DontSpawnShellsJamPatch().Enable();
                        new DontSpawnShellsAtAllReallyPatch().Enable();
                    }
                    if (framesaverWeatherUpdatesEnabledConfig.Value)
                    {
                        new CloudsControllerDelayUpdatesPatch().Enable();
                        new WeatherEventControllerDelayUpdatesPatch().Enable();
                    }
                }
                else
                {
                    QualitySettings.softParticles = defaultsoftParticles;
                    QualitySettings.particleRaycastBudget = defaultparticleRaycastBudget;
                    QualitySettings.softVegetation = defaultsoftVegetation;
                    QualitySettings.realtimeReflectionProbes = defaultrealtimeReflectionProbes;
                    QualitySettings.pixelLightCount = defaultpixelLightCount;
                    QualitySettings.shadows = defaultShadows;
                    QualitySettings.shadowCascades = defaultshadowCascades;
                    QualitySettings.shadowCascade2Split = defaultshadowCascade2Split;
                    QualitySettings.shadowCascade4Split = defaultshadowCascade4Split;
                    QualitySettings.masterTextureLimit = defaultmasterTextureLimit;
                    QualitySettings.lodBias = defaultlodBias;
                    new DontSpawnShellsFiringPatch().Disable();
                    new DontSpawnShellsJamPatch().Disable();
                    new DontSpawnShellsAtAllReallyPatch().Disable();
                    new AmbientLightOptimizeRenderingPatch().Disable();
                    new AmbientLightDisableUpdatesPatch().Disable();
                    new AmbientLightDisableLateUpdatesPatch().Disable();
                    new CloudsControllerDelayUpdatesPatch().Disable();
                    new WeatherEventControllerDelayUpdatesPatch().Disable();
                    new PhysicsUpdatePatch().Disable();
                    new PhysicsFixedUpdatePatch().Disable();
                }
            }
        }
        private void OnApplyDeclutterSettingChanged(object sender, EventArgs e)
        {
            applyDeclutter = declutterEnabledConfig.Value;
            if (deCluttered)
            {
                if (applyDeclutter)
                {
                    DeClutterEnabled();
                }
                else
                {
                    ReClutterEnabled();
                }
            }

        }
        private void OnSceneUnloaded(Scene scene)
        {
            savedClutterObjects.Clear();
            deCluttered = false;
        }
        private void Update()
        {
            if (!MapLoaded() || deCluttered || !declutterEnabledConfig.Value)
                return;

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null || IsInHideout())
                return;

            deCluttered = true;
            DeClutterScene();
            DeClutterVisuals();
        }
        private bool IsInHideout()
        {
            // Check if "bunker_2" is one of the active scene names
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == "bunker_2")
                {
                    //EFT.UI.ConsoleScreen.LogError("bunker_2 loaded, not running de-cluttering.");
                    return true;
                }
            }
            //EFT.UI.ConsoleScreen.LogError("bunker_2 not loaded, de-cluttering.");
            return false;
        }
        private void DeClutterVisuals()
        {
            if (applyFramesaver)
            {
                if (framesaverPhysicsEnabledConfig.Value)
                {
                    new PhysicsUpdatePatch().Enable();
                    new PhysicsFixedUpdatePatch().Enable();
                }
                if (framesaverParticlesEnabledConfig.Value)
                {
                    QualitySettings.softParticles = false;
                    QualitySettings.particleRaycastBudget = defaultparticleRaycastBudget / framesaverParticleBudgetDividerConfig.Value;
                }
                if (framesaverSoftVegetationEnabledConfig.Value)
                {
                    QualitySettings.softVegetation = false;
                }
                if (framesaverReflectionsEnabledConfig.Value)
                {
                    QualitySettings.realtimeReflectionProbes = false;
                }
                if (framesaverLightingShadowsEnabledConfig.Value)
                {
                    new AmbientLightOptimizeRenderingPatch().Enable();
                    new AmbientLightDisableUpdatesPatch().Enable();
                    new AmbientLightDisableLateUpdatesPatch().Enable();
                }
                if (framesaverLightingShadowCascadesEnabledConfig.Value)
                {
                    QualitySettings.pixelLightCount = defaultpixelLightCount / framesaverPixelLightDividerConfig.Value;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowCascades = defaultshadowCascades / framesaverShadowDividerConfig.Value;
                    QualitySettings.shadowCascade2Split = defaultshadowCascade2Split * framesaverShadowDividerConfig.Value;
                    QualitySettings.shadowCascade4Split = defaultshadowCascade4Split * framesaverShadowDividerConfig.Value;
                }
                if (framesaverTexturesEnabledConfig.Value)
                {
                    QualitySettings.masterTextureLimit = defaultmasterTextureLimit * framesaverTextureSizeConfig.Value;
                }
                if (framesaverLODEnabledConfig.Value)
                {
                    QualitySettings.lodBias = defaultlodBias - framesaverLODBiasConfig.Value;
                }
                if (framesaverShellChangesEnabledConfig.Value)
                {
                    new DontSpawnShellsFiringPatch().Enable();
                    new DontSpawnShellsJamPatch().Enable();
                    new DontSpawnShellsAtAllReallyPatch().Enable();
                }
                if (framesaverWeatherUpdatesEnabledConfig.Value)
                {
                    new CloudsControllerDelayUpdatesPatch().Enable();
                    new WeatherEventControllerDelayUpdatesPatch().Enable();
                }
            }
            else
            {
                QualitySettings.softParticles = defaultsoftParticles;
                QualitySettings.particleRaycastBudget = defaultparticleRaycastBudget;
                QualitySettings.softVegetation = defaultsoftVegetation;
                QualitySettings.realtimeReflectionProbes = defaultrealtimeReflectionProbes;
                QualitySettings.pixelLightCount = defaultpixelLightCount;
                QualitySettings.shadows = defaultShadows;
                QualitySettings.shadowCascades = defaultshadowCascades;
                QualitySettings.shadowCascade2Split = defaultshadowCascade2Split;
                QualitySettings.shadowCascade4Split = defaultshadowCascade4Split;
                QualitySettings.masterTextureLimit = defaultmasterTextureLimit;
                QualitySettings.lodBias = defaultlodBias;
                new DontSpawnShellsFiringPatch().Disable();
                new DontSpawnShellsJamPatch().Disable();
                new DontSpawnShellsAtAllReallyPatch().Disable();
                new AmbientLightOptimizeRenderingPatch().Disable();
                new AmbientLightDisableUpdatesPatch().Disable();
                new AmbientLightDisableLateUpdatesPatch().Disable();
                new CloudsControllerDelayUpdatesPatch().Disable();
                new WeatherEventControllerDelayUpdatesPatch().Disable();
                new PhysicsUpdatePatch().Disable();
                new PhysicsFixedUpdatePatch().Disable();
            }
        }
        private void DeClutterScene()
        {
            //EFT.UI.ConsoleScreen.LogError("Running DeClutterScript");
            // Find all GameObjects in the scene, including children
            GameObject[] allGameObjects = GetAllGameObjectsInScene();
            foreach (GameObject obj in allGameObjects)
            {
                if (obj != null && ShouldDisableObject(obj))
                {
                    obj.SetActive(false);
                    //EFT.UI.ConsoleScreen.LogError("Clutter Removed " + obj.name);
                }
            }
        }
        private void DeClutterEnabled()
        {
            foreach (GameObject obj in savedClutterObjects)
            {
                if (obj.activeSelf == true)
                {
                    obj.SetActive(false);
                }
            }
        }
        private void ReClutterEnabled()
        {
            foreach (GameObject obj in savedClutterObjects)
            {
                if (obj.activeSelf == false)
                {
                    obj.SetActive(true);
                }
            }
        }
        private GameObject[] GetAllGameObjectsInScene()
        {
            List<GameObject> allGameObjects = new List<GameObject>();
            GameObject[] rootObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject root in rootObjects)
            {
                bool isLODGroup = root.GetComponent<LODGroup>() != null;
                bool isTransform = root.GetComponent<Transform>() != null;
                bool isMesh = root.GetComponent<MeshRenderer>() != null;
                bool isTarkovObservedItem = root.GetComponent<ObservedLootItem>() != null;
                bool isTarkovItem = root.GetComponent<LootItem>() != null;
                bool isTarkovWeaponMod = root.GetComponent<WeaponModPoolObject>() != null;
                bool hasRainCondensator = root.GetComponent<RainCondensator>() != null;
                bool isPlayer = root.GetComponent<Player>() != null;
                bool isLocalPlayer = root.GetComponent<LocalPlayer>() != null;
                if ((isLODGroup || isTransform || isMesh) && !isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !isPlayer && !isLocalPlayer)
                {
                    // Add the root object
                    allGameObjects.Add(root);

                    // Recursively add children
                    AddChildren(root.transform, allGameObjects);
                }
            }
            return allGameObjects.ToArray();
        }
        private void AddChildren(Transform parent, List<GameObject> allGameObjects)
        {
            foreach (Transform child in parent)
            {
                bool isLODGroup = child.GetComponent<LODGroup>() != null;
                bool isTransform = child.GetComponent<Transform>() != null;
                bool isMesh = child.GetComponent<MeshRenderer>() != null;
                bool isTarkovObservedItem = child.GetComponent<ObservedLootItem>() != null;
                bool isTarkovItem = child.GetComponent<LootItem>() != null;
                bool isTarkovWeaponMod = child.GetComponent<WeaponModPoolObject>() != null;
                bool hasRainCondensator = child.GetComponent<RainCondensator>() != null;
                bool isPlayer = child.GetComponent<Player>() != null;
                bool isLocalPlayer = child.GetComponent<LocalPlayer>() != null;
                if ((isLODGroup || isTransform || isMesh) && !isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !isPlayer && !isLocalPlayer)
                {
                    allGameObjects.Add(child.gameObject);
                }
                AddChildren(child, allGameObjects);
            }
        }
        private Dictionary<string, bool> clutterNameDictionary = new Dictionary<string, bool>
        {
        };
        private void InitializeClutterNames()
        {
            if (declutterGarbageEnabledConfig.Value)
            {
                clutterNameDictionary["tray_"] = true;
                clutterNameDictionary["electronic_box"] = true;
                clutterNameDictionary["styrofoam_"] = true;
                clutterNameDictionary["polyethylene_set"] = true;
                clutterNameDictionary["penyok_"] = true;
                clutterNameDictionary["kaska"] = true;
                clutterNameDictionary["boot_"] = true;
                clutterNameDictionary["garbage_stone"] = true;
                clutterNameDictionary["garbage_paper"] = true;
                clutterNameDictionary["cable"] = true;
                clutterNameDictionary["drawing_"] = true;
                clutterNameDictionary["paper_"] = true;
                clutterNameDictionary["_paper"] = true;
                clutterNameDictionary["paper1"] = true;
                clutterNameDictionary["paper2"] = true;
                clutterNameDictionary["paper3"] = true;
                clutterNameDictionary["paper4"] = true;
                clutterNameDictionary["paper5"] = true;
                clutterNameDictionary["paper6"] = true;
                clutterNameDictionary["paper7"] = true;
                clutterNameDictionary["paper8"] = true;
                clutterNameDictionary["paper9"] = true;
                clutterNameDictionary["pan1"] = true;
                clutterNameDictionary["pan2"] = true;
                clutterNameDictionary["pan3"] = true;
                clutterNameDictionary["pan4"] = true;
                clutterNameDictionary["pan5"] = true;
                clutterNameDictionary["pan6"] = true;
                clutterNameDictionary["pan7"] = true;
                clutterNameDictionary["pan8"] = true;
                clutterNameDictionary["pan9"] = true;
                clutterNameDictionary["poster1"] = true;
                clutterNameDictionary["poster2"] = true;
                clutterNameDictionary["poster3"] = true;
                clutterNameDictionary["poster4"] = true;
                clutterNameDictionary["poster5"] = true;
                clutterNameDictionary["poster6"] = true;
                clutterNameDictionary["poster7"] = true;
                clutterNameDictionary["poster8"] = true;
                clutterNameDictionary["poster9"] = true;
                clutterNameDictionary["_junk"] = true;
                clutterNameDictionary["junk_"] = true;
                clutterNameDictionary["_trash"] = true;
                clutterNameDictionary["trash_"] = true;
                clutterNameDictionary["cardboard_"] = true;
                clutterNameDictionary["_cardboard"] = true;
                clutterNameDictionary["sticks"] = true;
                clutterNameDictionary["cloth_"] = true;
                clutterNameDictionary["pants_"] = true;
                clutterNameDictionary["shirt_"] = true;
                clutterNameDictionary["dishes_"] = true;
                clutterNameDictionary["cutlery_"] = true;
                clutterNameDictionary["book_"] = true;
                clutterNameDictionary["books_"] = true;
                clutterNameDictionary["folder_"] = true;
                clutterNameDictionary["folders_"] = true;
                clutterNameDictionary["magazine_"] = true;
                clutterNameDictionary["magazines_"] = true;
                clutterNameDictionary["fuel_tube"] = true;
                clutterNameDictionary["city_garbage_"] = true;
                clutterNameDictionary["city_road_garbage"] = true;
                clutterNameDictionary["reserve_garbage_"] = true;
                clutterNameDictionary["reserve_road_garbage"] = true;
                clutterNameDictionary["garbage_parking_"] = true;
                clutterNameDictionary["goshan_garbage"] = true;
                clutterNameDictionary["package_garbage"] = true;
                clutterNameDictionary["wood_board"] = true;
                clutterNameDictionary["leaves_"] = true;
            }

            if (declutterHeapsEnabledConfig.Value)
            {
                clutterNameDictionary["trash_pile_"] = true;
                clutterNameDictionary["_trash_pile"] = true;
                clutterNameDictionary["crushed_concrete"] = true;
                clutterNameDictionary["crushed_concreate"] = true;
                clutterNameDictionary["baked_garbage"] = true;
                clutterNameDictionary["garbage"] = true;
                clutterNameDictionary["garbage_"] = true;
                clutterNameDictionary["_garbage"] = true;
                clutterNameDictionary["garbage_constructor"] = true;
                clutterNameDictionary["_garb"] = true;
                clutterNameDictionary["garb_"] = true;
                clutterNameDictionary["_scrap"] = true;
                clutterNameDictionary["scrap_"] = true;
                clutterNameDictionary["heap_"] = true;
                clutterNameDictionary["_heap"] = true;
                clutterNameDictionary["_pile"] = true;
                clutterNameDictionary["pile_"] = true;
                clutterNameDictionary["_stuff"] = true;
                clutterNameDictionary["_rubble"] = true;
                clutterNameDictionary["rubble_"] = true;
                clutterNameDictionary["scatter_"] = true;
                clutterNameDictionary["_scatter"] = true;
                clutterNameDictionary["scattered_"] = true;
                clutterNameDictionary["_scattered"] = true;
                clutterNameDictionary["_floorset"] = true;
                clutterNameDictionary["floorset_"] = true;
                clutterNameDictionary["brick_pile"] = true;
                clutterNameDictionary["poletelen01"] = true;
                clutterNameDictionary["poletelen02"] = true;
                clutterNameDictionary["poletelen03"] = true;
                clutterNameDictionary["poletelen04"] = true;
                clutterNameDictionary["poletelen05"] = true;
                clutterNameDictionary["poletelen06"] = true;
                clutterNameDictionary["poletelen07"] = true;
                clutterNameDictionary["poletelen08"] = true;
                clutterNameDictionary["poletelen09"] = true;
                clutterNameDictionary["vetky1"] = true;
                clutterNameDictionary["vetky2"] = true;
                clutterNameDictionary["vetky1_"] = true;
                clutterNameDictionary["vetky2_"] = true;
                clutterNameDictionary["vetky3_"] = true;
                clutterNameDictionary["vetky4_"] = true;
                clutterNameDictionary["vetky5_"] = true;
                clutterNameDictionary["vetky6_"] = true;
            }

            if (declutterSpentCartridgesEnabledConfig.Value)
            {
                clutterNameDictionary["shotshell_"] = true;
                clutterNameDictionary["shells_"] = true;
                clutterNameDictionary["_shotshell"] = true;
                clutterNameDictionary["_shells"] = true;
                clutterNameDictionary["rifleshell_"] = true;
                clutterNameDictionary["_rifleshell"] = true;
            }

            if (declutterFakeFoodEnabledConfig.Value)
            {
                clutterNameDictionary["canned"] = true;
                clutterNameDictionary["canned_"] = true;
                clutterNameDictionary["can_"] = true;
                clutterNameDictionary["juice_"] = true;
                clutterNameDictionary["carton_"] = true;
                clutterNameDictionary["_creased"] = true;
                clutterNameDictionary["bottle"] = true;
                clutterNameDictionary["bottle_"] = true;
                clutterNameDictionary["crackers_"] = true;
                clutterNameDictionary["oat_flakes"] = true;
                clutterNameDictionary["chocolate_"] = true;
                clutterNameDictionary["biscuits"] = true;
                clutterNameDictionary["package_"] = true;
                clutterNameDictionary["cigarette_"] = true;
                clutterNameDictionary["medkit_"] = true;
                clutterNameDictionary["_cup"] = true;
                clutterNameDictionary["plasticcup_"] = true;
            }

            if (declutterDecalsEnabledConfig.Value)
            {
                clutterNameDictionary["goshan_decal"] = true;
                clutterNameDictionary["ground_decal"] = true;
                clutterNameDictionary["decalgraffiti"] = true;
                clutterNameDictionary["blood_"] = true;
                clutterNameDictionary["_blood"] = true;
                clutterNameDictionary["sand_decal"] = true;
                clutterNameDictionary["decal_dirt"] = true;
                clutterNameDictionary["decal_drip"] = true;
                clutterNameDictionary["decal_"] = true;
                clutterNameDictionary["decals_"] = true;
            }

            if (declutterPuddlesEnabledConfig.Value)
            {
                clutterNameDictionary["puddle"] = true;
                clutterNameDictionary["puddles_"] = true;
                clutterNameDictionary["_puddles"] = true;
                clutterNameDictionary["puddle group"] = true;
            }

            if (declutterShardsEnabledConfig.Value)
            {
                clutterNameDictionary["_glass"] = true;
                clutterNameDictionary["brokenglass_"] = true;
                clutterNameDictionary["glass_crush"] = true;
                clutterNameDictionary["plite_crush"] = true;
                clutterNameDictionary["lesa_crush"] = true;
                clutterNameDictionary["shards_"] = true;
                clutterNameDictionary["_shards"] = true;
            }
        }
        private Dictionary<string, bool> dontDisableDictionary = new Dictionary<string, bool>
        {
            { "item_", true },
            { "weapon_", true },
            { "barter_", true },
            { "mod_", true },
            { "audio", true },
            { "container", true },
            { "trigger", true },
            { "culling", true },
            { "group", true },
            { "manager", true },
            { "scene", true },
            { "player", true }
        };
        private bool ShouldDisableObject(GameObject obj)
        {
            if (obj == null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }

            GameObject childGameMeshObject = null;
            GameObject childGameColliderObject = null;
            string objName = obj.name.ToLower();
            bool isLODGroup = obj.GetComponent<LODGroup>() != null;
            bool isMesh = obj.GetComponent<MeshRenderer>() != null;
            bool isTarkovContainer = obj.GetComponent<LootableContainer>() != null;
            bool isTarkovContainerGroup = obj.GetComponent<LootableContainersGroup>() != null;
            bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
            bool isTarkovItem = obj.GetComponent<LootItem>() != null;
            bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
            bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
            bool hasBoxCollider = obj.GetComponent<BoxCollider>()?.enabled ?? false;
            bool childHasMesh = false;
            bool childHasCollider = false;
            bool dontDisableName = dontDisableDictionary.Keys.Any(key => obj.name.ToLower().Contains(key.ToLower()));

            if (isLODGroup &&
                !dontDisableName &&
                !isTarkovObservedItem &&
                !isTarkovItem &&
                !isTarkovWeaponMod &&
                !hasRainCondensator &&
                !hasBoxCollider &&
                !isTarkovContainer &&
                !isTarkovContainerGroup)
            {
                //EFT.UI.ConsoleScreen.LogError("Found Lod Group " + obj.name);
                bool foundClutterName = clutterNameDictionary.Keys.Any(key => obj.name.ToLower().Contains(key.ToLower()));
                if (foundClutterName)
                {
                    //EFT.UI.ConsoleScreen.LogError("Found Clutter Name" + obj.name);
                    foreach (Transform child in obj.transform)
                    {
                            childGameMeshObject = child.gameObject;
                            if (child.GetComponent<MeshRenderer>() != null && !childGameMeshObject.name.ToLower().Contains("shadow") && !childGameMeshObject.name.ToLower().StartsWith("col") && !childGameMeshObject.name.ToLower().EndsWith("der"))
                            {
                                childHasMesh = true;
                                // Exit the loop since we've found what we need
                                break;
                            }
                    }
                    foreach (Transform child in obj.transform)
                    {
                        if ((child.GetComponent<MeshCollider>() != null || child.GetComponent<BoxCollider>() != null) && child.GetComponent<BallisticCollider>() == null)
                        {
                            childGameColliderObject = child.gameObject;
                            if (childGameColliderObject != null && childGameColliderObject.activeSelf)
                            {
                                childHasCollider = true;
                                // Exit the loop since we've found what we need
                                break;
                            }
                        }
                    }
                    if (isMesh || childHasMesh)
                    {
                        //EFT.UI.ConsoleScreen.LogError("Passed the bullshit brigade " + obj.name);
                        float sizeOnY = GetMeshSizeOnY(childGameMeshObject);

                        if ((childHasCollider && sizeOnY > 0.0f && (sizeOnY <= 0.25f * declutterScaleOffsetConfig.Value)) || (!childHasCollider && (sizeOnY <= 1f * declutterScaleOffsetConfig.Value)))
                        {
                            savedClutterObjects.Add(obj);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private float GetMeshSizeOnY(GameObject childGameObject)
        {
            MeshRenderer meshRenderer = childGameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.enabled)
            {
                Bounds bounds = meshRenderer.bounds;
                return bounds.size.y;
            }
            return 0.0f;
        }
    }
}
