using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _NM.Editor
{
    public class AnimationCurveEditor : EditorWindow
    {
        [MenuItem("Tools/Curve Editor")]
        internal static void Init()
        {
            var window = (AnimationCurveEditor)GetWindow(typeof(AnimationCurveEditor), false, "Animation Curve Editor");
            window.position = new Rect(window.position.xMin + 100f, window.position.yMin + 100f, 400f, 400f);
        }
        
        private AnimationCurve reference = new (new (0, 0.5f), new (1, 0.5f));
        private AnimationCurve temp = new();
        private AnimationCurve result = new();
        private int shakeCount = 1;
        
        internal void OnGUI()
        {
            EditorGUILayout.LabelField("대상 Curve");
            reference = EditorGUILayout.CurveField(reference);
            
            EditorGUILayout.Space(10);

            shakeCount = EditorGUILayout.IntField("진동 횟수", shakeCount);
            
            if (GUILayout.Button("Generate"))
            {
                if (shakeCount <= 0) return;
                
                temp.ClearKeys();
                result.ClearKeys();
                
                var length = reference.keys.Max(e => e.time);

                for (int i = 0; i < shakeCount; i++)
                {
                    for (int j = 0; j < reference.length; j++)
                    {
                        if (i > 0 && j == 0) continue;
                        
                        var keyframe = reference.keys[j];
                        keyframe.time += (i * length);
                        keyframe.value -= 0.5f;
                        keyframe.value *= (shakeCount - i) / (float)shakeCount;
                        keyframe.value += 0.5f;
                        temp.AddKey(keyframe);
                    }
                }
                
                var tempLength = temp.keys.Max(e => e.time);
                
                tempLength += length;
                temp.AddKey(tempLength, 0.5f);
                
                for (int i = 0; i < temp.length; i++)
                {
                    var keyframe = temp.keys[i];
                    keyframe.time /= tempLength;
                    result.AddKey(keyframe);
                }

            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("결과");
            result = EditorGUILayout.CurveField(result);
        }
    }
}
