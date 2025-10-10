using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class ColorExtensions
{
    /// <summary>
    /// HSV(A)���w�肵�ď㏑������Color��Ԃ�
    /// </summary>
    public static Color WithHSVA(
        this Color baseColor,
        float? h = null,
        float? s = null,
        float? v = null,
        float? a = null)
    {
        // RGB �� HSV�ϊ�
        Color.RGBToHSV(baseColor, out float bh, out float bs, out float bv);

        // �l������Ȃ�㏑��
        float newH = h ?? bh;
        float newS = s ?? bs;
        float newV = v ?? bv;
        float newA = a ?? baseColor.a;

        // HSV �� RGB�ɖ߂�
        Color result = Color.HSVToRGB(newH, newS, newV);
        result.a = newA;
        return result;
    }
}
