﻿namespace RayTracing
{
    public class TestScene
    {
        private const string TEST_MESH_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\TextureSphere.obj";
        private const string TEST_TEXTURE_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\BASE_DL.jpg";
        private const string TEST_NORMAL_MAP_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\Normal_DL.jpg";

        private const string TEST_MESH_2_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Assets\Plane.obj";

        private const bool MULTI_THREADING_ENABLED = true;

        private const string SAVE_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Generated";

        private const int CHUNKS_X = 16;
        private const int CHUNKS_Y = 16;

        private const int RES_X = 1024;
        private const int RES_Y = 1024;

        private const int RAYS_PER_PIXEL_X = 1;
        private const int RAYS_PER_PIXEL_Y = 1;

        private const int BOUCES = 0;

        private World world;

        /*
        private RTColor CalculateColor(ColoredRay[][] clrs, float iDiv, float rDiv, float gDiv, float bDiv)
        {
            
        }
        */

        public TestScene()
        {
            Transfomration sphereTransform = new Transfomration(new Vector3D(0, -10, 0), new Vector3D(0, 0, 0), new Vector3D(5, 5, 5));
            Transfomration planeTransform = new Transfomration(new Vector3D(0, -25, 0), new Vector3D(0, 0, 0), new Vector3D(25, 25, 25));
            Transfomration cameraTransform = new Transfomration(new Vector3D(0, 0, 25), new Vector3D(-15, 0, 0), new Vector3D(200, 200, 25));

            RTColor sunColor = new RTColor(RTColor.MAX_INTENSITY, 255, 255, 255);
            Vector3D sunDir = new Vector3D(0, -1f, 0f);

            world = new World(null, null);

            Camera cam = new Camera(cameraTransform, new Int2D(RES_X, RES_Y), new DefaultPixelShader(new Int2D(RAYS_PER_PIXEL_X, RAYS_PER_PIXEL_Y)), BOUCES, new RTColor(RTColor.MAX_INTENSITY / 3, 123, 123, 123));

            OldCamera camera = new OldCamera(
                new Vector3D(1, 0, 0),
                new Vector3D(0, 1f, 0f),
                new Vector3D(-100, -100, 0),
                new Vector3D(0, 0, 25),
                200,
                200,
                0,
                new Int2D(RES_X, RES_Y),
                new Int2D(RAYS_PER_PIXEL_X, RAYS_PER_PIXEL_Y),
                BOUCES,
                new RTColor(RTColor.MAX_INTENSITY / 3, 123, 123, 123));

            world.SetMainCamera(cam);

            DefaultShapeShader sphereShader = new DefaultShapeShader(TextureLoader.Load(TEST_TEXTURE_LOCATION), null);
            MeshBuilder builder = MeshReader.ReadObj(TEST_MESH_LOCATION);
            world.AddShape(builder.Build(sphereTransform, sphereShader, true));
            MeshBuilder builder2 = MeshReader.ReadObj(TEST_MESH_2_LOCATION);
            world.AddShape(builder2.Build(planeTransform, new DefaultShapeShader(TextureLoader.Load(TEST_TEXTURE_LOCATION), TextureLoader.Load(TEST_NORMAL_MAP_LOCATION)), false));

            world.AddLightSource(new GlobalLight(new Transfomration(new Vector3D(0, 50, 0)), sunDir, sunColor));
        }

        public void Render()
        {
            string fileName = DateTime.Now.ToString().Replace(":", "-") + ".png";

            Renderer render = new Renderer(new Int2D(CHUNKS_X, CHUNKS_Y), world, Path.Combine(SAVE_LOCATION, fileName));
            render.Render(MULTI_THREADING_ENABLED);
        }
    }
}