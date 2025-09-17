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
    public string host;             // IP�ּ� (���ÿ��� 127.0.0.1)
    public int port;                // ��Ʈ �ּ� (3000������ express ����
    public string route;            // about �ּ� 

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
            var json = JsonConvert.SerializeObject(req);                                            // Ŭ���� -> JSON

            StartCoroutine(this.GetBasic(url, (raw) =>
            {
                Protocols.Packets.common res = JsonConvert.DeserializeObject<Protocols.Packets.common>(raw);
                Debug.LogFormat("{0},{1}", res.cmd, res.messsage);

            }));
        });
    }
    private IEnumerator GetData(string url, System.Action<string> callback)             // Get ��û�ϴ� �ڷ�ƾ �Լ�
    {
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        Debug.Log("Get :" + webRequest.downloadHandler.text);
        if (webRequest.result == UnityWebRequest.Result.ConnectionError          // ��� ���� ���� �����ϋ�
            || webRequest.result == UnityWebRequest.Result.ProtocolError)       // �������� ���� �ϋ�
        {
            Debug.Log("��Ʈ��ũ ȯ���� ���� �ʾƼ� ��� �Ұ�");                 // ��� �ȵ� ����ó��
        }
        else
        {
            callback(webRequest.downloadHandler.text);                          // ��ſϷ� �ǰ� �ش� �ؽ�Ʈ�� �����´�
        }
    }

    private IEnumerator PostData(string url, string json, System.Action<string> callback)            // Post ��û�ϴ� �ڷ�ƾ �Լ�
    {
        var webRequest = new UnityWebRequest(url, "POST");                                           // �� ��û POST
        var bodyRaw = Encoding.UTF8.GetBytes(json);                                                  // ����ȭ

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError                           // ��� ���� ���� �����ϋ�
            || webRequest.result == UnityWebRequest.Result.ProtocolError)                             // �������� ���� �ϋ�
        {
            Debug.Log("��Ʈ��ũ ȯ���� ���� �ʾƼ� ��� �Ұ�");                                   // ��� �ȵ� ����ó��
        }
        else
        {
            callback(webRequest.downloadHandler.text);                                            // ��ſϷ� �ǰ� �ش� �ؽ�Ʈ�� �����´�
        }

        webRequest.Dispose();                                                                     // �� ��û �� �޸𸮿��� ����
    }

    private IEnumerator GetBasic(string url, System.Action<string> callback)
    {
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError          // ��� ���� ���� �����ϋ�
            || webRequest.result == UnityWebRequest.Result.ProtocolError)       // �������� ���� �ϋ�
        {
            Debug.Log("��Ʈ��ũ ȯ���� ���� �ʾƼ� ��� �Ұ�");                 // ��� �ȵ� ����ó��
        }
        else
        {
            callback(webRequest.downloadHandler.text);                          // ��ſϷ� �ǰ� �ش� �ؽ�Ʈ�� �����´�
        }
    }
}
