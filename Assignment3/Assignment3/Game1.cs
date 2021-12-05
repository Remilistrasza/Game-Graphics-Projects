using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment3
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //SpriteFont font;
        Effect effect;
        Model model;
        Texture2D texture;
        Texture2D art, bumpTest, crossHatch, monkey, round, saint, science, square;
        SpriteFont font;
        Skybox skybox;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(
            new Vector3(0, 0, 20),
            new Vector3(0, 0, 0),
            Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45),
            800f / 600f,
            0.1f,
            100f);

        Vector3 cameraPosition, cameraTarget, lightPosition;
        Matrix lightView, lightProjection;

        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 40;
        int effectControl = 1;
        float U = 1, V = 1;
        bool showHint = true, showValue = true;

        MouseState preMouse;
        
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

            model = Content.Load<Model>("Torus");
            effect = Content.Load<Effect>("BumpMap");
            font = Content.Load<SpriteFont>("Font");

            art = Content.Load<Texture2D>("NormalMaps/art");
            bumpTest = Content.Load<Texture2D>("NormalMaps/Bumptest");
            crossHatch = Content.Load<Texture2D>("NormalMaps/crossHatch");
            monkey = Content.Load<Texture2D>("NormalMaps/monkey");
            round = Content.Load<Texture2D>("NormalMaps/round");
            saint = Content.Load<Texture2D>("NormalMaps/saint");
            science = Content.Load<Texture2D>("NormalMaps/science");
            square = Content.Load<Texture2D>("NormalMaps/square");
            texture = round;

            string[] skyboxTextures = {
                "skybox/nvlobby_new_negx", "skybox/nvlobby_new_posx",
                "skybox/nvlobby_new_negy", "skybox/nvlobby_new_posy",
                "skybox/nvlobby_new_negz", "skybox/nvlobby_new_posz",
            };
            skybox = new Skybox(512, skyboxTextures, Content, GraphicsDevice);
        }
        
        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //keyboard control
            bool shift = false;
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            foreach (Keys key in pressedKeys)
                if (key == Keys.LeftShift || key == Keys.RightShift) shift = true;
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) texture = art;
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) texture = bumpTest;
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) texture = crossHatch;
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) texture = monkey;
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) texture = round;
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) texture = saint;
            if (Keyboard.GetState().IsKeyDown(Keys.D7)) texture = science;
            if (Keyboard.GetState().IsKeyDown(Keys.D8)) texture = square;
            if (Keyboard.GetState().IsKeyDown(Keys.D9)) texture = art;
            if (Keyboard.GetState().IsKeyDown(Keys.F1)) effectControl = 1;
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) effectControl = 2;
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) effectControl = 3;
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) effectControl = 4;
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) effectControl = 5;
            if (Keyboard.GetState().IsKeyDown(Keys.U) && !shift) U += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.U) && shift) U -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.V) && !shift) V += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.V) && shift) V -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { angle = angle2 = angleL = angleL2 = 0; distance = 30; cameraTarget = Vector3.Zero; }
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

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) * Matrix.CreateTranslation(cameraTarget));
            lightPosition = Vector3.Transform(
                new Vector3(0, 0, 30),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));
            lightView = Matrix.CreateLookAt(
                lightPosition,
                Vector3.Zero,
                Vector3.Transform(
                    Vector3.UnitY,
                    Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL)));
            lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 50f);

            view = Matrix.CreateLookAt(
                cameraPosition,
                cameraTarget,
                Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));

            preMouse = Mouse.GetState();

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            RasterizerState originalRasterizerState = graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphics.GraphicsDevice.RasterizerState = rasterizerState;
            //skybox.Draw(view, projection, cameraPosition);

            graphics.GraphicsDevice.RasterizerState = originalRasterizerState;

            //effect.CurrentTechnique = effect.Techniques[0];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        Matrix worldInverseTranspose = Matrix.Transpose(
                            Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["normalMap"].SetValue(texture);
                        //effect.Parameters["EnvironmentMap"].SetValue(skybox.skyboxTexture);

                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1, 1, 1, 1));
                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1, 1, 1, 1));
                        effect.Parameters["SpecularIntensity"].SetValue(0.5f);
                        effect.Parameters["Shininess"].SetValue(20f);
                        effect.Parameters["control"].SetValue(effectControl);
                        effect.Parameters["UVScale"].SetValue(new Vector2(U, V));

                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount);
                    }
                }
            }

            spriteBatch.Begin();
            string userControl = "User Control:\n"
                               + "Rotate Camera: Mouse Left Drag\n"
                               + "Change Camera Distance: Mouse Right Drag\n"
                               + "Move Camera: Mouse Middle Drag\n"
                               + "Rotate Light: Arrow Keys\n"
                               + "Reset Camera and Light: [s]/[S]\n"
                               + "Change Textures: Number Keys [1] - [8]\n"
                               + "Change Effects: [F1] - [F10]\n"
                               + "Change U/V Scale: [U]/[V] [Shift]\n"
                               + "Show/Hide User Control: [?]\n"
                               + "Show/Hide Detailed Values: [h]/[H]";
            string value = "Data Value:"
                         + "\nShader Mode: " + effectControl
                         + "\nBump Height: "
                         + "\nU Scale: " + U
                         + "\nV Scale: " + V
                         + "\nMipMap: ";
            if (showHint)
                spriteBatch.DrawString(font, userControl, Vector2.UnitX + Vector2.UnitY * 12, Color.White);
            if (showValue)
                spriteBatch.DrawString(font, value, Vector2.UnitX * 1100 + Vector2.UnitY * 12, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void resetCamLight()
        {
            angle = 0;
            angle2 = 0;
            angleL = 0;
            angleL2 = 0;
            distance = 20;
        }
    }
}
