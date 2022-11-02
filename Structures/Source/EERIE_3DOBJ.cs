using Arx_Model_Exporter.Helpers;
using Arx_Model_Exporter.Structures.FTLFile;
using Arx_Model_Exporter.Structures.TEAFile;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_3DOBJ
    {
        public char[] name { get; set; }
        public string file { get; set; }
        public Vector3 pos { get; set; }
        public Vector3 point0 { get; set; }
        public Vector3 angle { get; set; }
        public long origin { get; set; }
        public long ident { get; set; }
        public long nbvertex { get; set; }
        public long true_nbvertex { get; set; }
        public long nbfaces { get; set; }
        public long nbpfaces { get; set; }
        public long nbmaps { get; set; }
        public long nbgroups { get; set; }
        public long nbaction { get; set; }
        public long nbselections { get; set; }
        public ulong drawflags { get; set; }
        public Vector3[] vertexlocal { get; set; }
        public EERIE_VERTEX[] vertexlist { get; set; }
        public EERIE_VERTEX[] vertexlist3 { get; set; }
        public EERIE_FACE[] facelist { get; set; }
        public EERIE_GROUPLIST[] grouplist { get; set; }
        public EERIE_ACTIONLIST[] actionlist { get; set; }
        public EERIE_SELECTIONS[] selections { get; set; }
        public TextureContainer[] texturecontainer { get; set; }
        public char[] originaltextures { get; set; }
        public long nblinked { get; set; }
        public CLOTHES_DATA cdata { get; set; }
        public COLLISION_SPHERES_DATA sdata { get; set; }
        public EERIE_C_DATA c_data { get; set; }
        public long nb_groups { get; internal set; }
        public long nb_key_frames { get; internal set; }
        internal EERIE_FRAME[] frames { get; set; }
        public EERIE_GROUP[] groups { get; internal set; }
        public byte[] voidgroups { get; internal set; }
        public float anim_time { get; internal set; }

        public struct ARX_FTL_PRIMARY_HEADER
        {
            public byte[] ident { get; }
            public float version { get; }

            public ARX_FTL_PRIMARY_HEADER(MemoryWrapper mem)
            {
                ident = mem.ReadBytes(-1, 4);
                version = mem.ReadFloat();
            }
        }

        public struct ARX_FTL_SECONDARY_HEADER
        {
            public int offset_3Ddata { get; }
            public int offset_cylinder { get; }
            public int offset_progressive_data { get; }
            public int offset_clothes_data { get; }
            public int offset_collision_spheres { get; }
            public int offset_physics_box { get; }

            public ARX_FTL_SECONDARY_HEADER(MemoryWrapper mem)
            {
                offset_3Ddata = mem.ReadInt();
                offset_cylinder = mem.ReadInt();
                offset_progressive_data = mem.ReadInt();
                offset_clothes_data = mem.ReadInt();
                offset_collision_spheres = mem.ReadInt();
                offset_physics_box = mem.ReadInt();
            }
        }

        public struct ARX_FTL_PROGRESSIVE_DATA_HEADER
        {
            public long nb_vertex {get;}

            public ARX_FTL_PROGRESSIVE_DATA_HEADER(MemoryWrapper mem)
            {
                nb_vertex = mem.ReadInt64();
            }
        }

        public struct ARX_FTL_CLOTHES_DATA_HEADER
        {
            public long nb_cvert { get; }
            public long nb_springs { get; }

            public ARX_FTL_CLOTHES_DATA_HEADER(MemoryWrapper mem)
            {
                nb_cvert = mem.ReadInt64();
                nb_springs = mem.ReadInt64();
            }
        }

        public struct ARX_FTL_COLLISION_SPHERES_DATA_HEADER
        {
            public long nb_spheres { get; }

            public ARX_FTL_COLLISION_SPHERES_DATA_HEADER(MemoryWrapper mem)
            {
                nb_spheres = mem.ReadInt64();
            }
        }

        public struct ARX_FTL_3D_DATA_HEADER
        {
            public long nb_vertex { get; }
            public long nb_faces { get; }
            public long nb_maps { get; }
            public long nb_groups { get; }
            public long nb_action { get; }
            public long nb_selections { get; }
            public long origin { get; }
            public string name { get; }

            public ARX_FTL_3D_DATA_HEADER(MemoryWrapper mem)
            {
                nb_vertex = mem.ReadInt64();
                nb_faces = mem.ReadInt64();
                nb_maps = mem.ReadInt64();
                nb_groups = mem.ReadInt64();
                nb_action = mem.ReadInt64();
                nb_selections = mem.ReadInt64();
                origin = mem.ReadInt64();
                char[] temp = mem.ReadChars(-1, 256);
                name = new string("");

                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] == '\0')
                    {
                        name = new string(temp, 0, i);
                        break;
                    }
                }
            }
        }

        public EERIE_3DOBJ(MemoryWrapper ftlFile)
        {
            ARX_FTL_PRIMARY_HEADER afph = new ARX_FTL_PRIMARY_HEADER(ftlFile);

            // Verify FTL file Signature
            if ((afph.ident[0] != 'F') && (afph.ident[1] != 'T') && (afph.ident[2] != 'L'))
            {
                MainWindow.LogMessage($"[ERROR] File is no FTL file: {afph.ident[0] + afph.ident[1] + afph.ident[2]}, expected: FTL");
                return;
            }

            // Verify FTL file version
            if (afph.version != 0.83257f)
            {
                MainWindow.LogMessage($"[ERROR] File has not the right version: {afph.version}, expected: 0.83257f");
                return;
            }

            // Checksum
            byte[] Checksum = ftlFile.ReadBytes(-1, 512);

            ARX_FTL_SECONDARY_HEADER afsh = new ARX_FTL_SECONDARY_HEADER(ftlFile);

            // Check For & Load 3D Data
            if (afsh.offset_3Ddata != -1)
            {
                ARX_FTL_3D_DATA_HEADER af3Ddh = new ARX_FTL_3D_DATA_HEADER(ftlFile);

                nbvertex = af3Ddh.nb_vertex;
                nbfaces = af3Ddh.nb_faces;
                nbmaps = af3Ddh.nb_maps;
                nbgroups = af3Ddh.nb_groups;
                nbaction = af3Ddh.nb_action;
                nbselections = af3Ddh.nb_selections;
                origin = af3Ddh.origin;
                file = af3Ddh.name;

                // Alloc'n'Copy vertices
                if (nbvertex > 0)
                {
                    //todo free
                    vertexlist = new EERIE_VERTEX[nbvertex];
                    vertexlist3 = new EERIE_VERTEX[nbvertex];

                    for (long ii = 0; ii < nbvertex; ii++)
                    {
                        vertexlist[ii] = vertexlist3[ii] = new EERIE_VERTEX(ftlFile);
                    }

                    Vector3 temp = point0;
                    temp.X = vertexlist[origin].v.X;
                    temp.Y = vertexlist[origin].v.Y;
                    temp.Z = vertexlist[origin].v.Z;
                    point0 = temp;

                    for (long i = 0; i < nbvertex; i++)
                    {
                        vertexlist[i].vert.color = Color.FromArgb(255, 0, 0, 0);
                        vertexlist3[i].vert.color = Color.FromArgb(255, 0, 0, 0);
                    }
                }

                // Alloc'n'Copy faces
                if (nbfaces > 0)
                {
                    //todo free
                    facelist = new EERIE_FACE[nbfaces];

                    for (long ii = 0; ii < af3Ddh.nb_faces; ii++)
                    {
                        facelist[ii] = new EERIE_FACE();

                        EERIE_FACE_FTL eff = new EERIE_FACE_FTL(ftlFile);
                        facelist[ii].facetype = eff.facetype;
                        facelist[ii].texid = eff.texid;
                        facelist[ii].transval = eff.transval;
                        facelist[ii].temp = eff.temp;
                        facelist[ii].norm = eff.norm;

                        for (long kk = 0; kk < 3; kk++)
                        {
                            facelist[ii].nrmls[kk] = eff.nrmls[kk];
                            facelist[ii].vid[kk] = eff.vid[kk];
                            facelist[ii].u[kk] = eff.u[kk];
                            facelist[ii].v[kk] = eff.v[kk];
                            facelist[ii].ou[kk] = eff.ou[kk];
                            facelist[ii].ov[kk] = eff.ov[kk];
                        }
                    }
                }

                // Alloc'n'Copy textures
                if (af3Ddh.nb_maps > 0)
                {
                    texturecontainer = new TextureContainer[af3Ddh.nb_maps];

                    for (long i = 0; i < af3Ddh.nb_maps; i++)
                    {
                        texturecontainer[i] = new TextureContainer(ftlFile);
                    }
                }

                // Alloc'n'Copy groups
                if (nbgroups > 0)
                {
                    grouplist = new EERIE_GROUPLIST[nbgroups];

                    for (long i = 0; i < nbgroups; i++)
                    {
                        grouplist[i] = new EERIE_GROUPLIST(ftlFile);
                    }

                    for (long i = 0; i < nbgroups; i++)
                    {
                        if (grouplist[i].nb_index > 0)
                        {
                            grouplist[i].indexes = new long[grouplist[i].nb_index];

                            for (long ii = 0; ii < grouplist[i].nb_index; ii++)
                            {
                                grouplist[i].indexes[ii] = ftlFile.ReadInt64();
                            }
                        }
                    }
                }

                // Alloc'n'Copy action points
                if (nbaction > 0)
                {
                    actionlist = new EERIE_ACTIONLIST[nbaction];
                    
                    for (long i = 0; i < nbaction; i++)
                    {
                        actionlist[i] = new EERIE_ACTIONLIST(ftlFile);
                    }
                }

                // Alloc'n'Copy selections
                if (nbselections > 0)
                {
                    selections = new EERIE_SELECTIONS[nbselections];

                    for (long i = 0; i < af3Ddh.nb_selections; i++)
                    {
                        selections[i] = new EERIE_SELECTIONS(ftlFile);
                        selections[i].selected = new long[selections[i].nb_selected];
                    }

                    for (long i = 0; i < af3Ddh.nb_selections; i++)
                    {
                        for (long ii = 0; ii < selections[i].nb_selected; ii++)
                        {
                            selections[i].selected[ii] = ftlFile.ReadInt64();
                        }
                    }
                }

                //pbox = null;
            }

            // Alloc'n'Copy Collision Spheres Data
            if (afsh.offset_collision_spheres != -1)
            {
                ARX_FTL_COLLISION_SPHERES_DATA_HEADER afcsdh = new ARX_FTL_COLLISION_SPHERES_DATA_HEADER(ftlFile);

                sdata = new COLLISION_SPHERES_DATA();
                sdata.nb_spheres = afcsdh.nb_spheres;
                sdata.spheres = new COLLISION_SPHERE[sdata.nb_spheres];

                for (long i = 0; i < sdata.nb_spheres; i++)
                {
                    sdata.spheres[i] = new COLLISION_SPHERE(ftlFile);
                }
            }

            // Alloc'n'Copy Progressive DATA
            if (afsh.offset_progressive_data != -1)
            {
                ARX_FTL_PROGRESSIVE_DATA_HEADER afpdh = new ARX_FTL_PROGRESSIVE_DATA_HEADER(ftlFile);
            }

            // Alloc'n'Copy Clothes DATA
            if (afsh.offset_clothes_data != -1)
            {
                cdata = new CLOTHES_DATA();

                ARX_FTL_CLOTHES_DATA_HEADER afcdh = new ARX_FTL_CLOTHES_DATA_HEADER(ftlFile);
                cdata.nb_cvert = (short)afcdh.nb_cvert;
                cdata.nb_springs = (short)afcdh.nb_springs;

                // now load cvert
                cdata.cvert = new CLOTHESVERTEX[cdata.nb_cvert];
                cdata.backup = new CLOTHESVERTEX[cdata.nb_cvert];

                for (long i = 0; i < cdata.nb_cvert; i++)
                {
                    cdata.cvert[i] = cdata.backup[i] = new CLOTHESVERTEX(ftlFile);
                }

                // now load springs
                cdata.springs = new EERIE_SPRINGS[cdata.nb_springs];

                for (long i = 0; i < cdata.nb_springs; i++)
                {
                    cdata.springs[i] = new EERIE_SPRINGS(ftlFile);
                }
            }

            // Should be read completely now
            if (ftlFile.fileStream.Length - ftlFile.GetFilePosition() > 0)
            {
                MainWindow.LogMessage("[ERROR] File wasnt read completely.");
            }

            EERIE_OBJECT_CenterObjectCoordinates(this);
            EERIE_CreateCedricData(this);
            var dshs = 0;
        }

        private void EERIE_CreateCedricData(EERIE_3DOBJ eobj)
        {
            eobj.c_data = new EERIE_C_DATA();

            if (eobj.nbgroups <= 0)
            {
                eobj.c_data.nb_bones = 1;
                eobj.c_data.bones = new EERIE_BONE[eobj.c_data.nb_bones];

                for (long i = 0; i < eobj.c_data.nb_bones; i++)
                {
                    eobj.c_data.bones[i] = new EERIE_BONE();
                }

                for (long i = 0; i < eobj.nbvertex; i++)
                {
                    AddIdxToBone(eobj.c_data.bones[0], i);
                }

                eobj.c_data.bones[0].quatinit = new Quaternion(0, 0, 0, 1);
                eobj.c_data.bones[0].quatanim = new Quaternion(0, 0, 0, 1);
                eobj.c_data.bones[0].scaleinit = new Vector3(0, 0, 0);
                eobj.c_data.bones[0].scaleanim = new Vector3(0, 0, 0);
                eobj.c_data.bones[0].transinit = new Vector3(0, 0, 0);
                eobj.c_data.bones[0].transinit_global = eobj.c_data.bones[0].transinit;
                eobj.c_data.bones[0].original_group = null;
                eobj.c_data.bones[0].father = -1;
                goto lasuite;
            }

            eobj.c_data.nb_bones = eobj.nbgroups;
            eobj.c_data.bones = new EERIE_BONE[eobj.c_data.nb_bones];

            for (long i = 0; i < eobj.c_data.nb_bones; i++)
            {
                eobj.c_data.bones[i] = new EERIE_BONE();
            }

            byte[] temp = new byte[eobj.nbvertex];

            for (long i = eobj.nbgroups - 1; i >= 0; i--)
            {
                EERIE_VERTEX v_origin = eobj.vertexlist[eobj.grouplist[i].origin];

                for (long j = 0; j < eobj.grouplist[i].nb_index; j++)
                {
                    if (temp[eobj.grouplist[i].indexes[j]] == 0) // if not initialized
                    {
                        temp[eobj.grouplist[i].indexes[j]] = 1;
                        AddIdxToBone(eobj.c_data.bones[i], eobj.grouplist[i].indexes[j]);
                    }
                }

                eobj.c_data.bones[i].quatinit = new Quaternion(0, 0, 0, 1);
                eobj.c_data.bones[i].quatanim = new Quaternion(0, 0, 0, 1);
                eobj.c_data.bones[i].scaleinit = new Vector3(0, 0, 0);
                eobj.c_data.bones[i].scaleanim = new Vector3(0, 0, 0);

                Vector3 TempV = eobj.c_data.bones[i].transinit;
                TempV.X = v_origin.v.X;
                TempV.Y = v_origin.v.Y;
                TempV.Z = v_origin.v.Z;
                eobj.c_data.bones[i].transinit = TempV;
                eobj.c_data.bones[i].transinit_global = eobj.c_data.bones[i].transinit;
                eobj.c_data.bones[i].original_group = eobj.grouplist[i];
                eobj.c_data.bones[i].father = GetFather(eobj, eobj.grouplist[i].origin, i - 1);
            }

            // Try to correct lonely vertex
            for (long i = 0; i < eobj.nbvertex; i++)
            {
                long ok = 0;

                for (long j = 0; j < eobj.nbgroups; j++)
                {
                    var A = eobj.grouplist[j];

                    for (long k = 0; k < A.nb_index; k++)
                    {
                        if (A.indexes[k] == i)
                        {
                            ok = 1;
                            break;
                        }
                    }

                    if (ok == 1)
                        break;
                }

                if (ok == 0)
                {
                    AddIdxToBone(eobj.c_data.bones[0], i);
                }
            }

            for (long i = eobj.nbgroups - 1; i >= 0; i--)
            {
                if (eobj.c_data.bones[i].father >= 0)
                {
                    Vector3 tempV = eobj.c_data.bones[i].transinit;
                    tempV.X -= eobj.c_data.bones[eobj.c_data.bones[i].father].transinit.X;
                    tempV.Y -= eobj.c_data.bones[eobj.c_data.bones[i].father].transinit.Y;
                    tempV.Z -= eobj.c_data.bones[eobj.c_data.bones[i].father].transinit.Z;
                    eobj.c_data.bones[i].transinit = tempV;
                }

                eobj.c_data.bones[i].transinit_global = eobj.c_data.bones[i].transinit;
            }


        lasuite:
            ;
	        {
		        EERIE_C_DATA obj = eobj.c_data;

		        for (int i = 0; i != obj.nb_bones; i++)
		        {
			        Quaternion qt1 = new Quaternion();

			        if (obj.bones[i].father >= 0)
			        {
				        /* Rotation*/
				        qt1 = new Quaternion(obj.bones[i].quatinit.X, obj.bones[i].quatinit.Y, obj.bones[i].quatinit.Z, obj.bones[i].quatinit.W);
                        obj.bones[i].quatanim = Quat_Multiply(obj.bones[obj.bones[i].father].quatanim, qt1);
                        /* Translation */
                        obj.bones[i].transanim = TransformVertexQuat(obj.bones[obj.bones[i].father].quatanim, obj.bones[i].transinit);
				        obj.bones[i].transanim = obj.bones[obj.bones[i].father].transanim + obj.bones[i].transanim;
				        /* Scale */
				        obj.bones[i].scaleanim = new Vector3(1.0f, 1.0f, 1.0f);
			        }
			        else
			        {
				        /* Rotation*/
				        obj.bones[i].quatanim = new Quaternion(obj.bones[i].quatinit.X, obj.bones[i].quatinit.Y, obj.bones[i].quatinit.Z, obj.bones[i].quatinit.W);
				        /* Translation */
				        obj.bones[i].transanim = new Vector3(obj.bones[i].transinit.X, obj.bones[i].transinit.Y, obj.bones[i].transinit.Z);
				        /* Scale */
				        obj.bones[i].scaleanim = new Vector3(1.0f, 1.0f, 1.0f);
			        }
		        }

		        eobj.vertexlocal = new Vector3[eobj.nbvertex];
		        
                for (int i = 0; i < eobj.nbvertex; i++)
                {
                    eobj.vertexlocal[i] = new Vector3();
                }

		        for (int i = 0; i != obj.nb_bones; i++)
		        {
			        Vector3	vector = new Vector3(obj.bones[i].transanim.X, obj.bones[i].transanim.Y, obj.bones[i].transanim.Z);

			        for (int v = 0; v != obj.bones[i].nb_idxvertices; v++)
			        {
                        EERIE_VERTEX inVert = eobj.vertexlist[obj.bones[i].idxvertices[v]];
                        Vector3 outVert = eobj.vertexlocal[obj.bones[i].idxvertices[v]];
                        Vector3 TempV = new Vector3();

                        TempV = inVert.v - vector;
                        TempV = TransformInverseVertexQuat(obj.bones[i].quatanim, TempV);
                        eobj.vertexlocal[obj.bones[i].idxvertices[v]] = TempV;
                    }
		        }
	        }
        }

        private Vector3 TransformInverseVertexQuat(Quaternion quat, Vector3 vertexin)
        {
            Vector3 vector = new Vector3();
            Quaternion rev_quat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
            rev_quat = Quat_Reverse(rev_quat);

            float x = vertexin.X;
            float y = vertexin.Y;
            float z = vertexin.Z;

            float qx = rev_quat.X;
            float qy = rev_quat.Y;
            float qz = rev_quat.Z;
            float qw = rev_quat.W;

            float rx = x * qw - y * qz + z * qy;
            float ry = y * qw - z * qx + x * qz;
            float rz = z * qw - x * qy + y * qx;
            float rw = x * qx + y * qy + z * qz;

            vector.X = qw * rx + qx * rw + qy * rz - qz * ry;
            vector.Y = qw * ry + qy * rw + qz * rx - qx * rz;
            vector.Z = qw * rz + qz * rw + qx * ry - qy * rx;

            return vector;
        }

        private Quaternion Quat_Reverse(Quaternion q)
        {
            return Quat_Divide(q, new Quaternion(0, 0, 0, 1));
        }

        private Quaternion Quat_Divide(Quaternion q1, Quaternion q2)
        {
            Quaternion quaternion = new Quaternion();

            quaternion.X = q1.W * q2.X - q1.X * q2.W - q1.Y * q2.Z + q1.Z * q2.Y;
            quaternion.Y = q1.W * q2.Y - q1.Y * q2.W - q1.Z * q2.X + q1.X * q2.Z;
            quaternion.Z = q1.W * q2.Z - q1.Z * q2.W - q1.X * q2.Y + q1.Y * q2.X;
            quaternion.W = q1.W * q2.W + q1.X * q2.X + q1.Y * q2.Y + q1.Z * q2.Z;

            return quaternion;
        }

        private Vector3 TransformVertexQuat(Quaternion quat, Vector3 vertexin)
        {
            Vector3 vector = new Vector3();

            float rx = vertexin.X * quat.W - vertexin.Y * quat.Z + vertexin.Z * quat.Y;
            float ry = vertexin.Y * quat.W - vertexin.Z * quat.X + vertexin.X * quat.Z;
            float rz = vertexin.Z * quat.W - vertexin.X * quat.Y + vertexin.Y * quat.X;
            float rw = vertexin.X * quat.X + vertexin.Y * quat.Y + vertexin.Z * quat.Z;

            vector.X = quat.W * rx + quat.X * rw + quat.Y * rz - quat.Z * ry;
            vector.Y = quat.W * ry + quat.Y * rw + quat.Z * rx - quat.X * rz;
            vector.Z = quat.W * rz + quat.Z * rw + quat.X * ry - quat.Y * rx;

            return vector;
        }

        private Quaternion Quat_Multiply(Quaternion q1, Quaternion q2)
        {
            Quaternion quaternion = new Quaternion();
            quaternion.X = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
            quaternion.Y = q1.W * q2.Y + q1.Y * q2.W + q1.Z * q2.X - q1.X * q2.Z;
            quaternion.Z = q1.W * q2.Z + q1.Z * q2.W + q1.X * q2.Y - q1.Y * q2.X;
            quaternion.W = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;

            return quaternion;
        }

        private long GetFather(EERIE_3DOBJ eobj, long origin, long startgroup)
        {
            for (long i = startgroup; i >= 0; i--)
            {
                for (long j = 0; j < eobj.grouplist[i].nb_index; j++)
                {
                    if (eobj.grouplist[i].indexes[j] == origin)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private void AddIdxToBone(EERIE_BONE bone, long idx)
        {
            //bone.idxvertices = new long[bone.nb_idxvertices + 1]; // reallocate not new+
            if (bone.idxvertices == null)
            {
                bone.idxvertices = new long[bone.nb_idxvertices + 1];
            }
            else
            {
                long[] New = bone.idxvertices;
                Array.Resize<long>(ref New, (int)bone.nb_idxvertices + 1);
                bone.idxvertices = New;
            }

            if (bone.idxvertices != null)
            {
                bone.idxvertices[bone.nb_idxvertices] = idx;
                bone.nb_idxvertices++;
            }
        }

        private void EERIE_OBJECT_CenterObjectCoordinates(EERIE_3DOBJ ret)
        {
            Vector3 offset = ret.vertexlist[ret.origin].v;

            if ((offset.X == 0) && (offset.Y == 0) && (offset.Z == 0))
                return;

            for (long i = 0; i < ret.nbvertex; i++)
            {
                Vector3 tempV = ret.vertexlist[i].v;
                tempV.X -= offset.X;
                tempV.Y -= offset.Y;
                tempV.Z -= offset.Z;
                ret.vertexlist[i].v = tempV;

                tempV = ret.vertexlist[i].vert.v;
                tempV.X -= offset.X;
                tempV.Y -= offset.Y;
                tempV.Z -= offset.Z;
                ret.vertexlist[i].vert.v = tempV;

                tempV = ret.vertexlist3[i].v;
                tempV.X -= offset.X;
                tempV.Y -= offset.Y;
                tempV.Z -= offset.Z;
                ret.vertexlist3[i].v = tempV;

                tempV = ret.vertexlist3[i].vert.v;
                tempV.X -= offset.X;
                tempV.Y -= offset.Y;
                tempV.Z -= offset.Z;
                ret.vertexlist3[i].vert.v = tempV;

                tempV = ret.vertexlist3[i].v;
                tempV.X -= offset.X;
                tempV.Y -= offset.Y;
                tempV.Z -= offset.Z;
                ret.vertexlist3[i].v = tempV;

                tempV = ret.vertexlist3[i].vert.v;
                tempV.X -= offset.X;
                tempV.Y -= offset.Y;
                tempV.Z -= offset.Z;
                ret.vertexlist3[i].vert.v = tempV;
            }

            Vector3 temp = ret.point0;
            temp.X -= offset.X;
            temp.Y -= offset.Y;
            temp.Z -= offset.Z;
            ret.point0 = temp;
        }
    }
}
