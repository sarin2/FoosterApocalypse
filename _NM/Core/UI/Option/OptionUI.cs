using _NM.Core.Input;
using _NM.Core.Manager;
using _NM.Core.UI.UICanvas;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Option
{
    public class OptionUI : CanvasPage
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Button backButton;

        protected override void Start()
        {
            base.Start();
            masterVolumeSlider.value = SoundManager.I.GetSettingVolume(SoundManager.VolumeType.Master) * masterVolumeSlider.maxValue;
            bgmVolumeSlider.value = SoundManager.I.GetSettingVolume(SoundManager.VolumeType.Bgm) * masterVolumeSlider.maxValue;
            sfxVolumeSlider.value = SoundManager.I.GetSettingVolume(SoundManager.VolumeType.Sfx) * masterVolumeSlider.maxValue;
            mouseSensitivitySlider.value = InputProvider.MouseSensitivity.Load() * 10;

            masterVolumeSlider.onValueChanged.AddListener(val =>
                SoundManager.I.SetSettingVolume(SoundManager.VolumeType.Master, val / masterVolumeSlider.maxValue)
                    .Forget());
            bgmVolumeSlider.onValueChanged.AddListener(val =>
                SoundManager.I.SetSettingVolume(SoundManager.VolumeType.Bgm, val / masterVolumeSlider.maxValue)
                    .Forget());
            sfxVolumeSlider.onValueChanged.AddListener(val =>
                SoundManager.I.SetSettingVolume(SoundManager.VolumeType.Sfx, val / masterVolumeSlider.maxValue)
                    .Forget());
            
            mouseSensitivitySlider.onValueChanged.AddListener(val =>
            {
                InputProvider.MouseSensitivity.Save(val / 10);
            });
            
            backButton.onClick.AddListener(() => Close().Forget());
        }
    }
}
