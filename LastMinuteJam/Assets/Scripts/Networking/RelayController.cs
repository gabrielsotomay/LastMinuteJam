using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using StinkySteak.NShooter.Netick.Transport;
public class RelayController : MonoBehaviour
{
    public static RelayController Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            NetickUnityTransport.Allocation = allocation;
            Debug.Log(joinCode);

            NetworkingController.Instance.StartHost();

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


            NetickUnityTransport.JoinAllocation = joinAllocation;


            NetworkingController.Instance.StartClient();
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            /*
            if (m_GameManager.isRunningMVP)
            {
                SceneManager.LoadScene("MVPGame");
            }
            else
            {
                SceneManager.LoadScene("Game");
            }
            NetworkManager.Singleton.StartClient();
            */
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
            

}
