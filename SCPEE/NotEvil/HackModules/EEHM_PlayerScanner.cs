using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace SCPEE.NotEvil.HackModules
{
    public class PlayerScanner : NetworkBehaviour
    {
        private int aliveMTF = 0;
        private int aliveCI = 0;
        private int aliveGuards = 0;
        private int aliveDBoys = 0;
        private int aliveScientists = 0;
        private int aliveSCP = 0;
        private string scanResultString = "";
        private GameObject localPlayer = null;
        private bool isEnabled = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
                isEnabled = !isEnabled;

            if (isEnabled)
            {
                localPlayer = Utils.Misc.GetLocalPlayerGameObject();
                if (localPlayer == null)
                    scanResultString = "PlayerScanner is on, but you are not ingame.";
                else
                    ScanRemainingPlayers();
            }
        }

        private void OnGUI()
        {
            if (isEnabled)
            {
                Rect scanLabelRect = Utils.Misc.CreateRect(scanResultString, anchor_x: 0, anchor_y: 20);
                GUI.Label(scanLabelRect, scanResultString);
            }
        }

        private void ClearScanResults()
        {
            aliveMTF = aliveCI = aliveGuards = aliveDBoys = aliveScientists = aliveSCP = 0;
            scanResultString = "";
        }

        private void ScanRemainingPlayers()
        {
            ClearScanResults();
            GameObject[] allPlayers = Utils.Misc.GetPlayerGameObjects();
            foreach (GameObject player in allPlayers)
            {
                if (!gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    int playerClass = player.GetComponent<CharacterClassManager>().curClass;
                    // TODO: Better way. This works for now though.
                    if ((new[] { 4, 11, 12, 13 }).Contains(playerClass))
                        aliveMTF += 1;
                    else if ((new[] { 5, 10, 7, 9, 3, 0, 17, 16 }).Contains(playerClass))
                        aliveSCP += 1;
                    else if (playerClass == ClassTypes.ChaosInsurgency)
                        aliveCI += 1;
                    else if (playerClass == ClassTypes.ClassD)
                        aliveDBoys += 1;
                    else if (playerClass == ClassTypes.FacilityGuard)
                        aliveGuards += 1;
                    else if (playerClass == ClassTypes.Scientist)
                        aliveScientists += 1;
                }
            }
            scanResultString = 
                $"-Player Scan Results-\n" +
                $"  MTF: {aliveMTF}\n" +
                $"  CI: {aliveCI}\n" +
                $"  Guards: {aliveGuards}\n" +
                $"  DBoys: {aliveDBoys}\n" +
                $"  Scientists: {aliveScientists}\n" +
                $"  SCP: {aliveSCP}";
        }
    }
}
