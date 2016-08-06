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
using TGC.Examples.Camara;

namespace TGC.Examples.Physics
{

    public class HelloWorldBullet : TGCExampleViewer
    {
        private TgcPlane floorMesh;
        private TgcBox boxMesh;
		private TgcSphere sphereMesh;

        //World configurations
        DiscreteDynamicsWorld dynamicsWorld;
        CollisionDispatcher dispatcher;
        DefaultCollisionConfiguration collisionConfiguration;
        SequentialImpulseConstraintSolver constraintSolver;
        BroadphaseInterface overlappingPairCache;

        //Rigid Bodies:
        RigidBody floorBody;
        RigidBody boxBody;
        RigidBody ballBody;

        public HelloWorldBullet(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Physics";
            Name = "Hello world Bullet";
            Description = "Este ejemplo integra el TGCViewer con el motor de fisica Bullet.";
        }

        public override void Init()
        {
            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache,
                                                        constraintSolver,
                                                        collisionConfiguration);
            dynamicsWorld.Gravity = new BsVector3(0, -10f, 0);

            //Creamos shapes y bodies.
            
            //El piso es un plano estatico se dice que si tiene masa 0 es estatico.
            var floorShape = new StaticPlaneShape(new BsVector3(0, 1, 0), 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);            
            floorBody = new RigidBody(floorInfo);
            dynamicsWorld.AddRigidBody(floorBody);

            //Se crea una caja de tamaño 20 con rotaciones y origien en 10,100,10 y 1kg de masa.
            var boxShape = new BoxShape(10, 10, 10);
            var boxTransform = BsMatrix.RotationYawPitchRoll(MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI);
            boxTransform.Origin = new BsVector3(10, 100, 10);
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            //Es importante calcular la inercia caso contrario el objeto no rotara.
            BsVector3 boxLocalInertia = boxShape.CalculateLocalInertia(1f);
            var boxInfo = new RigidBodyConstructionInfo(1f, boxMotionState, boxShape, boxLocalInertia);
            boxBody = new RigidBody(boxInfo);
            dynamicsWorld.AddRigidBody(boxBody);

            //Crea una bola de radio 10 origen 50 de 1 kg.
            var ballShape = new SphereShape(10);
            var ballTransform = BsMatrix.Identity;
            ballTransform.Origin = new BsVector3(0, 50, 0);
            var ballMotionState = new DefaultMotionState(ballTransform);
            //Podriamos no calcular la inercia para que no rote, pero es correcto que rote tambien.
            BsVector3 ballLocalInertia = ballShape.CalculateLocalInertia(1f);
            var ballInfo = new RigidBodyConstructionInfo(1, ballMotionState, ballShape, ballLocalInertia);
            ballBody  = new RigidBody(ballInfo);
            dynamicsWorld.AddRigidBody(ballBody);


            //Cargamos objetos de render del framework.
            var floorTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg");
            floorMesh = new TgcPlane(new Vector3(-200, 0, -200), new Vector3(400, 0f, 400), TgcPlane.Orientations.XZplane, floorTexture);

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "\\Texturas\\madera.jpg");
            //Es importante crear todos los mesh con centro en el 0,0,0 y que este coincida con el centro de masa definido caso contrario rotaria de otra forma diferente a la dada por el motor de fisica.
            boxMesh = TgcBox.fromSize(new Vector3(20, 20, 20), texture);
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TgcSphere(1, texture.Clone(), new Vector3(0, 0, 0));
            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();

            //Ubicar la camara.
            Camara = new TgcRotationalCamera(new Vector3(0, 50, 0), 200, Input);
        }

        public override void Update()
        {
            PreUpdate();

            //Realizamos un step (pueden ser mas internamente) de simulacion.
            //Es comun utilizar tiempos de simulacion fijos, de esta forma se gana estabilidad y desacoplamiento con el ElapsedTime
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

            //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
            Matrix m = toDxMatrix(boxBody.InterpolationWorldTransform);
            //Dibujar las cajas en pantalla
            boxMesh.Transform = m;
            boxMesh.render();
            
            //Obtenemos la transformacion de la pelota, en este caso se ve como se puede escalar esa transformacion.
            m = toDxMatrix(ballBody.InterpolationWorldTransform);
            sphereMesh.Transform = Matrix.Scaling(10, 10, 10) * m;
            sphereMesh.render();

            floorMesh.render();

            //Finalizamos el renderizado de la escena
            PostRender();
        }

        private Matrix toDxMatrix(BsMatrix bs)
        {
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
            return m;
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
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
            boxBody.Dispose();
            ballBody.Dispose();
            floorBody.Dispose();

            boxMesh.dispose();
            sphereMesh.dispose();
            floorMesh.dispose();
            
        }
    }
}