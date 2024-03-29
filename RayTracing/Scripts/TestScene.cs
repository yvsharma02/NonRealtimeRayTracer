﻿namespace RayTracing
{
    public class TestScene
    {
        private const string SPHERE_MESH = @"D:\Projects\VisualStudio\RayTracing\Assets\Sphere.obj";
        private const string METAL_BASE = @"D:\Projects\VisualStudio\RayTracing\Assets\MetalBase.jpg";
        private const string METAL_NORMAL = @"D:\Projects\VisualStudio\RayTracing\Assets\MetalNormal.jpg";

        private const string PLANE_MESH = @"D:\Projects\VisualStudio\RayTracing\Assets\Plane.obj";
        private const string BRICKS_BASE = @"D:\Projects\VisualStudio\RayTracing\Assets\BricksBase.jpg";
        private const string BRICKS_NORMAL = @"D:\Projects\VisualStudio\RayTracing\Assets\BricksNormal.jpg";

#if RELEASE
        private const bool MULTI_THREADING_ENABLED = true;
#elif DEBUG
        private const bool MULTI_THREADING_ENABLED = false;
#endif

        private const string SAVE_LOCATION = @"D:\Projects\VisualStudio\RayTracing\Generated";

        private const int CHUNKS_X = 16;
        private const int CHUNKS_Y = 16;

        private const int RES_X = 1920;
        private const int RES_Y = 1080;

        private const int RAYS_PER_PIXEL_X = 8;
        private const int RAYS_PER_PIXEL_Y = 8;

        private const int BOUCES = 1;

        private const int WORD_SUBDIVISION_LEVELS = 0;

        private World world;
        private Camera mainCamera;

        public TestScene()
        {
            Transformation sphere1transform = new Transformation(new Vector3D(0, -10, 10), new Vector3D(0, 0, 0), new Vector3D(5, 5, 5));
            Transformation sphere2transform = sphere1transform + new Transformation(new Vector3D(15, 0, 0));

            Transformation planeTransform = new Transformation(new Vector3D(0, -25, 0), new Vector3D(0, 0) * RTMath.DEG_TO_RAD, new Vector3D(50, 50, 50));
            planeTransform = new Transformation(planeTransform.Position, Quaternion.FromEulerAngles(new Vector3D(0, 0, 0) * RTMath.DEG_TO_RAD), planeTransform.Scale);
            Transformation cameraTransform = new Transformation(new Vector3D(0, 0, 25), new Vector3D(0, 180, 0) * RTMath.DEG_TO_RAD, new Vector3D(200f / 1024f * 1920f, 200f / 1024f * 1080f, 25));

            RTColor sunColor = new RTColor(RTColor.MAX_INTENSITY / 1.5f, 1f, 1f, 1f);
            Vector3D sunDir = new Vector3D(0f, -1f, 0f);

            world = new World(null, WORD_SUBDIVISION_LEVELS, default);

            mainCamera = new Camera(cameraTransform, new Int2D(RES_X, RES_Y), new DefaultPixelShader(new Int2D(RAYS_PER_PIXEL_X, RAYS_PER_PIXEL_Y)), BOUCES);

            world.SetMainCamera(mainCamera);

            ShapeShader sphere1shader = new DefaultShapeShader(TextureLoader.Load(METAL_BASE), TextureLoader.Load(METAL_NORMAL), 0f, .25f, 1f);
            ShapeShader sphere2shader = new DefaultShapeShader(null, null, 0f, 1f, 1f);

            ShapeShader planeShader = new DefaultShapeShader(TextureLoader.Load(BRICKS_BASE), TextureLoader.Load(BRICKS_NORMAL), 0f, 1f, 1f);

            MeshBuilder sphereBuilder = MeshReader.ReadObj(SPHERE_MESH);
            world.AddShapes(sphereBuilder.Build(sphere1transform, sphere1shader, true));
            world.AddShapes(sphereBuilder.Build(sphere2transform, sphere2shader, true));

            MeshBuilder planeBuilder = MeshReader.ReadObj(PLANE_MESH);
            world.AddShapes(planeBuilder.Build(planeTransform, planeShader, false));
            world.AddLightSource(new GlobalLight(new Transformation(new Vector3D(0, 50, 0)), sunDir, sunColor));
        }

        public void RenderAndSave()
        {
            RenderAndSave(Path.Combine(SAVE_LOCATION, DateTime.Now.ToString().Replace(":", "-") + ".png"));
            
        }

        public void RenderAndSave(string location)
        {
            Renderer.RenderAndWriteToDisk(world, mainCamera, new Int2D(CHUNKS_X, CHUNKS_Y), MULTI_THREADING_ENABLED, location);
        }
    }
}