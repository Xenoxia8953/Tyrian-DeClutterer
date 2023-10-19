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
                    bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
                    bool isTarkovItem = obj.GetComponent<LootItem>() != null;
                    bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
                    bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
                    bool hasBoxCollider = obj.GetComponent<BoxCollider>() != null;
                    if (!isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !hasBoxCollider)
                    {
                        obj.SetActive(false);
                        Transform colliderTransform = obj.transform.Find("Collider");
                        if (colliderTransform != null)
                        {
                            GameObject colliderObject = colliderTransform.gameObject;
                            colliderObject.SetActive(false);
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
                bool isTarkovObservedItem = root.GetComponent<ObservedLootItem>() != null;
                bool isTarkovItem = root.GetComponent<LootItem>() != null;
                bool isTarkovWeaponMod = root.GetComponent<WeaponModPoolObject>() != null;
                bool hasRainCondensator = root.GetComponent<RainCondensator>() != null;
                bool hasBoxCollider = root.GetComponent<BoxCollider>() != null;
                if (!isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !hasBoxCollider)
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
                objectsList.Add(child.gameObject);
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
                clutterNameList.Add("garbage");
                clutterNameList.Add("_garb");
                clutterNameList.Add("garb_");
                clutterNameList.Add("_scrap");
                clutterNameList.Add("scrap_");
                clutterNameList.Add("paper_");
                clutterNameList.Add("_paper");
                clutterNameList.Add("scatter_");
                clutterNameList.Add("_scatter");
                clutterNameList.Add("scattered_");
                clutterNameList.Add("_scattered");
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
                clutterNameList.Add("glass_crush");
                clutterNameList.Add("dishes_");
                clutterNameList.Add("cutlery_");
                clutterNameList.Add("_floorset");
                clutterNameList.Add("floorset_");
                clutterNameList.Add("book_");
                clutterNameList.Add("books_");
                clutterNameList.Add("folder_");
                clutterNameList.Add("folders_");
                clutterNameList.Add("magazine_");
                clutterNameList.Add("magazines_");
            }

            if (declutterHeapsEnabledConfig.Value)
            {
                clutterNameList.Add("heap_");
                clutterNameList.Add("_heap");
                clutterNameList.Add("_pile");
                clutterNameList.Add("pile_");
                clutterNameList.Add("_rubble");
                clutterNameList.Add("rubble_");
            }

            if (declutterSpentCartridgesEnabledConfig.Value)
            {
                clutterNameList.Add("shotshell_");
                clutterNameList.Add("shells_");
                clutterNameList.Add("_shotshell");
                clutterNameList.Add("_shells");
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
            }

            if (declutterDecalsEnabledConfig.Value)
            {
                clutterNameList.Add("goshan_decal");
                clutterNameList.Add("ground_decal");
                clutterNameList.Add("decalgraffiti");
                clutterNameList.Add("decal_");
                clutterNameList.Add("_decal");
                clutterNameList.Add("blood_");
                clutterNameList.Add("_blood");
            }

            if (declutterPuddlesEnabledConfig.Value)
            {
                clutterNameList.Add("puddles_");
                clutterNameList.Add("_puddles");
            }

            clutterNames = clutterNameList.ToArray();
        }
        private readonly string[] subModelNames = {
            "model",
            "musor"
        };
        private bool ShouldDisableObject(GameObject obj)
        {
            if (obj == null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }
            GameObject childGameObject = null;
            string objName = obj.name.ToLower();
            bool hasMesh = obj.GetComponent<MeshRenderer>() != null;
            foreach (Transform child in obj.transform)
            {
                foreach (string name in subModelNames)
                {
                    if (child.name.Contains(name))
                    {
                        childGameObject = child.gameObject;
                    }
                }
            }
            bool childHasMesh = childGameObject != null && childGameObject.GetComponent<MeshRenderer>() != null;
            bool hasStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
            bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
            bool isTarkovItem = obj.GetComponent<LootItem>() != null;
            bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
            bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
            bool hasBoxCollider = obj.GetComponent<BoxCollider>() != null;
            foreach (string name in clutterNames)
            {
                if (objName.Contains(name) && (hasMesh || childHasMesh || hasStaticDeferredDecal) && !objName.Contains("audio") 
                    && !isTarkovObservedItem && !isTarkovItem && !isTarkovWeaponMod && !hasRainCondensator && !hasBoxCollider)
                {
                    if (hasMesh)
                    {
                        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                        // Get the bounds of the mesh
                        Bounds bounds = meshRenderer.bounds;

                        // Check the size of the mesh on its various axis
                        float sizeOnY = bounds.size.y;

                        // Log the size on the Y-axis
                        //EFT.UI.ConsoleScreen.LogError("Found mesh object - " + objName);
                        //EFT.UI.ConsoleScreen.LogError("Y size is - " + sizeOnY);
                        Transform colliderTransform = obj.transform.Find("Collider");
                        bool coliderDisabled = colliderTransform != null && !colliderTransform.gameObject.activeSelf;
                        bool coliderExists = colliderTransform != null;
                        if ((sizeOnY >= 0 && sizeOnY <= 0.25) || ((coliderDisabled || !coliderExists) && sizeOnY <= 2))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (childHasMesh)
                    {
                        MeshRenderer childMeshRenderer = childGameObject.GetComponent<MeshRenderer>();
                        // Get the bounds of the mesh
                        Bounds childBounds = childMeshRenderer.bounds;

                        // Check the size of the mesh on its various axis
                        float childSizeOnY = childBounds.size.y;

                        // Log the size on the Y-axis
                        //EFT.UI.ConsoleScreen.LogError("Found mesh object - " + objName);
                        //EFT.UI.ConsoleScreen.LogError("Y size is - " + childSizeOnY);
                        Transform colliderTransform = obj.transform.Find("colider");
                        bool coliderDisabled = colliderTransform != null && !colliderTransform.gameObject.activeSelf;
                        bool coliderExists = colliderTransform != null;
                        if ((childSizeOnY >= 0 && childSizeOnY <= 0.25) || ((coliderDisabled || !coliderExists) && childSizeOnY <= 2))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (hasStaticDeferredDecal)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
