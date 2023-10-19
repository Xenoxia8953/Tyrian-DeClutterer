using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Interactive;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TYR_DeClutterer
{
    [BepInPlugin("com.TYR.DeClutter", "TYR_DeClutter", "1.0.5")]
    public class DeClutter : BaseUnityPlugin
    {
        private static GameWorld gameWorld;
        public static bool MapLoaded() => Singleton<GameWorld>.Instantiated;
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

        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            declutterEnabledConfig = Config.Bind("A - De-Clutter Enabler", "A - De-Clutterer Enabled", true, "Enables the De-Clutterer");
            declutterScaleOffsetConfig = Config.Bind<float>("A - De-Clutter Enabler", "B - De-Clutterer Scaler", 1f, new BepInEx.Configuration.ConfigDescription("Larger Scale = Larger the Clutter Removed.", new BepInEx.Configuration.AcceptableValueRange<float>(0.5f, 2f)));
            declutterGarbageEnabledConfig = Config.Bind("B - De-Clutter Settings", "A - Garbage & Litter De-Clutter", true, "De-Clutters things labeled 'garbage' or similar. Smaller garbage piles.");
            declutterHeapsEnabledConfig = Config.Bind("B - De-Clutter Settings", "B - Heaps & Piles De-Clutter", true, "De-Clutters things labeled 'heaps' or similar. Larger garbage piles.");
            declutterSpentCartridgesEnabledConfig = Config.Bind("B - De-Clutter Settings", "C - Spent Cartridges De-Clutter", true, "De-Clutters pre-generated spent ammunition on floor.");
            declutterFakeFoodEnabledConfig = Config.Bind("B - De-Clutter Settings", "D - Fake Food De-Clutter", true, "De-Clutters fake 'food' items.");
            declutterDecalsEnabledConfig = Config.Bind("B - De-Clutter Settings", "E - Decal De-Clutter", true, "De-Clutters decals (Blood, grafiti, etc.)");
            declutterPuddlesEnabledConfig = Config.Bind("B - De-Clutter Settings", "F - Puddle De-Clutter", true, "De-Clutters fake reflective puddles.");
            declutterShardsEnabledConfig = Config.Bind("B - De-Clutter Settings", "G - Glass & Tile Shards", true, "De-Clutters things labeled 'shards' or similar. The things you can step on that make noise.");
            InitializeClutterNames();
        }
        private void OnSceneUnloaded(Scene scene)
        {
            deCluttered = false;
        }
        private void Update()
        {
            if (!MapLoaded() || deCluttered || !declutterEnabledConfig.Value)
                return;

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null || IsInHideout())
                return;

            DeClutterScene();
            deCluttered = true;
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
        private void DeClutterScene()
        {
            //EFT.UI.ConsoleScreen.LogError("Running DeClutterScript");
            // Find all GameObjects in the scene, including children
            GameObject[] allGameObjects = GetAllGameObjectsInScene();
            foreach (GameObject obj in allGameObjects)
            {
                if (ShouldDisableObject(obj))
                {
                    obj.SetActive(false);
                    //EFT.UI.ConsoleScreen.LogError("Clutter Removed " + obj.name);
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
                bool isMesh = root.GetComponent<MeshRenderer>() != null;
                bool isTarkovObservedItem = root.GetComponent<ObservedLootItem>() != null;
                bool isTarkovItem = root.GetComponent<LootItem>() != null;
                bool isTarkovWeaponMod = root.GetComponent<WeaponModPoolObject>() != null;
                bool hasRainCondensator = root.GetComponent<RainCondensator>() != null;
                bool hasBoxCollider = root.GetComponent<BoxCollider>() != null;
                if ((isLODGroup || isMesh) && !isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !hasBoxCollider)
                {
                    // Add the root object
                    allGameObjects.Add(root);

                    // Recursively add children
                    AddChildren(root.transform, allGameObjects);
                }
            }
            return allGameObjects.ToArray();
        }
        private void AddChildren(Transform parent, List<GameObject> objectsList)
        {
            foreach (Transform child in parent)
            {
                bool isLODGroup = child.GetComponent<LODGroup>() != null;
                bool isMesh = child.GetComponent<MeshRenderer>() != null;
                bool isTarkovObservedItem = child.GetComponent<ObservedLootItem>() != null;
                bool isTarkovItem = child.GetComponent<LootItem>() != null;
                bool isTarkovWeaponMod = child.GetComponent<WeaponModPoolObject>() != null;
                bool hasRainCondensator = child.GetComponent<RainCondensator>() != null;
                bool hasBoxCollider = child.GetComponent<BoxCollider>() != null;
                if ((isLODGroup || isMesh) && !isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !hasBoxCollider)
                {
                    objectsList.Add(child.gameObject);
                }
                AddChildren(child, objectsList);
            }
        }
        private Dictionary<string, bool> clutterNameDictionary = new Dictionary<string, bool>
        {
        };
        private void InitializeClutterNames()
        {
            if (declutterGarbageEnabledConfig.Value)
            {
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
                clutterNameDictionary["dishes_"] = true;
                clutterNameDictionary["cutlery_"] = true;
                clutterNameDictionary["book_"] = true;
                clutterNameDictionary["books_"] = true;
                clutterNameDictionary["folder_"] = true;
                clutterNameDictionary["folders_"] = true;
                clutterNameDictionary["magazine_"] = true;
                clutterNameDictionary["magazines_"] = true;
                clutterNameDictionary["fuel_tube"] = true;
            }

            if (declutterHeapsEnabledConfig.Value)
            {
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
                clutterNameDictionary["can_"] = true;
                clutterNameDictionary["juice_"] = true;
                clutterNameDictionary["carton_"] = true;
                clutterNameDictionary["_creased"] = true;
                clutterNameDictionary["bottle"] = true;
                clutterNameDictionary["crackers_"] = true;
                clutterNameDictionary["oat_flakes"] = true;
                clutterNameDictionary["chocolate_"] = true;
                clutterNameDictionary["biscuits"] = true;
                clutterNameDictionary["package_"] = true;
                clutterNameDictionary["cigarette_"] = true;
                clutterNameDictionary["medkit_"] = true;
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
                clutterNameDictionary["glass_crush"] = true;
                clutterNameDictionary["plite_crush"] = true;
                clutterNameDictionary["lesa_crush"] = true;
                clutterNameDictionary["shards_"] = true;
                clutterNameDictionary["_shards"] = true;
            }
        }
        private bool ShouldDisableObject(GameObject obj)
        {
            if (obj == null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }

            GameObject childGameObject = null;
            string objName = obj.name.ToLower();
            bool isLODGroup = obj.GetComponent<LODGroup>() != null;
            bool isMesh = obj.GetComponent<MeshRenderer>() != null;
            bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
            bool isTarkovItem = obj.GetComponent<LootItem>() != null;
            bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
            bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
            bool hasBoxCollider = obj.GetComponent<BoxCollider>()?.enabled ?? false;
            bool childHasMesh = obj.transform.GetComponent<MeshRenderer>() != null;

            if (isLODGroup)
            {
                //EFT.UI.ConsoleScreen.LogError("Found Lod Group " + obj.name);
                bool foundClutterName = clutterNameDictionary.Keys.Any(key => obj.name.ToLower().Contains(key.ToLower()));
                if (foundClutterName)
                {
                    //EFT.UI.ConsoleScreen.LogError("Found Clutter Name" + obj.name);
                    foreach (Transform child in obj.transform)
                    {
                        if (child.GetComponent<MeshRenderer>() != null)
                        {
                            childGameObject = child.gameObject;
                            childHasMesh = childGameObject != null && childGameObject.GetComponent<MeshRenderer>() != null;
                            //EFT.UI.ConsoleScreen.LogError("Found child with same name as LOD Group and has mesh " + child.name);
                        }
                    }
                    if ((isMesh || childHasMesh) &&
                        (!objName.Contains("audio") &&
                        !objName.Contains("weapon") &&
                        !objName.Contains("barter") &&
                        !isTarkovObservedItem &&
                        !isTarkovItem &&
                        !isTarkovWeaponMod &&
                        !hasRainCondensator &&
                        !hasBoxCollider))
                    {
                        //EFT.UI.ConsoleScreen.LogError("Passed the bullshit brigade " + obj.name);
                        float sizeOnY = GetMeshSizeOnY(obj, childGameObject);

                        // Find the "Collider" or "colider" child object
                        Transform colliderTransform = FindChildTransform(obj, childGameObject, new string[] { "collider", "colider", "col" });

                        // Check the child's active state or existence
                        bool coliderDisabled = colliderTransform == null || (!colliderTransform.gameObject.activeSelf);

                        if ((!coliderDisabled && sizeOnY >= 0 && sizeOnY <= 0.4 * declutterScaleOffsetConfig.Value) || (coliderDisabled && sizeOnY <= 1.2 * declutterScaleOffsetConfig.Value))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private Transform FindChildTransform(GameObject obj, GameObject childGameObject, string[] namesToFind)
        {
            if (obj == null || childGameObject == null || namesToFind == null)
            {
                return null; // Handle null objects or namesToFind
            }

            foreach (string nameToFind in namesToFind)
            {
                Transform childTransform = obj.transform.Find(nameToFind) ?? childGameObject.transform.Find(nameToFind);
                if (childTransform != null)
                {
                    return childTransform;
                }
            }
            return null; // No matching child transform found
        }
        private float GetMeshSizeOnY(GameObject obj, GameObject childGameObject)
        {
            MeshRenderer meshRenderer = obj?.GetComponent<MeshRenderer>() ?? childGameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.enabled)
            {
                Bounds bounds = meshRenderer.bounds;
                return bounds.size.y;
            }
            return 0.0f;
        }
    }
}
