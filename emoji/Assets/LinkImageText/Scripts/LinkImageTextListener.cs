using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LinkImageTextListener : MonoBehaviour
{
    private LinkImageText m_txt;
    private string _initString;

    private StringBuilder _stringBuilder = new StringBuilder();
    private List<linkInfoClass> _linkInfoList = new List<linkInfoClass>();

    [SerializeField]
    private string leftSymbolInLinkMatchItem = "[";//匹配关键字默认左侧标识符号
    [SerializeField]
    private string rightSymbolInMatchItem = "]";//匹配关键字默认右侧标识符号

    //正则匹配关键字
    private static readonly Regex s_matchRegex = new Regex(@"\<link (.*?)\>", RegexOptions.IgnoreCase);

    private void Awake()
    {
        m_txt = this.gameObject.GetComponent<LinkImageText>();
        _initString = m_txt.text;
        m_txt.leftSymbol = leftSymbolInLinkMatchItem;
        m_txt.rightSymbol = rightSymbolInMatchItem;
        //处理为LinkImageText可以处理的字符串格式后，重新赋值，由LinkImageText添加点击响应
        try
        {
            m_txt.text = ParseString(_initString);
        }
        catch (Exception e)
        {
            // Debug.Log("[LinkImageTextListener] test process fail {0}", e.ToString());
        }
    }

    /// <summary>
    /// 将匹配出的link字符串转换为LinkImageText可以识别的字符串格式
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private string ParseString(string str)
    {
        _stringBuilder.Clear();
        _linkInfoList.Clear();
        int stringIndex = 0;
        int offset = 0;
        foreach (Match match in s_matchRegex.Matches(str))
        {
            offset = match.Index - stringIndex;
            _stringBuilder.Append(str.Substring(stringIndex, offset));
            stringIndex += match.Length + offset;
            _stringBuilder.Append(MatchStringProcess(match.Value));
        }
        _stringBuilder.Append(str.Substring(stringIndex, str.Length - stringIndex));
        return _stringBuilder.ToString();
    }

    private string MatchStringProcess(string match)
    {
        if (match.IndexOf("hyperlink") != -1)
        {
            linkInfoClass lc = new linkInfoClass();
            lc.type = "hyperlink";
            lc.index = _linkInfoList.Count + 1;
            string temp = Regex.Match(match, @"content=\S+").Value;
            lc.content = temp.Substring(8, temp.Length - 8);
            temp = Regex.Match(match, @"url=\S+").Value;
            lc.value = temp.Substring(4, temp.Length - 4 - 1);
            _linkInfoList.Add(lc);
            return "[-" + lc.index + "#" + lc.content + "]";
        }
        return "";
    }

    private void OnEnable()
    {
        m_txt.onHrefClick.AddListener(OnClickHrefText);
    }

    private void OnDisable()
    {
        m_txt.onHrefClick.RemoveListener(OnClickHrefText);
    }

    /// <summary>
    /// 监听LinkImageText的点击
    /// </summary>
    /// <param name="str"></param>
    /// <param name="id"></param>
    /// <param name="eventData"></param>
    private void OnClickHrefText(string str, int id, PointerEventData eventData)
    {
        if (id - 1 < _linkInfoList.Count)
        {
            if (_linkInfoList[id - 1].type == "hyperlink")
            {
                try
                {
                    // MSDKBridge.Instance.OpenWebview(_linkInfoList[id - 1].value);
                }
                catch (Exception e)
                {
                    // Debug.Log("[LinkImageTextListener] user msdk openwebview failed: {0}", e.ToString());
                }
            }
        }
    }

    class linkInfoClass
    {
        public int index;
        public string content;
        public string value;
        public string type;
    }
}
