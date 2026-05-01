
namespace Game.Client
{
    /// <summary>
    /// 界面编号。
    /// </summary>
    public enum UIFormId : byte
    {
        Undefined = 0,

        /// <summary>
        /// 弹出框。
        /// </summary>
        DialogForm = 1,

        /// <summary>
        /// 主菜单。
        /// </summary>
        MenuForm = 100,

        /// <summary>
        /// 设置。
        /// </summary>
        SettingForm = 101,

        /// <summary>
        /// 关于。
        /// </summary>
        AboutForm = 102,
        
        
        /// <summary>
        /// 战斗主界面
        /// </summary>
        BattleMainForm = 201,

        /// <summary>
        /// 战斗 HUD（ADR-0010）。
        /// </summary>
        BattleHudForm = 202,
    }
}
