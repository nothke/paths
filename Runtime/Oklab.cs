using UnityEngine;

/*
 *  Oklab Unity implementation by Ferran Bertomeu Castells @fonserbc
 *  
 *  Oklab by Björn Ottosson https://bottosson.github.io/posts/oklab/
 *  under Public Domain
 * 
 *  For the conversions, I understand unity's Color as a gamma-space color
 */

namespace Nothke.Paths.Utils
{
    public static class Oklab
    {
        public struct Lab { public float L; public float a; public float b; };

        public static Lab ToOklab(this Color color)
        {
            Color c = color.linear;

            float l = 0.4122214708f * c.r + 0.5363325363f * c.g + 0.0514459929f * c.b;
            float m = 0.2119034982f * c.r + 0.6806995451f * c.g + 0.1073969566f * c.b;
            float s = 0.0883024619f * c.r + 0.2817188376f * c.g + 0.6299787005f * c.b;

            float l_ = cbrtf(l);
            float m_ = cbrtf(m);
            float s_ = cbrtf(s);

            return new Lab
            {
                L = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
                a = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
                b = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_
            };
        }

        public static Color ToColor(this Lab c)
        {
            float l_ = c.L + 0.3963377774f * c.a + 0.2158037573f * c.b;
            float m_ = c.L - 0.1055613458f * c.a - 0.0638541728f * c.b;
            float s_ = c.L - 0.0894841775f * c.a - 1.2914855480f * c.b;

            float l = l_ * l_ * l_;
            float m = m_ * m_ * m_;
            float s = s_ * s_ * s_;

            return new Color(
                +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s,
                -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s,
                -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s
            ).gamma;
        }

        public static Lab Lerp(Lab a, Lab b, float f)
        {
            return new Lab
            {
                L = Mathf.Lerp(a.L, b.L, f),
                a = Mathf.Lerp(a.a, b.a, f),
                b = Mathf.Lerp(a.b, b.b, f)
            };
        }

        public static Color OklabLerp(Color a, Color b, float f)
        {
            return Lerp(a.ToOklab(), b.ToOklab(), f).ToColor();
        }

        public static Color EvaluateOklab(this Gradient g, float time)
        {
            int it = 0;
            while (it < g.colorKeys.Length - 1 && time >= g.colorKeys[it + 1].time)
                it++;

            Color c;
            if (it >= g.colorKeys.Length - 1)
                c = g.colorKeys[g.colorKeys.Length - 1].color;
            else
            {
                float lerpAmount = (time - g.colorKeys[it].time) / (g.colorKeys[it + 1].time - g.colorKeys[it].time);
                c = OklabLerp(g.colorKeys[it].color, g.colorKeys[it + 1].color, lerpAmount);
            }

            // alpha
            it = 0;
            while (it < g.alphaKeys.Length - 1 && time >= g.alphaKeys[it + 1].time)
                it++;

            if (it >= g.alphaKeys.Length - 1)
                c.a = g.alphaKeys[g.alphaKeys.Length - 1].alpha;
            else
            {
                float lerpAmount = (time - g.alphaKeys[it].time) / (g.alphaKeys[it + 1].time - g.alphaKeys[it].time);
                c.a = Mathf.Lerp(g.alphaKeys[it].alpha, g.alphaKeys[it + 1].alpha, lerpAmount);
            }

            return c;
        }

        // If using .NET core, please use System.Math.Cbrt for cuberoot
        static float cbrtf(float v)
        {
            return Mathf.Sign(v) * Mathf.Pow(Mathf.Abs(v), 1f / 3f);
        }

        public static Color OklchToColor(float lightness, float chroma, float hue)
        {
            Lab lab;
            float h = 2 * Mathf.PI * hue;
            float C = chroma;
            lab.a = C * Mathf.Cos(h);
            lab.b = C * Mathf.Sin(h);
            lab.L = lightness;

            return ToColor(lab);
        }
    }
}