using UnityEngine;
using System;
using System.Collections.Generic;

public static class Common
{
    public const int MyselfCamp = 1;

    public static int Epoch { get { return (int)((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000); } }

    //UISprite的atlas对象池
    //static Dictionary<string, AtlasCache> atlasPool = new Dictionary<string, AtlasCache>();

    public enum RenderingMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent,
    }

    public static Camera FindCameraForLayer(string layerName) { return FindCameraForLayer(LayerMask.NameToLayer(layerName)); }
    public static Camera FindCameraForLayer(int layer)
    {
        Camera[] cameras = Camera.allCameras;
        for (int i = 0, length = cameras.Length; i < length; ++i)
            if ((cameras[i].cullingMask & layer) > 0)
                return cameras[i];
        return null;
    }

    public static DateTime GetTime(string timeStamp)
    {
        DateTime dtStart = new DateTime(1970, 1, 1);
        long lTime = long.Parse(timeStamp + "0000000");
        TimeSpan toNow = new TimeSpan(lTime);
        return dtStart.Add(toNow).ToLocalTime();
    }

    public static int ConvertDateTimeInt(System.DateTime time)
    {
        int intResult = 0;
        System.DateTime startTime = new System.DateTime(1970, 1, 1);
        intResult = (int)(time - startTime).TotalSeconds;
        return intResult;
    }

    static char[] szCharIndex = {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
        'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        '1', '2', '3', '4', '5', '6', '7', '8', '9'
    };
    public static string RandomString(int length)
    {
        if (length < 1) return "";
        string result = "";
        for (int i = 0; i < length; ++i)
            result += szCharIndex[UnityEngine.Random.Range(0, szCharIndex.Length - 1)];
        return result;
    }

    public static string ShortColorString(Color color)
    {
        return ((int)(color.r * 255)).ToString("X2") + ((int)(color.g * 255)).ToString("X2") + ((int)(color.b * 255)).ToString("X2");
    }


    static char[] szRuChars = {
        'А', 'а', 'Е', 'е', 'Ё', 'ё', 'И', 'и', 'Й', 'й', 'О', 'о', 'У', 'у', 'Ы', 'ы', 'Э', 'э', 'Ю', 'ю', 'Я', 'я',
        'Б', 'б', 'В', 'в', 'Г', 'г', 'Д', 'д', 'Ж', 'ж', 'З', 'з', 'К', 'к', 'Л', 'л', 'М', 'м', 'Н', 'н', 'П', 'п',
        'Р', 'р', 'С', 'с', 'Т', 'т', 'Ф', 'ф', 'Х', 'х', 'Ц', 'ц', 'Ч', 'ч', 'Ш', 'ш', 'Щ', 'щ', 'Ь', 'ь', 'Ъ', 'ъ'
    };

    public static bool IsRuChar(char c)
    {
        foreach (char t in szRuChars)
            if (t == c)
                return true;
        return false;
    }

    private static byte[] m_keys = { 0xF1, 0x48, 0x63, 0x47 };
    public static string EncryptString(string str)
    {
        byte[] bStr = System.Text.Encoding.UTF8.GetBytes(str);
        int keyi = 0;
        string encrypt = "";
        for (int i = 0; i < bStr.Length; ++i)
        {
            bStr[i] = (byte)(bStr[i] ^ (m_keys[keyi] + (byte)i));
            char h = ((bStr[i] & 0xf0) >> 4).ToString("x")[0];
            encrypt += h;
            char l = (bStr[i] & 0x0f).ToString("x")[0];
            encrypt += l;
            if (++keyi >= 4) keyi = 0;
        }
        return encrypt;

    }
    public static string FillString(string fill, params string[] paramList)
    {
        int i = 0, nIndex = 0;
        while (true)
        {
            i = fill.IndexOf('$', nIndex);
            if (i < 0 || i >= fill.Length) break;
            int paramId = 0;
            if (!int.TryParse(fill[i + 1].ToString(), out paramId) || paramId >= paramList.Length)
                continue;
            fill = fill.Substring(0, i) + paramList[paramId] + fill.Substring(i + 2);
            nIndex = i + paramList[paramId].Length;
        }
        return fill;
    }
    public static string SecondToStrTime(int seconds)
    {
        return SecondToStrTime(seconds, "HH:mm:ss");
    }
    public static string SecondToStrTime(int seconds, string format)
    {
        return new System.DateTime(0).AddSeconds(seconds < 0 ? 0 : seconds).ToString(format);
    }

    //public static void AvailableBtn(GameObject obj, GameObject target, string functionName)
    //{
    //    if (obj == null)
    //        return;
    //    obj.SetActive(true);
    //    if (!target || string.IsNullOrEmpty(functionName))
    //        return;
    //    UIButton uiBtn = obj.GetComponent<UIButton>();
    //    if (uiBtn)
    //    {
    //        uiBtn.defaultColor = uiBtn.hover = Color.white;
    //        if (!uiBtn.isEnabled)
    //            uiBtn.isEnabled = true;
    //        else
    //            uiBtn.UpdateColor(true);
    //    }

    //    UIButtonMessage btnMsg = obj.GetComponent<UIButtonMessage>();
    //    btnMsg.target = target;
    //    btnMsg.functionName = functionName;
    //}

    //public static void DisabledBtn(GameObject obj)
    //{
    //    obj.SetActive(true);

    //    UIButton uiBtn = obj.GetComponent<UIButton>();
    //    if (uiBtn)
    //    {
    //        uiBtn.defaultColor = uiBtn.hover = uiBtn.disabledColor;
    //        uiBtn.isEnabled = false;
    //    }
    //}

    //public static void SetBtnFunctionName(GameObject obj, string functionName, GameObject Msgtarget, ref GameObject target, ref string fName)
    //{
    //    Collider col = obj.GetComponent<Collider>();
    //    if (col) col.enabled = true;
    //    UIButtonMessage btnMsg = obj.GetComponent<UIButtonMessage>();
    //    target = btnMsg.target;
    //    fName = btnMsg.functionName;
    //    if (Msgtarget)
    //        btnMsg.target = Msgtarget;
    //    btnMsg.functionName = functionName;
    //}



    public static void SaveFile(string path, byte[] buff)
    {
#if UNITY_EDITOR
        System.IO.FileStream stream = System.IO.File.Open(Application.dataPath + "/" + path, System.IO.FileMode.OpenOrCreate);
        stream.Write(buff, 0, buff.Length);
        stream.Flush();
        stream.Close();
#endif
    }

    //class AtlasCache
    //{
    //    public Material mat;
    //    public UIAtlas atlas;
    //}

    //public static void setShaderToGameObject(GameObject go, string shaderName)
    //{
    //    Shader shader = Shader.Find(shaderName);

    //    UITexture[] texes = go.GetComponentsInChildren<UITexture>();
    //    UISprite[] sps = go.GetComponentsInChildren<UISprite>();

    //    foreach (UITexture tex in texes)
    //    {
    //        tex.material.shader = shader;
    //    }

    //    foreach (UISprite sp in sps)
    //    {
    //        if (sp.atlas != null)
    //        {
    //            Material mat;
    //            UIAtlas newAtlas;

    //            string atlasName = sp.atlas.name.Contains("(Clone)") ? sp.atlas.name.Substring(0, sp.atlas.name.IndexOf("(Clone)")) : sp.atlas.name;

    //            if (atlasPool.ContainsKey(atlasName + "_" + shaderName))
    //            {
    //                mat = atlasPool[atlasName + "_" + shaderName].mat;
    //                newAtlas = atlasPool[atlasName + "_" + shaderName].atlas;
    //                sp.atlas = newAtlas;
    //            }
    //            else
    //            {
    //                mat = UnityEngine.Object.Instantiate<Material>(sp.material);
    //                mat.shader = shader;
    //                newAtlas = UnityEngine.Object.Instantiate<GameObject>(sp.atlas.gameObject).GetComponent<UIAtlas>();
    //                newAtlas.spriteMaterial = mat;
    //                sp.atlas = newAtlas;

    //                AtlasCache atlasCache = new AtlasCache();
    //                atlasCache.mat = mat;
    //                atlasCache.atlas = newAtlas;
    //                atlasPool[atlasName + "_" + shaderName] = atlasCache;
    //            }
    //        }
    //    }
    //    go.SetActive(false);
    //    go.SetActive(true);
    //}

    //public static void SetGrayColor(GameObject go, bool isGray)
    //{
    //    string shaderName = isGray ? "Custom/Unlit - ColorGrayed" : "Unlit/Transparent Colored";
    //    setShaderToGameObject(go, shaderName);
    //}

    public static bool IsPOT(int n)
    {
        return n > 1 && (n & (n - 1)) == 0;
    }

    public static bool IsPOTTexture(Texture tex)
    {
        return IsPOT(tex.width) && IsPOT(tex.height);
    }

    public static bool IsSuqareTexture(Texture tex)
    {
        return tex.width == tex.height;
    }

    public static bool IsMultiple4Texture(Texture tex)
    {
        return (tex.width % 4) == 0 && (tex.height % 4) == 0;
    }

    public static void SetMaterialRenderingMode(Material material, RenderingMode renderingMode)
    {
        switch (renderingMode)
        {
            case RenderingMode.Opaque:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case RenderingMode.Cutout:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case RenderingMode.Fade:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case RenderingMode.Transparent:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }

    public static bool IsPosInConeRange(Transform self, Vector3 tarPos, float angle, float maxRange, float minRange = 0)
    {
        float sqrDistance = self.position.SqrDistanceWith(tarPos);
        if (sqrDistance > maxRange * maxRange || sqrDistance < minRange * minRange) return false;
        if (Vector3.Angle(self.forward, tarPos - self.position) > angle / 2) return false;
        return true;
    }

    public static float GetGroundHeight(float posX, float posZ, float startY = float.MaxValue)
    {
        RaycastHit hit;
        if (!Physics.Raycast(new Vector3(posX, startY, posZ), Vector3.down, out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Default")))
            return 0;
        return hit.point.y;
    }
}
