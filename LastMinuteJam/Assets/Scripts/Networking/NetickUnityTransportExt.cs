using Netick;
using Unity.Networking.Transport;
using UnityEngine;

public static class NetickUnityTransportExt
{
    public static NetickUnityTransportEndPoint ToNetickEndPoint(this NetworkEndpoint networkEndpoint) => new NetickUnityTransportEndPoint(networkEndpoint);
}
