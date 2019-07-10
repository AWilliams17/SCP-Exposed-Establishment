using UnityEngine;
using UnityEngine.Networking;

namespace SCPEE.NotEvil.Utils
{
    public static class Misc
    {
        private static GUIStyle internalStyle;

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

        public static GameObject[] GetPlayerGameObjects()
        {
            return GameObject.FindGameObjectsWithTag("Player");
        }

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
    }
}
