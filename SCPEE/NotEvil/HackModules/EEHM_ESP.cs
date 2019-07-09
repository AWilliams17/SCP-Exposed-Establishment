using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;


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
            ScannedObjectLabelColor = LabelColor;
        }

        private static Rect CalculatePositionRect(Vector3 ObjectPosition)
        {
            Rect positionRect = new Rect
                (
                    ObjectPosition.x - 20f, Screen.height - ObjectPosition.y - 20f,
                    ObjectPosition.x + 40f, Screen.height - ObjectPosition.y + 50f
                );
            return positionRect;
        }
    }

    public class ESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<ScannedObject> scannedObjects = new List<ScannedObject>();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad2))
                isEnabled = !isEnabled;

            if (isEnabled)
            {
                localPlayer = Utils.Misc.GetLocalPlayerGameObject();
                if (localPlayer != null)
                {
                    scannedObjects.Clear();
                    ScanItems();
                    ScanPlayers();
                    ScanLocations();
                }
            }
        }

        private void OnGUI()
        {
            if (isEnabled)
            {
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
            
            Camera mainCamera = Camera.main;
            foreach (Pickup itemPickup in FindObjectsOfType<Pickup>())
            {
                Inventory playerInventory = localPlayer.GetComponent<Inventory>();
                string itemLabel = playerInventory.availableItems[itemPickup.info.itemId].label;
                Vector3 itemPosition = mainCamera.WorldToScreenPoint(itemPickup.transform.position);
                int itemDistanceFromPlayer = (int)Vector3.Distance(mainCamera.transform.position, itemPickup.transform.position);
                bool itemIsCloseEnough = itemDistanceFromPlayer <= 100;

                if (itemIsCloseEnough && itemPosition.z > 0f)
                {
                    for (int i = 0; i < itemsOfInterest.Length; i++)
                    {
                        if (itemLabel.ToLower().Contains(itemsOfInterest[i]))
                        {
                            ScannedObject scannedItem = new ScannedObject($"{itemLabel}:{itemDistanceFromPlayer}m", itemPosition, Color.cyan);
                            scannedObjects.Add(scannedItem);
                        }
                    }
                }
            }
        }

        private void ScanPlayers()
        {
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
                    bool playerIsCloseEnough = playerDistanceFromLocalPlayer <= 100;

                    if (playerIsCloseEnough && playerPosition.z > 0f)
                    {
                        ScannedObject scannedPlayer = new ScannedObject($"{playerClassname}:{playerDistanceFromLocalPlayer}m", playerPosition, Color.green);
                        scannedObjects.Add(scannedPlayer);
                    }
                }
            }
        }
        
        private void ScanLocations()
        {
            Camera mainCamera = Camera.main;

            // SCP 914
            GameObject scp914 = GameObject.FindGameObjectWithTag("914_use");
            Vector3 scp914Position = mainCamera.WorldToScreenPoint(scp914.transform.position);
            if (scp914Position.z > 0f)
            {
                ScannedObject scanned914Location = new ScannedObject($"SCP 914", scp914Position, Color.yellow);
                scannedObjects.Add(scanned914Location);
            }

            // Elevators
            Lift[] lifts = FindObjectsOfType<Lift>();
            for (int i = 0; i < lifts.Length; i++)
            {
                foreach (Lift.Elevator elevator in lifts[i].elevators)
                {
                    Vector3 elevatorPosition = mainCamera.WorldToScreenPoint(elevator.door.transform.position);
                    if (elevatorPosition.z > 0f)
                    {
                        ScannedObject scannedElevator = new ScannedObject($"Elevator", elevatorPosition, Color.blue);
                        scannedObjects.Add(scannedElevator);
                    }
                }
            }

            // Pocket Dimension Exits
            foreach (PocketDimensionTeleport pdteleport in FindObjectsOfType<PocketDimensionTeleport>())
            {
                Vector3 teleportLocation = mainCamera.WorldToScreenPoint(pdteleport.transform.position);
                int playerDistanceFromTeleporter = (int)Vector3.Distance(mainCamera.transform.position, teleportLocation);
                bool teleporterIsCloseEnough = playerDistanceFromTeleporter <= 50;

                if (teleporterIsCloseEnough && teleportLocation.z > 0f)
                {
                    ScannedObject scannedTeleporter = new ScannedObject($"Elevator:{playerDistanceFromTeleporter}m", teleportLocation, Color.white);
                    scannedObjects.Add(scannedTeleporter);
                }
            }
        }
    }
}