using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public class ActorModel
    {

        public enum ModelEffect
        {
            DefaultLighting,
            Fog,
            Player,
        };

        public Model model;
        public Vector3 boxSize; // distance from edge to edge
        public Vector3 boxExtents; // distance from centre to edge

        private static Matrix worldMatrix;

        public bool blocksMove; // characters can't move through this
        public bool blocksVis; // stops enemy vision

        public ActorModel(Model newModel, bool blocksCharacterMovement, bool blocksEnemyVision, ModelEffect effect)
        {
            model = newModel;
            
            boxSize = calculateBoundingBox();
            boxExtents = boxSize / new Vector3(2f, 2f, 2f);

            blocksMove = blocksCharacterMovement;
            blocksVis = blocksEnemyVision;

            switch(effect)
            {
                case ModelEffect.Fog:
                    (model.Meshes[0].Effects[0] as BasicEffect).View = Matrix.CreateLookAt(new Vector3(0, 8, 22), Vector3.Zero, Vector3.Up);
                    (model.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
                    (model.Meshes[0].Effects[0] as BasicEffect).SpecularColor = Vector3.Zero;
                    (model.Meshes[0].Effects[0] as BasicEffect).PreferPerPixelLighting = true;
                    (model.Meshes[0].Effects[0] as BasicEffect).FogColor = Color.Black.ToVector3();
                    (model.Meshes[0].Effects[0] as BasicEffect).FogEnabled = true;
                    (model.Meshes[0].Effects[0] as BasicEffect).FogStart = 0.0f;
                    (model.Meshes[0].Effects[0] as BasicEffect).FogEnd = 750.0f;
                    break;

                case ModelEffect.DefaultLighting:
                    (model.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
                    break;

                case ModelEffect.Player:
                    (model.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
                    (model.Meshes[0].Effects[0] as BasicEffect).LightingEnabled = true;
                    break;
            }
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
