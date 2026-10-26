using FishNet.Broadcast;

public struct NicknameBroadcast : IBroadcast
{
    public string Nickname;
}

public struct NicknameListBroadcast : IBroadcast
{
    public int[] ClientIds;
    public string[] Nicknames;
}
