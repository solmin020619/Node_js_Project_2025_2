using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;

public class UnityToNode : MonoBehaviour
{
    public Button btnGetExample;
    public Button btnPostExample;
    public Button btnResDataExample;
    public string host;             // IP주소 (로컬에서 127.0.0.1)
    public int port;                // 포트 주소 (3000번으로 express 동작
    public string route;            // about 주소 

    public string postUrl;
    public string resUrl;
    public int id;
    public string data;
    private void Start()
    {
        btnGetExample.onClick.AddListener(() =>
        {
            var url = string.Format("{0}:{1}/{2}", host, port, route);
            
            Debug.Log(url);
            StartCoroutine(GetData(url, (raw) =>
            {
                var res = JsonConvert.DeserializeObject<Protocols.Packets.common>(raw);
                Debug.LogFormat("{0},{1}", res.cmd, res.messsage);

            }));
        });

        btnPostExample.onClick.AddListener(() =>
        {
            var url = string.Format("{0}:{1}/{2}", host, port, postUrl);
            Debug.Log(url);

            var req = new Protocols.Packets.req_data();
            req.cmd = 1000;
            req.id = id;
            req.data = data;
            var json = JsonConvert.SerializeObject(req);                                            // 클래스 -> JSON

            StartCoroutine(this.GetBasic(url, (raw) =>
            {
                Protocols.Packets.common res = JsonConvert.DeserializeObject<Protocols.Packets.common>(raw);
                Debug.LogFormat("{0},{1}", res.cmd, res.messsage);

            }));
        });
    }
    private IEnumerator GetData(string url, System.Action<string> callback)             // Get 요청하는 코루틴 함수
    {
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        Debug.Log("Get :" + webRequest.downloadHandler.text);
        if (webRequest.result == UnityWebRequest.Result.ConnectionError          // 결과 값이 접속 오류일떄
            || webRequest.result == UnityWebRequest.Result.ProtocolError)       // 프로토콜 오류 일떄
        {
            Debug.Log("네트워크 환경이 좋지 않아서 통신 불가");                 // 통신 안됨 예외처리
        }
        else
        {
            callback(webRequest.downloadHandler.text);                          // 통신완료 되고 해당 텍스트를 가져온다
        }
    }

    private IEnumerator PostData(string url, string json, System.Action<string> callback)            // Post 요청하는 코루틴 함수
    {
        var webRequest = new UnityWebRequest(url, "POST");                                           // 웹 요청 POST
        var bodyRaw = Encoding.UTF8.GetBytes(json);                                                  // 적렬화

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError                           // 결과 값이 접속 오류일떄
            || webRequest.result == UnityWebRequest.Result.ProtocolError)                             // 프로토콜 오류 일떄
        {
            Debug.Log("네트워크 환경이 좋지 않아서 통신 불가");                                   // 통신 안됨 예외처리
        }
        else
        {
            callback(webRequest.downloadHandler.text);                                            // 통신완료 되고 해당 텍스트를 가져온다
        }

        webRequest.Dispose();                                                                     // 웹 요청 후 메모리에서 삭제
    }

    private IEnumerator GetBasic(string url, System.Action<string> callback)
    {
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError          // 결과 값이 접속 오류일떄
            || webRequest.result == UnityWebRequest.Result.ProtocolError)       // 프로토콜 오류 일떄
        {
            Debug.Log("네트워크 환경이 좋지 않아서 통신 불가");                 // 통신 안됨 예외처리
        }
        else
        {
            callback(webRequest.downloadHandler.text);                          // 통신완료 되고 해당 텍스트를 가져온다
        }
    }
}
