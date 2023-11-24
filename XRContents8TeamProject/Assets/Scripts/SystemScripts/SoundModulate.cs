using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundModulate : MonoBehaviour
{
    public AudioMixer mixer;

    public void AudioController(float sliderVal)
    {
        mixer.SetFloat("Master", Mathf.Log10(sliderVal) * 20);
    }

    // ��ư �ϳ��� �Ҹ� ����
    // Audio�Է¹޴� ��(AudioListener)���� ������ ���̴� ��
    //public void ToggleAudioVolum()
    //{  
    //    AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
    //}
}
