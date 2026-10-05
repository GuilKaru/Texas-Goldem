using UnityEngine;

namespace SimplePoker.Initializer
{

    /// <summary>
    /// Class responsible for initializing essential game objects before scene load.
    /// </summary>
    public class Initializer
    {
        /// <summary>
        /// Initializes essential game objects before scene load.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            GameObject essentials = Object.Instantiate(Resources.Load("Essentials")) as GameObject;
            Object.DontDestroyOnLoad(essentials);
        }
    }
}