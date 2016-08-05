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


namespace TGC.Examples.Physics
{
	
	public class HelloWorldBullet : TGCExampleViewer
    {
		//Variables para las cajas 3D
        private TgcBox box1;
		private TgcBox box2;
		private TgcBox box3;
        private CollisionWorld world;


        DiscreteDynamicsWorld dynamicsWorld;
        CollisionDispatcher dispatcher;
        DefaultCollisionConfiguration collisionConfiguration;
        SequentialImpulseConstraintSolver constraintSolver;
        AxisSweep3 overlappingPairCache;

        //shapes:
        BoxShape boxShape;
        RigidBody boxBody;


        public HelloWorldBullet(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Physics";
            Name = "Hello world Bullet";
            Description = "Whoa";
        }

        public override void Init()
        {
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache,
                                                        constraintSolver,
                                                        collisionConfiguration);
            dynamicsWorld.Gravity = new BsVector3(0, -9.81f, 0);

            boxShape = new BoxShape(5, 5, 5);
            var boxTransform = BsMatrix.Identity;
            boxTransform.Origin = new BsVector3(0, -5, 0);   // Top of box at Y=0
            DefaultMotionState motionState = new DefaultMotionState();
            var info = new RigidBodyConstructionInfo(1f, motionState, boxShape);
            boxBody = new RigidBody(info);
            dynamicsWorld.AddRigidBody(boxBody);

            var center = new Vector3(0, -5, 0);
            var size = new Vector3(10, 10, 10);
            var color = Color.Red;
            box1 = TgcBox.fromSize(size, color);
			box1.Transform = Matrix.Translation(center);

			//Ubicar la camara del framework mirando al centro de este objeto.
			//La camara por default del framework es RotCamera, cuyo comportamiento es
			//centrarse sobre un objeto y permitir rotar y hacer zoom con el mouse.
			//Con clic izquierdo del mouse se rota la cámara, con el derecho se traslada y con la rueda se hace zoom.
			//Otras cámaras disponibles (a modo de ejemplo) son: FpsCamera (1ra persona) y ThirdPersonCamera (3ra persona).
			Camara = new TgcRotationalCamera(box1.BoundingBox.calculateBoxCenter(),
                box1.BoundingBox.calculateBoxRadius() * 5, Input);
        }

        public override void Update()
        {
            PreUpdate();

            dynamicsWorld.StepSimulation(ElapsedTime);            
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
            
            var bs = boxBody.MotionState.WorldTransform;
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
            box1.Transform = m;
            //Dibujar las cajas en pantalla
            box1.render();
			

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
        }
    }
}