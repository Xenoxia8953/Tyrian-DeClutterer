using BepInEx;
using Comfort.Common;
using EFT;
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

        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        private void OnSceneUnloaded(Scene scene)
        {
            deCluttered = false;
        }
        private void Update()
        {
            if (!MapLoaded() || deCluttered || IsInHideout())
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
                // Add the root object
                allGameObjects.Add(root);

                // Recursively add children
                AddChildren(root.transform, allGameObjects);
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
        private readonly string[] clutterNames = {
            "book_",
            "books_",
            "shotshell_",
            "shells_",
            "garb_", 
            "_garb",
            "_floorset",
            "floorset_",
            "cutlery_",
            "dishes_",
            "bottle_",
            "glass_crush",
            "goshan_decal",
            "ground_decal",
            "stick",
            "shards_",
            "_shards",
            "rubble_",
            "_rubble",
            "paper_",
            "_paper",
            "puddle_",
            "_puddle",
            "garbage",
            "heap_", 
            "_heap",
            "_scrap", 
            "scrap_",
            "scatter_",
            "_junk", 
            "junk_",
            "_trash",
            "trash_",
            "_pile",
            "pile_",
            "cloth_",
            "cardboard_",
            "scatter_",
            "scattered_",
            "sand_decal",
            "decalgraffiti"
        };
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
            foreach (string name in clutterNames)
            {
                if (objName.Contains(name) && (hasMesh || childHasMesh || hasStaticDeferredDecal) && !objName.Contains("audio") && !isTarkovObservedItem && !isTarkovItem)
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
                        if ((sizeOnY >= 0 && sizeOnY <= 0.4) || ((coliderDisabled || !coliderExists) && sizeOnY <= 2))
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
                        if ((childSizeOnY >= 0 && childSizeOnY <= 0.4) || ((coliderDisabled || !coliderExists) && childSizeOnY <= 2))
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
