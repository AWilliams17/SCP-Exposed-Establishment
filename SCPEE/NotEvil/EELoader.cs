using UnityEngine;
using SCPEE.NotEvil.HackModules;

/***
static T GameObject GameObject.FindObject<T>() will iterate through all instantiated GameObjects and return Components of the specified type.
T GameObject.GetComponent<T>() (and its derivatives) will search for the Components of the given type in the GameObject you called this on.
*/

namespace SCPEE.NotEvil
{
    /// <summary>
    /// The 'loader' which creates the hack's GameObject, attaches all the hack 'modules' to it,
    /// and then attaches the hack's GameObject to the game.
    /// </summary>
    public class EELoader
    {
        private static GameObject _exposedEstablishment;

        private static void Init()
        {
            _exposedEstablishment = new GameObject();
            AttachHackModules();
            Object.DontDestroyOnLoad(_exposedEstablishment);
        }

        private static void AttachHackModules()
        {
            _exposedEstablishment.AddComponent<Brand>();
            _exposedEstablishment.AddComponent<PlayerScanner>();
            _exposedEstablishment.AddComponent<ESP>();
        }

        private static void Unload()
        {
            Object.Destroy(_exposedEstablishment);
        }
    }
}
