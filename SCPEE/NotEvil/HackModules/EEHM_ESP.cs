using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System;


namespace SCPEE.NotEvil.HackModules
{
    static internal class RectHelper {
        public static Rect CalculatePositionRect(Vector3 ObjectPosition)
        {
            Rect objectRect = new Rect
                    (
                        ObjectPosition.x - 20f,
                        Screen.height - ObjectPosition.y - 20f,
                        ObjectPosition.x + 40f,
                        Screen.height - ObjectPosition.y + 50f
                    );

            return objectRect;
        }
    }

    internal class Item
    {
        public string ItemName { get; private set; }
        public Rect ItemRect { get; private set; }

        public Item(string ScannedItemName, Vector3 ItemPosition)
        {
            ItemName = ScannedItemName;
            ItemRect = RectHelper.CalculatePositionRect(ItemPosition);
        }
    }

    internal class Player
    {
        public string PlayerClassName { get; private set; }
        public Rect PlayerRect { get; private set; }

        public Player(string ScannedPlayerClass, Vector3 PlayerPosition)
        {
            PlayerClassName = ScannedPlayerClass;
            PlayerRect = RectHelper.CalculatePositionRect(PlayerPosition);
        }
    }

    public class ItemESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<Item> items = new List<Item>();
        private List<Player> players = new List<Player>();

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
                    ScanPlayers();
                }
            }
        }

        private void OnGUI()
        {
            if (isEnabled)
            {
                foreach (Item item in items)
                {
                    GUI.contentColor = Color.cyan;
                    GUI.Label(item.ItemRect, item.ItemName);
                }

                foreach (Player player in players)
                {
                    GUI.contentColor = Color.green;
                    GUI.Label(player.PlayerRect, player.PlayerClassName);
                }
            }
        }

        private void ScanItems()
        {
            string[] itemsOfInterest = {
                "card", "p90", "com15", "rifle", "usp", "logicier",
                "grenade", "pistol", "scorpion", "mp7", "epsilon",
                "fusion"
            };

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

        private void ScanPlayers()
        {
            players.Clear();
            GameObject[] allPlayers = Utils.Misc.GetPlayerGameObjects();
            Camera mainCamera = Camera.main;
            foreach (GameObject player in allPlayers)
            {
                NetworkIdentity playerNetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
                if (!playerNetworkIdentity.isLocalPlayer)
                {
                    NicknameSync playerNicknameSync = player.transform.GetComponent<NicknameSync>();
                    CharacterClassManager playerClassManager = playerNicknameSync.GetComponent<CharacterClassManager>();
                    Vector3 playerPosition = mainCamera.WorldToScreenPoint(player.transform.position);
                    int playerDistanceFromLocalPlayer = (int)Vector3.Distance(mainCamera.transform.position, playerPosition);
                    bool playerIsCloseEnough = playerDistanceFromLocalPlayer <= 125;
                    if (playerIsCloseEnough)
                    {
                        Player scannedPlayer = new Player(playerClassManager.klasy[playerClassManager.curClass].fullName, playerPosition);
                        players.Add(scannedPlayer);
                    }
                }
            }
        }
    }
}