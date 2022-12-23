using System;
using Unity.Collections;
using Unity.Netcode;

public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int KillCount;
    public bool IsReady;
    public bool IsSpecialSkin;
    public int Skin;
    public int SkinIndex => Skin;

    public LobbyPlayerState(ulong clientId, FixedString32Bytes playerName, int killCount, bool isReady)
    {
        ClientId = clientId;
        PlayerName = playerName;
        KillCount = killCount;
        IsReady = isReady;
        IsSpecialSkin = false;
        Skin = 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref KillCount);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref IsSpecialSkin);
        serializer.SerializeValue(ref Skin);
    }

    public bool Equals(LobbyPlayerState other)
    {
        return ClientId == other.ClientId &&
               PlayerName.Equals(other.PlayerName) &&
               KillCount == other.KillCount &&
               IsReady == other.IsReady;
    }
}