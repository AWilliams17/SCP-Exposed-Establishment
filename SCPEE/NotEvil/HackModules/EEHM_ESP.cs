using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

namespace SCPEE.NotEvil.HackModules
{
    //internal class ScannedObject
    //{
    //    public string ScannedObjectLabel { get; private set; }
    //    public Rect ScannedObjectPosition { get; private set; }
    //    public Color ScannedObjectLabelColor { get; private set; }
    //
    //    public ScannedObject(string ObjectLabel, Vector3 ObjectScreenPoint, Color LabelColor)
    //    {
    //        ScannedObjectLabel = ObjectLabel;
    //        ScannedObjectPosition = CalculatePositionRect(ObjectScreenPoint);
    //        ScannedObjectLabelColor = LabelColor;
    //    }
    //
    //    private static Rect CalculatePositionRect(Vector3 ObjectScreenPoint)
    //    {
    //        Rect positionRect = new Rect
    //            (
    //                ObjectScreenPoint.x - 20f, Screen.height - ObjectScreenPoint.y - 20f,
    //                ObjectScreenPoint.x + 40f, Screen.height - ObjectScreenPoint.y + 50f
    //            );
    //        return positionRect;
    //    }
    //}
    internal class ScannedObject
    {
        public string ScannedObjectLabel { get; private set; }
        public GameObject ScannedGameObject { get; private set; }
        public Color ScannedObjectLabelColor { get; private set; }
        public int MinimumDistance { get; private set; }

        public ScannedObject(string ObjectLabel, GameObject @GameObject, Color LabelColor, int ObjectMinimumDistance)
        {
            ScannedObjectLabel = ObjectLabel;
            ScannedGameObject = @GameObject;
            ScannedObjectLabelColor = LabelColor;
            MinimumDistance = ObjectMinimumDistance;
        }
    }

    public class ESP : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<ScannedObject> scannedObjects = new List<ScannedObject>();

        //private void AddScannedObjectIfViable(Transform ScannedObjectTransform, string GUILabel, Color GUILabelColor)
        //{
        //    Camera mainCamera = Camera.main;
        //    Vector3 mainCameraPosition = mainCamera.transform.position;
        //    Vector3 scannedObjectPosition = ScannedObjectTransform.position;
        //    Vector3 scannedObjectScreenPoint = mainCamera.WorldToScreenPoint(scannedObjectPosition);
        //    
        //    if (!(Mathf.Abs(mainCameraPosition[2] - scannedObjectPosition[2]) > 300f) && scannedObjectScreenPoint.z > 0f)
        //    {
        //        ScannedObject scannedItem = new ScannedObject(GUILabel, scannedObjectScreenPoint, GUILabelColor);
        //        scannedObjects.Add(scannedItem);
        //    }
        //}

        private void Awake()
        {
            StartCoroutine("DoScans");
        }
        
        IEnumerator DoScans()
        {
            while (true)
            {
                localPlayer = Utils.Misc.GetLocalPlayerGameObject();
                scannedObjects.Clear();
                if (isEnabled && localPlayer != null)
                {
                    string[] itemsOfInterest = {
                        "card", "p90", "com15", "rifle", "usp", "logicier",
                        "grenade", "pistol", "scorpion", "mp7", "epsilon",
                        "fusion"
                    };

                    // Item pickups
                    foreach (Pickup itemPickup in FindObjectsOfType<Pickup>())
                    {
                        Inventory playerInventory = localPlayer.GetComponent<Inventory>();
                        string itemLabel = playerInventory.availableItems[itemPickup.info.itemId].label;

                        for (int i = 0; i < itemsOfInterest.Length; i++)
                        {
                            if (itemLabel.ToLower().Contains(itemsOfInterest[i]))
                            {
                                //AddScannedObjectIfViable(itemPickup.transform, itemLabel, Color.cyan);
                                ScannedObject viableItemObject = new ScannedObject(itemLabel, itemPickup.gameObject, Color.cyan, 100);
                                scannedObjects.Add(viableItemObject);
                            }
                        }
                    }

                    // SCP 914
                    ScannedObject scp914GameObject = new ScannedObject("SCP 914", GameObject.FindGameObjectWithTag("914_use"), Color.yellow, 150);
                    scannedObjects.Add(scp914GameObject);
                    //AddScannedObjectIfViable(scp914.transform, "SCP 914", Color.yellow);

                    // Elevators
                    Lift[] lifts = FindObjectsOfType<Lift>();
                    for (int i = 0; i < lifts.Length; i++)
                    {
                        foreach (Lift.Elevator elevator in lifts[i].elevators)
                        {
                            ScannedObject elevatorObject = new ScannedObject("Elevator", elevator.door.gameObject, Color.blue, 200);
                            scannedObjects.Add(elevatorObject);
                            //AddScannedObjectIfViable(elevator.door.transform, "Elevator", Color.blue);
                        }
                    }

                    // Pocket Dimension Exits
                    foreach (PocketDimensionTeleport pdTeleport in FindObjectsOfType<PocketDimensionTeleport>())
                    {
                        PocketDimensionTeleport.PDTeleportType teleporterType = pdTeleport.GetTeleportType();
                        if (teleporterType == PocketDimensionTeleport.PDTeleportType.Exit)
                        {
                            ScannedObject exitTeleporterObject = new ScannedObject("Exit", pdTeleport.gameObject, Color.white, 75);
                            scannedObjects.Add(exitTeleporterObject);
                            //AddScannedObjectIfViable(pdTeleport.transform, "Teleporter Out", Color.white);
                        }
                    }
                }

                yield return new WaitForSeconds(4);
            }
        }



        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad2))
                isEnabled = !isEnabled;

            //if (isEnabled)
            //{
            //    localPlayer = Utils.Misc.GetLocalPlayerGameObject();
            //    //if (localPlayer != null)
            //    //{
            //    //    ScanItems();
            //    //    ScanPlayers();
            //    //    ScanLocations();
            //    //}
            //}
        }

        private void OnGUI()
        {
            foreach (ScannedObject scannedObject in scannedObjects)
            {
                Camera mainCamera = Camera.main;
                Vector3 mainCameraPosition = mainCamera.transform.position;
                Vector3 scannedObjectPosition = scannedObject.ScannedGameObject.transform.position;
                Vector3 scannedObjectScreenPoint = mainCamera.WorldToScreenPoint(scannedObjectPosition);
                int scannedObjectDistanceFromPlayer = (int)Vector3.Distance(mainCameraPosition, scannedObjectPosition);

                if (!(Mathf.Abs(mainCameraPosition[2] - scannedObjectPosition[2]) > 300f) && scannedObjectScreenPoint.z > 0f && scannedObjectDistanceFromPlayer <= scannedObject.MinimumDistance)
                {
                    Rect positionRect = new Rect
                        (
                            scannedObjectScreenPoint.x - 20f, Screen.height - scannedObjectScreenPoint.y - 20f,
                            scannedObjectScreenPoint.x + 40f, Screen.height - scannedObjectScreenPoint.y + 50f
                        );
                    GUI.color = scannedObject.ScannedObjectLabelColor;
                    GUI.Label(positionRect, $"{scannedObject.ScannedObjectLabel} : {scannedObjectDistanceFromPlayer}");

                }
            }
        }
    }
}



