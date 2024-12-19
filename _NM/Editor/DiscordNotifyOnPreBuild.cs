using System;
using Cysharp.Threading.Tasks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace _NM.Editor
{
    public class DiscordNotifyOnPreBuild : MonoBehaviour, IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            string userName = Environment.UserName;
            string userDomainName = Environment.UserDomainName;

            // 빌드머신 아니면 디스코드 알림 전송 X
            if (userName != "LAPTOP" || userDomainName != "DESKTOP-5K3NQ3G") return;
            
            var platform = report.summary.platform;
            Discord.SendDiscordMessage($"[{platform}] 빌드 시작").Forget();
        }
    }
}
