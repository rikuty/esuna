using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    public class MegaSplatMeshProcessor
    {
        private List<Vector3> vertices;
        private List<int> tri;
        private List<Color32> col;
        private List<Vector3> normals;
        private List<Vector4> uv;
        private List<Vector4> uv2;
        private List<Vector4> uv3;
        private List<Vector4> uv4;
        private List<Vector4> tangents;

        public void Process(List<Vector3> vertices, List<int> tri, List<Color32> col, List<Vector3> normals, List<Vector4> uv, List<Vector4> uv2, List<Vector4> uv3, List<Vector4> uv4, List<Vector4> tangents)
        {
            this.vertices = vertices;
            this.tri = tri;
            this.col = col;
            this.normals = normals;
            this.uv = uv;
            this.uv2 = uv2;
            this.uv3 = uv3;
            this.uv4 = uv4;
            this.tangents = tangents;

            // PROCESSING MESH - MegaSplat needs to have 3 different vertex colors at each face-vertex

            int one, two, three, oneVrgb, twoVrgb, threeVrgb;
            var baseIndexCount = tri.Count;
            for (var l = 0; l < baseIndexCount; l += 3) {
                one = tri[l];
                two = tri[l + 1]; //improve readability...(and we would have to calculate that >30 times otherwise)
                three = tri[l + 2];
                oneVrgb = Vrgb(col[one]); // improves speed by about 5%
                twoVrgb = Vrgb(col[two]);
                threeVrgb = Vrgb(col[three]);

                /* first case: empty. Great that's easy */

                if (oneVrgb + twoVrgb + threeVrgb == 0) { //this case actually happens often, so we process this first (all colors are black)
                    col[one] = new Color32(255, 0, 0, col[one].a); // so we set one to red...
                    col[two] = new Color32(0, 255, 0, col[two].a); // one to green...
                    col[three] = new Color32(0, 0, 255, col[three].a); // guess
                } else {
                    /* at first fill colors when possible */
                    if (oneVrgb == 0) col[one] = GetNotUsed(col[two], col[three], col[one].a);
                    if (twoVrgb == 0) col[two] = GetNotUsed(col[one], col[three], col[two].a);
                    if (threeVrgb == 0) col[three] = GetNotUsed(col[two], col[one], col[three].a);
                    /* at this point a quad is happy. But what when our mesh hits itself? Yes in this case we do not have all colors in our triangle.  (like r-r-b, no green, that makes megasplat sad) */

                    /* until this point we could resolve the problem by just setting the color. Now we need to create vertices */

                    if (oneVrgb + twoVrgb + threeVrgb != 7) // 7 is the value of rgb 'bitmask'
                    {
                        if (oneVrgb == twoVrgb && twoVrgb == threeVrgb) //all vertices have same color
                        {
                            if (col[one].r != 0) // the 'same color' is red....great
                            {
                                CloneVertexTo(l + 1, new Color32(0, 255, 0, col[two].a));
                                CloneVertexTo(l + 2, new Color32(0, 0, 255, col[three].a));
                            }

                            if (col[one].g != 0) {
                                CloneVertexTo(l + 1, new Color32(255, 0, 0, col[two].a));
                                CloneVertexTo(l + 2, new Color32(0, 0, 255, col[three].a));
                            }

                            if (col[one].b != 0) {
                                CloneVertexTo(l + 1, new Color32(255, 0, 0, col[two].a));
                                CloneVertexTo(l + 2, new Color32(0, 255, 0, col[three].a));
                            }
                        } else // only two vertices are the same color
                        {
                            if (oneVrgb == twoVrgb) //its the first and the second
                            {
                                CloneVertexTo(l + 1, GetNotUsed(col[one], col[three], col[two].a)); // set the second vertex to the color the first and third do not have
                            }

                            if (oneVrgb == threeVrgb) // and so on
                            {
                                CloneVertexTo(l + 2, GetNotUsed(col[one], col[two], col[three].a));
                            }

                            if (twoVrgb == threeVrgb) // and so on
                            {
                                CloneVertexTo(l + 2, GetNotUsed(col[two], col[one], col[three].a));
                            }
                        }
                    }
                }
            }
        }

        private void CloneVertexTo(int index, Color32 to)
        {
            var vIdx = tri[index];
            vertices.Add(vertices[vIdx]);
            normals.Add(normals[vIdx]);
            col.Add(to);
            uv.Add(uv[vIdx]);
            uv2.Add(uv2[vIdx]);
            uv3.Add(uv3[vIdx]);
            uv4.Add(uv4[vIdx]);
            tangents.Add(tangents[vIdx]);
            tri[index] = vertices.Count - 1; //remap to clone 
        }

        private static Color32 GetNotUsed(Color32 a, Color32 b, byte x)
        {
            if (a.r == 0 && b.r == 0) return new Color32(255, 0, 0, x);
            if (a.g == 0 && b.g == 0) return new Color32(0, 255, 0, x);
            if (a.b == 0 && b.b == 0) return new Color32(0, 0, 255, x);
            
            // this face is completely invalid (double color)
            UDebug.Fatal("Unexpected vertex color error. Coloring algorithm made something wrong.");

            return new Color(0, 0, 0, x);
        }


        // returns a bit-int in following format [col-1-has-red][col-1-has-green][col-1-has-blue][col-2-has-red][col-2-has-green][col-2-has-blue]...
        private static int Vrgb(Color32 c)
        {
            return (c.r > 0 ? 1 : 0) + (c.g > 0 ? 2 : 0) + (c.b > 0 ? 4 : 0);
        }

        private static int Vrgb(Color32 c, Color32 c2, Color c3)
        {
            return (c.r > 0 ? 1 : 0) + (c.g > 0 ? 2 : 0) + (c.b > 0 ? 4 : 0) +
                   (c2.r > 0 ? 8 : 0) + (c2.g > 0 ? 16 : 0) + (c2.b > 0 ? 32 : 0) +
                   (c3.r > 0 ? 64 : 0) + (c3.g > 0 ? 128 : 0) + (c3.b > 0 ? 256 : 0);
        }
    }
}