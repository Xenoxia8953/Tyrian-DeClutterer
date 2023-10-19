using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Interactive;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TYR_DeClutterer
{
    [BepInPlugin("com.TYR.DeClutter", "TYR_DeClutter", "1.0.0")]
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

        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            declutterEnabledConfig = Config.Bind("A - De-Clutter Enabler", "De-Clutterer Enabled", true, "Enables the De-Clutterer");
            declutterGarbageEnabledConfig = Config.Bind("B - De-Clutter Settings", "A - Garbage & Litter De-Clutter", true, "De-Clutters things labeled 'garbage' or similar. Smaller garbage piles.");
            declutterHeapsEnabledConfig = Config.Bind("B - De-Clutter Settings", "B - Heaps & Piles De-Clutter", true, "De-Clutters things labeled 'heaps' or similar. Larger garbage piles.");
            declutterSpentCartridgesEnabledConfig = Config.Bind("B - De-Clutter Settings", "C - Spent Cartridges De-Clutter", true, "De-Clutters pre-generated spent ammunition on floor.");
            declutterFakeFoodEnabledConfig = Config.Bind("B - De-Clutter Settings", "D - Fake Food De-Clutter", true, "De-Clutters fake 'food' items.");
            declutterDecalsEnabledConfig = Config.Bind("B - De-Clutter Settings", "E - Decal De-Clutter", true, "De-Clutters decals (Blood, grafiti, etc.)");
            declutterPuddlesEnabledConfig = Config.Bind("B - De-Clutter Settings", "F - Puddle De-Clutter", true, "De-Clutters fake reflective puddles.");
            InitializeClutterNames();
        }
        private void OnSceneUnloaded(Scene scene)
        {
            deCluttered = false;
        }
        private void Update()
        {
            if (!MapLoaded() || deCluttered || IsInHideout() || !declutterEnabledConfig.Value)
                return;

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null)
                return;

            DeClutterScene();
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
                    bool isLODGroup = obj.GetComponent<LODGroup>() != null;
                    bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
                    bool isTarkovItem = obj.GetComponent<LootItem>() != null;
                    bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
                    bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
                    bool hasBoxCollider = obj.GetComponent<BoxCollider>() != null;
                    if (isLODGroup && !isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !hasBoxCollider)
                    {
                        // Find the "Collider" or "colider" child object
                        Transform colliderTransform = FindChildTransform(obj, "collider", "colider", "col");
                        float sizeOnY = GetMeshSizeOnY(obj, obj);
                        // Check the child's active state or existence
                        bool coliderDisabled = colliderTransform == null || (!colliderTransform.gameObject.activeSelf);
                        if ((!coliderDisabled && sizeOnY >= 0 && sizeOnY <= 1) || (coliderDisabled && sizeOnY <= 2))
                        {
                            obj.SetActive(false);
                        }
                        //EFT.UI.ConsoleScreen.LogError("Clutter Removed " + obj.name);
                    }
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
            deCluttered = true;
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
        private List<string> clutterNameList = new List<string>
        {
            
        };
        private string[] clutterNames;
        private void InitializeClutterNames()
        {
            if (declutterGarbageEnabledConfig.Value)
            {
                clutterNameList.Add("kaska");
                clutterNameList.Add("boot_");
                clutterNameList.Add("garbage_stone");
                clutterNameList.Add("garbage_paper");
                clutterNameList.Add("cable");
                clutterNameList.Add("drawing_");
                clutterNameList.Add("paper_");
                clutterNameList.Add("_paper");
                clutterNameList.Add("paper1");
                clutterNameList.Add("paper2");
                clutterNameList.Add("paper3");
                clutterNameList.Add("paper4");
                clutterNameList.Add("paper5");
                clutterNameList.Add("paper6");
                clutterNameList.Add("paper7");
                clutterNameList.Add("paper8");
                clutterNameList.Add("paper9");
                clutterNameList.Add("poster1");
                clutterNameList.Add("poster2");
                clutterNameList.Add("poster3");
                clutterNameList.Add("poster4");
                clutterNameList.Add("poster5");
                clutterNameList.Add("poster6");
                clutterNameList.Add("poster7");
                clutterNameList.Add("poster8");
                clutterNameList.Add("poster9");
                clutterNameList.Add("_junk");
                clutterNameList.Add("junk_");
                clutterNameList.Add("_trash");
                clutterNameList.Add("trash_");
                clutterNameList.Add("cardboard_");
                clutterNameList.Add("_cardboard");
                clutterNameList.Add("sticks");
                clutterNameList.Add("cloth_");
                clutterNameList.Add("shards_");
                clutterNameList.Add("_shards");
                clutterNameList.Add("dishes_");
                clutterNameList.Add("cutlery_");
                clutterNameList.Add("book_");
                clutterNameList.Add("books_");
                clutterNameList.Add("folder_");
                clutterNameList.Add("folders_");
                clutterNameList.Add("magazine_");
                clutterNameList.Add("magazines_");
                clutterNameList.Add("fuel_tube");
            }

            if (declutterHeapsEnabledConfig.Value)
            {
                clutterNameList.Add("crushed_concreate");
                clutterNameList.Add("crushed_concrete");
                clutterNameList.Add("baked_garbage");
                clutterNameList.Add("garbage");
                clutterNameList.Add("garbage_constructor");
                clutterNameList.Add("_garb");
                clutterNameList.Add("garb_");
                clutterNameList.Add("_scrap");
                clutterNameList.Add("scrap_");
                clutterNameList.Add("heap_");
                clutterNameList.Add("_heap");
                clutterNameList.Add("_pile");
                clutterNameList.Add("pile_");
                clutterNameList.Add("_rubble");
                clutterNameList.Add("rubble_");
                clutterNameList.Add("scatter_");
                clutterNameList.Add("_scatter");
                clutterNameList.Add("scattered_");
                clutterNameList.Add("_scattered");
                clutterNameList.Add("_floorset");
                clutterNameList.Add("floorset_");
                clutterNameList.Add("glass_crush");
                clutterNameList.Add("plite_crush");
                clutterNameList.Add("lesa_crush");
                clutterNameList.Add("brick_pile");
                clutterNameList.Add("poletelen01");
                clutterNameList.Add("poletelen02");
                clutterNameList.Add("poletelen03");
                clutterNameList.Add("poletelen04");
                clutterNameList.Add("poletelen05");
                clutterNameList.Add("poletelen06");
                clutterNameList.Add("poletelen07");
                clutterNameList.Add("poletelen08");
                clutterNameList.Add("poletelen09");
            }

            if (declutterSpentCartridgesEnabledConfig.Value)
            {
                clutterNameList.Add("shotshell_");
                clutterNameList.Add("shells_");
                clutterNameList.Add("_shotshell");
                clutterNameList.Add("_shells");
                clutterNameList.Add("rifleshell_");
                clutterNameList.Add("_rifleshell");
            }

            if (declutterFakeFoodEnabledConfig.Value)
            {
                clutterNameList.Add("canned");
                clutterNameList.Add("can_");
                clutterNameList.Add("juice_");
                clutterNameList.Add("carton_");
                clutterNameList.Add("_creased");
                clutterNameList.Add("bottle");
                clutterNameList.Add("crackers_");
                clutterNameList.Add("oat_flakes");
                clutterNameList.Add("chocolate_");
                clutterNameList.Add("biscuits");
                clutterNameList.Add("package_");
                clutterNameList.Add("cigarette_");
                clutterNameList.Add("medkit_");
            }

            if (declutterDecalsEnabledConfig.Value)
            {
                clutterNameList.Add("goshan_decal");
                clutterNameList.Add("ground_decal");
                clutterNameList.Add("decalgraffiti");
                clutterNameList.Add("blood_");
                clutterNameList.Add("_blood");
                clutterNameList.Add("sand_decal");
                clutterNameList.Add("decal_dirt");
                clutterNameList.Add("decal_drip");
            }

            if (declutterPuddlesEnabledConfig.Value)
            {
                clutterNameList.Add("puddles_");
                clutterNameList.Add("_puddles");
            }

            clutterNames = clutterNameList.ToArray();
        }
        private bool ShouldDisableObject(GameObject obj)
        {
            if (obj == null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }

            string objName = obj.name.ToLower();
            bool isLODGroup = obj.GetComponent<LODGroup>() != null;
            bool isMesh = obj.GetComponent<MeshRenderer>() != null;
            bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
            bool isTarkovItem = obj.GetComponent<LootItem>() != null;
            bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
            bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
            bool hasBoxCollider = obj.GetComponent<BoxCollider>()?.enabled ?? false;

            GameObject childGameObject = null;

            foreach (Transform child in obj.transform)
            {
                if (child.GetComponent<MeshRenderer>() != null)
                {
                    childGameObject = child.gameObject;
                    bool childHasMesh1 = childGameObject != null && childGameObject.GetComponent<MeshRenderer>() != null;
                    bool childHasRainCondensator1 = childGameObject != null && childGameObject.GetComponent<RainCondensator>() != null;
                    bool childHasBoxCollider1 = childGameObject != null && childGameObject.GetComponent<BoxCollider>() != null;
                    bool childIsTarkovObservedItem1 = childGameObject != null && childGameObject.GetComponent<ObservedLootItem>() != null;
                    bool childIsTarkovItem1 = childGameObject != null && childGameObject.GetComponent<LootItem>() != null;
                    bool childIsTarkovWeaponMod1 = childGameObject != null && childGameObject.GetComponent<WeaponModPoolObject>() != null;
                    foreach (string name in clutterNames)
                    {
                        bool isExactMatch = !name.Contains("_");
                        string pattern = isExactMatch ? $"^{name}( \\(\\d+\\))?$" : name;
                        if ((System.Text.RegularExpressions.Regex.IsMatch(objName, pattern) ||
                            (!isExactMatch && objName.Contains(name))) &&
                            ((isLODGroup && childHasMesh1) || (isLODGroup && isMesh)) &&
                            !objName.Contains("audio") &&
                            !objName.Contains("weapon") &&
                            !objName.Contains("barter") &&
                            !isTarkovObservedItem &&
                            !isTarkovItem &&
                            !isTarkovWeaponMod &&
                            !hasRainCondensator &&
                            !hasBoxCollider &&
                            !childHasRainCondensator1 &&
                            !childHasBoxCollider1 &&
                            !childIsTarkovObservedItem1 &&
                            !childIsTarkovItem1 &&
                            !childIsTarkovWeaponMod1)
                        {
                            float sizeOnY = GetMeshSizeOnY(obj, childGameObject);

                            // Find the "Collider" or "colider" child object
                            Transform colliderTransform = FindChildTransform(obj, "collider", "colider", "col");

                            // Check the child's active state or existence
                            bool coliderDisabled = colliderTransform == null || (!colliderTransform.gameObject.activeSelf);

                            if ((!coliderDisabled && sizeOnY >= 0 && sizeOnY <= 1) || (coliderDisabled && sizeOnY <= 2))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
            bool childHasMesh = childGameObject != null && childGameObject.GetComponent<MeshRenderer>() != null;
            bool childHasRainCondensator = childGameObject != null && childGameObject.GetComponent<RainCondensator>() != null;
            bool childHasBoxCollider = childGameObject != null && childGameObject.GetComponent<BoxCollider>() != null;
            bool childIsTarkovObservedItem = childGameObject != null && childGameObject.GetComponent<ObservedLootItem>() != null;
            bool childIsTarkovItem = childGameObject != null && childGameObject.GetComponent<LootItem>() != null;
            bool childIsTarkovWeaponMod = childGameObject != null && childGameObject.GetComponent<WeaponModPoolObject>() != null;
            foreach (string name in clutterNames)
            {
                bool isExactMatch = !name.Contains("_");
                string pattern = isExactMatch ? $"^{name}( \\(\\d+\\))?$" : name;
                if ((System.Text.RegularExpressions.Regex.IsMatch(objName, pattern) ||
                    (!isExactMatch && objName.Contains(name))) &&
                    ((isLODGroup && childHasMesh) || (isLODGroup && isMesh)) &&
                    !objName.Contains("audio") &&
                    !objName.Contains("weapon") &&
                    !objName.Contains("barter") &&
                    !isTarkovObservedItem &&
                    !isTarkovItem &&
                    !isTarkovWeaponMod &&
                    !hasRainCondensator &&
                    !hasBoxCollider &&
                    !childHasRainCondensator &&
                    !childHasBoxCollider &&
                    !childIsTarkovObservedItem &&
                    !childIsTarkovItem &&
                    !childIsTarkovWeaponMod)
                {
                    float sizeOnY = GetMeshSizeOnY(obj, childGameObject);

                    // Find the "Collider" or "colider" child object
                    Transform colliderTransform = FindChildTransform(obj, "collider", "colider", "col");

                    // Check the child's active state or existence
                    bool coliderDisabled = colliderTransform == null || (!colliderTransform.gameObject.activeSelf);

                    if ((!coliderDisabled && sizeOnY >= 0 && sizeOnY <= 1) || (coliderDisabled && sizeOnY <= 2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private Transform FindChildTransform(GameObject obj, params string[] namesToFind)
        {
            foreach (string nameToFind in namesToFind)
            {
                Transform childTransform = obj.transform.Find(nameToFind);
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
