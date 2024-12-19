using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace _NM.Editor
{
    public static class Discord
    {
        // 빌드 완료된 폴더를 압축 후 디스코드로 다운로드 링크를 공유하기 위한 디스코드 Web Hook 주소
        private static readonly string discordWebhook =
            "https://discord.com/api/webhooks/1210865317119787008/0ZgwmVTTVEnyocFPkTgDCRK41xYi44YXBcpF47IJc3eJZdaFyIB1z_q-Y0R7lgZs_KOH";
        
        public static async UniTask SendDiscordMessage(string message, Action<bool> callback = null)
        {
            WWWForm form = new WWWForm();
            form.AddField("content", message);
            using (UnityWebRequest www = UnityWebRequest.Post(discordWebhook, form))
            {
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    callback?.Invoke(false);
                }
                else
                {
                    callback?.Invoke(true);
                }
            }
        }
    }
}
