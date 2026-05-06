using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// 主菜单界面。功能：开始游戏、设置、退出。
    /// 对应 design/ux/main-menu.md 规格。
    /// </summary>
    public class MainMenuForm : UGuiForm
    {
        [Header("Buttons")]
        [SerializeField] private Button _btnStartGame;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnQuit;

        [Header("Visuals")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _buttonGroup;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            _btnStartGame?.onClick.AddListener(OnStartGameClicked);
            _btnSettings?.onClick.AddListener(OnSettingsClicked);
            _btnQuit?.onClick.AddListener(OnQuitClicked);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (_titleText != null)
                _titleText.text = "Echo of Blades";
        }

        private void OnStartGameClicked()
        {
            PlayUISound(0);
            Log.Info("[MainMenu] Start Game clicked");
            // TODO: 切换到战斗/探索场景
            Close();
        }

        private void OnSettingsClicked()
        {
            PlayUISound(0);
            Log.Info("[MainMenu] Settings clicked");
            // TODO: 打开设置面板
        }

        private void OnQuitClicked()
        {
            PlayUISound(0);
            Log.Info("[MainMenu] Quit clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            _btnStartGame?.onClick.RemoveListener(OnStartGameClicked);
            _btnSettings?.onClick.RemoveListener(OnSettingsClicked);
            _btnQuit?.onClick.RemoveListener(OnQuitClicked);
            base.OnClose(isShutdown, userData);
        }
    }
}
