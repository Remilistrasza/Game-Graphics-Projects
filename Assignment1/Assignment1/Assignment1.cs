using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Assignment1
{
    public class Assignment1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix world, view, projection;
        float angle, angle2, lightAngle, lightAngle2;
        float camX, camY, distance = 20;

        //Content variables
        Model model, box, bunny, sphere, teapot, torus;
        Effect effect;
        SpriteFont font;

        //Shader variables
        Vector3 lightPosition = new Vector3(20, 20, 20);
        Vector3 cameraPosition;

        Vector4 ambientColor = new Vector4(0, 0, 0, 0);
        float ambientIntensity = 0.1f;

        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        float diffuseIntensity = 1.0f;

        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        float specularIntensity = 1.0f;
        float shininess = 20f;

        //User control
        MouseState previousMouseState, currentMouseState;
        KeyboardState previousKeyState, currentKeyState;
        bool showHint = true, showValue = true;
        string shaderType = "Gouraud";

        public Assignment1()
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
            bunny = Content.Load<Model>("bunny");
            sphere = Content.Load<Model>("sphere");
            teapot = Content.Load<Model>("teapot");
            torus = Content.Load<Model>("Torus");
            model = torus;

            effect = Content.Load<Effect>("SimpleShading");
            font = Content.Load<SpriteFont>("Font");

            world = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f, 100f
                );
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //update control states
            currentMouseState = Mouse.GetState();
            currentKeyState = Keyboard.GetState();

            //camera control
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                angle += (previousMouseState.X - currentMouseState.X) / 100f;
                angle2 += (previousMouseState.Y - currentMouseState.Y) / 100f;
            }
            else if (currentMouseState.MiddleButton == ButtonState.Pressed &&
                previousMouseState.MiddleButton == ButtonState.Pressed)
            {
                camX += (previousMouseState.X - currentMouseState.X) / 100f;
                camY += (previousMouseState.Y - currentMouseState.Y) / 100f;
            }
            else if (currentMouseState.RightButton == ButtonState.Pressed &&
                previousMouseState.RightButton == ButtonState.Pressed)
            {
                distance -= (previousMouseState.Y - currentMouseState.Y) / 100f;
            }

            //light control
            else if (currentKeyState.IsKeyDown(Keys.Left) && previousKeyState.IsKeyDown(Keys.Left))
            {
                lightAngle -= 0.02f;
            }
            else if (currentKeyState.IsKeyDown(Keys.Right) && previousKeyState.IsKeyDown(Keys.Right))
            {
                lightAngle += 0.02f;
            }
            else if (currentKeyState.IsKeyDown(Keys.Up) && previousKeyState.IsKeyDown(Keys.Up))
            {
                lightAngle2 -= 0.02f;
            }
            else if (currentKeyState.IsKeyDown(Keys.Down) && previousKeyState.IsKeyDown(Keys.Down))
            {
                lightAngle2 += 0.02f;
            }

            //reset camera and light position
            else if (currentKeyState.IsKeyDown(Keys.S) && previousKeyState.IsKeyDown(Keys.S))
            {
                resetCamLightPosition();
            }

            //switch models
            else if (currentKeyState.IsKeyDown(Keys.D1) && !previousKeyState.IsKeyDown(Keys.D1))
            {
                model = box;
            }
            else if (currentKeyState.IsKeyDown(Keys.D2) && !previousKeyState.IsKeyDown(Keys.D2))
            {
                model = sphere;
            }
            else if (currentKeyState.IsKeyDown(Keys.D3) && !previousKeyState.IsKeyDown(Keys.D3))
            {
                model = torus;
            }
            else if (currentKeyState.IsKeyDown(Keys.D4) && !previousKeyState.IsKeyDown(Keys.D4))
            {
                model = teapot;
            }
            else if (currentKeyState.IsKeyDown(Keys.D5) && !previousKeyState.IsKeyDown(Keys.D5))
            {
                model = bunny;
            }

            //switch effects
            else if (currentKeyState.IsKeyDown(Keys.F1) && !previousKeyState.IsKeyDown(Keys.F1))
            {
                effect.CurrentTechnique = effect.Techniques[0];
                shaderType = "Gouraud";
            }
            else if (currentKeyState.IsKeyDown(Keys.F2) && !previousKeyState.IsKeyDown(Keys.F2))
            {
                effect.CurrentTechnique = effect.Techniques[1];
                shaderType = "Phong";
            }
            else if (currentKeyState.IsKeyDown(Keys.F3) && !previousKeyState.IsKeyDown(Keys.F3))
            {
                effect.CurrentTechnique = effect.Techniques[2];
                shaderType = "PhongBlinn";
            }
            else if (currentKeyState.IsKeyDown(Keys.F4) && !previousKeyState.IsKeyDown(Keys.F4))
            {
                effect.CurrentTechnique = effect.Techniques[3];
                shaderType = "Schlick";
            }
            else if (currentKeyState.IsKeyDown(Keys.F5) && !previousKeyState.IsKeyDown(Keys.F5))
            {
                effect.CurrentTechnique = effect.Techniques[4];
                shaderType = "Toon";
            }
            else if (currentKeyState.IsKeyDown(Keys.F6) && !previousKeyState.IsKeyDown(Keys.F6))
            {
                effect.CurrentTechnique = effect.Techniques[5];
                shaderType = "HalfLife";
            }

            //Change RGBL
            else if (currentKeyState.IsKeyDown(Keys.R) && previousKeyState.IsKeyDown(Keys.R))
            {
                if (currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift))
                    ambientColor.X -= 0.1f;
                else
                    ambientColor.X += 0.1f;
            }
            else if (currentKeyState.IsKeyDown(Keys.G) && previousKeyState.IsKeyDown(Keys.G))
            {
                if (currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift))
                    ambientColor.Y -= 0.1f;
                else
                    ambientColor.Y += 0.1f;
            }
            else if (currentKeyState.IsKeyDown(Keys.B) && previousKeyState.IsKeyDown(Keys.B))
            {
                if (currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift))
                    ambientColor.Z -= 0.1f;
                else
                    ambientColor.Z += 0.1f;
            }
            else if (currentKeyState.IsKeyDown(Keys.L) && previousKeyState.IsKeyDown(Keys.L))
            {
                if (currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift))
                    ambientIntensity -= 0.1f;
                else
                    ambientIntensity += 0.1f;
            }

            //change specular intensity or shininess
            else if ((currentKeyState.IsKeyDown(Keys.OemPlus) && previousKeyState.IsKeyDown(Keys.OemPlus))
                || (currentKeyState.IsKeyDown(Keys.Add) && previousKeyState.IsKeyDown(Keys.Add)))
            {
                if (currentKeyState.IsKeyDown(Keys.LeftControl))
                    shininess += 0.1f;
                else
                    specularIntensity += 0.1f;
            }
            else if ((currentKeyState.IsKeyDown(Keys.OemMinus) && previousKeyState.IsKeyDown(Keys.OemMinus))
                || (currentKeyState.IsKeyDown(Keys.Subtract) && previousKeyState.IsKeyDown(Keys.Subtract)))
            {
                if (currentKeyState.IsKeyDown(Keys.LeftControl))
                    shininess -= 0.1f;
                else
                    specularIntensity -= 0.1f;
            }

            //toggle information
            else if (currentKeyState.IsKeyDown(Keys.OemQuestion) && !previousKeyState.IsKeyDown(Keys.OemQuestion))
            {
                showHint = !showHint;
            }
            else if (currentKeyState.IsKeyDown(Keys.H) && !previousKeyState.IsKeyDown(Keys.H))
            {
                showValue = !showValue;
            }

            //update camera
            cameraPosition = Vector3.Transform(new Vector3(camX, camY, distance),
                  Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            //update light
            lightPosition = Vector3.Transform(new Vector3(20, 20, 20),
                  Matrix.CreateRotationX(lightAngle2) * Matrix.CreateRotationY(lightAngle));

            view = Matrix.CreateLookAt(
                cameraPosition,
                Vector3.Zero,
                Vector3.Transform(Vector3.Up,
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));

            //update control state
            previousMouseState = Mouse.GetState();
            previousKeyState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        Matrix worldInverseTransposeMatrix = Matrix.Transpose(
                            Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
                        effect.Parameters["AmbientColor"].SetValue(ambientColor);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);

                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        pass.Apply(); //send data to GPU
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
                               + "Change Models: Number Keys [1] - [5]\n"
                               + "Change Effects: [F1] - [F6]\n"
                               + "Change Red Value: [r]/[R] (+[Shift] to decrease)\n"
                               + "Change Green Value: [g]/[G] (+[Shift] to decrease)\n"
                               + "Change Blue Value: [b]/[B] (+[Shift] to decrease)\n"
                               + "Change Light Intensity: [l]/[L]\n"
                               + "Change Specular Intensity: [+]/[-]\n"
                               + "Change Shininess: [Left Control] + [+]/[-]\n"
                               + "Show/Hide User Control: [?]\n"
                               + "Show/Hide Detailed Values: [h]/[H]";
            string value = "Data Value:"
                         + "\nCamera Position: X:" + Math.Round(cameraPosition.X, 2) + " Y:" 
                         + Math.Round(cameraPosition.Y, 2) + " Z:" + Math.Round(cameraPosition.Z, 2)
                         + "\nCamera Angles: " + Math.Round(angle, 2) + " / " + Math.Round(angle2, 2)
                         + "\nLight Position: X:" + Math.Round(lightPosition.X, 2) + " Y:"
                         + Math.Round(lightPosition.Y, 2) + " Z:" + Math.Round(lightPosition.Z, 2)
                         + "\nLight Angles: " + Math.Round(lightAngle, 2) + " / " + Math.Round(lightAngle2, 2)
                         + "\nShader Type: " + shaderType
                         + "\nLight Color(RGB): R:" + Math.Round(ambientColor.X, 2) + " G:"
                         + Math.Round(ambientColor.Y, 2) + " B:" + Math.Round(ambientColor.Z, 2)
                         + "\nLight Intensity: " + Math.Round(ambientIntensity, 2)
                         + "\nSpecular Intensity: " + Math.Round(specularIntensity, 2)
                         + "\nShininess: " + Math.Round(shininess, 2);
            if (showHint)
                spriteBatch.DrawString(font, userControl, Vector2.UnitX + Vector2.UnitY * 12, Color.White);
            if (showValue)
                spriteBatch.DrawString(font, value, Vector2.UnitX * 1100 + Vector2.UnitY * 12, Color.White);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void resetCamLightPosition()
        {
            distance = 20;
            angle = 0;
            angle2 = 0;
            camX = 0;
            camY = 0;
            lightAngle = 0;
            lightAngle2 = 0;
        }
    }
}
