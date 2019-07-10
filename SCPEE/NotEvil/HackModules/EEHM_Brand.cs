using UnityEngine;

namespace SCPEE.NotEvil.HackModules
{
    public class Brand : MonoBehaviour
    {
        private bool isEnabled = true;
        private string eeBrand = "SCP Exposed Establishment";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                isEnabled = !isEnabled;
            }
        }

        private void OnGUI()
        {
            if (isEnabled)
            {
                Rect brandRect = Utils.Misc.CreateRect(eeBrand, anchor_x: Screen.width - 190, anchor_y: 10);
                GUI.color = Color.red;
                GUI.Label(brandRect, eeBrand);
            }
        }
    }
}
