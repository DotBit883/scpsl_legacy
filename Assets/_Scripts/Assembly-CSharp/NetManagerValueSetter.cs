using UnityEngine;
using Mirror;

public class NetManagerValueSetter : MonoBehaviour
{
	private CustomNetworkManager singleton;

	private void Start()
	{
		singleton = NetworkManager.singleton.GetComponent<CustomNetworkManager>();
	}

	public void ChangeIP(string ip)
	{
		singleton.networkAddress = ip;
	}

	public void ChangePort(int port)
	{
		// singleton.transport.port = port;
	}

	public void JoinGame()
	{
		singleton.StartClient();
	}

	public void HostGame()
	{
		singleton.StartHost();
	}

	public void Disconnect()
	{
		singleton.StopHost();
	}
}
