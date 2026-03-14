using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.Wages
{
    [StaticConstructorOnStartup]
    public static class Textures_Wages
    {
        public static Texture2D WagesMenuIcon = ContentFinder<Texture2D>.Get("WagesMenuIcon", false);
        public static Texture2D MoodIcon = ContentFinder<Texture2D>.Get("MoodIcon", false);

        public static void DrawIcon(Rect outerRect, Texture2D icon, float scale = 1.0f)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Vector2 texProportions = new Vector2(icon.width, icon.height);
                Rect texCoords = new Rect(0f, 0f, 1f, 1f);

                Rect rect = new Rect(0f, 0f, texProportions.x, texProportions.y);
                float num = ((!(rect.width / rect.height < outerRect.width / outerRect.height)) ? (outerRect.width / rect.width) : (outerRect.height / rect.height));
                num *= scale;
                rect.width *= num;
                rect.height *= num;
                rect.x = outerRect.x + outerRect.width / 2f - rect.width / 2f;
                rect.y = outerRect.y + outerRect.height / 2f - rect.height / 2f;
                GenUI.DrawTextureWithMaterial(rect, icon, null, texCoords);
            }
        }
    }
}
