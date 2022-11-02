using Arx_Model_Exporter.Structures.EERIE;
using Arx_Model_Exporter.Structures.FTLFile;
using SharpGLTF.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Quaternion = System.Numerics.Quaternion;

namespace Arx_Model_Exporter.Structures.GLTFFile
{
    public class GLTFSkeleton
    {
        private int count { get; set; }
        public NodeBuilder[] binds { get; private set; }

        public GLTFSkeleton(EERIE_3DOBJ eerieObj)
        {
            EERIE_C_DATA skeleton = eerieObj.c_data;
            binds = new NodeBuilder[skeleton.nb_bones];
            EERIE_BONE SkeletonRoot = skeleton.bones.Where(b => b.father == -1).Single();
            NodeBuilder Root = new NodeBuilder(new string(SkeletonRoot.original_group.name));

            //Matrix4x4.Invert(Root.WorldMatrix, out Matrix4x4 inverse);

            if (SkeletonRoot.quatinit != new Quaternion(0, 0, 0, 1)) Root.WithLocalRotation(SkeletonRoot.quatinit);
            if (SkeletonRoot.transinit != new Vector3(0, 0, 0)) Root.WithLocalTranslation(SkeletonRoot.transinit);
            if (SkeletonRoot.scaleinit != new Vector3(0, 0, 0)) Root.WithLocalScale(SkeletonRoot.scaleinit);

            //Matrix4x4 matrix = Root.LocalMatrix * inverse;
            //if (matrix.M44 != 1.0f) matrix.M44 = 1.0f;
            //Root.LocalMatrix = matrix;

            binds[count] = Root;
            SkeletonRoot.bindID = count;
            count++;
            BuildSkeleton(0, Root, skeleton);
            
            //NodeBuilder Fake = new NodeBuilder("Fake");
            //Fake.AddNode(Root);
            //Root.WithLocalRotation(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI));
        }

        private void BuildSkeleton(int parentIndex, NodeBuilder parentNode, EERIE_C_DATA skeleton)
        {
            for (int i = 0; i < skeleton.nb_bones; i++)
            {
                EERIE_BONE Child = skeleton.bones[i];
                
                if (Child.father == parentIndex)
                {
                    NodeBuilder child = parentNode.CreateNode(new string(Child.original_group.name));

                    //Matrix4x4.Invert(child.WorldMatrix, out Matrix4x4 inverse);

                    if (Child.quatinit != new Quaternion(0, 0, 0, 1)) child.WithLocalRotation(Child.quatinit);
                    if (Child.transinit != new Vector3(0, 0, 0)) child.WithLocalTranslation(Child.transinit);
                    if (Child.scaleinit != new Vector3(0, 0, 0)) child.WithLocalScale(Child.scaleinit);

                    //Matrix4x4 matrix = child.LocalMatrix * inverse;
                    //if (matrix.M44 != 1.0f) matrix.M44 = 1.0f;
                    //child.LocalMatrix = matrix;

                    binds[count] = child;
                    Child.bindID = count;
                    count++;
                    BuildSkeleton(i, child, skeleton);
                }
            }
        }
    }
}
