using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace HexBaseEnhancements.Patches
{
    [HarmonyPatch(typeof(Door), nameof(Door.SetState))]
    internal static class PatchDoorSetState
    {
        private const float VanillaAnimationSpeed = 1f;
        private const float FastOpenAnimationSpeed = 50f;

        [HarmonyPrefix]
        private static void Prefix(int state, Animator ___m_animator)
        {
            if (___m_animator == null)
            {
                return;
            }

            ___m_animator.speed = Plugin.DoorsOpensFaster && state != 0
                ? FastOpenAnimationSpeed
                : VanillaAnimationSpeed;
        }

        [HarmonyPostfix]
        private static void Postfix(Door __instance, int state, ZNetView ___m_nview)
        {
            if (!Plugin.DoorsAutoClose || __instance == null || ___m_nview == null)
            {
                return;
            }

            if (state == 0 || !___m_nview.IsValid() || !___m_nview.IsOwner())
            {
                return;
            }

            __instance.StartCoroutine(AutoCloseDoor(___m_nview));
        }

        private static IEnumerator AutoCloseDoor(ZNetView nview)
        {
            yield return new WaitForSeconds(Plugin.DoorAutoCloseInSeconds);

            if (nview == null || !nview.IsValid() || !nview.IsOwner())
            {
                yield break;
            }

            var zdo = nview.GetZDO();

            if (zdo == null || zdo.GetInt(ZDOVars.s_state, 0) == 0)
            {
                yield break;
            }

            nview.InvokeRPC("UseDoor", false);
        }
    }
}