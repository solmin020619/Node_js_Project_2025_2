using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System;

public class GameAPI : MonoBehaviour
{
    private string baseUrl = "http://localhost:4000/api";       // Node.js ������ URL

    public IEnumerator RegisterPlayer(string playerName, string password)
    {
        var requsetData = new { name = playerName, password = password };
        string jsonData = JsonConvert.SerializeObject(requsetData);
        Debug.Log($"Registering player: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/register", "Post"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error registering player : {request.result}");
            }
            else
            {
                Debug.Log("Player registered successfully");
            }
        }
    }

    public IEnumerator LoginPlayer(string playerName, string password, Action<PlayerModel> onSuccess)
    {
        var requsetData = new { name = playerName, password = password };
        string jsonData = JsonConvert.SerializeObject(requsetData);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error login player : {request.result}");
            }
            else
            {
                // ������ ó���Ͽ� PlayerModel ����
                string responseBody = request.downloadHandler.text;

                try
                {
                    var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

                    // ���� ���信�� PlayerModel ����
                    PlayerModel playerMode = new PlayerModel(responseData["playerName"].ToString())
                    {
                        metal = Convert.ToInt32(responseData["metal"]),
                        crystal = Convert.ToInt32(responseData["crystal"]),
                        deuterium = Convert.ToInt32(responseData["deuterium"]),
                        planets = new List<PlanetModel>()
                    };
                    onSuccess?.Invoke(playerMode);
                    Debug.Log("Login successful");
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Error processing login responce : {ex.Message}");
                }
            }
        }
    }
}


