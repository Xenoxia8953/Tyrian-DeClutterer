using System;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
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
        private static MethodInfo _updateClass1707Method;
        private static MethodInfo _method_4WeatherControllerMethod;
        public static bool everyOtherLateUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            Type class1707Type = PatchConstants.EftTypes.Single(x => x.Name == "Class1707");
            _updateClass1707Method = AccessTools.Method(class1707Type, "Update");
            _method_4WeatherControllerMethod = AccessTools.Method(typeof(WeatherController), "method_4");
            return AccessTools.Method(typeof(WeatherController), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix(WeatherController __instance, object ___class1707_0, ToDController ___TimeOfDayController)
        {
            everyOtherLateUpdate = !everyOtherLateUpdate;
            if (everyOtherLateUpdate)
            {
                ___TimeOfDayController.Update();
                _updateClass1707Method.Invoke(___class1707_0, null);
                _method_4WeatherControllerMethod.Invoke(__instance, null);
            }
            return false;
        }
    }
    public class SkyDelayUpdatesPatch : ModulePatch
    {
        private static MethodInfo _method_17_TOD_SkyMethod;
        private static MethodInfo _method_18_TOD_SkyMethod;
        private static MethodInfo _method_0_TOD_SkyMethod;
        private static MethodInfo _method_1_TOD_SkyMethod;
        private static MethodInfo _method_2_TOD_SkyMethod;
        private static MethodInfo _method_3_TOD_SkyMethod;
        public static bool everyOtherLateUpdate = false;
        protected override MethodBase GetTargetMethod()
        {
            _method_17_TOD_SkyMethod = AccessTools.Method(typeof(TOD_Sky), "method_17");
            _method_18_TOD_SkyMethod = AccessTools.Method(typeof(TOD_Sky), "method_18");
            _method_0_TOD_SkyMethod = AccessTools.Method(typeof(TOD_Sky), "method_0");
            _method_1_TOD_SkyMethod = AccessTools.Method(typeof(TOD_Sky), "method_1");
            _method_2_TOD_SkyMethod = AccessTools.Method(typeof(TOD_Sky), "method_2");
            _method_3_TOD_SkyMethod = AccessTools.Method(typeof(TOD_Sky), "method_3");
            return AccessTools.Method(typeof(TOD_Sky), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix(TOD_Sky __instance)
        {
            everyOtherLateUpdate = !everyOtherLateUpdate;
            if (everyOtherLateUpdate)
            {
                _method_17_TOD_SkyMethod.Invoke(__instance, null);
                _method_18_TOD_SkyMethod.Invoke(__instance, null);
                _method_0_TOD_SkyMethod.Invoke(__instance, null);
                _method_1_TOD_SkyMethod.Invoke(__instance, null);
                _method_2_TOD_SkyMethod.Invoke(__instance, null);
                _method_3_TOD_SkyMethod.Invoke(__instance, null);
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