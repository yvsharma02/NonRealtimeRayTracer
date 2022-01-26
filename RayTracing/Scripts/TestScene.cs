﻿namespace RayTracing
{
    public class TestScene
    {
        private const string TEST_MESH_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\TextureSphere.obj";
        private const string TEST_TEXTURE_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\BASE_DL.jpg";
        private const string TEST_NORMAL_MAP_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\Normal_DL.jpg";

        private const string TEST_BALL_TEXTURE = @"D:\Projects\VisualStudio\RayTracing\Assets\MetalTexture.png";

        private const string TEST_MESH_2_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\Plane2.obj";

#if RELEASE
        private const bool MULTI_THREADING_ENABLED = true;
#elif DEBUG
        private const bool MULTI_THREADING_ENABLED = false;
#endif

        private const string SAVE_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Generated";

        private const int CHUNKS_X = 16;
        private const int CHUNKS_Y = 16;

        private const int RES_X = 1024;
        private const int RES_Y = 1024;

        private const int RAYS_PER_PIXEL_X = 2;
        private const int RAYS_PER_PIXEL_Y = 2;
         
        private const int BOUCES = 2;

        private World world;

        public TestScene()
        {
            Transformation sphereTransform = new Transformation(new Vector3D(0, -10, 10), new Vector3D(0, 0, 0), new Vector3D(5, 5, 5));
            Transformation planeTransform = new Transformation(new Vector3D(0, -25, 0), new Vector3D(0, 0, 0), new Vector3D(25, 25, 25));
            Transformation cameraTransform = new Transformation(new Vector3D(0, 0, 25), new Vector3D(-15, 0, 0), new Vector3D(200, 200, 25));

            RTColor sunColor = new RTColor(RTColor.MAX_INTENSITY / 2, 1, 1, 1);
            Vector3D sunDir = new Vector3D(0, -1f, -1f);

            world = new World(null, null);

            Camera cam = new Camera(cameraTransform, new Int2D(RES_X, RES_Y), new DefaultPixelShader(new Int2D(RAYS_PER_PIXEL_X, RAYS_PER_PIXEL_Y)), BOUCES, new RTColor(0, 0, 0, 0));

            world.SetMainCamera(cam);

            ShapeShader sphereShader = new AdvanceShapeShader(null, null, 0f, 1f, .09f);
            ShapeShader planeShader = new AdvanceShapeShader(TextureLoader.Load(TEST_TEXTURE_LOCATION), TextureLoader.Load(TEST_NORMAL_MAP_LOCATION), 0f, 1f, 1f);

            MeshBuilder builder = MeshReader.ReadObj(TEST_MESH_LOCATION);
            world.AddShape(builder.Build(sphereTransform, sphereShader, true));
//            world.AddShape(builder.Build(sphereTransform + new Transfomration(new Vector3D(10, 10, 0), default, Vector3D.One), sphereShader, true));
            MeshBuilder builder2 = MeshReader.ReadObj(TEST_MESH_2_LOCATION);
            world.AddShape(builder2.Build(planeTransform, planeShader, false));

            world.AddLightSource(new GlobalLight(new Transformation(new Vector3D(0, 50, 0)), sunDir, sunColor));

            String location = Path.Combine(SAVE_LOCATION, DateTime.Now.ToString().Replace(":", "-") + ".png");
            Renderer.RenderAndWriteToDisk(world, world.GetMainCamera(), new Int2D(CHUNKS_X, CHUNKS_Y), MULTI_THREADING_ENABLED, location);
        }
    }
}