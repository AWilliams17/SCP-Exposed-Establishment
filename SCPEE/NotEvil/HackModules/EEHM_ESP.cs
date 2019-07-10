using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

namespace SCPEE.NotEvil.HackModules
{
    internal class ESPObject
    {
        public string ESPLabel { get; private set; }
        public GameObject ESPGameObject { get; private set; }
        public Color ESPLabelColor { get; private set; }
        public int ESPMinimumDistance { get; private set; }

        public ESPObject(string Label, Color LabelColor, GameObject GameObject, int MinimumDistance)
        {
            ESPLabel = Label;
            ESPLabelColor = LabelColor;
            ESPGameObject = GameObject;
            ESPMinimumDistance = MinimumDistance;
        }
    }

    public class ESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<ESPObject> espObjects = new List<ESPObject>();

        private void Awake()
        {
            StartCoroutine("DoScans");
        }
        
        IEnumerator DoScans()
        {
            while (true)
            {
                localPlayer = Utils.Misc.GetLocalPlayerGameObject();
                if (isEnabled && localPlayer != null)
                {
                    espObjects.Clear();
                    ScanForItems();
                    ScanForPlayers();
                    ScanForLocations();
                }

                yield return new WaitForSeconds(4);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad2))
                isEnabled = !isEnabled;
        }

        private void OnGUI()
        {
            foreach (ESPObject espObject in espObjects)
            {
                Camera mainCamera = Camera.main;
                Vector3 mainCameraPosition = mainCamera.transform.position;
                Vector3 objectPosition = espObject.ESPGameObject.transform.position;
                Vector3 objectScreenPoint = mainCamera.WorldToScreenPoint(objectPosition);
                int objectDistanceFromPlayer = (int)Vector3.Distance(mainCameraPosition, objectPosition);

                bool objectIsWithinRange = !(Mathf.Abs(mainCameraPosition[2] - objectPosition[2]) > 300f) && objectScreenPoint.z > 0f;
                bool objectIsCloseEnough = objectDistanceFromPlayer <= espObject.ESPMinimumDistance;
                if (objectIsWithinRange && objectIsCloseEnough)
                {
                    Rect positionRect = new Rect
                        (
                            objectScreenPoint.x - 20f, Screen.height - objectScreenPoint.y - 20f,
                            objectScreenPoint.x + 40f, Screen.height - objectScreenPoint.y + 50f
                        );
                    GUI.color = espObject.ESPLabelColor;
                    GUI.Label(positionRect, $"{espObject.ESPLabel} : {objectDistanceFromPlayer}");

                }
            }
        }

        private void ScanForItems()
        {
            string[] itemsOfInterest = 
                {
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
                        ESPObject viableItemObject = new ESPObject(itemLabel, Color.cyan, itemPickup.gameObject, 100);
                        espObjects.Add(viableItemObject);
                    }
                }
            }
        }

        private void ScanForPlayers()
        {
            GameObject[] allPlayers = Utils.Misc.GetPlayerGameObjects();
            foreach (GameObject player in allPlayers)
            {
                NetworkIdentity playerNetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
                if (!playerNetworkIdentity.isLocalPlayer)
                {
                    NicknameSync playerNicknameSync = player.transform.GetComponent<NicknameSync>();
                    CharacterClassManager playerClassManager = playerNicknameSync.GetComponent<CharacterClassManager>();
                    string playerClassname = playerClassManager.klasy[playerClassManager.curClass].fullName;
                    
                    ESPObject playerObject = new ESPObject(playerClassname, Color.green, player, 100);
                    espObjects.Add(playerObject);
                }
            }
        }

        private void ScanForLocations()
        {
            // SCP 914
            ESPObject scp914GameObject = new ESPObject("SCP 914", Color.yellow, GameObject.FindGameObjectWithTag("914_use"), 150);
            espObjects.Add(scp914GameObject);

            // Elevators
            Lift[] lifts = FindObjectsOfType<Lift>();
            for (int i = 0; i < lifts.Length; i++)
            {
                foreach (Lift.Elevator elevator in lifts[i].elevators)
                {
                    ESPObject elevatorObject = new ESPObject("Elevator", Color.blue, elevator.door.gameObject, 200);
                    espObjects.Add(elevatorObject);
                }
            }

            // Pocket Dimension Exits
            foreach (PocketDimensionTeleport pdTeleport in FindObjectsOfType<PocketDimensionTeleport>())
            {
                PocketDimensionTeleport.PDTeleportType teleporterType = pdTeleport.GetTeleportType();
                if (teleporterType == PocketDimensionTeleport.PDTeleportType.Exit)
                {
                    ESPObject exitTeleporterObject = new ESPObject("Exit", Color.white, pdTeleport.gameObject, 75);
                    espObjects.Add(exitTeleporterObject);
                }
            }
        }
    }
}
