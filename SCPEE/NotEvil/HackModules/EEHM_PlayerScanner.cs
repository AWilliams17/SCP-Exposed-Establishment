using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SCPEE.NotEvil.HackModules
{
    /// <summary>
    /// The Exposed Establishment PlayerScanner.
    /// It tells the user how many players of each character class are currently alive.
    /// </summary>
    public class PlayerScanner : NetworkBehaviour
    {
        private bool isEnabled = false;
        private GameObject localPlayer = null;
        private List<GameObject> players = new List<GameObject>();
        int aliveMTF, aliveCI, aliveGuards, aliveDBoys, aliveScientists, aliveSCP;

        private void Awake()
        {
            StartCoroutine("DoScans");
        }

        IEnumerator DoScans()
        {
            while (true)
            {
                players.Clear();
                localPlayer = Utils.Misc.GetLocalPlayerGameObject();

                if (isEnabled && localPlayer != null)
                {
                    ScanRemainingPlayers();
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
                isEnabled = !isEnabled;
        }

        private void OnGUI()
        {
            if (isEnabled)
            {
                aliveMTF = aliveCI = aliveGuards = aliveDBoys = aliveScientists = aliveSCP = 0;
                foreach (GameObject player in players)
                {
                    NicknameSync playerNicknameSync = player.transform.GetComponent<NicknameSync>();
                    CharacterClassManager playerClassManager = playerNicknameSync.GetComponent<CharacterClassManager>();
                    int playerClass = playerClassManager.curClass;
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

                    string scanResultString =
                        $"-Player Scan Results-\n" +
                        $"  MTF: {aliveMTF}\n" +
                        $"  CI: {aliveCI}\n" +
                        $"  Guards: {aliveGuards}\n" +
                        $"  DBoys: {aliveDBoys}\n" +
                        $"  Scientists: {aliveScientists}\n" +
                        $"  SCP: {aliveSCP}";

                    Rect scanLabelRect = Utils.Misc.CreateRect(scanResultString, anchor_x: 0, anchor_y: 20);
                    GUI.color = Color.red;
                    GUI.Label(scanLabelRect, scanResultString);
                }
            }
        }

        /// <summary>
        /// Grabs all GameObjects representing players - adds them to the players list for iteration.
        /// </summary>
        private void ScanRemainingPlayers()
        {
            GameObject[] allPlayers = Utils.Misc.GetPlayerGameObjects();
            foreach (GameObject player in allPlayers)
            {
                NetworkIdentity playerNetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
                if (!playerNetworkIdentity.isLocalPlayer)
                {
                    players.Add(player);
                }
            }
        }
    }
}
