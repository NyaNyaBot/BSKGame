using System;
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;

namespace Game.Client
{
    public class MenuModel: AbstractModel
    {
        public readonly List<DRBattle> AvailableBattles = new List<DRBattle>();
        public DRBattle SelectBattle;
        protected override void OnInit()
        {
            //读取所有Battle

            SelectBattle = null;
            AvailableBattles.Clear();
            var dtBattle = GameEntry.DataTable.GetDataTable<DRBattle>();
            foreach (var drBattle in dtBattle.GetAllDataRows())
            {
                AvailableBattles.Add(drBattle);
            }

            // Pre-production：列表优先 Main，BattleStorm 老验证场景置后，便于从菜单做流程验证。
            AvailableBattles.Sort(CompareBattleForPreProdMenuOrder);
        }

        private static int CompareBattleForPreProdMenuOrder(DRBattle a, DRBattle b)
        {
            return PreProdBattleMenuRank(a).CompareTo(PreProdBattleMenuRank(b));
        }

        private static int PreProdBattleMenuRank(DRBattle b)
        {
            if (b?.BattleScenePath == null)
            {
                return 50;
            }

            string p = b.BattleScenePath;
            if (p.IndexOf("/Main.", StringComparison.OrdinalIgnoreCase) >= 0
                || p.EndsWith("/Main.unity", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (p.IndexOf("BattleStorm", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 20;
            }

            return 10;
        }

        protected override void OnDeinit()
        {
            SelectBattle = null;
            AvailableBattles.Clear();
            base.OnDeinit();
        }
    }
}