﻿namespace RayTracing
{
    public class AdvanceShapeShader : ShapeShader
    {
        public readonly Texture MainTexture;
        public readonly Texture NormalMap;

        public readonly float Reflectiveness = 0f;
        public readonly float Absorbance = 0f;
        public readonly float TextureStrength = 1f;

        public AdvanceShapeShader(Texture mainTexture, Texture normalMap, float absorbance, float textureStrength, float reflectiveness)
        {
            this.Reflectiveness = reflectiveness;
            this.MainTexture = mainTexture;
            this.NormalMap = normalMap;
            this.Absorbance = absorbance;
            this.TextureStrength = textureStrength;
        }

        public override Vector3D CalculateNormal(Shape shape, Vector3D poc)
        {
            if (NormalMap == null)
                return shape.CalculateNormal(poc);

//            if (!(shape is MeshTriangle))
//            {
                System.Drawing.Color normalColor = NormalMap.GetColorFromUV(shape.CalculateUV(poc));

                Vector3D nmNormal = new Vector3D((normalColor.R / 127.5f) - 1f, (normalColor.G / 127.5f) - 1f, (normalColor.B / 255f));
                Vector3D geomNormal = shape.CalculateNormal(poc).Normalize();

                Vector3D forward = new Vector3D(0, 0, 1);

                Transformation transform = Transformation.CalculateRequiredRotationTransform(Vector3D.Zero, forward, geomNormal);

                return transform.Transform(nmNormal, false, true, false).Normalize();
//            }
            /*
            else
            {
                MeshTriangle triangle = (MeshTriangle)shape;

                float du1 = triangle.UVTriangle[1].x - triangle.UVTriangle[0].x;
                float du2 = triangle.UVTriangle[2].x - triangle.UVTriangle[0].x;

                float dv1 = triangle.UVTriangle[1].y - triangle.UVTriangle[0].y;
                float dv2 = triangle.UVTriangle[2].y - triangle.UVTriangle[0].y;

                float coeff = 1f / (du1 * dv2 - du2 * dv1);

                Vector3D V1 = triangle.VertexTriangle[1] - triangle.VertexTriangle[0];
                Vector3D V2 = triangle.VertexTriangle[2] - triangle.VertexTriangle[1];

                Vector3D t = (V1 * dv2 - V2 * dv1) * coeff;
                Vector3D b = (V2 * du1 - V1 * du2) * coeff;

                return Vector3D.Cross()
            }
            */
        }
        
        public override RTColor CalculateBounceColor(Shape shape, EmmisionChain[] hittingRays, Vector3D pointOfContact, Vector3D outgoingRayDir)
        {
            float lightOnlyIntensity = 0f;
            float totalIntensity = 0f;

            RawRTColor totalColor = RTColor.Black;
            RawRTColor lightOnlyColor = RTColor.Black;

            for (int i = 0; i < hittingRays.Length; i++)
            {
                totalIntensity += hittingRays[i].EmmitedRay.DestinationColor.Intensity;
 
                if (hittingRays[i].LastEmmiter != null && ((hittingRays[i].LastEmmiter.TypeID & (int)TypeID.Light) != 0))
                    lightOnlyIntensity += hittingRays[i].EmmitedRay.DestinationColor.Intensity;
            }

            for (int i = 0; i < hittingRays.Length; i++)
            {
                float lightOnlyWeight = hittingRays[i].EmmitedRay.DestinationColor.Intensity / lightOnlyIntensity;
                float weight = hittingRays[i].EmmitedRay.DestinationColor.Intensity / totalIntensity;

                if (!float.IsNormal(weight))
                    weight = 1f;

                if (!float.IsNormal(lightOnlyWeight))
                    lightOnlyWeight = 1f;

                Vector3D normal = CalculateNormal(shape, pointOfContact);
                float dot = Vector3D.Dot(normal, hittingRays[i].EmmitedRay.Direction * -1f);

                if (dot < 0)
                    dot = 0;

                totalColor += ((RawRTColor) hittingRays[i].EmmitedRay.DestinationColor) * weight * dot;

               if (hittingRays[i].LastEmmiter != null && ((hittingRays[i].LastEmmiter.TypeID & (int)TypeID.Light) != 0))
                    lightOnlyColor += ((RawRTColor) hittingRays[i].EmmitedRay.DestinationColor) * dot * lightOnlyWeight;
            }

            float finalIntensity = lightOnlyIntensity + (totalIntensity - lightOnlyIntensity) * Reflectiveness;
            RawRTColor finalColor = lightOnlyColor + (totalColor - lightOnlyColor) * Reflectiveness;

            if (finalIntensity < Vector3D.EPSILON || !float.IsNormal(finalIntensity))
                return RTColor.Black;

            if (MainTexture != null)
            {
                float rgbMultiplier = 1f / 255f;

                var rgb = MainTexture.GetColorFromUV(shape.CalculateUV(pointOfContact));
                RTColor textureColor = new RTColor(0f, rgb.R * rgbMultiplier, rgb.G * rgbMultiplier, rgb.B * rgbMultiplier);

                finalColor = (finalColor + (((RawRTColor)textureColor) * TextureStrength)) / (1f + TextureStrength);
            }

            return new RTColor(finalIntensity, finalColor.R, finalColor.G, finalColor.B);
        }

        public override Ray[] GetOutgoingRays(Shape shape, Ray tracingRay, Vector3D pointOfContact)
        {
            if (NormalMap != null)
                return new Ray[] { new Ray(pointOfContact, RTMath.CalculateReflectedRayDirection(tracingRay.Direction, CalculateNormal(shape, pointOfContact))) };
            else
                return base.GetOutgoingRays(shape, tracingRay, pointOfContact);
        }
    }
}