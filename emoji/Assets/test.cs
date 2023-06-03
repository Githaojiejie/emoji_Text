using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<UnityEngine.UI.Text>().text =
            "[-100#超链接(100)][-1#超链接(1)]"+
            "图文<quad name=https://testmall-res.xwindlab.com/test/20230526/operationImage/2bf0cf674332465f9019191f36ca8479.png size=75 width=1 />混排" +
            "图文<quad name=https://testmall-res.xwindlab.com/test/20230526/operationImage/2bf0cf674332465f9019191f36ca8479.png size=5 width=1 />混排" +
            "图文<quad name=https://testmall-res.xwindlab.com/test/20230526/operationImage/2bf0cf674332465f9019191f36ca8479.png size=25 width=1 />混排" +
            "图文<quad name=https://testmall-res.xwindlab.com/test/20230526/operationImage/2bf0cf674332465f9019191f36ca8479.png size=35 width=1 />混排" +
            "图文<quad name=https://testmall-res.xwindlab.com/test/20230526/operationImage/2bf0cf674332465f9019191f36ca8479.png size=45 width=1 />混排";

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
