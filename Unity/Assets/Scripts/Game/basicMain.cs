using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class basicMain : MonoBehaviour
{
    public Button hello;
    public string host;             // IP�ּ� (���ÿ��� 127.0.0.1)
    public int port;                // ��Ʈ �ּ� (3000������ express ����
    public string route;            // about �ּ� 


    private IEnumerator GetBasic(string url, System.Action<string> callback)             // Get ��û�ϴ� �ڷ�ƾ �Լ�
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
