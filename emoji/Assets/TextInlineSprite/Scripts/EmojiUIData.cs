using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmojiUI
{
	#region sprites
	[System.Serializable]
	public class SpriteInfo
	{
		/// <summary>
		/// 精灵
		/// </summary>
		public Sprite sprite;
		/// <summary>
		/// 标签
		/// </summary>
		public string tag;
		/// <summary>
		/// uv
		/// </summary>
		public Vector2[] uv;
	}

	[System.Serializable]
	public class SpriteInfoGroup
	{
		public string tag = "";

		public float width = 1.0f;
		public float size = 100.0f;

		public float x;
		public float y;

		public List<SpriteInfo> spritegroups = new List<SpriteInfo>();
	}

    [System.Serializable]
    /// <summary>
	/// 超链接信息类
	/// </summary>
	public class HrefInfo
    {
        /// <summary>
        /// 超链接id
        /// </summary>
        public int Id;
        /// <summary>
        /// 顶点开始索引值
        /// </summary>
        public int StartIndex;
        /// <summary>
        /// 顶点结束索引值
        /// </summary>
        public int EndIndex;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 碰撞盒范围
        /// </summary>
        public readonly List<Rect> Boxes = new List<Rect>();
    }
    #endregion
}

