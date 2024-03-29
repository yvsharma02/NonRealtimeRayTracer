﻿namespace RayTracing
{
    public struct Vector2D
    {
        public readonly float x;
        public readonly float y;

        public Vector2D()
        {
            this.x = 0;
            this.y = 0;
        }

        public Vector2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x + b.x, a.y + b.y);
        }

        public static Vector2D operator -(Vector2D v)
        {
            return new Vector2D(-v.x, -v.y);
        }

        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return a + (-b);
        }

        public static Vector2D operator *(Vector2D a, float b)
        {
            return new Vector2D(a.x * b, a.y * b);
        }

        public static Vector2D operator /(Vector2D a, float b)
        {
            return a * (1f / b);
        }

        public float this[int index]
        {
            get
            {
                if (index == 0) return this.x;
                if (index == 1) return this.y;

                throw new IndexOutOfRangeException();
            }
        }

        public static implicit operator Vector3D(Vector2D v2d)
        {
            return new Vector3D(v2d.x, v2d.y, 0);
        }

        public static implicit operator Vector2D(Vector3D v3d)
        {
            return new Vector2D(v3d.x, v3d.y);
        }
    }
}