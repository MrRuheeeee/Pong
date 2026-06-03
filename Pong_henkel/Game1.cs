using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using System;

namespace Pong_henkel
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        private Texture2D paddleTexture;
        private Rectangle player1;
        private Rectangle player2;
        private Rectangle ball;
        private Vector2 ballVelocity;
        private Vector2 scoreFontPlayer1;

        private Random random;
        private uint Gamemode { get; set; } = 0;
        private bool Start { get; set; } = false;
        private bool IsColliding { get; set; } = false;
        private bool Cheats { get; set; } = true;
        private bool HoldingCheatsKey = false;
        private uint PointsPlayer1 { get; set; } = 0;
        private uint PointsPlayer2 { get; set; } = 0;
        private SpriteFont scoreFont;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            paddleTexture = new Texture2D(GraphicsDevice, 1, 1);
            paddleTexture.SetData([Color.White]);

            player1 = new Rectangle(50, 200, 10, 100);   // x, y, breite, höhe
            player2 = new Rectangle(730, 200, 10, 100);
            ball = new Rectangle(390, 240, 10, 10);
            ballVelocity = new Vector2(4, 4);

            scoreFont = Content.Load<SpriteFont>("ScoreFont");
            scoreFontPlayer1 = new Vector2(200, 20);

            random = new Random();

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var state = Keyboard.GetState();

            #region mouse movement

            // maus position finden
            MouseState mouse = Mouse.GetState();
            Point mousePos = new Point(mouse.X, mouse.Y);

            // auswahl des 1v1 modus
            if (GamemodeSign1V1.Contains(mousePos) && mouse.LeftButton == ButtonState.Pressed)
            {
                Gamemode = 1;
            }
            if (GamemodeSignVSBOT.Contains(mousePos) && mouse.LeftButton == ButtonState.Pressed)
            {
                Gamemode = 2;
            }

            #endregion

            #region player 1 movements (keep)

            // Button Presses
            if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.W)) player1.Y -= 7;
            if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.S)) player1.Y += 7;

            // bounce off walls
            if (player1.Y >= -50) { player1.Y -= 7; }
            if (player1.Y < (GraphicsDevice.Viewport.Height - 50)) { player1.Y += 7; }

            #endregion

            #region player movements (1v1)

            #region cheats

            // deactivate cheats, in case they know 👁️👁️
            if (state.IsKeyDown(Keys.CapsLock) && !HoldingCheatsKey)
            {
                Cheats = !Cheats;
                HoldingCheatsKey = true;
            }

            if(!state.IsKeyDown(Keys.CapsLock))
                HoldingCheatsKey = false;

            // move enemy with arrow keys (muhehehehe)
            if (Cheats == true)
            {
                if (state.IsKeyDown(Keys.Left)) player1.X -= 2;
                if (state.IsKeyDown(Keys.Right)) player1.X += 2;
                if (state.IsKeyDown(Keys.NumPad0)) ballVelocity.X -= 0.8f;
                if (state.IsKeyDown(Keys.NumPad1)) player1.Y -= 2;
            }

            #endregion

            // Player2 Controls (arrow keys)
            if (state.IsKeyDown(Keys.Up) && Gamemode == 1) player2.Y -= 7;
            if (state.IsKeyDown(Keys.Down) && Gamemode == 1) player2.Y += 7;

            if (player2.Y >= -50) { player2.Y -= 7; }
            if (player2.Y < (GraphicsDevice.Viewport.Height - 50)) { player2.Y += 7; }

            #endregion

            #region BOT movement
            if (Gamemode == 2)
            {
                if (ball.X > 0)
                {
                    
                    if (((player2.Y + 50) < ball.Y))
                    {
                        //player2.Y = ball.Y;
                        player2.Y += 7;
                    }
                    else
                    {
                        //player2.Y = ball.Y;
                        player2.Y -= 7;
                    }
                }
                
            }
            
            #endregion

            #region balll movement

            // move ball after space button
            if (state.IsKeyDown(Keys.Space)) Start = true;

            // ball movement
            if (Start)
            {
                ball.X += (int)ballVelocity.X;
                ball.Y += (int)ballVelocity.Y;
            }

            // Bouncing off walls
            if (ball.Y <= 0 || ball.Y + ball.Height >= GraphicsDevice.Viewport.Height)
                ballVelocity.Y *= -1;

            // score points & reset ball
            if (ball.X < 0 || ball.X > GraphicsDevice.Viewport.Width)
            {
                if (ball.X < 0)
                {
                    PointsPlayer1 += 1;
                }
                if (ball.X > GraphicsDevice.Viewport.Width)
                {
                    PointsPlayer2 += 1;
                }
                if (ballVelocity.X < 2 && ballVelocity.X > -2)
                {
                    ballVelocity.X = 1f;
                }
                else
                {
                    ballVelocity.X /= 1.4f;
                }
                ball.X = 390;
                ball.Y = 240;
                
                ballVelocity.Y /= 3;
                Start = false;
            }

            // Player-Kollision
            if (ball.Intersects(player1) || ball.Intersects(player2) && !IsColliding)
            {
                // auf standard wieder setzen - IsColliding ist dafür da, damit bei einer berührung alles nur 1x ausgeführt wird
                IsColliding = true;
                
                if (ballVelocity.X < 10 && ballVelocity.X > -10)
                {
                    ballVelocity.X *= -1.04f;
                }
                else if (ballVelocity.X < 15 && ballVelocity.X > -15)
                {
                    ballVelocity.X *= -1.01f;
                }
                else
                {
                    ballVelocity.X *= -1;
                }
                if ((state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W)) && ballVelocity.Y > -7)
                {
                    ballVelocity.Y -= 1 + (0.05f * random.Next(1, 5));
                }
                else if ((state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S)) && ballVelocity.Y < 7)
                {
                    ballVelocity.Y += 1 + (0.05f * random.Next(1, 5));
                }
                else
                {
                    ballVelocity.Y += (random.Next(5) - 2) * 0.5f;
                }
            }
            else
            {
                IsColliding = false;
            }

            #endregion

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private Rectangle GamemodeSign1V1 = new(360, 80, 70, 40); // x, y, breite, höhe
        private Rectangle GamemodeSignVSBOT = new(360, 130, 70, 40);

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(paddleTexture, player1, Color.White);
            _spriteBatch.Draw(paddleTexture, player2, Color.White);
            _spriteBatch.Draw(paddleTexture, ball, Color.White);
            _spriteBatch.DrawString(scoreFont, PointsPlayer2.ToString(), scoreFontPlayer1, Color.White);
            _spriteBatch.DrawString(scoreFont, PointsPlayer1.ToString(), new Vector2(600, 20), Color.White);


            if (Gamemode == 0)
            {
                // 1v1 sign
                DrawRectangle(_spriteBatch, paddleTexture, GamemodeSign1V1, 4, Color.White); // frame for gamemode buttons
                _spriteBatch.DrawString(scoreFont, "1 vs 1", new Vector2(372, 91), Color.White);

                // PvAI sign
                DrawRectangle(_spriteBatch, paddleTexture, GamemodeSignVSBOT, 4, Color.White); // frame for gamemode buttons
                _spriteBatch.DrawString(scoreFont, "vs BOT", new Vector2(370, 140), Color.White);

            }
            if (Gamemode != 0 && !Start)
            {
                _spriteBatch.DrawString(scoreFont, "press space to start", new Vector2(330, 300), Color.White);
                
            }
            if (Gamemode != 0)
            {
                _spriteBatch.DrawString(scoreFont, ball.X.ToString(), new Vector2(200, 300), Color.White);
                _spriteBatch.DrawString(scoreFont, ball.Y.ToString(), new Vector2(200, 330), Color.White);
                _spriteBatch.DrawString(scoreFont, player2.Y.ToString(), new Vector2(200, 360), Color.White);
            }

                _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        // usable function to draw Rectangles
        private static void DrawRectangle(SpriteBatch spriteBatch, Texture2D texture, Rectangle box, int thickness, Color color)
        {
            // Oben
            spriteBatch.Draw(texture, new Rectangle(box.X, box.Y, box.Width, thickness), color);
            // Unten
            spriteBatch.Draw(texture, new Rectangle(box.X, box.Y + box.Height - thickness, box.Width, thickness), color);
            // Links
            spriteBatch.Draw(texture, new Rectangle(box.X, box.Y, thickness, box.Height), color);
            // Rechts
            spriteBatch.Draw(texture, new Rectangle(box.X + box.Width - thickness, box.Y, thickness, box.Height), color);
        }


    }

}