using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Cysharp.Threading.Tasks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace _NM.Editor
{
    public class ShareToDiscordOnPostBuild : IPostprocessBuildWithReport
    {
        // 빌드 아카이브에 포함되지 않아야 할 Directory를 임시로 이동할 경로
        private readonly string temporaryDirectory = @"C:\Users\LAPTOP\Documents\temp";

        public int callbackOrder => 0;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            string userName = Environment.UserName;
            string userDomainName = Environment.UserDomainName;

            // 빌드머신 아니면 디스코드 알림 전송 X
            if (userName != "LAPTOP" || userDomainName != "DESKTOP-5K3NQ3G") return;
            
            Debug.Log($"Build Result: {report.summary.result}");
            var platform = report.summary.platform;
            
            if (report.summary.totalSize > 0)
            {
                var outputDirectory = Path.GetDirectoryName(report.summary.outputPath);
                if (string.IsNullOrEmpty(outputDirectory) == false)
                {
                    var parent = Directory.GetParent(outputDirectory);
                    if (parent == null) return;
                    var zipFileDirectory = $@"{parent.FullName}\{platform}_Archives";
                    var zipFileName = $@"{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.zip";

                    if (Directory.Exists(zipFileDirectory) == false)
                    {
                        Directory.CreateDirectory(zipFileDirectory);
                    }
                        
                    var zipFilePath = @$"{zipFileDirectory}\{zipFileName}";

                    List<(string original, string moved)> directories = new();

                    foreach (var directory in Directory.GetDirectories(outputDirectory, "*_DoNotShip"))
                    {
                        try
                        {
                            var directoryName = Directory.GetParent(directory);
                            if (directoryName == null) continue;
                            var newDirectory = @$"{temporaryDirectory}\{directoryName.Name}";
                            Directory.Move(directory, newDirectory);
                            directories.Add((directory, newDirectory));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                            
                    ZipFile.CreateFromDirectory(outputDirectory, zipFilePath);

                    foreach (var pair in directories)
                    {
                        Directory.Move(pair.moved, pair.original);
                    }

                    Discord.SendDiscordMessage($"[{platform}] 새로운 빌드 파일이 생성되었습니다!\nhttp://sweetsd.iptime.org:9000/Builds/{platform}_Archives/{zipFileName}").Forget();
                }
            }
            else
            {
                Discord.SendDiscordMessage($"[{platform}] 빌드에 실패했습니다. ㅠ_ㅠ").Forget();
            }
        }
    }
}
