using Game.Core;

namespace Game.Client
{
    public class MenuArchitecture : Architecture<MenuArchitecture>
    {
        protected override void Init()
        {
            this.RegisterModel<MenuModel>(new MenuModel());
        }
    }
}
