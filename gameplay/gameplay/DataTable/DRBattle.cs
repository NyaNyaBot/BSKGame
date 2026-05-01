//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-05-01 20:08:42.490
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Game.Gameplay;
using GameFramework.Runtime;
using Game.Math;

namespace Game.Gameplay
{
    /// <summary>
    /// 战役配置表。
    /// </summary>
    public class DRBattle : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取战役Id。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取场景路径。
        /// </summary>
        public string BattleScenePath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public int Role1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public int Role2
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取生成RoleCell。
        /// </summary>
        public float3 Role1Cell
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float3 Role2Cell
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取生成EnemyRoleId。
        /// </summary>
        public int EnemyRole1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public int EnemyRole2
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取生成EnemyRoleCell。
        /// </summary>
        public float3 EnemyRole1Cell
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float3 EnemyRole2Cell
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
            BattleScenePath = columnStrings[index++];
            Role1 = int.Parse(columnStrings[index++]);
            Role2 = int.Parse(columnStrings[index++]);
            Role1Cell = DataTableExtension.Parsefloat3(columnStrings[index++]);
            Role2Cell = DataTableExtension.Parsefloat3(columnStrings[index++]);
            EnemyRole1 = int.Parse(columnStrings[index++]);
            EnemyRole2 = int.Parse(columnStrings[index++]);
            EnemyRole1Cell = DataTableExtension.Parsefloat3(columnStrings[index++]);
            EnemyRole2Cell = DataTableExtension.Parsefloat3(columnStrings[index++]);

            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    BattleScenePath = binaryReader.ReadString();
                    Role1 = binaryReader.Read7BitEncodedInt32();
                    Role2 = binaryReader.Read7BitEncodedInt32();
                    Role1Cell = binaryReader.Readfloat3();
                    Role2Cell = binaryReader.Readfloat3();
                    EnemyRole1 = binaryReader.Read7BitEncodedInt32();
                    EnemyRole2 = binaryReader.Read7BitEncodedInt32();
                    EnemyRole1Cell = binaryReader.Readfloat3();
                    EnemyRole2Cell = binaryReader.Readfloat3();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private KeyValuePair<int, int>[] m_Role = null;

        public int RoleCount
        {
            get
            {
                return m_Role.Length;
            }
        }

        public int GetRole(int id)
        {
            foreach (KeyValuePair<int, int> i in m_Role)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetRole with invalid id '{0}'.", id));
        }

        public int GetRoleAt(int index)
        {
            if (index < 0 || index >= m_Role.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetRoleAt with invalid index '{0}'.", index));
            }

            return m_Role[index].Value;
        }

        private KeyValuePair<int, int>[] m_EnemyRole = null;

        public int EnemyRoleCount
        {
            get
            {
                return m_EnemyRole.Length;
            }
        }

        public int GetEnemyRole(int id)
        {
            foreach (KeyValuePair<int, int> i in m_EnemyRole)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetEnemyRole with invalid id '{0}'.", id));
        }

        public int GetEnemyRoleAt(int index)
        {
            if (index < 0 || index >= m_EnemyRole.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetEnemyRoleAt with invalid index '{0}'.", index));
            }

            return m_EnemyRole[index].Value;
        }

        private void GeneratePropertyArray()
        {
            m_Role = new KeyValuePair<int, int>[]
            {
                new KeyValuePair<int, int>(1, Role1),
                new KeyValuePair<int, int>(2, Role2),
            };

            m_EnemyRole = new KeyValuePair<int, int>[]
            {
                new KeyValuePair<int, int>(1, EnemyRole1),
                new KeyValuePair<int, int>(2, EnemyRole2),
            };
        }
    }
}
