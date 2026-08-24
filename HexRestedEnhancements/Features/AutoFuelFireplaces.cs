using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HexBaseEnhancements.Features
{
    internal static class AutoFuelFireplaces
    {
        private const float AutoFuelThreshold = 1f;
        private const float MissingFuelMessageCooldown = 10f;

        private static float _nextMissingFuelMessageTime;
        private static int _autoFuelScanMask;

        private static readonly Collider[] ScanColliders = new Collider[8192];
        private static readonly HashSet<Fireplace> Fireplaces = new HashSet<Fireplace>();

        private static readonly FieldInfo NViewField = AccessTools.Field(typeof(Fireplace), "m_nview");

        internal static void TryAutoFuel(Player player)
        {
            if (!Plugin.IsAutoFuelFireplacesEnabled || player == null)
            {
                return;
            }

            var colliderCount = Physics.OverlapSphereNonAlloc(
                player.transform.position,
                Plugin.FireplaceDetectionRadiusInMeters,
                ScanColliders,
                GetAutoFuelScanMask(),
                QueryTriggerInteraction.Collide);

            var containers = new HashSet<Container>();

            for (var i = 0; i < colliderCount; i++)
            {
                var collider = ScanColliders[i];

                if (collider == null)
                {
                    continue;
                }

                var container = collider.GetComponentInParent<Container>();

                if (container != null)
                {
                    containers.Add(container);
                }
            }

            var radius = Plugin.FireplaceDetectionRadiusInMeters;
            var radiusSquared = radius * radius;
            var playerPosition = player.transform.position;

            foreach (var fireplace in Fireplaces)
            {
                if (fireplace == null)
                {
                    continue;
                }

                if ((fireplace.transform.position - playerPosition).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                TryFuelFireplace(player, fireplace, containers);
            }
        }

        internal static void Register(Fireplace fireplace)
        {
            if (fireplace == null)
            {
                return;
            }

            Fireplaces.Add(fireplace);

            var nview = NViewField?.GetValue(fireplace) as ZNetView;
            var fuel = nview?.GetZDO()?.GetFloat(ZDOVars.s_fuel, -1f) ?? -1f;
        }

        private static void TryFuelFireplace(Player player, Fireplace fireplace, IEnumerable<Container> containers)
        {
            if (fireplace == null ||
                fireplace.m_infiniteFuel ||
                !fireplace.m_canRefill ||
                fireplace.m_fuelItem == null)
            {
                return;
            }

            var nview = NViewField?.GetValue(fireplace) as ZNetView;

            if (nview == null || !nview.IsValid() || !nview.IsOwner())
            {
                return;
            }

            var zdo = nview.GetZDO();

            if (zdo == null)
            {
                return;
            }

            var fuel = zdo.GetFloat(ZDOVars.s_fuel, 0f);

            if (fuel > AutoFuelThreshold)
            {
                return;
            }

            var fuelName = fireplace.m_fuelItem.m_itemData.m_shared.m_name;
            var fuelNeeded = Mathf.CeilToInt(fireplace.m_maxFuel - fuel);

            if (fuelNeeded <= 0)
            {
                return;
            }

            var playerInventory = player.GetInventory();

            if (playerInventory == null)
            {
                return;
            }

            var playerFuel = playerInventory.CountItems(fuelName, -1, true);
            var containerFuel = CountFuelInContainers(containers, fuelName);
            var availableFuel = playerFuel + containerFuel;

            if (availableFuel <= 0)
            {
                ShowMissingFuelMessage(player, fuelName);
                return;
            }

            var fuelToAdd = Mathf.Min(fuelNeeded, availableFuel);
            var remaining = fuelToAdd;
            var removedFuel = 0;

            var playerFuelToRemove = Mathf.Min(playerFuel, remaining);

            if (playerFuelToRemove > 0)
            {
                var playerFuelBefore = playerInventory.CountItems(fuelName, -1, true);

                playerInventory.RemoveItem(
                    fuelName,
                    playerFuelToRemove,
                    -1,
                    true);

                var playerFuelAfter = playerInventory.CountItems(
                    fuelName,
                    -1,
                    true);

                var actualRemovedFromPlayer =
                    playerFuelBefore - playerFuelAfter;

                removedFuel += actualRemovedFromPlayer;
                remaining -= actualRemovedFromPlayer;
            }

            if (remaining > 0)
            {
                removedFuel += RemoveFuelFromContainers(
                    containers,
                    fuelName,
                    remaining);
            }

            if (removedFuel <= 0)
            {
                return;
            }

            var targetFuel = Mathf.Min(
                fireplace.m_maxFuel,
                fuel + removedFuel);

            fireplace.SetFuel(targetFuel);
        }

        private static int CountFuelInContainers(IEnumerable<Container> containers, string fuelName)
        {
            var total = 0;

            foreach (var container in containers)
            {
                if (container == null)
                {
                    continue;
                }

                var inventory = container.GetInventory();

                if (inventory == null)
                {
                    continue;
                }

                var count = inventory.CountItems(fuelName,-1,true);

                total += count;
            }

            return total;
        }

        private static int RemoveFuelFromContainers(IEnumerable<Container> containers, string fuelName, int amount)
        {
            var remaining = amount;
            var removed = 0;

            foreach (var container in containers)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (container == null)
                {
                    continue;
                }

                var inventory = container.GetInventory();

                if (inventory == null)
                {
                    continue;
                }

                var available = inventory.CountItems(
                    fuelName,
                    -1,
                    true);

                if (available <= 0)
                {
                    continue;
                }

                var amountToRemove =
                    Mathf.Min(available, remaining);

                inventory.RemoveItem(
                    fuelName,
                    amountToRemove,
                    -1,
                    true);

                var availableAfter = inventory.CountItems(
                    fuelName,
                    -1,
                    true);

                var actualRemoved =
                    available - availableAfter;

                removed += actualRemoved;
                remaining -= actualRemoved;
            }

            return removed;
        }

        private static void ShowMissingFuelMessage(
            Player player,
            string fuelName)
        {
            if (Time.time < _nextMissingFuelMessageTime)
            {
                return;
            }

            _nextMissingFuelMessageTime =
                Time.time + MissingFuelMessageCooldown;

            player.Message(
                MessageHud.MessageType.TopLeft,
                "$msg_outof " + fuelName);
        }

        private static int GetAutoFuelScanMask()
        {
            if (_autoFuelScanMask == 0)
            {
                _autoFuelScanMask = LayerMask.GetMask(
                    "piece",
                    "piece_nonsolid");
            }

            return _autoFuelScanMask;
        }
    }
}