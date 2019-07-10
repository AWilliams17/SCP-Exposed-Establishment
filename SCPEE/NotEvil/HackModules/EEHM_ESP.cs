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

        public ScannedObject(string ObjectLabel, Vector3 ObjectScreenPoint, Color LabelColor)
        {
            ScannedObjectLabel = ObjectLabel;
            ScannedObjectPosition = CalculatePositionRect(ObjectScreenPoint);
            ScannedObjectLabelColor = LabelColor;
        }

        private static Rect CalculatePositionRect(Vector3 ObjectScreenPoint)
        {
            Rect positionRect = new Rect
                (
                    ObjectScreenPoint.x - 20f, Screen.height - ObjectScreenPoint.y - 20f,
                    ObjectScreenPoint.x + 40f, Screen.height - ObjectScreenPoint.y + 50f
                );
            return positionRect;
        }
    }
    
    public class ESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<ScannedObject> scannedObjects = new List<ScannedObject>();

        private void AddScannedObjectIfViable(Transform ScannedObjectTransform, string GUILabel, Color GUILabelColor)
        {
            Camera mainCamera = Camera.main;
            Vector3 mainCameraPosition = mainCamera.transform.position;
            Vector3 scannedObjectPosition = ScannedObjectTransform.position;
            Vector3 scannedObjectScreenPoint = mainCamera.WorldToScreenPoint(scannedObjectPosition);
            
            if (!(Mathf.Abs(mainCameraPosition[2] - scannedObjectPosition[2]) > 300f) && scannedObjectScreenPoint.z > 0f)
            {
                ScannedObject scannedItem = new ScannedObject($"{GUILabel}", scannedObjectScreenPoint, GUILabelColor);
                scannedObjects.Add(scannedItem);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad2))
                isEnabled = !isEnabled;

            if (isEnabled)
            {
                localPlayer = Utils.Misc.GetLocalPlayerGameObject();
                if (localPlayer != null && scannedObjects.Count == 0)
                {
                    ScanItems();
                    ScanPlayers();
                    ScanLocations();
                }
            }
        }

        private void OnGUI()
        {
            if (isEnabled && scannedObjects.Count != 0)
            {
                // Putting the scan calls here is the only way to keep
                //  the labels from breaking. I assume there's a way around
                //  this, but I'm not very familiar with Unity and this is a pet project so \o/
                foreach (ScannedObject scannedObject in scannedObjects)
                {
                    GUI.contentColor = scannedObject.ScannedObjectLabelColor;
                    GUI.Label(scannedObject.ScannedObjectPosition, scannedObject.ScannedObjectLabel);
                }

                scannedObjects.Clear();
            }
        }

        private void ScanItems()
        {
            string[] itemsOfInterest = {
                "card", "p90", "com15", "rifle", "usp", "logicier",
                "grenade", "pistol", "scorpion", "mp7", "epsilon",
                "fusion"
            };
            
            foreach (Pickup itemPickup in FindObjectsOfType<Pickup>())
            {
                Inventory playerInventory = localPlayer.GetComponent<Inventory>();
                string itemLabel = playerInventory.availableItems[itemPickup.info.itemId].label;
                
                for (int i = 0; i < itemsOfInterest.Length; i++)
                {
                    if (itemLabel.ToLower().Contains(itemsOfInterest[i]))
                    {
                        AddScannedObjectIfViable(itemPickup.transform, itemLabel, Color.cyan);
                    }
                }
            }
        }

        private void ScanPlayers()
        {
            // Untested.
            GameObject[] allPlayers = Utils.Misc.GetPlayerGameObjects();
            foreach (GameObject player in allPlayers)
            {
                NetworkIdentity playerNetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
                if (!playerNetworkIdentity.isLocalPlayer)
                {
                    NicknameSync playerNicknameSync = player.transform.GetComponent<NicknameSync>();
                    CharacterClassManager playerClassManager = playerNicknameSync.GetComponent<CharacterClassManager>();
                    //Vector3 playerPosition = mainCamera.WorldToScreenPoint(player.transform.position);

                    string playerClassname = playerClassManager.klasy[playerClassManager.curClass].fullName;
                    //int playerDistanceFromLocalPlayer = (int)Vector3.Distance(mainCamera.transform.position, playerPosition);
                    //bool playerIsCloseEnough = playerDistanceFromLocalPlayer <= 100;
                    

                    AddScannedObjectIfViable(player.transform, playerClassname, Color.green);
                    
                    //if (playerIsCloseEnough && playerPosition.z > 0f)
                    //{
                    //    ScannedObject scannedPlayer = new ScannedObject($"{playerClassname}:{playerDistanceFromLocalPlayer}m", playerPosition, Color.green);
                    //    scannedObjects.Add(scannedPlayer);
                    //}
                }
            }
        }


        private void ScanLocations()
        {
            // SCP 914
            GameObject scp914 = GameObject.FindGameObjectWithTag("914_use");
            AddScannedObjectIfViable(scp914.transform, "SCP 914", Color.yellow);

            // Elevators
            Lift[] lifts = FindObjectsOfType<Lift>();
            for (int i = 0; i < lifts.Length; i++)
            {
                foreach (Lift.Elevator elevator in lifts[i].elevators)
                {
                    AddScannedObjectIfViable(elevator.door.transform, "Elevator", Color.blue);
                }
            }
            
            // Pocket Dimension Exits
            foreach (PocketDimensionTeleport pdTeleport in FindObjectsOfType<PocketDimensionTeleport>())
            {
                PocketDimensionTeleport.PDTeleportType teleporterType = pdTeleport.GetTeleportType();
                if (teleporterType == PocketDimensionTeleport.PDTeleportType.Exit)
                {
                    AddScannedObjectIfViable(pdTeleport.transform, "Teleporter Out", Color.white);
                }
            }
        }
    }
}



