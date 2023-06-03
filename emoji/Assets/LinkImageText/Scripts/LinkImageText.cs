using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EmojiUI;

 //public static string ReplaceSpace(string inStr)
 //   {
 //       return inStr.Replace(" ", no_breaking_space);
 //   }
 //   public static readonly string no_breaking_space = "\u00A0";


/// <summary>
/// 文本控件，支持超链接、图片
/// </summary>
[AddComponentMenu("UI/LinkImageText", 10)]
[ExecuteAlways]
public class LinkImageText : Text, IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler
{
    /// <summary>
    /// 是否3d（3d下不需要屏幕适配）
    /// </summary>
    public bool Is3dText = false;

    /// <summary>
    /// 图片缩放比例
    /// </summary>
    public float imageScle = 1;

    /// <summary>
    /// 图文大小比率
    /// </summary>
    public float ImageTextRate = 1;

    /// <summary>
    /// 解析完最终的文本
    /// </summary>
    private string m_OutputText = "";

    /// <summary>
    /// 图片池
    /// </summary>
    protected readonly List<Image> m_ImagesPool = new List<Image>();
    public List<Image> ImagesPool
    {
        get { return m_ImagesPool; }
    }
    /// <summary>
    /// 图片的占位符的索引
    /// </summary>
    private readonly List<int> m_ImagesCharIndex = new List<int>();

    /// <summary>
    /// 超链接信息列表
    /// </summary>
    private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

    /// <summary>
    /// 文本构造器
    /// </summary>
    protected static readonly StringBuilder s_TextBuilder = new StringBuilder();

    [Serializable]
    public class HrefClickEvent : UnityEvent<string, int, PointerEventData> { }

    [SerializeField]
    private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

    [Serializable]
    public class TextClickEvent : UnityEvent<GameObject> { }

    [SerializeField]
    private TextClickEvent m_OnTextClick = new TextClickEvent();

    /// <summary>
    /// 超链接点击事件
    /// </summary>
    public HrefClickEvent onHrefClick
    {
        get { return m_OnHrefClick; }
        set { m_OnHrefClick = value; }
    }

    /// <summary>
    /// 正则取出所需要的属性
    /// </summary>
    private static readonly Regex s_ImageRegex =
        new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

    private static readonly Regex _inputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#((\[神工\])?.+?)\]", RegexOptions.Singleline);

    /// <summary>
    /// 加载精灵图片方法
    /// </summary>
    public Func<string, List<Sprite>> funLoadSprite;

    private float pressDuration = 0;

    private float screenRatio = 1;

    protected override void Awake()
    {
        base.Start();

        SetManager();

        Font.textureRebuilt += delegate (Font font1)
        {
            if (this.font == font1)
            {
                //Debug.LogError("!!!!!Font.textureRebuilt");
                this.FontTextureChanged();
            }
        };
        
        screenRatio = GetScreenRatio();
    }
    
    public static Vector2 projectScreenReference = new Vector2(1920, 1080);

    private float GetScreenRatio()
    {
        if (Is3dText)
        {
            return 1;
        }

        if ((float)Screen.height / Screen.width > projectScreenReference.y / projectScreenReference.x)//比例小于16:9
        {
            return projectScreenReference.x / Screen.width;
        }
        else
        {
            return  projectScreenReference.y / Screen.height;
        }
    }
    private void SetManager()
    {
        funLoadSprite = LinkImageManager.LoadSprites;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        funLoadSprite = null;
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
#if UNITY_EDITOR
        screenRatio = GetScreenRatio();
#endif
    }

    public override string text
    {
        get { return m_Text; }
        set
        {
            m_OutputText = GetOutputText(value);
            m_Text = m_OutputText;
            UpdateQuadImage();
        }

    }

    protected void UpdateQuadImage()
    {
#if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
        {
            return;
        }
#endif
        m_ImagesCharIndex.Clear();
        foreach (Match match in s_ImageRegex.Matches(m_OutputText))
        {
            m_ImagesCharIndex.Add(match.Index);

            m_ImagesPool.RemoveAll(image => image == null);
            if (m_ImagesPool.Count == 0)
            {
                GetComponentsInChildren<Image>(m_ImagesPool);
            }
            if (m_ImagesCharIndex.Count > m_ImagesPool.Count)
            {
                var resources = new DefaultControls.Resources();
                var go = DefaultControls.CreateImage(resources);
                go.layer = gameObject.layer;
                var rt = go.transform as RectTransform;
                if (rt)
                {
                    rt.SetParent(rectTransform);

                    //int align = (int)this.alignment;//根据text的对齐方式进行适配
                    rt.pivot = rectTransform.pivot;
                    rt.anchorMin = rectTransform.anchorMin;  //new Vector2(align%3*0.5f, (1-align/3*0.5f));
                    rt.anchorMax = rectTransform.anchorMax;
                    rt.localPosition = Vector3.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }
                m_ImagesPool.Add(go.GetComponent<Image>());
            }

            var spriteName = match.Groups[1].Value;
            var size = float.Parse(match.Groups[2].Value);
            var img = m_ImagesPool[m_ImagesCharIndex.Count - 1];
            if (img.sprite == null || img.sprite.name != spriteName)
            {
                bool isloaded = false;
                if (funLoadSprite != null)
                {
                    List<Sprite> spriteList = funLoadSprite(spriteName);
                    if (spriteList.Count <= 0)
                    {
                        isloaded = false;
                    }
                    else
                    {
                        isloaded = true;
                        if (spriteList.Count <= 1)
                        {
                            UISpriteAnimation ani = img.GetComponent<UISpriteAnimation>();
                            if (ani != null)
                            {
                                ani.Stop();
                                ani.enabled = false;
                            }
                            img.sprite = spriteList[0];
                        }
                        else
                        {
                            img.sprite = spriteList[0];
                            UISpriteAnimation ani = img.GetComponent<UISpriteAnimation>();
                            if (ani == null)
                            {
                                ani = img.gameObject.AddComponent<UISpriteAnimation>();
                            }
                            ani.Sprites = spriteList;
                            ani.enabled = true;
                        }
                    }
                    
                }

                if (!isloaded)
                {
                    if (!isloaded)
                {
                    img.color = Color.clear;

                    if (spriteName.Contains("http"))
                    {
                        // var imageForUrl = img.gameObject.AddComponent<ImageForUrl>();
                        // imageForUrl.SetUrl(spriteName);
                        img.rectTransform.pivot = rectTransform.pivot;
                        img.rectTransform.anchorMin =
                            new Vector2(0.5f, 0.5f); //new Vector2(align%3*0.5f, (1-align/3*0.5f));
                        img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        img.rectTransform.localRotation = Quaternion.identity;
                        img.rectTransform.localScale = Vector3.one;

                        Debug.LogError($"index : {match.Index}");
                        // img.rectTransform.localPosition =
                            // this.CalculateCharPos(this, match.Index, GetComponentInParent<Canvas>());

                        // Debug.LogError($"localPosition : {this.CalculateCharPos(this, 100000 , GetComponentInParent<Canvas>())}");

                        img.color = Color.white;
                    }
                    else
                    {
                        // ResLoad.AsyncLoadAsset(spriteName, typeof(Sprite), null,
                        //     delegate(AsyncLoadResult oAsyncResult, string strCurAssetName)
                        //     {
                        //         if (oAsyncResult == null || string.IsNullOrEmpty(strCurAssetName) ||
                        //             oAsyncResult.m_oAssetObject == null) return;
                        //         if (MyUtils.UnityObjectIsNull(img))
                        //         {
                        //             return;
                        //         }
                        //
                        //         Sprite sprite = oAsyncResult.m_oAssetObject as Sprite;
                        //         img.sprite = sprite;
                        //         img.color = Color.white;
                        //     });
                    }
                }
                }
                
//                 img.sprite = funLoadSprite != null ? funLoadSprite(spriteName) :
//                     Resources.Load<Sprite>(spriteName);
            }
            img.rectTransform.sizeDelta = new Vector2(size, size);
            img.enabled = true;
        }

        for (var i = m_ImagesCharIndex.Count; i < m_ImagesPool.Count; i++)
        {
            if (m_ImagesPool[i])
            {
                m_ImagesPool[i].enabled = false;
            }
        }
    }
    
     /// <summary>
    /// 计算第x个字符，在text组件上的position
    /// </summary>
    /// <param name="targetText">text组件</param>
    /// <param name="targetCharIndex">字符index，从0开始</param>
    /// <param name="targetCanvas"></param>
    /// <returns></returns>
    public Vector3 CalculateCharPos(Text targetText, int targetCharIndex, Canvas targetCanvas = null)
    {
        // if (targetCanvas == null) targetCanvas = GameObject.Find("PatchUIManager").GetComponent<Canvas>();
        if (targetCanvas == null) targetCanvas = GetComponentInParent<Canvas>();
        string calculateStr = targetText.text;

        TextGenerator textGen = new TextGenerator(calculateStr.Length);
        Vector2 extents = targetText.GetComponent<RectTransform>().rect.size;
        textGen.Populate(calculateStr, targetText.GetGenerationSettings(extents));

        int indexOfTextQuad = targetCharIndex * 4;
        Vector3 charPos1 = Vector3.zero;
        Vector3 charPos2 = Vector3.zero;
        Vector3 charPos3 = Vector3.zero;
        Vector3 charPos4 = Vector3.zero;
        if (indexOfTextQuad < textGen.vertexCount)
            charPos1 = textGen.verts[indexOfTextQuad].position / targetCanvas.scaleFactor; //左上角顶点
        if (indexOfTextQuad + 1 < textGen.vertexCount)
            charPos2 = textGen.verts[indexOfTextQuad + 1].position / targetCanvas.scaleFactor; //右上角顶点
        if (indexOfTextQuad < textGen.vertexCount)
            charPos3 = textGen.verts[indexOfTextQuad + 2].position / targetCanvas.scaleFactor; //右下角顶点
        if (indexOfTextQuad < textGen.vertexCount)
            charPos4 = textGen.verts[indexOfTextQuad + 3].position / targetCanvas.scaleFactor; //左下角顶点


        if (indexOfTextQuad < textGen.vertexCount)
        {
            Vector3 avgPos = (charPos1 + charPos2 + charPos3 + charPos4) / 4f;
            var offset = avgPos + emojiOffset;
            Debug.LogError($"offset : {offset}");
            return offset;
        }

        Debug.LogError($"不应该走到这里吧 : {charPos4 + emojiOffset}");
        return charPos4 + emojiOffset;
    }

    private Vector3 emojiOffset = new Vector3(-5, 0, 0);

    private UIVertex vert = new UIVertex();
    private Vector2 tmpVector2 = Vector2.zero;
    private Vector3 tmpPos = Vector3.zero;
    private Vector2 tmpSize = Vector2.zero;

    private UICharInfo charInfo;

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        //var orignText = m_Text;
        //m_Text = m_OutputText;
        base.OnPopulateMesh(toFill);
        //m_Text = orignText;

        int charIndex = 0;
        RectTransform rt = null;
        float deltaX = 0;
        float deltaY = 0;
        int count = m_ImagesCharIndex.Count;
        for (var i = 0; i < count ; i++)
        {
            charIndex = m_ImagesCharIndex[i];
            rt = m_ImagesPool[i].rectTransform;
            tmpSize = rt.sizeDelta;
            if (charIndex < cachedTextGenerator.characterCount)
            {
                //toFill.PopulateUIVertex(ref vert, endIndex);
                charInfo = cachedTextGenerator.characters[charIndex];
                //Debug.LogError("~~~~~~~~~~ {0}  {1}  {2}  {3}", charInfo.charWidth, charInfo.cursorPos, charIndex, screenRatio);
                deltaX = tmpSize.x * rt.pivot.x;
                deltaY = tmpSize.y * (rt.pivot.y - 0.15f);//减去0.15是为了居中
                tmpVector2.x = charInfo.cursorPos.x * screenRatio + deltaX;
                tmpVector2.y = charInfo.cursorPos.y * screenRatio + deltaY / ImageTextRate;
                rt.anchoredPosition = tmpVector2;
            }
        }

    }

    //处理超链接包围框
    protected virtual void SetHrefInfoBoxes()
    {
        int count = m_HrefInfos.Count;
        HrefInfo hrefInfo;
        Vector2 first = Vector2.zero;
        Vector2 last = Vector2.zero;
        UICharInfo lastCharInfo = charInfo;

        float height = this.preferredHeight / cachedTextGenerator.lineCount;
        for (var i = 0; i < count; i++)
        {
            hrefInfo = m_HrefInfos[i];
            hrefInfo.boxes.Clear();

            if (hrefInfo.startIndex < cachedTextGenerator.characterCount)
            {
                // 将超链接里面的文本顶点索引坐标加入到包围框
                for (int j = hrefInfo.startIndex, m = hrefInfo.endIndex; j <= m; j++)
                {
                    if (j >= cachedTextGenerator.characterCount)
                    {
                        break;
                    }

                    charInfo = cachedTextGenerator.characters[j];
                    

                    tmpVector2.x = charInfo.cursorPos.x * screenRatio;
                    tmpVector2.y = charInfo.cursorPos.y * screenRatio - height;

                    if (j == hrefInfo.startIndex)
                    {
                        first = tmpVector2;
                    }

                    if (tmpVector2.y < first.y - 1) // 换行重新添加包围框
                    {
                        hrefInfo.boxes.Add(new Rect(first, new Vector2(last.x - first.x + lastCharInfo.charWidth, height)));
                        first = tmpVector2;
                    }
                    last = tmpVector2;
                    lastCharInfo = charInfo;
                }
                hrefInfo.boxes.Add(new Rect(first, new Vector2(last.x - first.x + charInfo.charWidth, height)));
            }
        }
    }

    //自定义左右符号
    public string leftSymbol = "[";
    public string rightSymbol = "]";

    //根据正则规则更新文本
    protected virtual string GetOutputText(string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
            return "";

        s_TextBuilder.Remove(0, s_TextBuilder.Length);
        int textIndex = 0;
        string imageText;

        if (inputText != m_OutputText)
        {
            m_HrefInfos.Clear();
        }


        MatchCollection mc = _inputTagRegex.Matches(inputText);

        foreach (Match match in mc)
        {
            int tempId = 0;
            if (!string.IsNullOrEmpty(match.Groups[1].Value) && !match.Groups[1].Value.Equals("-"))
                tempId = int.Parse(match.Groups[1].Value);
            string tempTag = match.Groups[2].Value;
            //更新超链接
            if (tempId != 0)
            {
                s_TextBuilder.Append(inputText.Substring(textIndex, match.Index - textIndex));
                s_TextBuilder.Append("<color=blue>");  // 超链接颜色
                s_TextBuilder.Append(leftSymbol);

    
                var hrefInfo = new HrefInfo
                {
                    id = Mathf.Abs(tempId),
                    startIndex = s_TextBuilder.Length - leftSymbol.Length, // 超链接里的文本起始顶点索引
                    endIndex = s_TextBuilder.Length + tempTag.Length + 1 - rightSymbol.Length,
                    name = tempTag
                };
                m_HrefInfos.Add(hrefInfo);

                s_TextBuilder.Append(match.Groups[2].Value);
                s_TextBuilder.Append(rightSymbol);
                s_TextBuilder.Append("</color>");
                textIndex = match.Index + match.Length;

            }
            //更新表情
            else
            {
                SetManager();
                if (!LinkImageManager.alltags.ContainsKey(tempTag))
                    continue;

                SpriteInfoGroup tempGroup = LinkImageManager.alltags[tempTag];

                s_TextBuilder.Append(inputText.Substring(textIndex, match.Index - textIndex));
                int tempIndex = s_TextBuilder.Length * 4;
                s_TextBuilder.Append(@"<quad name=" + tempTag + " size=" + tempGroup.size* imageScle + " width=" + tempGroup.width + " />");
            }
            textIndex = match.Index + match.Length;
        }

        if (textIndex > 0)
        {
            s_TextBuilder.Append(inputText.Substring(textIndex, inputText.Length - textIndex));
            imageText = s_TextBuilder.ToString();
        }
        else
        {
            imageText = inputText;
        }

        //拼上隐藏占位符的格式
        s_TextBuilder.Clear();
        textIndex = 0;
        if (!imageText.Contains("<color=#00000000>"))
        {
            foreach (Match match in s_ImageRegex.Matches(imageText))
            {
                s_TextBuilder.Append(imageText.Substring(textIndex, match.Index - textIndex));
                s_TextBuilder.Append("<color=#00000000>");
                s_TextBuilder.Append(match.Value);
                s_TextBuilder.Append("</color>");
                textIndex = match.Index + match.Length;
            }
        }

        if (textIndex > 0)
        {
            s_TextBuilder.Append(imageText.Substring(textIndex, imageText.Length - textIndex));
            imageText = s_TextBuilder.ToString();
        }

        int startIndex = 0;
        for (int i = 0; i < m_HrefInfos.Count; i++)
        {
            HrefInfo info = m_HrefInfos[i];
            string name = string.Format("{0}{1}{2}",leftSymbol, info.name, rightSymbol);
            info.startIndex = imageText.IndexOf(name, startIndex);
            info.endIndex = info.startIndex + name.Length - 1;
            startIndex = info.endIndex + 1;
        }

        return imageText;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressDuration = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// 点击事件检测是否点击到超链接文本
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        SetHrefInfoBoxes();

        pressDuration = Time.realtimeSinceStartup - pressDuration;

        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        foreach (var hrefInfo in m_HrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    m_OnHrefClick.Invoke(hrefInfo.name, hrefInfo.id, eventData);
                    Debug.LogError($"{hrefInfo.name} {hrefInfo.id}");
                    return;
                }
            }
        }

        m_OnTextClick.Invoke(this.gameObject);
    }

    // public void OnHrefClickAddListenerForLua(LuaTable table, LuaFunction fuc)
    // {
    //     UnityAction<string, int, PointerEventData> onClick = (string hrefName, int id, PointerEventData eventData) =>
    //     {
    //         if (fuc != null)
    //         {
    //             fuc.Call(table, hrefName, id, eventData, pressDuration);
    //         }
    //     };
    //     m_OnHrefClick.RemoveAllListeners();
    //     m_OnHrefClick.AddListener(onClick);
    // }
    //
    //
    // public void OnHrefClickRemoveListenerForLua()
    // {
    //     m_OnHrefClick.RemoveAllListeners();
    // }
    //
    // public void OnTextClickAddListenerForLua(LuaTable table, LuaFunction fuc)
    // {
    //     UnityAction<GameObject> onClick = (GameObject go) =>
    //     {
    //         if (fuc != null)
    //         {
    //             fuc.Call(table, go);
    //         }
    //     };
    //     m_OnTextClick.RemoveAllListeners();
    //     m_OnTextClick.AddListener(onClick);
    // }


    public void OnTextClickRemoveListenerForLua()
    {
        m_OnTextClick.RemoveAllListeners();
    }

    /// <summary>
    /// 超链接信息类
    /// </summary>
    private class HrefInfo
    {
        /// <summary>
		/// 超链接id
		/// </summary>
		public int id;

        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // throw new NotImplementedException();
    }
}