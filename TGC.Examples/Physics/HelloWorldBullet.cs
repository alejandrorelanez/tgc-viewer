using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;
using TGC.Core.Textures;
using TGC.Core.Utils;
using Microsoft.DirectX.DirectInput;
using BulletSharp;
using BsVector3 = BulletSharp.Math.Vector3;
using BsMatrix = BulletSharp.Math.Matrix;
using BsQuaternion = BulletSharp.Math.Quaternion;
using TGC.Core.Direct3D;

namespace TGC.Examples.Physics
{

    public class HelloWorldBullet : TGCExampleViewer
    {
        private TgcPlane floor;
        private TgcBox box1;
		private TgcSphere sphere;
		private TgcBox box3;


        DiscreteDynamicsWorld dynamicsWorld;
        CollisionDispatcher dispatcher;
        DefaultCollisionConfiguration collisionConfiguration;
        SequentialImpulseConstraintSolver constraintSolver;
        BroadphaseInterface overlappingPairCache;

        //shapes:
        RigidBody floorBody;

        BoxShape boxShape;
        RigidBody boxBody;
        SphereShape ballShape;
        RigidBody ballBody;

        public HelloWorldBullet(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Physics";
            Name = "Hello world Bullet";
            Description = "Whoa";
        }

        public override void Init()
        {
            var floorTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg");
            floor = new TgcPlane(new Vector3(-200, 0, -200), new Vector3(400, 0f, 400), TgcPlane.Orientations.XZplane, floorTexture);

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "\\Texturas\\madera.jpg");

            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache,
                                                        constraintSolver,
                                                        collisionConfiguration);
            dynamicsWorld.Gravity = new BsVector3(0, -10f, 0);

            var floorShape = new StaticPlaneShape(new BsVector3(0, 1, 0), 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            floorBody = new RigidBody(floorInfo);
            dynamicsWorld.AddRigidBody(floorBody);

            boxShape = new BoxShape(5, 5, 5);
            var boxTransform = BsMatrix.Identity;
            boxTransform.Origin = new BsVector3(10, 100, 10);
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            BsVector3 boxLocalInertia = boxShape.CalculateLocalInertia(1f);
            var boxInfo = new RigidBodyConstructionInfo(1f, boxMotionState, boxShape);
            boxBody = new RigidBody(boxInfo);
            dynamicsWorld.AddRigidBody(boxBody);

            ballShape = new SphereShape(10);
            var ballTransform = BsMatrix.Identity;
            ballTransform.Origin = new BsVector3(0, 50, 0);
            var ballMotionState = new DefaultMotionState(ballTransform);
            BsVector3 ballLocalInertia = ballShape.CalculateLocalInertia(1f);
            var ballInfo = new RigidBodyConstructionInfo(1, ballMotionState, ballShape, ballLocalInertia);
            ballInfo.LinearSleepingThreshold = 0.1f;
            ballInfo.AngularSleepingThreshold = 0.1f;
            ballBody  = new RigidBody(ballInfo);
            dynamicsWorld.AddRigidBody(ballBody);

            var center = new Vector3(0, 0, 0);
            var size = new Vector3(10, 10, 10);
            var color = Color.Red;
            box1 = TgcBox.fromSize(size, texture);
            sphere = new TgcSphere(1, texture.Clone(), new Vector3(0, 0, 0));
            sphere.updateValues();
            //Ubicar la camara del framework mirando al centro de este objeto.
            //La camara por default del framework es RotCamera, cuyo comportamiento es
            //centrarse sobre un objeto y permitir rotar y hacer zoom con el mouse.
            //Con clic izquierdo del mouse se rota la cámara, con el derecho se traslada y con la rueda se hace zoom.
            //Otras cámaras disponibles (a modo de ejemplo) son: FpsCamera (1ra persona) y ThirdPersonCamera (3ra persona).
            Camara = new TgcRotationalCamera(new Vector3(0, 20, 0), 100, Input);
        }

        public override void Update()
        {
            PreUpdate();

            dynamicsWorld.StepSimulation(1/60f, 10);
            
                
                    
		}

        /// <summary>
        ///     Método que se invoca todo el tiempo. Es el render-loop de una aplicación gráfica.
        ///     En este método se deben dibujar todos los objetos que se desean mostrar.
        ///     Antes de llamar a este método el framework limpia toda la pantalla.
        ///     Por lo tanto para que un objeto se vea hay volver a dibujarlo siempre.
        ///     La variable elapsedTime indica la cantidad de segundos que pasaron entre esta invocación
        ///     y la anterior de render(). Es útil para animar e interpolar valores.
        /// </summary>
        public override void Render()
        {
            //Iniciamoss la escena
            PreRender();

            DrawText.drawText("boxBody: " + boxBody.MotionState.WorldTransform.ToString(), 5, 20, System.Drawing.Color.Red);
            DrawText.drawText("ballBody: " + ballBody.MotionState.WorldTransform.ToString(), 5, 40, System.Drawing.Color.Red);
            DrawText.drawText("boxBody interpolated: " + boxBody.InterpolationWorldTransform.ToString(), 5, 60, System.Drawing.Color.Red);
            DrawText.drawText("ballBody interpolated: " + ballBody.InterpolationWorldTransform.ToString(), 5, 80, System.Drawing.Color.Red);


            var bs = boxBody.InterpolationWorldTransform;
            var m = new Matrix();
            m.M11 = bs.M11;
            m.M12 = bs.M12;
            m.M13 = bs.M13;
            m.M14 = bs.M14;
            m.M21 = bs.M21;
            m.M22 = bs.M22;
            m.M23 = bs.M23;
            m.M24 = bs.M24;
            m.M31 = bs.M31;
            m.M32 = bs.M32;
            m.M33 = bs.M33;
            m.M34 = bs.M34;
            m.M41 = bs.M41;
            m.M42 = bs.M42;
            m.M43 = bs.M43;
            m.M44 = bs.M44;
            //box1.Transform = Matrix.Identity * m;
            //Dibujar las cajas en pantalla
            box1.Transform = m;
            box1.render();
            
            bs = ballBody.InterpolationWorldTransform;
            m = new Matrix();
            m.M11 = bs.M11;
            m.M12 = bs.M12;
            m.M13 = bs.M13;
            m.M14 = bs.M14;
            m.M21 = bs.M21;
            m.M22 = bs.M22;
            m.M23 = bs.M23;
            m.M24 = bs.M24;
            m.M31 = bs.M31;
            m.M32 = bs.M32;
            m.M33 = bs.M33;
            m.M34 = bs.M34;
            m.M41 = bs.M41;
            m.M42 = bs.M42;
            m.M43 = bs.M43;
            m.M44 = bs.M44;
            sphere.Transform = Matrix.Scaling(10,10,10)*m;//Matrix.Identity * Matrix.Translation(0, 30f, 0);
            //sphere.render();
            sphere.render();

            floor.render();

            //Finalizamos el renderizado de la escena
            PostRender();
        }

        /// <summary>
        ///     Método que se invoca una sola vez al finalizar el ejemplo.
        ///     Se debe liberar la memoria de todos los recursos utilizados.
        /// </summary>
        public override void Dispose()
        {
            //Liberar memoria de las cajas 3D.
            //Por mas que estamos en C# con Garbage Collector igual hay que liberar la memoria de los recursos gráficos.
            //Porque están utilizando memoria de la placa de video (y ahí no hay Garbage Collector).
            box1.dispose();
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
            boxShape.Dispose();
            boxBody.Dispose();

            sphere.dispose();
            ballShape.Dispose();
            ballBody.Dispose();
        }
    }
}