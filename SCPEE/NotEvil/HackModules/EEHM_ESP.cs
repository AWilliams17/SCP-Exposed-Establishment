﻿using UnityEngine;
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

        private static Dictionary<string, Color> characterLabelColors = new Dictionary<string, Color>();

        /// <summary>
        /// Represents an object (player, item, location, etc) which 
        /// is to be rendered in the OnGUI() call.
        /// </summary>
        /// <param name="Label">The label which is displayed over the object on the screen.</param>
        /// <param name="LabelColor">The color of the aforementioned label.</param>
        /// <param name="GameObject">The actual GameObject of the thing being shown.</param>
        /// <param name="MinimumDistance">How close to the position the player is before it shows up on screen.</param>
        public ESPObject(string Label, Color LabelColor, GameObject GameObject, int MinimumDistance)
        {
            ESPLabel = Label;
            ESPLabelColor = LabelColor;
            ESPGameObject = GameObject;
            ESPMinimumDistance = MinimumDistance;
        }
    }

    /// <summary>
    /// The Exposed Establishment ESP.
    /// Displays close items, elevators, players, and locations.
    /// </summary>
    public class ESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private List<ESPObject> espObjects = new List<ESPObject>();

        private void Awake()
        {
            StartCoroutine("DoScans");
        }
        
        IEnumerator DoScans()
        {
            while (true)
            {
                espObjects.Clear();
                if (isEnabled)
                {
                    ScanForItems();
                    ScanForPlayers();
                    ScanForLocations();
                }

                yield return new WaitForSeconds(0.5f);
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
                // Skip dead game objects.
                if (espObject.ESPGameObject == null && !ReferenceEquals(espObject.ESPGameObject, null)) continue;

                // mainCameraPosition is essentially just where the player is currently looking, and
                //   the origin of the camera... So basically the player's position as well.
                // objectScreenPoint is basically just where the object is on the screen. 
                //   Including how far away it is (from the camera's perspective).
                // So what we do is, check if the object is within range, and if it is, create a new
                //   Rect which will display 'on top' of the object. The subtractions are just to control
                //   the position it's in. I want it to be slightly in the middle.
                Camera mainCamera = Camera.main;
                Vector3 mainCameraPosition = mainCamera.transform.position;
                Vector3 objectPosition = espObject.ESPGameObject.transform.position;
                Vector3 objectScreenPoint = mainCamera.WorldToScreenPoint(objectPosition);
                int objectDistanceFromPlayer = (int)Vector3.Distance(mainCameraPosition, objectPosition);

                // Couldn't think of a better name for these two bools. They literally exist solely because I didn't
                //   want the conditions being checked to cluster up in the if statement and take up space.
                bool objectIsWithinRange = !(Mathf.Abs(mainCameraPosition[2] - objectPosition[2]) > 300f) && objectScreenPoint.z > 0f;
                bool objectIsCloseEnough = objectDistanceFromPlayer <= espObject.ESPMinimumDistance;
                bool espObjectRepresentsPlayer = espObject.ESPGameObject.tag == "Player";

                if (objectIsWithinRange && objectIsCloseEnough)
                {
                    Rect positionRect = new Rect
                        (
                            objectScreenPoint.x - 20f, Screen.height - objectScreenPoint.y - 20f,
                            objectScreenPoint.x + 40f, Screen.height - objectScreenPoint.y + 50f
                        );

                    // Hack to get around player character classnames going out of sync if 
                    // directly used as the value of the 'Label' attribute in ESPObject.
                    if (espObjectRepresentsPlayer)
                    {
                        NicknameSync playerNicknameSync = espObject.ESPGameObject.transform.GetComponent<NicknameSync>();
                        CharacterClassManager playerClassManager = playerNicknameSync.GetComponent<CharacterClassManager>();
                        string playerClassname = playerClassManager.klasy[playerClassManager.curClass].fullName;
                        string playerClassnameLower = playerClassname.ToLower().Split(' ')[0];
                        
                        GUI.color = Utils.Misc.ColorFromClassname(playerClassnameLower);
                        GUI.Label(positionRect, $"{playerClassname} : {objectDistanceFromPlayer}");
                    }
                    else
                    {
                        GUI.color = espObject.ESPLabelColor;
                        GUI.Label(positionRect, $"{espObject.ESPLabel} : {objectDistanceFromPlayer}");
                    }
                }
            }
        }

        /// <summary>
        /// Scans for items of interest, which are (currently just) keycards and guns.
        /// </summary>
        private void ScanForItems()
        {
            string[] itemsOfInterest = 
                {
                    "card", "p90", "com15", "rifle", "usp", "logicier",
                    "grenade", "pistol", "scorpion", "mp7", "epsilon",
                    "fusion", "project"
                };
            
            foreach (Pickup itemPickup in FindObjectsOfType<Pickup>())
            {
                GameObject localPlayer = Utils.Misc.GetLocalPlayerGameObject();
                Inventory playerInventory = localPlayer.GetComponent<Inventory>();
                string itemLabel = playerInventory.availableItems[itemPickup.info.itemId].label;

                for (int i = 0; i < itemsOfInterest.Length; i++)
                {
                    if (itemLabel.ToLower().Contains(itemsOfInterest[i]))
                    {
                        ESPObject viableItemObject = new ESPObject(itemLabel, Color.cyan, itemPickup.gameObject, 125);
                        espObjects.Add(viableItemObject);
                    }
                }
            }
        }

        /// <summary>
        /// Scans for players - uses their character's class name as the label.
        /// </summary>
        private void ScanForPlayers()
        {
            GameObject[] allPlayers = Utils.Misc.GetPlayerGameObjects();
            foreach (GameObject player in allPlayers)
            {
                NetworkIdentity playerNetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
                if (!playerNetworkIdentity.isLocalPlayer)
                {
                    // Class names go out of sync frequently, so using them as a label here leads
                    // to issues. The label will be handled in OnGui. As will its color.
                    // TODO: Find a way around this. (there probably isn't one using this design).
                    ESPObject playerObject = new ESPObject(null, Color.black, player, 125);
                    espObjects.Add(playerObject);
                }
            }
        }

        /// <summary>
        /// Scans for locations of importance, which are (currently just) SCP-914, the Intercom, Elevators, and Pocket Dimension exits.
        /// </summary>
        private void ScanForLocations()
        {
            // SCP 914
            ESPObject scp914Object = new ESPObject("SCP 914", Color.white, GameObject.FindGameObjectWithTag("914_use"), 180);
            espObjects.Add(scp914Object);
            
            // Elevators
            Lift[] lifts = FindObjectsOfType<Lift>();
            for (int i = 0; i < lifts.Length; i++)
            {
                foreach (Lift.Elevator elevator in lifts[i].elevators)
                {
                    ESPObject elevatorObject = new ESPObject("Elevator", Color.white, elevator.door.gameObject, 250);
                    espObjects.Add(elevatorObject);
                }
            }

            // Pocket Dimension Exits
            foreach (PocketDimensionTeleport pdTeleport in FindObjectsOfType<PocketDimensionTeleport>())
            {
                if (pdTeleport.GetTeleportType() == PocketDimensionTeleport.PDTeleportType.Exit)
                {
                    ESPObject exitTeleporterObject = new ESPObject("Exit", Color.white, pdTeleport.gameObject, 100);
                    espObjects.Add(exitTeleporterObject);
                }
            }
        }
    }
}
