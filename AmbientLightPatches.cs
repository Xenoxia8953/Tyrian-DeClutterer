using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using EFT.Weather;
using HarmonyLib;

namespace Framesaver
{
    class AmbientLightOptimizeRenderingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("method_8", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    class AmbientLightDisableUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    class AmbientLightDisableLateUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    class CloudsControllerDelayUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CloudsController).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    public class WeatherLateUpdatePatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WeatherController), "FixedUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                AccessTools.Method(typeof(WeatherController), "TimeOfDayController.Update()");
                AccessTools.Method(typeof(WeatherController), "class1707_0.Update()");
                AccessTools.Method(typeof(WeatherController), "method_4");
            }
            return false;
        }
    }
    public class SkyDelayUpdatesPatch : ModulePatch
    {
        public static bool everyOtherFixedUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TOD_Sky), "FixedUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            everyOtherFixedUpdate = !everyOtherFixedUpdate;
            if (everyOtherFixedUpdate)
            {
                AccessTools.Method(typeof(TOD_Sky), "method_17");
                AccessTools.Method(typeof(TOD_Sky), "method_18");
                AccessTools.Method(typeof(TOD_Sky), "method_0");
                AccessTools.Method(typeof(TOD_Sky), "method_1");
                AccessTools.Method(typeof(TOD_Sky), "method_2");
                AccessTools.Method(typeof(TOD_Sky), "method_3");
            }
            return false;
        }
    }
    class WeatherEventControllerDelayUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeatherEventController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}