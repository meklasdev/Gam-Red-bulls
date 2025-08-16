using UnityEngine;

// Minimalne atrapy typów z Unity Netcode, pozwalające kompilować menedżer multiplayer
namespace Unity.Netcode
{
    /// <summary>
    /// Podstawowa klasa bazowa dla zachowań sieciowych.
    /// </summary>
    public class NetworkBehaviour : MonoBehaviour { }

    /// <summary>
    /// Pojedynczy menedżer sieci – w prawdziwej grze odpowiada za komunikację.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; } = new NetworkManager();

        public void StartHost() { }
        public void StartClient() { }
        public void Shutdown() { }
    }
}
