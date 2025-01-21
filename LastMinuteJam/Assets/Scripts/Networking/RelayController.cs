using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayController : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation;


            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = joinAllocation.ToRelayServerData("dtls");


            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (m_GameManager.isRunningMVP)
            {
                SceneManager.LoadScene("MVPGame");
            }
            else
            {
                SceneManager.LoadScene("Game");
            }
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
