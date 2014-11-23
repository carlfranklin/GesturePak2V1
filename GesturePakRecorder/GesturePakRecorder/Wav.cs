using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class Wav
{
    [DllImport("winmm.dll", SetLastError = true)]
    static extern bool PlaySound(string pszSound, UIntPtr hmod, uint fdwSound);

    [Flags]
    public enum SoundFlags
    {
        SND_SYNC = 0x0000,
        SND_ASYNC = 0x0001,
        SND_NODEFAULT = 0x0002,
        SND_MEMORY = 0x0004,
        SND_LOOP = 0x0008,
        SND_NOSTOP = 0x0010,
        SND_PURGE = 0x40,
        SND_NOWAIT = 0x00002000,
        SND_ALIAS = 0x00010000,
        SND_ALIAS_ID = 0x00110000,
        SND_FILENAME = 0x00020000,
        SND_RESOURCE = 0x00040004
    }

    public static void Play(string FileName)
    {
        PlaySound(FileName, UIntPtr.Zero, 
            (uint)(SoundFlags.SND_FILENAME | SoundFlags.SND_ASYNC));
    }
}
