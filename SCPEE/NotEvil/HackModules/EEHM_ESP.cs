using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

namespace SCPEE.NotEvil.HackModules
{
    internal class Item
    {
        public string ItemName { get; private set; }
        public Rect ItemRect { get; private set; }

        public Item(string ScannedItemName, Vector3 ItemPosition)
        {
            ItemName = ScannedItemName;
            ItemRect = CalculateRect(ItemPosition);
        }

        private static Rect CalculateRect(Vector3 ItemPosition)
        {
            Rect itemRect = new Rect
                    (
                        ItemPosition.x - 20f,
                        Screen.height - ItemPosition.y - 20f,
                        ItemPosition.x + 40f,
                        Screen.height - ItemPosition.y + 50f
                    );

            return itemRect;
        }
    }

    public class ItemESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<Item> items = new List<Item>();
        private readonly string[] itemsOfInterest = {
            "card", "p90", "com15", "rifle", "usp", "logicier",
            "grenade", "pistol", "scorpion", "mp7", "epsilon",
        };
       

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad2))
                isEnabled = !isEnabled;

            if (isEnabled)
            {
                localPlayer = Utils.Misc.GetLocalPlayerGameObject();
                if (localPlayer != null)
                {
                    ScanItems();
                }
            }
        }

        private void OnGUI()
        {
            if (isEnabled)
            {
                foreach (Item item in items)
                {
                    GUI.Label(item.ItemRect, item.ItemName);
                }
            }
        }

        private void ScanItems()
        {
            items.Clear();
            Camera mainCamera = Camera.main;
            foreach (Pickup itemPickup in FindObjectsOfType<Pickup>())
            {
                Inventory playerInventory = localPlayer.GetComponent<Inventory>();
                string itemLabel = playerInventory.availableItems[itemPickup.info.itemId].label;
                Vector3 itemPosition = mainCamera.WorldToScreenPoint(itemPickup.transform.position);
                int itemDistanceFromPlayer = (int)Vector3.Distance(mainCamera.transform.position, itemPickup.transform.position);
                bool itemIsCloseEnough = itemDistanceFromPlayer <= 125;
                
                if (itemIsCloseEnough)
                {
                    for (int i = 0; i < itemsOfInterest.Length; i++)
                    {
                        if (itemLabel.ToLower().Contains(itemsOfInterest[i]))
                        {
                            Item scannedItem = new Item($"{itemLabel}:{itemDistanceFromPlayer}m", itemPosition);
                            items.Add(scannedItem);
                        }
                    }
                }
            }
        }
    }
}
