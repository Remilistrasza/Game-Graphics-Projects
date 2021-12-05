using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment2
{
    public class Assignment2 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        Model model, box, bunny, sphere, teapot, torus, helicopter;
        Effect effect;
        Texture2D texture;
        SpriteFont font;
        Skybox skybox, testColor, officeRoom, daytimeSky, custom;

        Matrix world, view, projection;
        float cameraAngleX, cameraAngleY, lightAngleX, lightAngleY;
        float distance = 10, camX, camY;

        bool hasTexture = false;
        float reflectivity = 0.5f, refractivity = 0.5f;
        Vector3 etaRatio = new Vector3(1, 0.7f, 0.3f);
        float fresnelBias = 0.4f, fresnelScale = 0.3f, fresnelPower = 1f;

        Vector3 lightPosition = new Vector3(0, 0, 1);
        Vector3 cameraPosition;

        //User control
        MouseState previousMouseState, currentMouseState;
        KeyboardState previousKeyState;
        bool showHint = true, showValue = true;

        public Assignment2()
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

            box = Content.Load<Model>("box");
            bunny = Content.Load<Model>("bunnyUV");
            sphere = Content.Load<Model>("sphere");
            teapot = Content.Load<Model>("teapot");
            torus = Content.Load<Model>("Torus");
            helicopter = Content.Load<Model>("Helicopter");
            model = bunny;

            effect = Content.Load<Effect>("shader");
            texture = Content.Load<Texture2D>("HelicopterTexture");
            font = Content.Load<SpriteFont>("Font");

            string[] skyboxTextures = {
                "skybox/debug_negx", "skybox/debug_posx",
                "skybox/debug_negy", "skybox/debug_posy",
                "skybox/debug_negz", "skybox/debug_posz",
            };
            testColor = new Skybox(256, skyboxTextures, Content, GraphicsDevice);

            string[] skyboxTextures1 = {
                "skybox/nvlobby_new_negx", "skybox/nvlobby_new_posx",
                "skybox/nvlobby_new_negy", "skybox/nvlobby_new_posy",
                "skybox/nvlobby_new_negz", "skybox/nvlobby_new_posz",
            };
            officeRoom = new Skybox(512, skyboxTextures1, Content, GraphicsDevice);

            string[] skyboxTextures2 = {
                "skybox/grandcanyon_negx", "skybox/grandcanyon_posx",
                "skybox/grandcanyon_negy", "skybox/grandcanyon_posy",
                "skybox/grandcanyon_negz", "skybox/grandcanyon_posz",
            };
            daytimeSky = new Skybox(512, skyboxTextures2, Content, GraphicsDevice);

            string[] skyboxTextures3 = {
                "skybox/custom_negx", "skybox/custom_posx",
                "skybox/custom_negy", "skybox/custom_posy",
                "skybox/custom_negz", "skybox/custom_posz",
            };
            custom = new Skybox(512, skyboxTextures3, Content, GraphicsDevice);
            skybox = testColor;
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            currentMouseState = Mouse.GetState();
            //keyboard control
            bool shift = false;
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            foreach (Keys key in pressedKeys)
                if (key == Keys.LeftShift || key == Keys.RightShift) shift = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) lightAngleX += 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) lightAngleX -= 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) lightAngleY += 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) lightAngleY -= 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) model = box;
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) model = sphere;
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) model = torus;
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { model = teapot; }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) { model = bunny; hasTexture = false; }
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) { model = helicopter; hasTexture = true; }
            if (Keyboard.GetState().IsKeyDown(Keys.D7)) skybox = testColor;
            if (Keyboard.GetState().IsKeyDown(Keys.D8)) skybox = officeRoom;
            if (Keyboard.GetState().IsKeyDown(Keys.D9)) skybox = daytimeSky;
            if (Keyboard.GetState().IsKeyDown(Keys.D0)) skybox = custom;
            if (Keyboard.GetState().IsKeyDown(Keys.F7)) effect.CurrentTechnique = effect.Techniques[0];
            if (Keyboard.GetState().IsKeyDown(Keys.F8)) effect.CurrentTechnique = effect.Techniques[1];
            if (Keyboard.GetState().IsKeyDown(Keys.F9)) effect.CurrentTechnique = effect.Techniques[2];
            if (Keyboard.GetState().IsKeyDown(Keys.F10)) effect.CurrentTechnique = effect.Techniques[3];
            if (Keyboard.GetState().IsKeyDown(Keys.Q) && !shift) fresnelPower += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Q) && shift) fresnelPower -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && !shift) fresnelScale += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && shift) fresnelScale -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.E) && !shift) fresnelBias += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.E) && shift) fresnelBias -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.R) && !shift) etaRatio.X += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.R) && shift) etaRatio.X -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.G) && !shift) etaRatio.Y += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.G) && shift) etaRatio.Y -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.B) && !shift) etaRatio.Z += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.B) && shift) etaRatio.Z -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus)) reflectivity += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus)) reflectivity -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && !previousKeyState.IsKeyDown(Keys.OemQuestion)) showHint = !showHint;
            if (Keyboard.GetState().IsKeyDown(Keys.H) && !previousKeyState.IsKeyDown(Keys.H)) showValue = !showValue;

            //mouse control
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                cameraAngleY += (previousMouseState.X - currentMouseState.X) / 100f;
                cameraAngleX += (previousMouseState.Y - currentMouseState.Y) / 100f;
            }
            else if (currentMouseState.MiddleButton == ButtonState.Pressed &&
                previousMouseState.MiddleButton == ButtonState.Pressed)
            {
                camX -= (previousMouseState.X - currentMouseState.X) / 100f;
                camY -= (previousMouseState.Y - currentMouseState.Y) / 100f;
            }
            else if (currentMouseState.RightButton == ButtonState.Pressed &&
                previousMouseState.RightButton == ButtonState.Pressed)
            {
                distance -= (previousMouseState.Y - currentMouseState.Y) / 100f;
            }

            //camera/light position recalculation
            cameraPosition = Vector3.Transform(new Vector3(camX, camY, distance),
                  Matrix.CreateRotationX(cameraAngleX) * 
                  Matrix.CreateRotationY(cameraAngleY));
            lightPosition = Vector3.Transform(new Vector3(0, 0, 1),
                Matrix.CreateRotationX(lightAngleX) *
                Matrix.CreateRotationY(lightAngleY));

            //update matrix
            world = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
            view = Matrix.CreateLookAt(
                cameraPosition,
                Vector3.Zero,
                Vector3.Transform(
                    Vector3.Up,
                    Matrix.CreateRotationX(cameraAngleX) *
                    Matrix.CreateRotationY(cameraAngleY)
                )
             );

            previousMouseState = Mouse.GetState();
            previousKeyState = Keyboard.GetState();

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            //draw skybox
            RasterizerState originalRasterizerState = graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            
            graphics.GraphicsDevice.RasterizerState = originalRasterizerState;
            DrawModelWithEffect();

            spriteBatch.Begin();
            string userControl = "User Control:\n"
                               + "Rotate Camera: Mouse Left Drag\n"
                               + "Change Camera Distance: Mouse Right Drag\n"
                               + "Move Camera: Mouse Middle Drag\n"
                               + "Rotate Light: Arrow Keys\n"
                               + "Reset Camera and Light: [s]/[S]\n"
                               + "Change Models: Number Keys [1] - [6]\n"
                               + "Change Skybox: Number Keys [7] - [0]\n"
                               + "Change Effects: [F7] - [F10]\n"
                               + "Change Red Value: [r]/[R] (+[Shift] to decrease)\n"
                               + "Change Green Value: [g]/[G] (+[Shift] to decrease)\n"
                               + "Change Blue Value: [b]/[B] (+[Shift] to decrease)\n"
                               + "Change Fresnel Power: [q]/[Q] (+[Shift] to decrease)\n"
                               + "Change Fresnel Scale: [w]/[W] (+[Shift] to decrease)\n"
                               + "Change Fresnel Bias: [e][E] (+[Shift] to decrease)\n"
                               + "Change Reflectivity: [+]/[-]\n"
                               + "Show/Hide User Control: [?]\n"
                               + "Show/Hide Detailed Values: [h]/[H]";
            string value = "Data Value:"
                         + "\nCamera Position: X:" + cameraPosition.X.ToString("0.00") + " Y:"
                         + cameraPosition.Y.ToString("0.00") + " Z:" + cameraPosition.Z.ToString("0.00")
                         + "\nCamera Angles: " + cameraAngleX.ToString("0.00") + " / " + cameraAngleY.ToString("0.00")
                         + "\nLight Position: X:" + lightPosition.X.ToString("0.00") + " Y:"
                         + lightPosition.Y.ToString("0.00") + " Z:" + lightPosition.Z.ToString("0.00")
                         + "\nLight Angles: " + lightAngleX.ToString("0.00") + " / " + lightAngleY.ToString("0.00")
                         + "\neta Ratio(RGB): R:" + etaRatio.X.ToString("0.00") + " G:"
                         + etaRatio.Y.ToString("0.00") + " B:" + etaRatio.Z.ToString("0.00")
                         + "\nFresnel Power: " + fresnelPower.ToString("0.00")
                         + "\nFresnel Scale: " + fresnelScale.ToString("0.00")
                         + "\nFresnel Bias: " + fresnelBias.ToString("0.00")
                         + "\nReflectivity: " + reflectivity.ToString("0.00");
            if (showHint)
                spriteBatch.DrawString(font, userControl, Vector2.UnitX + Vector2.UnitY * 12, Color.White);
            if (showValue)
                spriteBatch.DrawString(font, value, Vector2.UnitX * 1100 + Vector2.UnitY * 12, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawModelWithEffect()
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        Matrix worldInverseTranspose = Matrix.Transpose(
                            Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                        effect.Parameters["DecalMap"].SetValue(texture);
                        effect.Parameters["hasTexture"].SetValue(hasTexture);
                        effect.Parameters["EnvironmentMap"].SetValue(skybox.skyboxTexture);
                        effect.Parameters["Reflectivity"].SetValue(reflectivity);
                        effect.Parameters["Refractivity"].SetValue(refractivity);
                        effect.Parameters["EtaRatio"].SetValue(etaRatio);
                        effect.Parameters["FresnelPower"].SetValue(fresnelPower);
                        effect.Parameters["FresnelScale"].SetValue(fresnelScale);
                        effect.Parameters["FresnelBias"].SetValue(fresnelBias);

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
        }
    }
}
