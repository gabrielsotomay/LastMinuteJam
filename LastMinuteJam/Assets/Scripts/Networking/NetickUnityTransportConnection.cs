using Netick;
using System;
using Unity.Networking.Transport;
using UnityEngine;

public unsafe class NetickUnityTransportConnection : TransportConnection
{
    public NetickUnityTransport Transport;
    public Unity.Networking.Transport.NetworkConnection Connection;

    public override IEndPoint EndPoint => Transport._driver.GetRemoteEndpoint(Connection).ToNetickEndPoint();
    public override int Mtu => MaxPayloadSize;

    public int MaxPayloadSize;

    public NetickUnityTransportConnection(NetickUnityTransport transport)
    {
        Transport = transport;
    }

    public unsafe override void Send(IntPtr ptr, int length)
    {
        if (!Connection.IsCreated)
            return;
        Transport._driver.BeginSend(NetworkPipeline.Null, Connection, out var networkWriter);
        networkWriter.WriteBytesUnsafe((byte*)ptr.ToPointer(), length);
        Transport._driver.EndSend(networkWriter);
    }
}