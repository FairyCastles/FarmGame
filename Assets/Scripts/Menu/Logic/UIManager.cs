using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Farm.Audio;

namespace Farm.Menu
{
    public class UIManager : MonoBehaviour
    {
        private GameObject menuCanvas;

        public GameObject actionBarUI;
        public GameObject gameTimeUI;

        public Button pauseButton;
        public Button exitButton;
        public Button returnMenuButton;

        public Button startGameButton;
        public Button endGameButton;

        public GameObject pausePannel;
        public GameObject savePannel;
        public GameObject titlePannel;
        public Slider volumeSlider;

        #region Life Function

        private void Awake()
        {
            pauseButton.onClick.AddListener(TogglePausePannel);
            exitButton.onClick.AddListener(ExitGame);
            startGameButton.onClick.AddListener(OpenSavePannel);
            endGameButton.onClick.AddListener(ExitGame);
            returnMenuButton.onClick.AddListener(ReturnMenuCanvas);
            volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
        }

        private void Start()
        {
            menuCanvas = GameObject.FindWithTag("MenuCanvas");
            OpenTitlePannel();
        }

        #endregion

        #region Event Function

        private void OnAfterSceneLoaded()
        {
            menuCanvas.gameObject.SetActive(false);
            actionBarUI.SetActive(true);
            gameTimeUI.SetActive(true);
        }

        #endregion

        private void TogglePausePannel()
        {
            bool isOpen = pausePannel.activeInHierarchy;

            if (isOpen)
            {
                pausePannel.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                // 暂停游戏时，执行垃圾回收
                System.GC.Collect();
                pausePannel.SetActive(true);
                Time.timeScale = 0;
            }
        }

        private void ReturnMenuCanvas()
        {
            Time.timeScale = 1;
            StartCoroutine(BackToMenu());
        }

        private IEnumerator BackToMenu()
        {
            pausePannel.SetActive(false);
            EventHandler.CallEndGameEvent();
            // 等待一秒
            yield return new WaitForSeconds(1f);
            actionBarUI.SetActive(false);
            gameTimeUI.SetActive(false);
            menuCanvas.SetActive(true);
            // 回到主菜单页面，设置为标题界面
            OpenTitlePannel();
        }

        private void ExitGame()
        {
            Debug.Log("Exit Game");
            // TODO: 在退出游戏时候，是否要保存当前数据
            Application.Quit();
        }

        private void OpenTitlePannel()
        {
            titlePannel.SetActive(true);
            savePannel.SetActive(false);
        }

        private void OpenSavePannel()
        {
            savePannel.SetActive(true);
            titlePannel.SetActive(false);
        }
    }
}

