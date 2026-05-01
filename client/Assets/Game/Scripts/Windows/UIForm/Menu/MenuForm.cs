
using Game.Core;
using Game.Gameplay;
using Game.UIFramework;
using GameFramework.Runtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    //不要删除，让hybrid获取引用
    public class TestSignal : ASignal
    {
        
    }
    public class MenuForm : UGuiForm, IController
    {
        //退出Button
        public GameObject QuitButton = null;
        //战役选择面板滚动列表
        public ScrollRect SelectBattleRect;
        //战役选择面板CanvasGroup
        public CanvasGroup SelectBattleCanvasGroup;
        //战斗选择面板战役预制体id
        public GameObject SelectBattleItemPrefab;
        //战斗选择面板
        //public GameObject SelectBattlePanel;
        
        //普通菜单面板
        public CanvasGroup NormalMenuPanelCanvasGroup;
        //确认战役按钮
        public CanvasGroup BattleConfirmCanvasGroup;
        
        //private ProcedureMenu m_ProcedureMenu = null;
        //private TempGameState m_GameState;
        private bool m_HasInstantiatedBattle = false;
        

        private void OnBattleSelect(DRBattle battle)
        {
            var menuModel = this.GetModel<MenuModel>();
            menuModel.SelectBattle = battle;
            BattleConfirmCanvasGroup.alpha = 1;
        }

        public void OnBattleSelectConfirm()
        {
            var menuModel = this.GetModel<MenuModel>();
            if (menuModel.SelectBattle == null)
            {
                Log.Warning("OnBattleSelectConfirm: no battle selected.");
                return;
            }

            GameStarter.Instance.RequestStartBattle(menuModel.SelectBattle);
        }
        
        private void EnterSelectBattle(bool enable)
        {
            SelectBattleCanvasGroup.alpha = enable ? 1 : 0;
            NormalMenuPanelCanvasGroup.alpha = enable ? 0 : 1;
        }
        
        
        public void OnEnterSelectBattleButtonClick()
        {
            EnterSelectBattle(true);
            if (false == m_HasInstantiatedBattle)
            {
                m_HasInstantiatedBattle = true;
                var menuModel = this.GetModel<MenuModel>();
                
                foreach (var availableBattle in menuModel.AvailableBattles)
                {
                    var battleItem = GameObject.Instantiate(SelectBattleItemPrefab, SelectBattleRect.content);
                    var text = battleItem.GetComponentInChildren<Text>();
                    if(text != null)
                        text.text = availableBattle.BattleScenePath;
                    var button = battleItem.GetComponentInChildren<Button>();
                    button.onClick.AddListener(()=>OnBattleSelect(availableBattle));
                }
            }
        }
        
        public void OnExitSelectBattleButtonClick()
        {
            EnterSelectBattle(false);
            BattleConfirmCanvasGroup.alpha = 0;
        }
        
        public void OnSettingButtonClick()
        {
            GameEntry.UI.OpenUIForm(UIFormId.SettingForm);
        }

        public void OnAboutButtonClick()
        {
            GameEntry.UI.OpenUIForm(UIFormId.AboutForm);
        }

        public void OnQuitButtonClick()
        {
            // GameEntry.UI.OpenDialog(new DialogParams()
            // {
            //     Mode = 2,
            //     Title = GameEntry.Localization.GetString("AskQuitGame.Title"),
            //     Message = GameEntry.Localization.GetString("AskQuitGame.Message"),
            //     OnClickConfirm = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            // });
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            //m_GameState = (TempGameState)userData;
            
            //@TODO：获取游戏状态等
            // m_ProcedureMenu = (ProcedureMenu)userData;
            // if (m_ProcedureMenu == null)
            // {
            //     Log.Warning("ProcedureMenu is invalid when open MenuForm.");
            //     return;
            // }

            QuitButton.SetActive(Application.platform != RuntimePlatform.IPhonePlayer);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            //m_ProcedureMenu = null;

            base.OnClose(isShutdown, userData);
        }

        public IArchitecture GetArchitecture()
        {
            return MenuArchitecture.Interface;
        }
    }
}
