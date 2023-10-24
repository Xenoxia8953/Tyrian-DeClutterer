using Aki.Reflection.Patching;
using EFT;
using System.Reflection;

namespace Framesaver
{
    // DontSpawnShellsFiringPatch removes the spawning of spent shell casings
    // when firing a gun. Very cool, but it has an expensive update cycle
    // in GameWorld to clean them up.
    class DontSpawnShellsFiringPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass1555).GetMethod("method_9", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    // DontSpawnShellsJamPatch does similar to DontSpawnShellsFiringPatch, but
    // removes the processing for clearing a jam.
    class DontSpawnShellsJamPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass1555).GetMethod("SpawnShellAfterJam", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    class DontSpawnShellsAtAllReallyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass1555).GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix(bool __result)
        {
            __result = false;
            return false;
        }
    }
}