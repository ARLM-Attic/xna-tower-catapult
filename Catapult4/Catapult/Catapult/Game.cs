using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

namespace CatapultMiniGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CatapultGame : Microsoft.Xna.Framework.Game
    {
        #region Fields and properties
        #region Graphic-Fields
        const int screenWidth = 1280;
        const int screenHeight = 720;


        GraphicsDeviceManager graphics;
        public SpriteBatch SpriteBatch { get; set; }
        #endregion

        #region Input-Fields
        public GamePadState CurrentGamePadState { get; set; }

        public GamePadState LastGamePadState { get; set; }

        public KeyboardState CurrentKeyboardState { get; set; }

        public KeyboardState LastKeyboardState { get; set; }

        #endregion
        #region Fonts
        SpriteFont tahomaFont;
        #endregion

        #region Audio
        AudioEngine audioEngine;
        WaveBank waveBank;
        public SoundBank SoundBank { get; set; }
        Cue loopingMusic = null;
        #endregion

        bool runGame = true;

        bool playingGame = false;

        public int HighScore { get; set; }

        Texture2D backGroundTexture;
        Texture2D skyTexture;

        Vector2 screenPosition = Vector2.Zero;

        Texture2D endObjectTexture;

        Vector2 endObjectPos = new Vector2(1000, 500);

        Catapult playerCatapult;

        public float PumpkinDistance { get; set; }

        IAsyncResult result = null;
        bool GameLoadRequested = true;
        StorageDevice device;
        #endregion

        public CatapultGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.ApplyChanges();

            IsFixedTimeStep = true;

            playerCatapult = new Catapult(this);
            Content.RootDirectory = "Content";
        }



        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Init sounds
            audioEngine = new AudioEngine("Content/Sounds/Sounds.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Sounds/Wave Bank.xwb");
            SoundBank = new SoundBank(audioEngine,"Content/Sounds/Sound Bank.xsb");

            loopingMusic = SoundBank.GetCue("Music");
            loopingMusic.Play();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            backGroundTexture = Content.Load<Texture2D>("Textures/ground");
            skyTexture = Content.Load<Texture2D>("Textures/sky");
            endObjectTexture = Content.Load<Texture2D>("Textures/log");

            tahomaFont = Content.Load<SpriteFont>("Fonts/TahomaFont");
          

            playerCatapult.Initialize();


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                runGame = false;
            }

            // We need to load the gamedata
            // Implement later 
            //TODO: Implement Storage

            //Update sounds
            audioEngine.Update();

           

            // Update input
            LastGamePadState = CurrentGamePadState;
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            if (CurrentKeyboardState.IsKeyDown(Keys.F) && LastKeyboardState.IsKeyUp(Keys.F))
                graphics.ToggleFullScreen();

            //we have to wait 3 seconds until we start playing
            if (gameTime.TotalGameTime.Seconds > 3 && playingGame == false)
            {
                playingGame = true;

            }

            //after 3 seconds start the game
            if (playingGame)
            {
                //Update players Catapult
                playerCatapult.Update(gameTime);



                if (playerCatapult.CurrentState == CatapultState.Reset)
                {
                    // reset background and log
                    screenPosition = Vector2.Zero;

                    endObjectPos.X = 1000;
                    endObjectPos.Y = 500;
                }

                // Move the background
                if (playerCatapult.CurrentState == CatapultState.ProjectileFlying)
                {
                    screenPosition.X = (playerCatapult.PumpkinPosition.X -
                        playerCatapult.PumpkinLaunchPosition) * -1.0f;
                    endObjectPos.X = (playerCatapult.PumpkinPosition.X -
                        playerCatapult.PumpkinLaunchPosition) * -1.0f + 1000;
                }

                //Calculate the pumpkin flying distance
                if (playerCatapult.CurrentState == CatapultState.ProjectileFlying ||
                    playerCatapult.CurrentState == CatapultState.ProjectileHit)
                {
                    PumpkinDistance = playerCatapult.PumpkinPosition.X -
                        playerCatapult.PumpkinLaunchPosition;
                    PumpkinDistance /= 15.0f;
                }

                //Is this a highscore?
                if (HighScore < PumpkinDistance)
                    HighScore = (int)PumpkinDistance;
            }

            // Exit game
            if (!runGame)
                Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Where to draw the Sky
            Vector2 skyDrawPos = Vector2.Zero;
            skyDrawPos.Y -= 50;
            skyDrawPos.X = (screenPosition.X / 6) % 3840;

            // Where to draw the background hills
            Vector2 backgroundDrawPos = Vector2.Zero;
            backgroundDrawPos.Y += 225;
            backgroundDrawPos.X = screenPosition.X % 1920;

            string printString;
            Vector2 FontOrigin;

            //start Drawing
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);


            //draw the sky
            SpriteBatch.Draw(skyTexture, skyDrawPos, Color.White);
            SpriteBatch.Draw(skyTexture, skyDrawPos + new Vector2(skyTexture.Width, 0),
               null, Color.White, 0, Vector2.Zero, 1,
               SpriteEffects.FlipHorizontally, 0);
            //Check to see if we need to draw another sky
            if (skyDrawPos.X <= -(skyTexture.Width * 2) + screenWidth)
            {
                skyDrawPos.X += skyTexture.Width * 2;
                SpriteBatch.Draw(skyTexture, skyDrawPos, Color.White);
                SpriteBatch.Draw(skyTexture, skyDrawPos +
                    new Vector2(skyTexture.Width, 0),
                    null, Color.White, 0, Vector2.Zero, 1,
                    SpriteEffects.FlipHorizontally, 0);
            }

            // draw the background
            SpriteBatch.Draw(backGroundTexture, backgroundDrawPos, Color.White);
            SpriteBatch.Draw(backGroundTexture, backgroundDrawPos +
                     new Vector2(backGroundTexture.Width, 0),
                     null, Color.White, 0, Vector2.Zero, 1,
                     SpriteEffects.FlipHorizontally, 0);

            // Check to see if we need to draw another background
            if (backgroundDrawPos.X <= -(backGroundTexture.Width * 2) + screenWidth)
            {
                backgroundDrawPos.X += backGroundTexture.Width * 2;
                SpriteBatch.Draw(backGroundTexture, backgroundDrawPos, Color.White);
                SpriteBatch.Draw(backGroundTexture, backgroundDrawPos +
                new Vector2(backGroundTexture.Width, 0),
                null, Color.White, 0, Vector2.Zero, 1,
                SpriteEffects.FlipHorizontally, 0);
            }

            // Draw the background once
            SpriteBatch.Draw(backGroundTexture, backgroundDrawPos, Color.White);
            SpriteBatch.Draw(backGroundTexture, backgroundDrawPos +
                new Vector2(backGroundTexture.Width, 0),
                null, Color.White, 0, Vector2.Zero, 1,
                SpriteEffects.FlipHorizontally, 0);

            // Draw the log at the end
            SpriteBatch.Draw(endObjectTexture, endObjectPos, Color.White);

            //Draw the Catapult
            playerCatapult.Draw(gameTime);

            if (!playingGame)
            {
                printString = "Catapult";
                FontOrigin = tahomaFont.MeasureString(printString) / 2;
                SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 252),
                new Color(new Vector4(0, 0, 0, 3 -
                    (float)gameTime.TotalGameTime.TotalSeconds)), 0, FontOrigin, 2, SpriteEffects.None, 0);
                SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 250),
                    new Color(new Vector4(0.5f, 1.0f, 0.2f, 3 -
                        (float)gameTime.TotalGameTime.TotalSeconds)), 0, FontOrigin, 2, SpriteEffects.None, 0);
            }
            else
            {
                //we have started
                if (playerCatapult.CurrentState == CatapultState.Rolling)
                {
                    float rightTriggerAmt = CurrentGamePadState.Triggers.Right;
                    if (rightTriggerAmt > 0.5f)
                        rightTriggerAmt = 1.0f - rightTriggerAmt;
                    if (CurrentKeyboardState.IsKeyDown(Keys.B))
                        rightTriggerAmt = 0.5f;

                    rightTriggerAmt *= 2;
                    printString = "Power Bonus: " + rightTriggerAmt.ToString("p1");
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(52, 62),
                        Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(52, 60),
                        Color.Azure, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }

                printString = "Distance: " + ((int)PumpkinDistance).ToString() + "ft.";
                SpriteBatch.DrawString(tahomaFont, printString, new Vector2(802, 17),
                    Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                SpriteBatch.DrawString(tahomaFont, printString, new Vector2(800, 15),
                    Color.Azure, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                if (playerCatapult.PumpkinPosition.Y < -32)
                {
                    float pumpkinHeight = -playerCatapult.PumpkinPosition.Y / 15.0f;
                    printString = ((int)pumpkinHeight).ToString() + " ft.";
                    SpriteBatch.DrawString(tahomaFont, printString,
                                new Vector2(playerCatapult.PumpkinLaunchPosition, 62),
                                Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    SpriteBatch.DrawString(tahomaFont, printString,
                                new Vector2(playerCatapult.PumpkinLaunchPosition, 60),
                                Color.Azure, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }

                // print new highscore if over 1000
                if (playerCatapult.CurrentState == CatapultState.ProjectileHit &&
                    HighScore == (int)PumpkinDistance && HighScore > 1000)
                {
                    printString = "High Score!";
                    FontOrigin = tahomaFont.MeasureString(printString) / 2;
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 252), Color.Black, 0, FontOrigin, 2, SpriteEffects.None, 0);
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 250), Color.Gold, 0, FontOrigin, 2, SpriteEffects.None, 0);
                }
                else if (playerCatapult.CurrentState == CatapultState.Crash)
                {
                    printString = "Crash!";
                    FontOrigin = tahomaFont.MeasureString(printString) / 2;
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 252), Color.Black, 0, FontOrigin, 2, SpriteEffects.None, 0);
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 250), Color.Red, 0, FontOrigin, 2, SpriteEffects.None, 0);
                }
                //Did we get boost power?
                else if (playerCatapult.BoostPower > 0)
                {
                    string boosPoints = "";
                    for (int i = 0; i < playerCatapult.BoostPower; i++)
                        boosPoints += "!";

                    printString = "Boost Power" + boosPoints;
                    FontOrigin = tahomaFont.MeasureString(printString) / 2;
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 252), Color.Black, 0, FontOrigin, 2, SpriteEffects.None, 0);
                    SpriteBatch.DrawString(tahomaFont, printString, new Vector2(screenWidth / 2, 250), Color.Red, 0, FontOrigin, 2, SpriteEffects.None, 0);
                }

            }

            SpriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
