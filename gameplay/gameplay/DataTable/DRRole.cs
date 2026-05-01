//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-05-01 20:08:42.455
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
    /// 角色表。
    /// </summary>
    public class DRRole : DataRowBase
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
        /// 获取名称。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取属性Id。
        /// </summary>
        public int PropertyId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取模型相对路径。
        /// </summary>
        public string Model
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取队伍，默认怪物1，玩家2。
        /// </summary>
        public int Team
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
            Name = columnStrings[index++];
            PropertyId = int.Parse(columnStrings[index++]);
            Model = columnStrings[index++];
            Team = int.Parse(columnStrings[index++]);

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
                    Name = binaryReader.ReadString();
                    PropertyId = binaryReader.Read7BitEncodedInt32();
                    Model = binaryReader.ReadString();
                    Team = binaryReader.Read7BitEncodedInt32();
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
