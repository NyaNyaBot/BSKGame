//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-05-01 20:08:42.460
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
    /// 属性表。
    /// </summary>
    public class DRProperty : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取属性Id。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float Health
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float MaxHealth
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float Mana
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float MaxMana
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float Armor
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取。
        /// </summary>
        public float MaxArmor
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
            Health = float.Parse(columnStrings[index++]);
            MaxHealth = float.Parse(columnStrings[index++]);
            Mana = float.Parse(columnStrings[index++]);
            MaxMana = float.Parse(columnStrings[index++]);
            Armor = float.Parse(columnStrings[index++]);
            MaxArmor = float.Parse(columnStrings[index++]);

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
                    Health = binaryReader.ReadSingle();
                    MaxHealth = binaryReader.ReadSingle();
                    Mana = binaryReader.ReadSingle();
                    MaxMana = binaryReader.ReadSingle();
                    Armor = binaryReader.ReadSingle();
                    MaxArmor = binaryReader.ReadSingle();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
