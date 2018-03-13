using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public class ActorModel
    {
        public Model model;
        public Vector3 boxSize; // distance from edge to edge
        public Vector3 boxExtents; // distance from centre to edge

        private static Matrix worldMatrix;

        public bool blocksMove; // characters can't move through this
        public bool blocksVis; // stops enemy vision

        public ActorModel(Model newModel, bool blocksCharacterMovement, bool blocksEnemyVision)
        {
            model = newModel;
            
            boxSize = calculateBoundingBox();
            boxExtents = boxSize / new Vector3(2f, 2f, 2f);

            blocksMove = blocksCharacterMovement;
            blocksVis = blocksEnemyVision;
        }

        Vector3 calculateBoundingBox()
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldMatrix);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            return max - min;
        }

        public bool blocksMovement
        {
            get
            {
            return blocksMove;
            }
            set
            {
                blocksMove = value;
            }
        }

        public bool blocksVision
        {
            get
            {
                return blocksVis;
            }
            set
            {
                blocksVis = value;
            }
        }

        public void resizeHitbox(Vector3 multiplier)
        {
            boxSize = boxSize * multiplier;
            boxExtents = boxExtents * multiplier;
        }

        public static void setWorldMatrix(Matrix newWorldMatrix)
        {
            worldMatrix = newWorldMatrix;
        }
    }
}
