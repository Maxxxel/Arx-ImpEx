using Arx_Model_Exporter.Structures.EERIE;
using Arx_Model_Exporter.Structures.FTLFile;
using Arx_Model_Exporter.Structures.TEAFile;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Arx_Model_Exporter.Structures.GLTFFile
{
    using static System.Net.Mime.MediaTypeNames;
    using static System.Net.WebRequestMethods;
    using Frame = TEAFile.Frame;
    using MESH = MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>;
    using PRIMITIVE = PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexTexture1, VertexJoints4>;
    using VERTEX = VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>;

    public class GLTF
    {
        public ModelRoot? Root { get; }

        public GLTF(FTL ftl, string? modelName, EERIE_ANIM[] animations)
        {
            SceneBuilder Scene = new SceneBuilder("DefaultScene");
            EERIE_VERTEX[] Vertices = ftl.eerieObject.vertexlist;
            PRIMITIVE[] Primitives = new PRIMITIVE[ftl.eerieObject.nbmaps];
            MESH CompleteMesh = new MESH("Mesh");

            for (int TextureIndex = 0; TextureIndex < ftl.eerieObject.nbmaps; TextureIndex++)
            {
                TextureContainer Texture = ftl.eerieObject.texturecontainer[TextureIndex];

                MaterialBuilder Material = new MaterialBuilder("Texture #" + TextureIndex);
                Material.WithChannelImage(KnownChannel.BaseColor, Texture.Img.ToArray());
                Primitives[TextureIndex] = CompleteMesh.UsePrimitive(Material);
            }

            for (int FaceIndex = 0; FaceIndex < ftl.eerieObject.nbfaces; FaceIndex++)
            {
                EERIE_FACE Face = ftl.eerieObject.facelist[FaceIndex];

                if (Face.facetype == 0)
                {
                    MainWindow.LogMessage("[DEBUG] Face is FLAT"); // hmmm
                }
                else if (Face.facetype == 1)
                {
                    // 99% sure it's using Texture
                }
                else if (Face.facetype == 2)
                {
                    MainWindow.LogMessage("[DEBUG] Face is DOUBLE"); // Not used????
                }
                else if (Face.facetype == 3)
                {
                    MainWindow.LogMessage("[DEBUG] FaceType 3 again..."); // WHAT is that???
                }
                else
                {
                    MainWindow.LogMessage($"Undefined facetype: {Face.facetype}.");
                }

                if (Face.texid != -1)
                {
                    PRIMITIVE Primitive = Primitives[Face.texid];

                    EERIE_VERTEX Vertex0 = Vertices[Face.vid[0]];
                    EERIE_VERTEX Vertex1 = Vertices[Face.vid[1]];
                    EERIE_VERTEX Vertex2 = Vertices[Face.vid[2]];

                    IEnumerable<EERIE_BONE>? Result1 = ftl.eerieObject.c_data.bones.Where(bone => bone.idxvertices != null && bone.idxvertices.Contains(Face.vid[0]));
                    IEnumerable<EERIE_BONE>? Result2 = ftl.eerieObject.c_data.bones.Where(bone => bone.idxvertices != null && bone.idxvertices.Contains(Face.vid[1]));
                    IEnumerable<EERIE_BONE>? Result3 = ftl.eerieObject.c_data.bones.Where(bone => bone.idxvertices != null && bone.idxvertices.Contains(Face.vid[2]));

                    if (Result1.Count() > 1 || Result2.Count() > 1 || Result3.Count() > 1) MainWindow.LogMessage("Surprise!!! Some Bones are influenced by more than one vertexGroup.");

                    VERTEX V0 = new VERTEX((Vertex0.v, Vertex0.norm), new VertexTexture1(new Vector2(Face.u[0], Face.v[0])), new (int, float)[]
                    {
                        ((int)Result1.First().bindID, 1f),
                        (-1, 0f),
                        (-1, 0f),
                        (-1, 0f)
                    });
                    VERTEX V1 = new VERTEX((Vertex1.v, Vertex1.norm), new VertexTexture1(new Vector2(Face.u[1], Face.v[1])), new (int, float)[]
                    {
                        ((int)Result2.First().bindID, 1f),
                        (-1, 0f),
                        (-1, 0f),
                        (-1, 0f)
                    });
                    VERTEX V2 = new VERTEX((Vertex2.v, Vertex2.norm), new VertexTexture1(new Vector2(Face.u[2], Face.v[2])), new (int, float)[]
                    {
                        ((int)Result3.First().bindID, 1f),
                        (-1, 0f),
                        (-1, 0f),
                        (-1, 0f)
                    });

                    Primitive.AddTriangle(V0, V1, V2);
                }
                else if (Face.texid == -1)
                {
                    MainWindow.LogMessage("Detected Colored Vertex! TODO...");
                }
            };

            if (ftl.Skeleton != null)
            {
                Scene.AddSkinnedMesh(CompleteMesh, Matrix4x4.Identity, ftl.Skeleton.binds);
            }
            else
            {
                Scene.AddRigidMesh(CompleteMesh, Matrix4x4.Identity);
            }

            if (animations.Length > 0)
            {
                EERIE_3DOBJ eobj = ftl.eerieObject;
                
                for (int animationIndex = 0; animationIndex < animations.Length; animationIndex++)
                {
                    EERIE_ANIM eanim = animations[animationIndex];
                    if (eanim.nb_groups != eobj.nbgroups) continue;

                    for (int b = 0; b < ftl.Skeleton.binds.Length; b++)
                    {
                        NodeBuilder boneNode = ftl.Skeleton.binds[b];
                        Dictionary<float, Vector3> Translation = new Dictionary<float, Vector3>();
                        Dictionary<float, Quaternion> Rotation = new Dictionary<float, Quaternion>();
                        Dictionary<float, Vector3> Scale = new Dictionary<float, Vector3>();

                        for (int j = 0; j < eanim.nb_key_frames; j++)
                        {
                            float keyTime = eanim.frames[j].time / 1000;
                            EERIE_GROUP data = eanim.groups[b + (j * (int)eanim.nb_groups)];

                            if (!data.translate.Equals(Vector3.Zero)) Translation.Add(keyTime, data.translate);
                            if (!data.quat.Equals(Quaternion.Identity)) Rotation.Add(keyTime, data.quat);
                            if (!data.zoom.Equals(Vector3.Zero)) Scale.Add(keyTime, data.zoom);
                        }

                        boneNode
                            .WithLocalRotation(eanim.anim_name, Rotation)
                            .WithLocalTranslation(eanim.anim_name, Translation)
                            .WithLocalScale(eanim.anim_name, Scale);
                    }
                }
            }

            Root = Scene.ToGltf2();
        }
    }
}
