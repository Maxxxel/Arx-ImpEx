using Arx_Model_Exporter.Helpers;
using Arx_Model_Exporter.Structures.EERIE;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class TEA
    {
        private const float DIV24 = 0.0416666666666666666667f;
        private const bool bIsActive = false;

        public char[] identity { get; private set; }
        public uint version { get; private set; }
        public char[] anim_name { get; private set; }
        public int nb_frames { get; private set; }
        public int nb_groups { get; private set; }
        public int nb_key_frames { get; private set; }
        internal Keyframe2015[] keyframes2015 { get; private set; }

        public struct THEA_HEADER
        {
            public char[] identity;
            public long version;
            public string anim_name;
            public long nb_frames;
            public long nb_groups;
            public long nb_key_frames;

            public THEA_HEADER(FileWrapper file)
            {
                identity = file.ReadChars(-1, 20);
                version = file.ReadInt64();
                char[] name = file.ReadChars(-1, 256);
                anim_name = new String("");
                
                for (int i = 0; i < name.Length; i++)
                {
                    if (name[i] == '\0')
                    {
                        anim_name = new string(name, 0, i);
                        break;
                    }
                }

                nb_frames = file.ReadInt();
                nb_groups = file.ReadInt();
                nb_key_frames = file.ReadInt();
            }
        }

        public EERIE_ANIM TheaToEerie(string fileName)
        {
            long lastnum = 0;
            FileWrapper file = new FileWrapper(fileName);
            THEA_HEADER th = new THEA_HEADER(file);
            THEA_KEYFRAME tkf;
            THEA_KEYFRAME_2015 tkf2015;
            THEA_KEYMOVE tkm;
            THEO_GROUPANIM tga;
            THEA_SAMPLE ts;
            EERIE_GROUP eg;
            EERIE_ANIM eerie = new EERIE_ANIM();
            Quaternion quat;
            int num_sample;
            int num_sfx;

            if (th.version < 2014)
            {
                throw new Exception("Unsupported version of TEA file.");
            }

            eerie.anim_name = th.anim_name;
            eerie.nb_groups = th.nb_groups;
            eerie.nb_key_frames = th.nb_key_frames;
            eerie.frames = new EERIE_FRAME[th.nb_key_frames];

            for (long i = 0; i < th.nb_key_frames; i++)
            {
                eerie.frames[i] = new EERIE_FRAME();
            }

            eerie.groups = new EERIE_GROUP[th.nb_key_frames * th.nb_groups];

            for (long i = 0; i < th.nb_key_frames * th.nb_groups; i++)
            {
                eerie.groups[i] = new EERIE_GROUP();
            }

            eerie.voidgroups = new byte[th.nb_groups];
            eerie.anim_time = 0f;
            lastnum = 0;
            eerie.tkf2015s = new THEA_KEYFRAME_2015[th.nb_key_frames];

            // Go For Keyframes read...
            for (long i = 0; i < th.nb_key_frames; i++)
            {
                if (th.version >= 2015)
                {
                    tkf2015 = new THEA_KEYFRAME_2015(file);
                }
                else
                {
                    tkf = new THEA_KEYFRAME(file);
                    tkf2015 = new THEA_KEYFRAME_2015(file);
                    tkf2015.num_frame = tkf.num_frame;
                    tkf2015.flag_frame = tkf.flag_frame;
                    tkf2015.master_key_frame = tkf.master_key_frame;
                    tkf2015.key_frame = tkf.key_frame;
                    tkf2015.key_move = tkf.key_move;
                    tkf2015.key_orient = tkf.key_orient;
                    tkf2015.key_morph = tkf.key_morph;
                    tkf2015.time_frame = tkf.time_frame;
                }

                eerie.tkf2015s[i] = tkf2015;
                eerie.frames[i].master_key_frame = tkf2015.master_key_frame ? 1: 0;
                eerie.frames[i].num_frame = tkf2015.num_frame;

                long lKeyOrient = tkf2015.key_orient ? 1 : 0;
                long lKeyMove = tkf2015.key_move ? 1 : 0;
                eerie.frames[i].f_rotate = ARX_CLEAN_WARN_CAST_SHORT(lKeyOrient);
                eerie.frames[i].f_translate = ARX_CLEAN_WARN_CAST_SHORT(lKeyMove);

                tkf2015.time_frame = (tkf2015.num_frame) * 1000;
                lastnum = tkf2015.num_frame;
                eerie.frames[i].time = tkf2015.time_frame * DIV24;
                eerie.anim_time += tkf2015.time_frame;
                eerie.frames[i].flag = tkf2015.flag_frame;

                // Is There a Global translation ?
                if (tkf2015.key_move == true)
                {
                    tkm = new THEA_KEYMOVE(file);
                    Vector3 temp = eerie.frames[i].translate;
                    temp.X = tkm.x;
                    temp.Y = tkm.y;
                    temp.Z = tkm.z;
                    eerie.frames[i].translate = temp;
                    tkf2015.translate = temp;
                }

                // Is There a Global Rotation ?
                if (tkf2015.key_orient == true)
                {
                    byte[] Skipped = file.ReadBytes(-1, 8);
                    quat = file.ReadArxQuaternion();
                    Quaternion temp = eerie.frames[i].quat;
                    temp.X = quat.X;
                    temp.Y = quat.Y;
                    temp.Z = quat.Z;
                    temp.W = quat.W;
                    eerie.frames[i].quat = temp;
                    tkf2015.quat = temp;
                }

                // Is There a Global Morph ? (IGNORED!)
                if (tkf2015.key_morph == true)
                {
                    var THEAMORPH = file.ReadBytes(-1, 16);
                }

                tkf2015.groups = new THEO_GROUPANIM[th.nb_groups];
                // Now go for Group Rotations/Translations/scaling for each GROUP
                for (long j = 0; j < th.nb_groups; j++)
                {
                    tga = new THEO_GROUPANIM(file);
                    tkf2015.groups[j] = tga;
                    eg = eerie.groups[j + i * th.nb_groups];
                    eg.key = tga.key_group ? 1 : 0;

                    Quaternion temp = eg.quat;
                    temp.X = tga.Quaternion.X;
                    temp.Y = tga.Quaternion.Y;
                    temp.Z = tga.Quaternion.Z;
                    temp.W = tga.Quaternion.W;
                    eg.quat = temp;

                    Vector3 tempV = eg.translate;
                    tempV.X = tga.translate.X;
                    tempV.Y = tga.translate.Y;
                    tempV.Z = tga.translate.Z;
                    eg.translate = tempV;

                    Vector3 tempZ = eg.zoom;
                    tempZ.X = tga.zoom.X;
                    tempZ.Y = tga.zoom.Y;
                    tempZ.Z = tga.zoom.Z;
                    eg.zoom = tempZ;
                }

                // Now Read Sound Data included in this frame
                num_sample = file.ReadInt();
                eerie.frames[i].sample = -1;

                if (num_sample != -1)
                {
                    ts = new THEA_SAMPLE(file);
                    eerie.frames[i].sample = 1; // ARX_SOUND_Load(ts.sample_name, file);
                }

                num_sfx = file.ReadInt();
            }

            for (long i = 0; i < th.nb_key_frames; i++)
            {
                if (eerie.frames[i].f_translate == 0)
                {
                    long k = i;

                    while ((k >= 0) && (eerie.frames[k].f_translate == 0))
                    {
                        k--;
                    }

                    long j = i;

                    while ((j < th.nb_key_frames) && (eerie.frames[j].f_translate == 0))
                    {
                        j++;
                    }

                    if ((j < th.nb_key_frames) && (k >= 0))
                    {
                        float r1 = GetTimeBetweenKeyFrames(eerie, k, i);
                        float r2 = GetTimeBetweenKeyFrames(eerie, i, j);
                        float tot = 1f / (r1 + r2);
                        r1 *= tot;
                        r2 *= tot;

                        Vector3 temp = eerie.frames[i].translate;
                        temp.X = eerie.frames[j].translate.X * r1 + eerie.frames[k].translate.X * r2;
                        temp.Y = eerie.frames[j].translate.Y * r1 + eerie.frames[k].translate.Y * r2;
                        temp.Z = eerie.frames[j].translate.Z * r1 + eerie.frames[k].translate.Z * r2;
                        eerie.frames[i].translate = temp;
                    }
                }

                if (eerie.frames[i].f_rotate == 0)
                {
                    long k = i;

                    while ((k >= 0) && (eerie.frames[k].f_rotate == 0))
                    {
                        k--;
                    }

                    long j = i;

                    while ((j < th.nb_key_frames) && (eerie.frames[j].f_rotate == 0))
                    {
                        j++;
                    }

                    if ((j < th.nb_key_frames) && (k >= 0))
                    {
                        float r1 = GetTimeBetweenKeyFrames(eerie, k, i);
                        float r2 = GetTimeBetweenKeyFrames(eerie, i, j);
                        float tot = 1f / (r1 + r2);
                        r1 *= tot;
                        r2 *= tot;

                        Quaternion temp = eerie.frames[i].quat;
                        temp.W = eerie.frames[j].quat.W * r1 + eerie.frames[k].quat.W * r2;
                        temp.X = eerie.frames[j].quat.X * r1 + eerie.frames[k].quat.X * r2;
                        temp.Y = eerie.frames[j].quat.Y * r1 + eerie.frames[k].quat.Y * r2;
                        temp.Z = eerie.frames[j].quat.Z * r1 + eerie.frames[k].quat.Z * r2;
                        eerie.frames[i].quat = temp;
                    }
                }
            }

            for (long i = 0; i < th.nb_key_frames; i++)
            {
                eerie.frames[i].f_translate = 1;
                eerie.frames[i].f_rotate = 1;
            }

            // Sets Flag for voidgroups (unmodified groups for whole animation)
            for (long i = 0; i < eerie.nb_groups; i++)
            {
                long voidd = 1;

                for (long j = 0; j < eerie.nb_key_frames; j++)
                {
                    long pos = i + (j * eerie.nb_groups);

                    if ((eerie.groups[pos].quat.X != 0f)
                            || (eerie.groups[pos].quat.Y != 0f)
                            || (eerie.groups[pos].quat.Z != 0f)
                            || (eerie.groups[pos].quat.W != 1f)
                            || (eerie.groups[pos].translate.X != 0f)
                            || (eerie.groups[pos].translate.Y != 0f)
                            || (eerie.groups[pos].translate.Z != 0f)
                            || (eerie.groups[pos].zoom.X != 0f)
                            || (eerie.groups[pos].zoom.Y != 0f)
                            || (eerie.groups[pos].zoom.Z != 0f)
                    )
                    {
                        voidd = 0;
                        break;
                    }
                }

                if (voidd == 1) eerie.voidgroups[i] = 1;
            }

            eerie.anim_time = (float)th.nb_frames * DIV24;

            if (eerie.anim_time < 1) eerie.anim_time = 1;

            return eerie;
        }

        private float GetTimeBetweenKeyFrames(EERIE_ANIM ea, long f1, long f2)
        {
            if (ea == null) return 0;

            if (f1 < 0) return 0;

            if (f1 > ea.nb_key_frames - 1) return 0;

            if (f2 < 0) return 0;

            if (f2 > ea.nb_key_frames - 1) return 0;

            float time = 0;

            for (long kk = f1 + 1; kk <= f2; kk++)
            {
                time += ea.frames[kk].time;
            }

            return time;
        }

        private short ARX_CLEAN_WARN_CAST_SHORT(long _x)
        {
            if (_x > 32767) return 32767;

            if (_x < -32768) return -32768;

            return (short)_x;
        }
    }
}
