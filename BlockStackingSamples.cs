// Copyright(c) 2017 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php
using System;
using System.Collections.Generic;
using Urho;
using Urho.Physics;
using Urho.Shapes;
using Urho.SharpReality;

namespace UrhnoBlockStackingSamples
{
    public class BlockStackingSamples : StereoApplication
    {

        private Queue<Node> generateCube;

        /// <summary>
        /// root node of spatial mapping surface.  
        /// </summary>
        private Node environmentNode;

        /// <summary>
        /// material for spatial mapping surface
        /// </summary>
        private Material spatialMaterial;


        public BlockStackingSamples(ApplicationOptions opts) : base(opts)
        {
        }

        protected override async void Start()
        {
            // Create a basic scene, see StereoApplication
            base.Start();

            generateCube = new Queue<Node>();
            
            environmentNode = Scene.CreateChild();

            // Gesture Enabled.
            EnableGestureTapped = true;

            // Scene has a lot of pre-configured components, such as Cameras (eyes), Lights, etc.
            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

            // Set Cortana Command
            await RegisterCortanaCommands(new Dictionary<string, Action>()
            {
                {"generate",OnGestureTapped}
            });

            // Set spatial Mapping. 
            spatialMaterial = Material.FromColor(Color.Red);
            spatialMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);
            await StartSpatialMapping(new Vector3(10, 10, 10), 1000, Color.Red);
        }


        public override void OnGestureTapped()
        {
            //Create Empty UrhoObject.
            var child = Scene.CreateChild("Cube"+Guid.NewGuid());
            generateCube.Enqueue(child);
            if (generateCube.Count > 20)
            {
                var dequeue = generateCube.Dequeue();
                Scene.RemoveChild(dequeue);
                dequeue.Dispose();
            }
            
            //Set position and scale.
            child.Position = HeadPosition + RightCamera.Node.WorldDirection * 2f;
            child.Scale = new Vector3(0.2f, 0.2f, 0.2f);

            //add Compoent of Box shape.
            var component = child.CreateComponent<Box>();
            
            //set box color.
            component.Material = Material.FromColor(Color.White);

            //set rigidbody and useGravity.
            var rigidBody = child.CreateComponent<RigidBody>();
            rigidBody.Mass = 1f;
            rigidBody.RollingFriction = 0.5f;
            rigidBody.UseGravity = true;

            //set collision.
            var shape = child.CreateComponent<CollisionShape>();
            shape.SetBox(Vector3.One, Vector3.Zero, Quaternion.Identity);
        }

        protected override void OnUpdate(float timeStep)
        {
            if (spatialMaterial != null)
                spatialMaterial.FillMode = FillMode.Wireframe;
        }

        public override void OnSurfaceAddedOrUpdated(SpatialMeshInfo surface, Model generatedModel)
        {
            var isNew = false;
            StaticModel staticModel = null;

            //check already created by Id.
            var node = environmentNode.GetChild(surface.SurfaceId, false);
            if (node != null)
            {
                //get StaticModel.
                staticModel = node.GetComponent<StaticModel>();
            }
            else
            {
                //if it is new,create node.
                isNew = true;
                node = environmentNode.CreateChild(surface.SurfaceId);
                staticModel = node.CreateComponent<StaticModel>();
            }
            node.Position = surface.BoundsCenter;
            node.Rotation = surface.BoundsRotation;
            staticModel.Model = generatedModel;

            if (isNew)
            {
                //set collisionShape.
                staticModel.SetMaterial(spatialMaterial);
                var rigidBody = node.CreateComponent<RigidBody>();
                rigidBody.RollingFriction = 0.5f;
                rigidBody.Friction = 0.5f;
                var collisionShape = node.CreateComponent<CollisionShape>();
                collisionShape.SetTriangleMesh(generatedModel, 0, Vector3.One, Vector3.Zero, Quaternion.Identity);
            }
        }
    }
}