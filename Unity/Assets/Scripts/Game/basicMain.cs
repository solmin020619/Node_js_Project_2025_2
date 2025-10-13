using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class basicMain : MonoBehaviour
{
    public Button hello;
    public string host;             // IP주소 (로컬에서 127.0.0.1)
    public int port;                // 포트 주소 (3000번으로 express 동작
    public string route;            // about 주소 


    private IEnumerator GetBasic(string url, System.Action<string> callback)             // Get 요청하는 코루틴 함수
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
    private void Start()
    {
        this.hello.onClick.AddListener(() => 
        {
            var url = string.Format("{0}:{1}/{2}" , host , port, route);
            Debug.Log(url);

            StartCoroutine(this.GetBasic(url, (raw) =>
            {
                Debug.LogFormat("{0}", raw);
            }));
        });
    }
}
