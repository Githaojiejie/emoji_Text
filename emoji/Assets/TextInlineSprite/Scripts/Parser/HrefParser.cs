using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

namespace EmojiUI
{
	public class HrefParser : IParser
	{

		public int Hot { get; set; }
		public bool ParsetContent(InlineText text, StringBuilder textfiller, Match data,int matchindex)
		{
            string value = data.Value;
            if (!string.IsNullOrEmpty(value))
            {
                int index = value.IndexOf('#');
                int atlasId = 0;
                if (index != -1)
                {
                    string subId = value.Substring(1, index - 1);
                    if (int.TryParse(subId, out atlasId))
                    {
                        if (atlasId < 0)
                        {
                            textfiller.Append("<color=blue>");
                            int startIndex = textfiller.Length * 4;
                            textfiller.Append("[" + data.Groups[2].Value + "]");
                            int endIndex = textfiller.Length * 4 - 1;
                            textfiller.Append("</color>");


                            HrefInfo hrefInfo = new HrefInfo();
                            hrefInfo.Id = Mathf.Abs(atlasId);
                            hrefInfo.StartIndex = startIndex;// 超链接里的文本起始顶点索引
                            hrefInfo.EndIndex = endIndex;
                            hrefInfo.Name = data.Groups[2].Value;
                            text._listHrefInfos.Add(hrefInfo);
                        }
                    }
                }
                
                return true;
            }

            return false;
        }
    }
}
