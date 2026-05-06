using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// 暂停覆盖层。功能：继续、重新开始、返回主菜单。
    /// 对应 design/ux/pause-menu.md 规格。
    /// </summary>
    public class PauseOverlayForm : UGuiForm
    {
        [Header("Buttons")]
        [SerializeField] private Button _btnResume;
        [SerializeField] private Button _btnRestart;
        [SerializeField] private Button _btnMainMenu;

        [Header("Visuals")]
        [SerializeField] private Image _dimBackground;
        [SerializeField] private Text _pauseTitle;

        private bool _isPaused;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            _btnResume?.onClick.AddListener(OnResumeClicked);
            _btnRestart?.onClick.AddListener(OnRestartClicked);
            _btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            SetPaused(true);

            if (_pauseTitle != null)
                _pauseTitle.text = "PAUSED";
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            SetPaused(false);
            _btnResume?.onClick.RemoveListener(OnResumeClicked);
            _btnRestart?.onClick.RemoveListener(OnRestartClicked);
            _btnMainMenu?.onClick.RemoveListener(OnMainMenuClicked);
            base.OnClose(isShutdown, userData);
        }

        private void OnResumeClicked()
        {
            PlayUISound(0);
            Close();
        }

        private void OnRestartClicked()
        {
            PlayUISound(0);
            Log.Info("[PauseOverlay] Restart clicked");
            SetPaused(false);
            Close(true);
            // TODO: 重新加载当前战斗场景
        }

        private void OnMainMenuClicked()
        {
            PlayUISound(0);
            Log.Info("[PauseOverlay] Return to Main Menu clicked");
            SetPaused(false);
            Close(true);
            // TODO: 切换到主菜单场景
        }

        private void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
