﻿using System.Drawing;

namespace RayTracing
{
    public static class RayTracer
    {
        public static Task StartRender(Color[,] image, World world, Camera camera, Int2D startingPixelIndex, Int2D count, bool startImmediately)
        {
            Task task = new Task(() => Render(image, world, camera, startingPixelIndex, count));

            if (startImmediately)
                task.Start();

            return task;
        }

        public static void Render(Color[,] image, World world, Camera camera, Int2D startingPixelIndex, Int2D pixelCount)
        {
            if (world == null)
                throw new NullReferenceException();

            if (camera == null)
                throw new NullReferenceException();

            Int2D res = camera.Resolution;

            if (startingPixelIndex.x < 0 || startingPixelIndex.y < 0)
                throw new IndexOutOfRangeException();

            if (startingPixelIndex.x + pixelCount.x > res.x || startingPixelIndex.y + pixelCount.y > res.y)
                throw new IndexOutOfRangeException();

            for (int i = 0; i < pixelCount.x; i++)
            {
                for (int j = 0; j < pixelCount.y; j++)
                {
                    Camera cam = world.GetMainCamera();
                    Ray[] emmitedRays = cam.Shader.GetEmmitedRays(cam, new Int2D(startingPixelIndex.x + i, startingPixelIndex.y + j));

                    EmmisionChain[][] raysReachingPixel = new EmmisionChain[emmitedRays.Length][];

                    if (startingPixelIndex.x + i == 490 && startingPixelIndex.y + j == 590)
                        System.Diagnostics.Debugger.Break();
                    
                    for (int k = 0; k < raysReachingPixel.Length; k++)
                        raysReachingPixel[k] = StartTrace(emmitedRays[k], world, cam);

                    image[startingPixelIndex.x + i, startingPixelIndex.y + j] = cam.Shader.CalculateFinalPixelColor(cam, new Int2D(startingPixelIndex.x + i, startingPixelIndex.y + j), raysReachingPixel);
                }
            }
        }

        public static EmmisionChain[] StartTrace(Ray reverseRay, World world, Camera camera)
        {
            return Trace(world, camera, camera.BounceLimit, reverseRay, out Vector3D _);
        }

        public static EmmisionChain[] StartTrace(World world, Camera camera, int bounces, Ray tracingRay)
        {
            return Trace(world, camera, bounces, tracingRay, out Vector3D _);
        }

        private static EmmisionChain[] Trace(World world, Camera renderCamera, int bouncesRemaining, Ray tracingRay, out Vector3D poc)
        {
            poc = default;
            return Trace(world, renderCamera, bouncesRemaining, tracingRay);
        }

        private static EmmisionChain[] Trace(World world, Camera renderCamera, int bouncesRemaining, Ray tracingRay)
        {
            Vector3D truePOC;
            Shape hitShape = world.ClosestShapeHit(tracingRay, out truePOC);

            Vector3D pointOfContact;

            if (hitShape == null)
                pointOfContact = tracingRay.Origin + tracingRay.DirectionReversed * float.PositiveInfinity;
            else
                pointOfContact = truePOC + hitShape.Shader.CalculateNormal(hitShape, truePOC) * Vector3D.EPSILON;

            bool calcLighting = hitShape != null;
            bool calcBounces = (hitShape != null) && (bouncesRemaining > 0);
            bool calcSkybox = hitShape == null;
            
//            if (bouncesRemaining == 0)
//                System.Console.WriteLine();

            LinkedList<EmmisionChain> hittingRays = new LinkedList<EmmisionChain>();

            if (calcLighting)
            {
                for (int i = 0; i < world.LightSourcesCount; i++)
                {
                    LightSource light = world.GetLightSource(i);
                    ColoredRay reachingRay = light.GetReachingRays(world, pointOfContact);
                    hittingRays.AddLast(new EmmisionChain(light, reachingRay, null));
                }
            }

            if (calcSkybox)
            {
                /*
                RTColor reachingRayClr = renderCamera.NoHitColor;
                ColoredRay reachingRay = new ColoredRay(pointOfContact, tracingRay.DirectionReversed, tracingRay.Origin, reachingRayClr, reachingRayClr);
                hittingRays.AddLast(new EmmisionChain(renderCamera, reachingRay, null));
                */
            }

            if (calcBounces)
            {
                Ray[] bouncedTracingRays = hitShape.Shader.GetOutgoingRays(hitShape, tracingRay, pointOfContact);
                for (int i = 0; i < bouncedTracingRays.Length; i++)
                {
                    EmmisionChain[] bouncedIncomingRays = Trace(world, renderCamera, bouncesRemaining - 1, bouncedTracingRays[i]);
                    for (int j = 0; j < bouncedIncomingRays.Length; j++)
                        hittingRays.AddLast(bouncedIncomingRays[j]);
                }
            }

            if (hitShape != null)
            {
                var hittingRaysArray = hittingRays.ToArray();
               
                RTColor finalColor = hitShape.Shader.CalculateBounceColor(hitShape, hittingRaysArray, truePOC, tracingRay.DirectionReversed);
                RTColor destColor = hitShape.Shader.CalculateDestinationColor(finalColor, truePOC, tracingRay.Origin);

                return new EmmisionChain[] { new EmmisionChain(hitShape, new ColoredRay(pointOfContact, tracingRay.DirectionReversed, tracingRay.Origin, finalColor, destColor), hittingRaysArray) };
            }

            return hittingRays.ToArray();
        }

        /*
        private static EmmisionChain Trace(World world, Camera renderCamera, int bouncesRemaining, Ray tracingRay, out Vector3D tracingRayHitPoint)
        {
            Vector3D truePOC = default(Vector3D);
            Shape closestShape = world.ClosestShapeHit(tracingRay, out truePOC);

            if (closestShape != null)
            {
                Vector3D normal = closestShape.Shader.CalculateNormal(closestShape, truePOC);
                tracingRayHitPoint = truePOC + (normal * Vector3D.EPSILON);

                int c = 0;
                EmmisionChain[] incomingRays = null;

                if (bouncesRemaining > 0)
                {
                    Ray[] outgoingRays = closestShape.Shader.GetOutgoingRays(closestShape, tracingRay, truePOC);
                    if (outgoingRays != null)
                    {
                        incomingRays = new EmmisionChain[world.LightSourcesCount + outgoingRays.Length];
                        for (int i = 0; i < outgoingRays.Length; i++)
                        {
                            Vector3D incomingRaySource;
                            incomingRays[c++] = Trace(world, renderCamera, bouncesRemaining - 1, new Ray(tracingRayHitPoint, outgoingRays[i].Direction), out incomingRaySource);
                        }
                    }
                }

                if (incomingRays == null)
                    incomingRays = new EmmisionChain[world.LightSourcesCount];

                for (int i = 0; i < world.LightSourcesCount; i++)
                {
                    LightSource ls = world.GetLightSource(i);
                    incomingRays[c++] = new EmmisionChain(ls, ls.ReachingRays(world, tracingRayHitPoint), null);
                }

                RTColor srcColor = closestShape.Shader.CalculateBounceColor(closestShape, incomingRays, truePOC, tracingRay.DirectionReversed);
                RTColor destinationClr = closestShape.Shader.CalculateDestinationColor(srcColor, tracingRayHitPoint, tracingRay.Origin);

                ColoredRay ray = new ColoredRay(truePOC, tracingRay.DirectionReversed, tracingRay.Origin, srcColor, destinationClr);

                return new EmmisionChain(closestShape, ray, incomingRays);
            }
            else
            {
                if (bouncesRemaining > 0)
                {
                    tracingRayHitPoint = tracingRay.Origin + tracingRay.Direction * float.PositiveInfinity;
                    ColoredRay ray = new ColoredRay(tracingRayHitPoint, tracingRay.DirectionReversed, tracingRay.Origin, renderCamera.NoHitColor, renderCamera.NoHitColor);
                    return new EmmisionChain(renderCamera, ray, null);
                }
                else
                {
                    tracingRayHitPoint = tracingRay.Origin + tracingRay.Direction * float.PositiveInfinity;
                    return new EmmisionChain(null, new ColoredRay(tracingRayHitPoint, tracingRay.DirectionReversed, tracingRay.Origin, RTColor.Black, RTColor.Black), null);
                }
            }
        }
        */
    }
}