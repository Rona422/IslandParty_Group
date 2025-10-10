using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class ColorExtensions
{
    /// <summary>
    /// HSV(A)‚ğw’è‚µ‚Äã‘‚«‚µ‚½Color‚ğ•Ô‚·
    /// </summary>
    public static Color WithHSVA(
        this Color baseColor,
        float? h = null,
        float? s = null,
        float? v = null,
        float? a = null)
    {
        // RGB ¨ HSV•ÏŠ·
        Color.RGBToHSV(baseColor, out float bh, out float bs, out float bv);

        // ’l‚ª‚ ‚é‚È‚çã‘‚«
        float newH = h ?? bh;
        float newS = s ?? bs;
        float newV = v ?? bv;
        float newA = a ?? baseColor.a;

        // HSV ¨ RGB‚É–ß‚·
        Color result = Color.HSVToRGB(newH, newS, newV);
        result.a = newA;
        return result;
    }
}
