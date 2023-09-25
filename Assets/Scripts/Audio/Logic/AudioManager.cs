using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Farm.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Music Data")]
        public SoundDetailsList_SO soundDetailsData;
        public SceneSoundList_SO sceneSoundData;
        [Header("Audio Source")]
        public AudioSource ambientSource;
        public AudioSource gameSource;

        private Coroutine soundRoutine;

        [Header("Audio Mixer")]
        public AudioMixer audioMixer;

        [Header("Snapshots")]
        public AudioMixerSnapshot normalSnapShot;
        public AudioMixerSnapshot ambientSnapShot;
        public AudioMixerSnapshot muteSnapShot;
        public float musicTransitionSecond = 8f;


        public float MusicStartSecond => Random.Range(5f, 15f);

        #region Lift Function

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
            EventHandler.PlaySoundEvent += OnPlaySound;
            EventHandler.EndGameEvent += OnEndGame;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
            EventHandler.PlaySoundEvent -= OnPlaySound;
            EventHandler.EndGameEvent -= OnEndGame;
        }

        #endregion

        #region Event Function
        
        /// <summary>
        /// 场景加载后，获取当前场景的音效信息
        /// </summary>
        private void OnAfterSceneLoaded()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            SceneSoundItem sceneSound = sceneSoundData.GetSceneSoundItem(currentScene);
            if (sceneSound == null) return;

            SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSound.ambient);
            SoundDetails music = soundDetailsData.GetSoundDetails(sceneSound.music);

            if (soundRoutine != null)
                StopCoroutine(soundRoutine);
            soundRoutine = StartCoroutine(PlaySoundRoutine(music, ambient));
        }

        // FIXME: 目前音效系统有 BUG，声音很奇怪，需要修复
        private void OnPlaySound(SoundName soundName)
        {
            SoundDetails soundDetails = soundDetailsData.GetSoundDetails(soundName);
            if (soundDetails != null)
                EventHandler.CallInitSoundEffectEvent(soundDetails);
        }

        private void OnEndGame()
        {
            if (soundRoutine != null)
                StopCoroutine(soundRoutine);
            muteSnapShot.TransitionTo(1f);
        }

        #endregion

        private IEnumerator PlaySoundRoutine(SoundDetails music, SoundDetails ambient)
        {
            if (music != null && ambient != null)
            {
                PlayAmbientClip(ambient, 1f);
                yield return new WaitForSeconds(MusicStartSecond);
                PlayMusicClip(music, musicTransitionSecond);
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="soundDetails"></param>
        private void PlayMusicClip(SoundDetails soundDetails, float transitionTime)
        {
            audioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.soundVolume));
            gameSource.clip = soundDetails.soundClip;
            if (gameSource.isActiveAndEnabled)
                gameSource.Play();

            normalSnapShot.TransitionTo(transitionTime);
        }


        /// <summary>
        /// 播放环境音效
        /// </summary>
        /// <param name="soundDetails"></param>
        private void PlayAmbientClip(SoundDetails soundDetails, float transitionTime)
        {
            audioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.soundVolume));
            ambientSource.clip = soundDetails.soundClip;
            if (ambientSource.isActiveAndEnabled)
                ambientSource.Play();

            ambientSnapShot.TransitionTo(transitionTime);
        }


        /// <summary>
        /// 转换 [0, 1] -> [-80, 20]
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private float ConvertSoundVolume(float amount)
        {
            return amount * 100 - 80;
        }

        public void SetMasterVolume(float value)
        {
            audioMixer.SetFloat("MasterVolume", ConvertSoundVolume(value));
        }
    }
}