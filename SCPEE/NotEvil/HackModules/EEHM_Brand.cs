using UnityEngine;

namespace SCPEE.NotEvil.HackModules
{
    /// <summary>
    /// Does nothing at all other than displays "SCP Exposed Establishment"
    /// up in the corner of the screen.
    /// </summary>
    public class Brand : MonoBehaviour
    {
        private bool isEnabled = true;
        private const string eeBrand = "SCP Exposed Establishment";

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
