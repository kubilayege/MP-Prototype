using UnityEngine;

public abstract class BaseLobbyTransport : MonoBehaviour
{
    public abstract void Initialize();
    public abstract void CreateHost(string hostAddress);
    public abstract void ConnectTo(string hostAddress);
}