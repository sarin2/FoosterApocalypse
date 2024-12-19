using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace _NM.Editor
{
    public class ProjectBuilder : ScriptableObject
    {
        [MenuItem("Build/Build_WIN64")]
        public static void Build_WIN64()
        {
            Build(BuildTarget.StandaloneWindows64, Array.Empty<string>());
        }
        
        [MenuItem("Build/CHRONICLE_BUILD")]
        public static void Build_CHRONICLE_WIN64()
        {
            Build(BuildTarget.StandaloneWindows64, new[] { "CHRONICLE_BUILD" });
        }
        
        [MenuItem("Build/Build_WIN64")]
        public static void Build(BuildTarget target, string[] defines)
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

            List<string> scenes = new ();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if(!scene.enabled) continue;
                scenes.Add(scene.path);
            }
            
            buildPlayerOptions.extraScriptingDefines = defines;
            buildPlayerOptions.scenes = scenes.ToArray();
            buildPlayerOptions.locationPathName = $"Builds/{target}/FoosterApocalypse.exe";
            buildPlayerOptions.target = target;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }
    }
}