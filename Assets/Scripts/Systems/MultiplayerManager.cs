using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Prosty menedżer trybu multiplayer: uruchamianie hosta, klienta i opuszczanie sesji.
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Awake()
    {
        // W docelowej implementacji NetworkManager jest komponentem w scenie
        networkManager = NetworkManager.Singleton;
    }

    /// <summary>
    /// Startuje grę jako host (serwer + klient).
    /// </summary>
    public void StartHost()
    {
        networkManager.StartHost();
    }

    /// <summary>
    /// Dołącza do istniejącej sesji jako klient.
    /// </summary>
    public void StartClient()
    {
        networkManager.StartClient();
    }

    /// <summary>
    /// Zamyka aktualne połączenie sieciowe.
    /// </summary>
    public void LeaveGame()
    {
        networkManager.Shutdown();
    }
}
