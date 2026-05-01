//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-05-01 20:08:42.473
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
    /// 技能表。
    /// </summary>
    public class DRSkill : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取唯一id。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取局内描述。
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取释放距离。
        /// </summary>
        public int Distance
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取伤害范围类型，1为distance， 2为正方形range。
        /// </summary>
        public int DamageRangeType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取伤害范围。
        /// </summary>
        public int DamageRangeNum
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取伤害量。
        /// </summary>
        public float DamageNum
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
            Description = columnStrings[index++];
            Distance = int.Parse(columnStrings[index++]);
            DamageRangeType = int.Parse(columnStrings[index++]);
            DamageRangeNum = int.Parse(columnStrings[index++]);
            DamageNum = float.Parse(columnStrings[index++]);

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
                    Description = binaryReader.ReadString();
                    Distance = binaryReader.Read7BitEncodedInt32();
                    DamageRangeType = binaryReader.Read7BitEncodedInt32();
                    DamageRangeNum = binaryReader.Read7BitEncodedInt32();
                    DamageNum = binaryReader.ReadSingle();
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
