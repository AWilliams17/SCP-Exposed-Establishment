using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System;


namespace SCPEE.NotEvil.HackModules
{
    internal class ScannedObject
    {
        public string ScannedObjectLabel { get; private set; }
        public Rect ScannedObjectPosition { get; private set; }
        public Color ScannedObjectLabelColor { get; private set; }

        public ScannedObject(string ObjectLabel, Vector3 ObjectPosition, Color LabelColor)
        {
            ScannedObjectLabel = ObjectLabel;
            ScannedObjectPosition = CalculatePositionRect(ObjectPosition);
        }

        private Rect CalculatePositionRect(Vector3 ObjectPosition)
        {
            return new Rect (
                            ObjectPosition.x - 20f, Screen.height - ObjectPosition.y - 20f, 
                            ObjectPosition.x + 40f, Screen.height - ObjectPosition.y + 50f
                        );
        }
    }

    public class ItemESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<ScannedObject> scannedObjects = new List<ScannedObject>();
        private List<ScannedObject> items = new List<ScannedObject>();
        private List<ScannedObject> players = new List<ScannedObject>();

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
                //foreach (ScannedObject item in items)
                //{
                //    GUI.contentColor = Color.cyan;
                //    GUI.Label(item.ScannedObjectPosition, item.ScannedObjectName);
                //}
                //
                //foreach (ScannedObject player in players)
                //{
                //    GUI.contentColor = Color.green;
                //    GUI.Label(player.ScannedObjectPosition, player.ScannedObjectName);
                //}
                foreach (ScannedObject scannedObject in scannedObjects)
                {
                    GUI.contentColor = scannedObject.ScannedObjectLabelColor;
                    GUI.Label(scannedObject.ScannedObjectPosition, scannedObject.ScannedObjectLabel);
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

            //items.Clear();
            scannedObjects.Clear();
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
                            ScannedObject scannedItem = new ScannedObject($"{itemLabel}:{itemDistanceFromPlayer}m", itemPosition, Color.cyan);
                            scannedObjects.Add(scannedItem);
                            //Item scannedItem = new Item($"{itemLabel}:{itemDistanceFromPlayer}m", itemPosition);
                            //items.Add(scannedItem);
                        }
                    }
                }
            }
        }

        private void ScanPlayers()
        {
            //players.Clear();
            scannedObjects.Clear();
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
                    string playerClassname = playerClassManager.klasy[playerClassManager.curClass].fullName;
                    int playerDistanceFromLocalPlayer = (int)Vector3.Distance(mainCamera.transform.position, playerPosition);
                    bool playerIsCloseEnough = playerDistanceFromLocalPlayer <= 125;
                    if (playerIsCloseEnough)
                    {
                        ScannedObject scannedPlayer = new ScannedObject($"{playerClassname}:{playerDistanceFromLocalPlayer}m", playerPosition, Color.green);
                        scannedObjects.Add(scannedPlayer);
                        //Player scannedPlayer = new Player(playerClassManager.klasy[playerClassManager.curClass].fullName, playerPosition);
                        //players.Add(scannedPlayer);
                    }
                }
            }
        }
    }
}