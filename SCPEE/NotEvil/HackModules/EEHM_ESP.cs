using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

namespace SCPEE.NotEvil.HackModules
{
    internal class ESPGenericObject
    {
        public string ESPLabel { get; private set; }
        public GameObject ESPGameObject { get; private set; }
        public Color ESPLabelColor { get; private set; }
        
        /// <summary>
        /// Represents a generic object (generic as in an item, location, basically anything not a player) which 
        /// is to be rendered in the OnGUI() call.
        /// </summary>
        /// <param name="Label">The label which is displayed over the object on the screen.</param>
        /// <param name="LabelColor">The color of the aforementioned label.</param>
        /// <param name="GameObject">The GameObject of the item/location/etc.</param>
        public ESPGenericObject(string Label, Color LabelColor, GameObject GameObject)
        {
            ESPLabel = Label;
            ESPLabelColor = LabelColor;
            ESPGameObject = GameObject;
        }
    }

    internal class ESPPlayerObject
    {
        public GameObject ESPPlayerGameObject { get; private set; }
        private NicknameSync playerNicknameSync;
        private CharacterClassManager playerClassManager;
        
        private static Color dClassPersonnelColor = new Color(1.0f, 0.64f, 0.0f);
        private static readonly Dictionary<string, Color> _characterLabelColors = new Dictionary<string, Color>()
        {
            { "scp", Color.red },
            { "nine", Color.blue },
            { "chaos", Color.green },
            { "facility", Color.gray },
            { "scientist", Color.yellow },
            { "class", dClassPersonnelColor }
        };

        /// <summary>
        /// Represents a player object which is to be rendered in the OnGUI() call.
        /// </summary>
        /// <param name="PlayerGameObject">The GameObject of the player.</param>
        public ESPPlayerObject(GameObject PlayerGameObject)
        {
            ESPPlayerGameObject = PlayerGameObject;
            playerNicknameSync = ESPPlayerGameObject.transform.GetComponent<NicknameSync>();
            playerClassManager = ESPPlayerGameObject.GetComponent<CharacterClassManager>();
        }

        public string GetPlayerClassname()
        {
            string playerClassname = playerClassManager.klasy[playerClassManager.curClass].fullName;
            return playerClassname;
        }

        /// <summary>
        /// Takes the player's current classname and grabs an appropriate color for 
        /// it for usage in the OnGUI call.
        /// </summary>
        /// <param name="CharacterClassname">The player's class name.</param>
        /// <returns>The Color for the player's class. If no appropriate color could be found, returns magenta.</returns>
        public Color GetPlayerLabelColor(string CharacterClassname)
        {
            string characterClassLowerCase = CharacterClassname.ToLower();
            string characterClassSubstring = characterClassLowerCase.Contains("-") ?
                characterClassLowerCase.Split('-')[0] : characterClassLowerCase.Split(' ')[0];

            if (_characterLabelColors.TryGetValue(characterClassSubstring, out Color characterClassColor))
                return characterClassColor;
            else
                return Color.magenta;
        }
    }

    internal class ESPObjectGUIDetails
    {
        public bool ObjectIsCloseEnough { get; private set; }
        public int ObjectDistanceFromPlayer { get; private set; }
        public Rect ObjectPositionRect { get; private set; }

        /// <summary>
        /// Makes calculations to retrieve a game object's distance from the player and determines
        /// if it is close enough to be rendered, and also calculates the Rect for the information in
        /// the OnGUI call to go in.
        /// </summary>
        /// <param name="ESPGameObject">The GameObject to be used for making calculations on.</param>
        public ESPObjectGUIDetails(GameObject ESPGameObject)
        {
            /*
               (This description is based off my current 
               understanding of the Unity concepts involved. These calculations are cleaned up versions of the calculations
               in https://github.com/chrysls's SCP:SL ESP, since I'm not so great at vectors... Or Unity.)

             mainCameraPosition is essentially just where the player is currently looking, and
               the origin of the camera... So basically the player's position in the game world as well.

             objectScreenPoint is basically just where the object is on the screen. 
               Including how far away it is (from the camera's perspective).

             So what we do is, check if the object is within range, and if it is, create a new
               Rect which will display 'on top' of the object. The subtractions are just to control
               the position it's in. I want it to be slightly in the middle of the object on screen.
            */
            Camera mainCamera = Camera.main;
            Vector3 mainCameraPosition = mainCamera.transform.position;
            Vector3 objectPosition = ESPGameObject.transform.position;
            Vector3 objectScreenPoint = mainCamera.WorldToScreenPoint(objectPosition);
            int objectDistanceFromPlayer = (int)Vector3.Distance(mainCameraPosition, objectPosition);
            bool objectIsWithinRange = !(Mathf.Abs(mainCameraPosition[2] - objectPosition[2]) > 300f) && objectScreenPoint.z > 0f;

            Rect objectPositionRect = new Rect
            (
                objectScreenPoint.x - 20f, Screen.height - objectScreenPoint.y - 20f,
                objectScreenPoint.x + 40f, Screen.height - objectScreenPoint.y + 50f
            );

            ObjectIsCloseEnough = objectIsWithinRange;
            ObjectDistanceFromPlayer = objectDistanceFromPlayer;
            ObjectPositionRect = objectPositionRect;
        }
    }

    /// <summary>
    /// The Exposed Establishment ESP.
    /// Displays close items, elevators, players, and locations.
    /// </summary>
    public class ESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private List<ESPGenericObject> genericESPObjects = new List<ESPGenericObject>();
        private List<ESPPlayerObject> playerESPObjects = new List<ESPPlayerObject>();

        private void Awake()
        {
            StartCoroutine("DoScans");
        }
        
        IEnumerator DoScans()
        {
            while (true)
            {
                genericESPObjects.Clear();
                playerESPObjects.Clear();
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
            // I'm unsure if the 'skip dead game objects' checks in both loops are necessary or not. I'm not 100% certain
            //   how Unity would handle them not being there and me attempting to access information in a no longer valid
            //   GameObject, so I'm going to play it safe and leave them in, since they don't hurt anything by leaving them.

            foreach (ESPGenericObject genericEspObject in genericESPObjects)
            {
                // Skip dead game objects.
                if (genericEspObject.ESPGameObject == null && !ReferenceEquals(genericEspObject.ESPGameObject, null)) continue;

                ESPObjectGUIDetails espObjectGUIDetails = new ESPObjectGUIDetails(genericEspObject.ESPGameObject);

                if (espObjectGUIDetails.ObjectIsCloseEnough)
                {
                    GUI.color = genericEspObject.ESPLabelColor;
                    GUI.Label
                    (
                        espObjectGUIDetails.ObjectPositionRect, 
                        $"{genericEspObject.ESPLabel} : {espObjectGUIDetails.ObjectDistanceFromPlayer}"
                    );
                }
            }

            foreach (ESPPlayerObject espPlayerObject in playerESPObjects)
            {
                // Skip dead game objects.
                if (espPlayerObject.ESPPlayerGameObject == null && !ReferenceEquals(espPlayerObject.ESPPlayerGameObject, null)) continue;

                ESPObjectGUIDetails espObjectGUIDetails = new ESPObjectGUIDetails(espPlayerObject.ESPPlayerGameObject);
                if (espObjectGUIDetails.ObjectIsCloseEnough)
                {
                    string playerLabel = espPlayerObject.GetPlayerClassname();
                    Color labelColor = espPlayerObject.GetPlayerLabelColor(playerLabel);

                    GUI.color = labelColor;
                    GUI.Label
                    (
                        espObjectGUIDetails.ObjectPositionRect,
                        $"{playerLabel} : {espObjectGUIDetails.ObjectDistanceFromPlayer}"
                    );
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
                        ESPGenericObject viableItemObject = new ESPGenericObject(itemLabel, Color.cyan, itemPickup.gameObject);
                        genericESPObjects.Add(viableItemObject);
                    }
                }
            }
        }

        /// <summary>
        /// Scans for all players, ignoring the local player.
        /// </summary>
        private void ScanForPlayers()
        {
            GameObject[] allPlayers = Utils.Misc.GetPlayerGameObjects();
            foreach (GameObject player in allPlayers)
            {
                NetworkIdentity playerNetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
                if (!playerNetworkIdentity.isLocalPlayer)
                {
                    ESPPlayerObject playerObject = new ESPPlayerObject(player);
                    playerESPObjects.Add(playerObject);
                }
            }
        }

        /// <summary>
        /// Scans for locations of importance, which are (currently just) SCP-914, Elevators, and Pocket Dimension exits.
        /// </summary>
        private void ScanForLocations()
        {
            // SCP 914
            ESPGenericObject scp914Object = new ESPGenericObject("SCP 914", Color.white, GameObject.FindGameObjectWithTag("914_use"));
            genericESPObjects.Add(scp914Object);
            
            // Elevators
            Lift[] lifts = FindObjectsOfType<Lift>();
            for (int i = 0; i < lifts.Length; i++)
            {
                foreach (Lift.Elevator elevator in lifts[i].elevators)
                {
                    ESPGenericObject elevatorObject = new ESPGenericObject("Elevator", Color.white, elevator.door.gameObject);
                    genericESPObjects.Add(elevatorObject);
                }
            }

            // Pocket Dimension Exits
            foreach (PocketDimensionTeleport pdTeleport in FindObjectsOfType<PocketDimensionTeleport>())
            {
                if (pdTeleport.GetTeleportType() == PocketDimensionTeleport.PDTeleportType.Exit)
                {
                    ESPGenericObject exitTeleporterObject = new ESPGenericObject("Exit", Color.white, pdTeleport.gameObject);
                    genericESPObjects.Add(exitTeleporterObject);
                }
            }
        }
    }
}
