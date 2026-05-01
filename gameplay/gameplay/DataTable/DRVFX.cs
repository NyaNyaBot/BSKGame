//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-05-01 20:08:42.467
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
    /// 特效配置表。
    /// </summary>
    public class DRVFX : DataRowBase
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
        /// 获取特效名。
        /// </summary>
        public string AssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取生命周期（秒）。
        /// </summary>
        public float Lifetime
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取特效销毁时机。
        /// </summary>
        public int DespawnType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取预加载数量（临时字段，未来删除）。
        /// </summary>
        public int PreloadCount
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
            AssetName = columnStrings[index++];
            index++;
            Lifetime = float.Parse(columnStrings[index++]);
            DespawnType = int.Parse(columnStrings[index++]);
            PreloadCount = int.Parse(columnStrings[index++]);

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
                    AssetName = binaryReader.ReadString();
                    Lifetime = binaryReader.ReadSingle();
                    DespawnType = binaryReader.Read7BitEncodedInt32();
                    PreloadCount = binaryReader.Read7BitEncodedInt32();
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
