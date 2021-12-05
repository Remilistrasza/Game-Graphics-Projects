using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Assignment4
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // **** TEMPLATE *****        
        //SpriteFont font;
        Effect effect;
        Texture2D texture, fire, smoke, water;
        Model model, cube;
        SpriteFont font;
        int texture_no = 0, fountain = 1, shape = 0;
        float friction = 0, resilience = 0.7f;
        bool wind = false, velo = false, showHint = true, showValue = true;
        Vector3 cus_velocity = new Vector3();

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(
            new Vector3(0, 0, 10),
            new Vector3(0, 0, 0),
            Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45),
            800f / 600f, 0.1f, 100f);

        Vector3 cameraPosition, cameraTarget, lightPosition;
        float angle = 0, angle2 = 0, angleL = 0, angleL2 = 0, distance = 30;

        MouseState preMouse;
        KeyboardState preKeyboard;
        // **** END OF TEMPLATE *****   

        ParticleManager particleManager;
        System.Random random;
        Vector3 particlePosition;
        Matrix inverseCamera;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }
        
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();
            IsMouseVisible = true;
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("ParticleShader");
            font = Content.Load<SpriteFont>("Font");
            fire = Content.Load<Texture2D>("fire");
            smoke = Content.Load<Texture2D>("smoke");
            water = Content.Load<Texture2D>("water");
            texture = fire;
            cube = Content.Load<Model>("cube");
            model = Content.Load<Model>("Plane");

            random = new System.Random();
            particleManager = new ParticleManager(GraphicsDevice, 1000);
            particlePosition = new Vector3(0, 2, 0);
        }
        
        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // *** TEMPLATE ***
            bool shift = false;
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            foreach (Keys key in pressedKeys)
                if (key == Keys.LeftShift || key == Keys.RightShift) shift = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { texture_no = 0; }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { texture = smoke; texture_no = 1; }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { texture = water; texture_no = 2; }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { texture = fire; texture_no = 3; }

            if (Keyboard.GetState().IsKeyDown(Keys.F1)) fountain = 1;
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) fountain = 2;
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) fountain = 3;
            if (!preKeyboard.IsKeyDown(Keys.F4) && Keyboard.GetState().IsKeyDown(Keys.F4)) shape = (shape + 1) % 3;

            if (!preKeyboard.IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.W)) wind = !wind;
            if (!preKeyboard.IsKeyDown(Keys.V) && Keyboard.GetState().IsKeyDown(Keys.V)) velo = !velo;
            if (Keyboard.GetState().IsKeyDown(Keys.F) && !shift) friction += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.F) && shift) friction -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.R) && !shift) resilience += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.R) && shift) resilience -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.F) && !shift) friction += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.F) && shift) friction -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.U) && !shift) cus_velocity.X += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.U) && shift) cus_velocity.X -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.I) && !shift) cus_velocity.Y += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.I) && shift) cus_velocity.Y -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.O) && !shift) cus_velocity.Z += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.O) && shift) cus_velocity.Z -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.S) && shift) { angle = 0; angle2 = 0; angleL = 0; angleL2 = 0; distance = 30; }
            if (!preKeyboard.IsKeyDown(Keys.OemQuestion) && Keyboard.GetState().IsKeyDown(Keys.OemQuestion)) showHint = !showHint;
            if (!preKeyboard.IsKeyDown(Keys.H) && Keyboard.GetState().IsKeyDown(Keys.H)) showValue = !showValue;


            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                angle -= (Mouse.GetState().X - preMouse.X) / 100f;
                angle2 += (Mouse.GetState().Y - preMouse.Y) / 100f;
            }
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().X - preMouse.X) / 100f;
            }
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                cameraTarget -= ViewRight * (Mouse.GetState().X - preMouse.X) / 10f;
                cameraTarget += ViewUp * (Mouse.GetState().Y - preMouse.Y) / 10f;
            }

            // Update Camera
            cameraPosition = Vector3.Transform(
                new Vector3(0, 10, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) * Matrix.CreateTranslation(cameraTarget));
            view = Matrix.CreateLookAt(
                cameraPosition, cameraTarget,
                Vector3.Transform(Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));
            // Update Light
            lightPosition = Vector3.Transform(
                new Vector3(0, 10, 100),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));

            preMouse = Mouse.GetState();
            preKeyboard = Keyboard.GetState();
            // *** END OF TEMPLATE ***

            //particle generate
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                Particle particle;
                switch (fountain)
                {
                    case 1:
                        particle = particleManager.getNext();
                        particle.Position = particlePosition;
                        particle.Velocity = new Vector3(0, 5, 0);
                        particle.Acceleration = new Vector3(0, 0, 0);
                        particle.MaxAge = 5;
                        particle.Init();
                        break;
                    case 2:
                        particle = particleManager.getNext();
                        particle.Position = particlePosition;
                        particle.Velocity = new Vector3(random.Next(-3, 3), 3, random.Next(-3, 3));
                        particle.Acceleration = new Vector3(0, -10, 0);
                        particle.MaxAge = 5;
                        particle.Init();
                        break;
                    case 3:
                        particle = particleManager.getNext();
                        particle.Position = particlePosition;
                        particle.Velocity = new Vector3(random.Next(-3, 3), 3, random.Next(-3, 3));
                        particle.Acceleration = new Vector3(0, -10, 0);
                        particle.MaxAge = 5;
                        particle.Bounce = true;
                        if (wind) particle.Acceleration += new Vector3(random.Next(-10, 10), 0, random.Next(-10, 10));
                        if (velo) {
                            particle.Velocity = cus_velocity;
                            particle.Acceleration = new Vector3();
                        }
                        particle.Friction = friction;
                        particle.Resilience = resilience;
                        particle.Init();
                        break;
                    default:
                        break;
                }
            }
            particleManager.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);

            inverseCamera = Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle);

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            model.Draw(Matrix.Identity, view, projection);

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.CurrentTechnique = effect.Techniques[0];
            effect.CurrentTechnique.Passes[0].Apply();
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["LightPosition"].SetValue(lightPosition);
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        //Matrix worldInverseTransposeMatrix = Matrix.Transpose(
                        //                 Matrix.Invert(mesh.ParentBone.Transform));
                        //effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["InverseCamera"].SetValue(inverseCamera);
            effect.Parameters["Texture_no"].SetValue(texture_no);

            particleManager.Draw(GraphicsDevice);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            spriteBatch.Begin();
            string userControl = "User Control:\n"
                               + "Rotate Camera: Mouse Left Drag\n"
                               + "Change Camera Distance: Mouse Right Drag\n"
                               + "Move Camera: Mouse Middle Drag\n"
                               + "Rotate Light: Arrow Keys\n"
                               + "Reset Camera and Light: [s]/[S]\n"
                               + "Change Textures: Number Keys [1] - [4]\n"
                               + "Change Fountain: [F1] - [F3]\n"
                               + "Change Shapes: [F4]\n"
                               + "Change Resilience/Friction: [R]/[F] + [Shift]\n"
                               + "Toggle Wind/Custom Velocity: [W]/[V] + [Shift]\n"
                               + "Change Custom Velocity: [U]/[I]/[O] + [Shift]\n"
                               + "Show/Hide User Control: [?]\n"
                               + "Show/Hide Detailed Values: [h]/[H]";
            string value = "Data Value:"
                         + "\nTexture: " + texture_no
                         + "\nFountain: " + fountain
                         + "\nShape: " + shape
                         + "\nResilience: " + resilience
                         + "\nFriction: " + friction
                         + "\nWind: " + wind
                         + "\nCustom Velocity: " + velo
                         + "\nCustom Velocity Value: " + cus_velocity;
            
            if (showHint)
                spriteBatch.DrawString(font, userControl, Vector2.UnitX + Vector2.UnitY * 12, Color.White);
            if (showValue)
                spriteBatch.DrawString(font, value, Vector2.UnitX * 1100 + Vector2.UnitY * 12, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
