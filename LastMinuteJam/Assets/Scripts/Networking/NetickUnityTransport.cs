using Netick;
using Unity.Networking.Transport;
using UnityEngine;

public unsafe class NetickUnityTransport : NetworkTransport
{
    public struct NetickUnityTransportEndPoint : IEndPoint
    {
        public NetworkEndpoint EndPoint;
        string IEndPoint.IPAddress => EndPoint.Address.ToString();
        int IEndPoint.Port => EndPoint.Port;
        public NetickUnityTransportEndPoint(NetworkEndpoint networkEndpoint)
        {
            EndPoint = networkEndpoint;
        }
        public override string ToString()
        {
            return $"{EndPoint.Address}";
        }
    }
}