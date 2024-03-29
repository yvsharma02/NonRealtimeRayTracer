﻿namespace RayTracing
{
    public abstract class WorldObject
    {
        protected Transformation oldTransform = new Transformation(Vector3D.Zero, Vector3D.Zero, new Vector3D(1f, 1f, 1f));  
        protected Transformation transform = new Transformation(Vector3D.Zero, Vector3D.Zero, new Vector3D(1f, 1f, 1f));

        protected bool newTransformApplied = false;
        
        public abstract int TypeID { get; }

        public Transformation Transform
        {
            get
            {
                return transform;
            }
        }

        public void SetLocalTransform(Transformation newTransform)
        {
            SetTransform(newTransform, true);
        }

        public WorldObject(Transformation transform)
        {
            // We want to skip calling OnTransformChange here as the child may not have been initialized info needed to process Transform Change yet.
            this.transform = transform;
        }

        protected virtual void SetTransform(Transformation newTransform, bool transformImmediately)
        {
            this.oldTransform = transform;
            this.transform = newTransform;

            newTransformApplied = false;

            if (transformImmediately)
                ApplyTransform();
        }

        /// <summary>
        /// By default, just sets newTransformApplied to true.
        /// </summary>
        protected virtual void ApplyTransform()
        {
            newTransformApplied = true;
        }
    }
}