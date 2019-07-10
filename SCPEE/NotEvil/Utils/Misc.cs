using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SCPEE.NotEvil.Utils
{
    /// <summary>
    /// Various miscellaneous classes/functions go here.
    /// </summary>
    public static class Misc
    {
        private static GUIStyle internalStyle;

        /// <summary>
        /// Grabs the GameObject representing the local player.
        /// </summary>
        /// <returns>null if it was not found (the player isn't ingame), otherwise the GameObject.</returns>
        public static GameObject GetLocalPlayerGameObject()
        {
            foreach (GameObject gameObject in GetPlayerGameObjects())
            {
                if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    return gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the GameObjects representing *all* of the players in the game.
        /// </summary>
        /// <returns>A list of type GameObject containing the GameObjects representing the players.</returns>
        public static GameObject[] GetPlayerGameObjects()
        {
            return GameObject.FindGameObjectsWithTag("Player");
        }

        /// <summary>
        /// Creates an incredibly simple Rect. Slated for removal.
        /// </summary>
        /// <param name="content">The string to be displayed in the rect.</param>
        /// <param name="anchor_x">Where horizontally to render the rect.</param>
        /// <param name="anchor_y">Where vertically to render the rect.</param>
        /// <returns></returns>
        public static Rect CreateRect(string content, int anchor_x, int anchor_y) // Using this until I write a GUI manager.
        {
            if (internalStyle == null)
            {
                internalStyle = new GUIStyle();
                internalStyle.normal.textColor = Color.cyan;
            }
            Rect newRect = GUILayoutUtility.GetRect(new GUIContent(content), internalStyle);
            newRect.height *= 2.0f;
            newRect.x = anchor_x;
            newRect.y = anchor_y;
            return newRect;
        }

        /// <summary>
        /// Specifically made for the player ESP. Attempts to associate
        /// the substring of a player's lowercase classname with a color value.
        /// If no such value exists for a given substring, a default color of Magenta is returned.
        /// </summary>
        /// <param name="LowercaseClassname">The lowercase substring to get the color for, EG: 'scp' -> red</param>
        /// <returns>The appropriate color from the classname, or magenta if a color could not be found.</returns>
        public static Color ColorFromClassname(string LowercaseClassname)
        {
            // I hate doing if branches like this, but my original idea of using a dictionary seemed
            // like a bigger pain from a usage standpoint than a programming one, so whatever.
            // TODO: I'll keep this until I figure up something better. Or until it starts nagging at my mind.
            if (LowercaseClassname.Contains("scp"))
                return Color.red;
            else if (LowercaseClassname.Contains("nine"))
                return Color.blue;
            else if (LowercaseClassname.Contains("chaos"))
                return Color.green;
            else if (LowercaseClassname.Contains("facility"))
                return Color.grey;
            else if (LowercaseClassname == "scientist")
                return Color.yellow;
            else if (LowercaseClassname.Contains("class"))
                return new Color(1.0f, 0.64f, 0.0f);
            else return Color.magenta;
        }
    }
}
