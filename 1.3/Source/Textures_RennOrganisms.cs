using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    [StaticConstructorOnStartup]
    public static class Textures_RennOrganisms
    {
        public static Texture2D RennFiber = ContentFinder<Texture2D>.Get("RennFiber/RennFiber", false);
        public static Texture2D RennFiberDomestic = ContentFinder<Texture2D>.Get("RennFiberDomestic/RennFiberDomestic", false);
        public static Texture2D RennFiberEngineered = ContentFinder<Texture2D>.Get("RennFiberEngineered/RennFiberEngineered", false);
    }
}
