using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace Framesaver
{
    public class PhysicsUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass570), "Update");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass570.GClass571.Update();
                GClass570.GClass572.Update();
            }
            return false;
        }
    }
    public class PhysicsFixedUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass570), "FixedUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass570.GClass571.FixedUpdate();
            }
            return false;
        }
    }
    public class RagdollPhysicsLateUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CorpseRagdollTestApplication), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                GClass570.SyncTransforms();
            }
            return false;
        }
    }
}