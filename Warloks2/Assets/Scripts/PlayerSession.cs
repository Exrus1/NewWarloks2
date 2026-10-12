using UnityEngine;

/// <summary>Local player nickname shared across menu and lobby.</summary>
public static class PlayerSession
{
    const string PrefKey = "WarloksNickname";
    public const ushort DefaultPort = 7770;

    public static string Nickname
    {
        get
        {
            if (!PlayerPrefs.HasKey(PrefKey))
            {
                string generated = "Игрок" + Random.Range(1, 9999);
                PlayerPrefs.SetString(PrefKey, generated);
                PlayerPrefs.Save();
            }
            return PlayerPrefs.GetString(PrefKey);
        }
        set
        {
            PlayerPrefs.SetString(PrefKey, value);
            PlayerPrefs.Save();
        }
    }
}
