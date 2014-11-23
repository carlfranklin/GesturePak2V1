using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GesturePak
{
    public static class ExtensionMethods
    {
        public static string ToFriendlyCase(this string EnumString)
        {
            return Regex.Replace(EnumString, "(?!^)([A-Z])", " $1");
        }

        public static void ResetGestureMatchFlags(this List<Gesture> Gestures)
        {
            foreach (Gesture g in Gestures)
            {
                g.Matched = false;
                foreach (Frame p in g.Frames)
                { 
                    p.Matched = false;
                }
            }
        }

        public static void Renumber(this Gesture Gesture)
        {
            for (int i = 0; i < Gesture.Frames.Count; i++)
            {
                Gesture.Frames[i].Name = "Frame " + (i + 1).ToString();
            }
        }
    }
}
