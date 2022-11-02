using Arx_Model_Exporter.Structures.FTLFile;
using System;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class AnimationData
    {
        public int nb_groups { get; set; }
        public int nb_key_frames { get; set; }
        public Frame[] frames { get; set; }
        public GroupAnimation[] groups { get; set; }
        public int[]? voidgroups { get; set; }
        public float anim_time { get; set; }
        public int lastnum { get; set; }
        public string name { get; set; }

        public AnimationData(TEA Animation)
        {
            int index = -1;
            for (int i = 0; i < Animation.anim_name.Length; i++)
            {
                char c = Animation.anim_name[i];
                if (c == 0)
                {
                    index = i;
                    break;
                }
            }

            name = new String(Animation.anim_name).Substring(0, index);
            nb_groups = Animation.nb_groups;
            nb_key_frames = Animation.nb_key_frames;
            frames = new Frame[nb_key_frames]; // EEAnimFrames
            groups = new GroupAnimation[nb_groups * nb_key_frames]; // EEAnimGroups
            voidgroups = new int[nb_groups]; // EEAnimVoidGroups
            anim_time = 0f;
            lastnum = 0;

            for (int i = 0; i < Animation.nb_key_frames; i++)
            {
                var tkf2015 = Animation.keyframes2015[i];
                frames[i] = new Frame();
                frames[i].master_key_frame = tkf2015.master_key_frame ? 1 : 0;
                frames[i].num_frame = tkf2015.num_frame;
                var lKeyOrient = tkf2015.key_orient;
                var lKeyMove = tkf2015.key_move;
                frames[i].f_rotate = lKeyOrient;
                frames[i].f_translate = lKeyMove;
                tkf2015.time_frame = tkf2015.num_frame * 1000;
                lastnum = tkf2015.num_frame;
                frames[i].time = tkf2015.time_frame * 0.0416666666666666666667f;
                anim_time += tkf2015.time_frame;
                frames[i].flag = tkf2015.flag_frame;

                if (tkf2015.key_move)
                {
                    frames[i].translate = tkf2015.tkm;
                }

                if (tkf2015.key_orient)
                {
                    var Quat = tkf2015.quat;
                    var Target = frames[i].quat;
                    Target.X = Quat.X;
                    Target.Y = Quat.Y;
                    Target.Z = Quat.Z;
                    Target.W = Quat.W;
                    frames[i].quat = Target;
                }

                for (int j = 0; j < Animation.nb_groups; j++)
                {
                    var tga = tkf2015.group_animations[j];
                    var eg = groups[j + i * Animation.nb_groups] = new GroupAnimation();
                    eg.key_group = tga.key_group;

                    var egq = eg.quaternion = new Quaternion();
                    egq.X = tga.quaternion.X;
                    egq.Y = tga.quaternion.Y;
                    egq.Z = tga.quaternion.Z;
                    egq.W = tga.quaternion.W;
                    eg.quaternion = egq;

                    var egt = eg.translate = new Vector3();
                    egt.X = tga.translate.X;
                    egt.Y = tga.translate.Y;
                    egt.Z = tga.translate.Z;
                    eg.translate = egt;

                    var egz = eg.zoom = new Vector3();
                    egz.X = tga.zoom.X;
                    egz.Y = tga.zoom.Y;
                    egz.Z = tga.zoom.Z;
                    eg.zoom = egz;

                    groups[j + i * Animation.nb_groups] = eg;
                }
            }

            for (int i = 0; i < Animation.nb_key_frames; i++)
            {
                if (!frames[i].f_translate)
                {
                    long k = i;

                    while ((k >= 0) && (!frames[k].f_translate))
                    {
                        k--;
                    }

                    long j = i;

                    while ((j < Animation.nb_key_frames) && (!frames[j].f_translate))
                    {
                        j++;
                    }

                    if ((j < Animation.nb_key_frames) && (k >= 0))
                    {
                        float r1 = GetTimeBetweenKeyFrames(nb_key_frames, k, i, frames);
                        float r2 = GetTimeBetweenKeyFrames(nb_key_frames, i, j, frames);
                        float tot = 1f / (r1 + r2);
                        r1 *= tot;
                        r2 *= tot;
                        var frt = frames[i].translate;
                        frt.X = frames[j].translate.X * r1 + frames[k].translate.X * r2;
                        frt.Y = frames[j].translate.Y * r1 + frames[k].translate.Y * r2;
                        frt.Z = frames[j].translate.Z * r1 + frames[k].translate.Z * r2;
                        frames[i].translate = frt;
                    }
                }

                if (!frames[i].f_rotate)
                {
                    long k = i;

                    while ((k >= 0) && (!frames[k].f_rotate))
                    {
                        k--;
                    }

                    long j = i;

                    while ((j < Animation.nb_key_frames) && (!frames[j].f_rotate))
                    {
                        j++;
                    }

                    if ((j < Animation.nb_key_frames) && (k >= 0))
                    {
                        float r1 = GetTimeBetweenKeyFrames(nb_key_frames, k, i, frames);
                        float r2 = GetTimeBetweenKeyFrames(nb_key_frames, i, j, frames);
                        float tot = 1f / (r1 + r2);
                        r1 *= tot;
                        r2 *= tot;
                        var frq = frames[i].quat;
                        frq.W = frames[j].quat.W * r1 + frames[k].quat.W * r2;
                        frq.X = frames[j].quat.X * r1 + frames[k].quat.X * r2;
                        frq.Y = frames[j].quat.Y * r1 + frames[k].quat.Y * r2;
                        frq.Z = frames[j].quat.Z * r1 + frames[k].quat.Z * r2;
                        frames[i].quat = frq;
                    }
                }
            }

            for (int i = 0; i < Animation.nb_key_frames; i++)
            {
                frames[i].f_translate = true;
                frames[i].f_rotate = true;
            }

            for (int i = 0; i < nb_groups; i++)
            {
                long voidd = 1;

                for (int j = 0; j < nb_key_frames; j++)
                {
                    long pos = i + (j * nb_groups);

                    if ((groups[pos].quaternion.X != 0f)
                            || (groups[pos].quaternion.Y != 0f)
                            || (groups[pos].quaternion.Z != 0f)
                            || (groups[pos].quaternion.W != 1f)
                            || (groups[pos].translate.X != 0f)
                            || (groups[pos].translate.Y != 0f)
                            || (groups[pos].translate.Z != 0f)
                            || (groups[pos].zoom.X != 0f)
                            || (groups[pos].zoom.Y != 0f)
                            || (groups[pos].zoom.Z != 0f)
                        )
                    {
                        voidd = 0;
                        break;
                    }
                }

                if (voidd == 1) voidgroups[i] = 1;
            }

            anim_time = (float)Animation.nb_frames * 1000f * 0.0416666666666666666667f;
            if (anim_time < 1) anim_time = 1;
        }

        public float GetTimeBetweenKeyFrames(int nb_key_frames, long f1, long f2, Frame[] frames)
        {
            if (f1 < 0) return 0;

            if (f1 > nb_key_frames - 1) return 0;

            if (f2 < 0) return 0;

            if (f2 > nb_key_frames - 1) return 0;

            float time = 0;

            for (long kk = f1 + 1; kk <= f2; kk++)
            {
                time += frames[kk].time;
            }

            return time;
        }
    }
}
