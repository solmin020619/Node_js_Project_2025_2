using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft;
using Newtonsoft.Json;

public class AuthManager : MonoBehaviour
{
    // ���� URL �� PlayerPrefs Ű ��� ����
    private const string SERVER_URL = "http://localhost:4000";
    private const string ACESS_TOKEN_PREFS_KEY = "AccessToken";
    private const string REFRESH_TOKEN_PREFS_KEY = "RefreshToken";
    private const string TOKEN_EXPIRY_PREFS_KEY = "TokenExpiry";


    // ��ū �� ���� �ð� ���� ����
    private string accessToken;
    private string refreshToken;
    private DateTime tokenExpiryTime;

    // PlayerPrefs ���� ��ū ���� �ε�
    private void LoadTokenFromPrefs()
    {
        accessToken = PlayerPrefs.GetString(ACESS_TOKEN_PREFS_KEY, "");
        refreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_PREFS_KEY, "");
        long expiryTicks = Convert.ToInt64(PlayerPrefs.GetString(TOKEN_EXPIRY_PREFS_KEY, "0"));
        tokenExpiryTime = new DateTime(expiryTicks);
    }

    // PlayerPrefs ��ū ���� ����
    private void SaveTokenPrefs(string accessToken, string refreshToken, DateTime expiryTime)
    {
        PlayerPrefs.SetString(ACESS_TOKEN_PREFS_KEY, accessToken);
        PlayerPrefs.SetString(REFRESH_TOKEN_PREFS_KEY, refreshToken);
        PlayerPrefs.SetString(TOKEN_EXPIRY_PREFS_KEY, expiryTime.Ticks.ToString());

        this.accessToken = accessToken;
        this.refreshToken = refreshToken;
        this.tokenExpiryTime = expiryTime;
    }

    // ����� ��� �ڷ�ƾ
    public IEnumerator Register(string username, string password)
    {
        var user = new {username, password};
        var jsonData = JsonConvert.SerializeObject(user);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{SERVER_URL}/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registration Error : {www.error}");
            }
            else
            {
                Debug.Log("Registration successful");
            }
        }
    }


    public IEnumerator Login(string username, string password)
    {
        var user = new { username, password };
        var jsonData = JsonConvert.SerializeObject(user);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{SERVER_URL}/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registration Error : {www.error}");
            }
            else
            {
                var response = JsonConvert.DeserializeObject<LoginResponse>(www.downloadHandler.text);
                Debug.Log(response.accessToken);
                SaveTokenPrefs(response.accessToken, response.refreshToken,DateTime.UtcNow.AddMinutes(15));
                Debug.Log("Login Successful");
            }
        }
    }

    // �α��� ���� ������ ����
    [System.Serializable]
    public class LoginResponse
    {
        public string accessToken;
        public string refreshToken;
    }

    // ��ū ���� ���� ������ ����
    [System.Serializable]
    public class RefreshTokenResponse
    {
        public string accessToken;
    }

}

