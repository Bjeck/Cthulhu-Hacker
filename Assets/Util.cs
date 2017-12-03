using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util {

    public static Color green = new Color(100f / 256f, 176f / 256f, 48f / 256);
    public static Color blue = new Color(49f / 256f, 156f / 256f, 184f / 256);
    public static Color yellow = new Color(228f / 256f, 236f / 256f, 80f / 256);

    public static float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

}
