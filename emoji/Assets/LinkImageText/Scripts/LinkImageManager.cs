using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmojiUI;

public class LinkImageManager : MonoBehaviour {
    public SpriteAsset spriteAsset;
    public SpriteAsset staticEmojiSpriteAsset;

    public static readonly Dictionary<string, SpriteInfoGroup> alltags = new Dictionary<string, SpriteInfoGroup>(CustomOrdinalStringComparer.GetComparer());
    public static SpriteAsset g_staticEmojiSpriteAsset = null;

    private void Awake()
    {
        alltags.Clear();
        for (int j = 0; j < spriteAsset.listSpriteGroup.Count; ++j)
        {
            SpriteInfoGroup infogroup = spriteAsset.listSpriteGroup[j];
            SpriteInfoGroup group;
            if (alltags.TryGetValue(infogroup.tag, out group))
            {
                // Debug.LogError("already exist :{0} ", infogroup.tag);
            }

            alltags[infogroup.tag] = infogroup;
        }
        g_staticEmojiSpriteAsset = staticEmojiSpriteAsset;
    }

    private void OnDestroy()
    {
        alltags.Clear();
        g_staticEmojiSpriteAsset = null;
    }

    public static List<Sprite> LoadSprites(string name)
    {
        List<Sprite> _list = new List<Sprite>();
        if (alltags.ContainsKey(name))
        {
            for (int i = 0; i < alltags[name].spritegroups.Count; i++)
            {
                _list.Add(alltags[name].spritegroups[i].sprite);
            }
        }
        return _list;
    }

    public static bool HasSprites()
    {
        return alltags.Count > 0;
    }
}
