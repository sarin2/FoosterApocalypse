using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace _NM.Editor
{
    public class AutoIncrementBuildNumberOnPreBuild : MonoBehaviour, IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            PlayerSettings.bundleVersion = IncrementBuildNumber(PlayerPrefs.GetString("VERSION", PlayerSettings.bundleVersion));
            PlayerPrefs.SetString("VERSION", PlayerSettings.bundleVersion);
            PlayerPrefs.Save();
        }
        
        private string IncrementBuildNumber(string buildNumber)
        {
            var split = buildNumber.Split('.');

            split[^1] = (int.Parse(split[^1]) + 1).ToString();

            return string.Join('.', split);
        }
    }
}
