using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;




namespace Snake
{
    public class Game1 : Game
    {
        enum GameState
        {
            MainMenu,
            GamePlay,
            AchievementsAndStats,
            Debug,
            ControlsMenu,
            OptionsMenu,
            Controls2,
            ResolutionPicker,
            SkinChooser,
            ColorPicker,
            BackgroundColorPicker,
            ColorConfirm,
            GameOver
        };


        public class AchievementStat
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }


        class Food //should be renamed to PowerUp
        {
            public Point Pos;
            public int Type;
            public bool IsShuffling = true; // Whether the food is shuffling colors
            public float shuffleTimer = 0f; // Timer to control color shuffling
            public float totalElapsedTime = 0f; // Total elapsed time since shuffling started
            public Color Color; // Current color of the food
        }

        class Snake
        {
            public Point Pos;
            public Point Vel;
            public double Speed;
            public double StepTimer;
            public int TailLength;
            public Point[] Tail;
            public float[] TailAlpha;
            public bool IsVisible { get; set; }

            private const int MaxTailLength = 1000; // Adjust this value as needed

            public Snake()
            {
                Tail = new Point[MaxTailLength];
                TailAlpha = new float[MaxTailLength];
                ResetSnake();
            }

            public void ResetSnake()
            {
                Pos = new Point(0, 0);
                Vel = new Point(0, 0);
                Speed = 0.1;
                StepTimer = 0;
                TailLength = 0;
                IsVisible = true;

                // Reset all tail alphas to 1.0f
                for (int i = 0; i < TailAlpha.Length; i++)
                {
                    TailAlpha[i] = 1.0f;
                }
            }

            public void AddTailSegment()
            {
                if (TailLength < MaxTailLength)
                {
                    Tail[TailLength] = Pos;
                    TailAlpha[TailLength] = 1.0f;
                    TailLength++;
                }
            }

            public void UpdateTailAlpha(float alpha)
            {
                for (int i = 0; i < TailLength; i++)
                {
                    TailAlpha[i] = alpha;
                }
            }
        }

        private int prevScrollWheelValue;

        GameState gameState;
        Random random;
        SpriteFont spriteFont;
        int cellSize;
        int score;
        int defaultFoodPosX;
        int defaultFoodPosY;
        Point worldSize;
        int worldScreenTopY;
        Snake snake;
        Food[] food;
        Food defaultFood;
        Color[] foodColorList;
        SoundEffect foodMunch;
        SoundEffect lowBeep;
        SoundEffect highBeep;

        int selectedSlider;
        int selectedResolution;
        string redString;
        bool enterKeyPressed;
        KeyboardState prevKeyboardState;
        int colorIndex = 0;
        int currentTextureIndex = 0;
        int highScore;
        bool isScorePassed;
        bool wasUpArrowPressed;
        bool wasDownArrowPressed;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        bool isMainMenuVisible = true;
        string[] menuOptions = { "Start Game", "Achivements & Stats", "Controls Menu", "Options", "Exit" };
        int SelectedMainMenuIndex = 0;

        bool isControlsMenuVisible = false;
        string[] controlsMenuOptions = { "Move Up", "Move Down", "Move Left", "Move Right", "Enter", "Back" };
        int selectedControlsMenuIndex = 0;


        bool isOptionsMenuVisible = false;
        string[] optionsMenuOptions = { "Color Picker Options", "Back" };
        int selectedOptionsMenuIndex = 0;


        bool isColorPickerOptionsVisible = false;
        string[] colorPickerOptions = { "Background Color", "Back" };
        int selectedColorPickerIndex = 0;


        SpriteFont font;
        SpriteFont mainMenuTextFont;
        Texture2D texPressEnter;
        Texture2D[] textures;
        Texture2D[] resolutionTextureSize;
        Texture2D texture;
        Texture2D texSnakeArt;
        Texture2D texSnakeGameText;
        Texture2D texO;
        Texture2D texN;
        Texture2D texS;
        Texture2D texC;
        Texture2D texB;
        Texture2D texR;
        Texture2D texM;
        Texture2D texSkinSelector;
        Texture2D texColorPickerText;
        Texture2D texControlsText;
        Texture2D texBackgroundColorText;
        Texture2D texResolutionPicker;
        Texture2D texAvailableResolutions;
        Texture2D texUp;
        Texture2D texDown;
        Texture2D texLeft;
        Texture2D texRight;
        Texture2D texEsc;
        Texture2D texBack;
        Texture2D texEnter;
        Texture2D texSlideBarBorder;
        Texture2D texChangeConfirm;
        Texture2D texGameOverText;
        Texture2D texY;
        Texture2D texBlackViper;
        Texture2D texRedSliderBar;
        Texture2D texGreenSliderBar;
        Texture2D texBlueSliderBar;
        Texture2D texSliderBarLong;
        Texture2D texBlackTexture;
        Texture2D texEnterKey;
        Texture2D texUpArrowButton;
        Texture2D texDownArrowButton;

        private Texture2D gradientTexture;






        Color background;
        Color redPreview;
        Color redBoxPreview;
        Color greenBoxPreview;
        Color blueBoxPreview;
        Color backgroundColor = Color.DarkOliveGreen; // Default color
        Color[][] snakTailColorPresets = new Color[][]
          {
       new Color[] { Color.Red, Color.Black },
       new Color[] { Color.White, Color.Blue },
       new Color[] { Color.Cyan, Color.PaleVioletRed, Color.DarkBlue }
        };


        private LifetimeStats lifetimeStats;

        float xPosition = 380f; // Initial x position
        const float xPositionIncrement = 1.96078431372549f; // Increment value for moving left/right
        const float minX = 380f; // Minimum x position
        const float maxX = 880f; // Maximum x position

        float xRed = 0f; // Initial Red position
        const float xRedIncrement = 1f; // Increment value for moving left/right
        const float minRed = 0f; // Minimum Red position
        const float maxRed = 255f; // Maximum Red position

        float yPosition = 380f; // Initial x position
        const float YPositionIncrement = 1.96078431372549f; // Increment value for moving left/right
        const float minY = 380f; // Minimum x position
        const float maxY = 880f; // Maximum x position

        float xGreen = 0f; // Initial Red position
        const float xGreenIncrement = 1f; // Increment value for moving left/right
        const float minGreen = 0f; // Minimum Red position
        const float maxGreen = 255f; // Maximum Red position

        float zPosition = 380f; // Initial x position
        const float ZPositionIncrement = 1.96078431372549f; // Increment value for moving left/right
        const float minZ = 380f; // Minimum x position
        const float maxZ = 880f; // Maximum x position

        float xBlue = 0f; // Initial Red position
        const float xBlueIncrement = 1f; // Increment value for moving left/right
        const float minBlue = 0f; // Minimum Red position
        const float maxBlue = 255f; // Maximum Red position

        float xBarPosition; // Will be updated to match xPosition
        float yBarPosition; // Will be updated to match yPosition
        float zBarPosition; // Will be updated to match zPosition

        bool isFading = false; // Flag to check if the fade effect is active
        bool fadingIn = false; // Indicates whether fading in or out
        float fadeSpeed = 1.0f; // Speed of the fade effect
        float fadeAlpha = 0.0f; // Alpha value of the fade overlay


        public float[] TailAlpha; // Array to store alpha values for each tail segment



        private float flashDuration = 0.1f; // Duration of each flash cycle (seconds)
        private float totalFlashTime = 1f; // Total time for flashing effect (seconds)
        private float elapsedTime = 0f;     // Time elapsed for current flash cycle
        private float flashTimeElapsed = 0f; // Time elapsed since flashing started
        private bool isFlashing = false;    // Toggle flashing state
        private string flashText = "";      // Text to display during flashing
        private Vector2 textPosition = new Vector2(100, 100); // Position of the text
        private Color textColor = Color.White; // Default color
        private float textScale = 2f;        // Scale of the text
        private bool isTextVisible = false; // Toggle text visibility
        private bool isPressKeyPromptVisible = true; // Flag to control visibility of the prompt



        private bool isShuffling = false;       // Flag to indicate if shuffling is active
        public bool IsShuffling = true; // Whether the food is shuffling colors
        public float totalElapsedTime = 0f; // Total elapsed time since shuffling started


        private float currentCellSize;
        private float targetCellSize;
        private float transitionSpeed = 2f; // Adjust this to control the speed of the transition

        List<AchievementStat> achievementStats;
        int scrollPosition = 0;
        int maxVisibleItems = 15; // Number of items visible at once
        int itemHeight = 35; // Height of each item
        Rectangle scrollBarBounds;
        bool isDragging = false;
        Vector2 lastMousePosition;
        Rectangle achievementsBox;
        Rectangle scrollBarHandle;

        private float scrollSmoothness = 0.1f; // Increased from 0.1f
        private float scrollThreshold = 5f; // Adjust this value to change sensitivity
        private float targetScrollPosition = 0f;
        private float currentScrollPosition = 0f;
        private float scrollInterpolationSpeed = 0.1f; // Adjust this for smoother or faster scrolling
        private float maxScrollPosition; // Maximum scroll in pixels

        private bool startedWithArrow = false;
        private bool isInteractingWithScrollbar = false;
        private bool isScrollbarActive = true; // This will allow interaction with the scrollbar by default
        bool isInteractingWithControls = false;
        private const float arrowClickScrollSpeed = 2f; // Adjust this value as needed
        private bool isDraggingScrollBarFromHandle = false;
        private bool isHoldingUpArrowButton = false;
        private bool isHoldingDownArrowButton = false;






        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;


            // Use variables for default width and height
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

        }

        protected override void Initialize()
        {
            base.Initialize();
            LoadHighScore();
            LoadLifetimeStats();
            prevMouseState = Mouse.GetState();


            random = new Random();
            KeyboardState prevKeyboardState;
            prevScrollWheelValue = Mouse.GetState().ScrollWheelValue;




            foodColorList = new Color[]
            {
                Color.Red,      // Type 0
                Color.Orange,   // Type 1
                Color.Yellow,   // Type 2
                Color.Green,    // Type 3
                Color.Blue,     // Type 4
                Color.Purple,   // Type 5
                Color.Brown,    // Type 6
                Color.Black,     // Type 7
                Color.Pink
            };

            cellSize = 16;
            worldScreenTopY = 16;
            worldSize.X = GraphicsDevice.Viewport.Width / cellSize;
            worldSize.Y = (GraphicsDevice.Viewport.Height - worldScreenTopY) / cellSize;
            snake = new Snake();
            snake.Tail = new Point[worldSize.X * worldSize.Y];
            xBarPosition = xPosition;
            yBarPosition = yPosition;
            zBarPosition = zPosition;
            defaultFoodPosX = random.Next(0, worldSize.X);
            defaultFoodPosY = random.Next(0, worldSize.Y);
            Color DefaultColor = Color.White;
            int highScore = 0;
            food = new Food[1];

            for (int i = 0; i < food.Length; ++i)
            {
                food[0] = new Food();

            }

            InitializeAchievements();
            scrollBarBounds = new Rectangle(GraphicsDevice.Viewport.Width - 20, 100, 20, 400);

            List<AchievementStat> achievementStats;
            int scrollPosition = 0;
            int maxVisibleItems = 5; // Number of items visible at once
            int itemHeight = 30; // Height of each

            achievementStats = new List<AchievementStat>();
            // Populate achievementStats with your data
            for (int i = 0; i < 30; i++)
            {
                achievementStats.Add(new AchievementStat { Name = $"Achievement {i}", Value = $"Value {i}" });
            }

            scrollBarBounds = new Rectangle(GraphicsDevice.Viewport.Width - 20, 100, 20, 400);



        }

        protected override void LoadContent()
        {
            // one time asset creation
            Window.Position = new Point(1200, 100);
            mainMenuTextFont = Content.Load<SpriteFont>("CourierNew");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            foodMunch = Content.Load<SoundEffect>("munch");
            lowBeep = Content.Load<SoundEffect>("lowBeep");
            highBeep = Content.Load<SoundEffect>("highBeep");

            spriteFont = Content.Load<SpriteFont>("spriteFont"); // Ensure this matches your asset name

            texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData<Color>(new Color[] { Color.White });

            textures = new Texture2D[2];
            textures[0] = Content.Load<Texture2D>("gameOver");
            textures[1] = Content.Load<Texture2D>("blackViper");


            resolutionTextureSize = new Texture2D[16];

            prevKeyboardState = Keyboard.GetState();
            CreateGradientTexture();



            texPressEnter = Content.Load<Texture2D>("texPressEnter");
            texSnakeArt = Content.Load<Texture2D>("snake");
            texSnakeGameText = Content.Load<Texture2D>("snakeGameText");
            texBlackTexture = Content.Load<Texture2D>("texBlackTexture");
            texO = Content.Load<Texture2D>("O");
            texN = Content.Load<Texture2D>("N");
            texS = Content.Load<Texture2D>("texS");
            texC = Content.Load<Texture2D>("texC");
            texB = Content.Load<Texture2D>("texB");
            texR = Content.Load<Texture2D>("texR");
            texM = Content.Load<Texture2D>("texM");
            texEsc = Content.Load<Texture2D>("Esc");
            texEnter = Content.Load<Texture2D>("texEnter");
            texUp = Content.Load<Texture2D>("up");
            texDown = Content.Load<Texture2D>("down");
            texLeft = Content.Load<Texture2D>("left");
            texRight = Content.Load<Texture2D>("right");
            texBack = Content.Load<Texture2D>("back");
            texGameOverText = Content.Load<Texture2D>("GameOverText");
            texY = Content.Load<Texture2D>("Y");
            texSkinSelector = Content.Load<Texture2D>("texSkinSelector");
            texColorPickerText = Content.Load<Texture2D>("texColorPickerText");
            texControlsText = Content.Load<Texture2D>("texControlsText");
            texBackgroundColorText = Content.Load<Texture2D>("texBackgroundColorText");
            texRedSliderBar = Content.Load<Texture2D>("texRedSliderBar");
            texGreenSliderBar = Content.Load<Texture2D>("texGreenSliderBar");
            texBlueSliderBar = Content.Load<Texture2D>("texBlueSliderBar");
            texSlideBarBorder = Content.Load<Texture2D>("texSlideBarBorder");
            texChangeConfirm = Content.Load<Texture2D>("texChangeConfirm");
            texSliderBarLong = Content.Load<Texture2D>("texSliderBarLong");
            texResolutionPicker = Content.Load<Texture2D>("texResolutionPicker");
            texAvailableResolutions = Content.Load<Texture2D>("texAvailableResolutions");
            texUpArrowButton = Content.Load<Texture2D>("texUpArrowButton");
            texDownArrowButton = Content.Load<Texture2D>("texDownArrowButton");

        }

        //do a condition check to see if the high score is passed


        Color GetFlashingColor(GameTime gameTime, Color color1, Color color2)
        {
            // Get a value that oscillates between 0 and 1 using a sine wave
            float t = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5); // 2 is the speed of the flashing
            t = (t + 1) / 2; // Normalize to 0-1 range

            // Interpolate between the two colors based on the t value
            return Color.Lerp(color1, color2, t);
        }

        void InitGamePlay()
        {
            gameState = GameState.GamePlay;
            score = 0;
            snake.ResetSnake();
            snake.Pos.X = worldSize.X / 2;
            snake.Pos.Y = worldSize.Y / 2;
            snake.Vel.X = 0;
            snake.Vel.Y = 0;

            isFadingTail = false;
            fadeTimer = 0f;

            // No need to reset TailAlpha here, as it's done in ResetSnake()

            for (int i = 0; i < food.Length; ++i)
            {
                SpawnFood(i);
            }

            isBlackFadeActive = false;
            blackFadeTimer = 0f;
            blackFadeAlpha = 1.0f;

            // Reset any other game state variables as needed
        }

        private string GetHighScorePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string gameFolderPath = Path.Combine(appDataPath, "SnakeGame");

            // Create the game folder if it doesn't exist
            if (!Directory.Exists(gameFolderPath))
            {
                Directory.CreateDirectory(gameFolderPath);
            }

            return Path.Combine(gameFolderPath, "highscore.txt");
        }

        private void SaveHighScore()
        {
            string path = GetHighScorePath();
            File.WriteAllText(path, highScore.ToString());
        }

        private void LoadHighScore()
        {
            string path = GetHighScorePath();
            if (File.Exists(path))
            {
                string scoreStr = File.ReadAllText(path);
                if (int.TryParse(scoreStr, out int loadedScore))
                {
                    highScore = loadedScore;
                }
            }
            else
            {
                highScore = 0;
                SaveHighScore();
            }
        }

        private string GetLifetimeStatsPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string gameFolderPath = Path.Combine(appDataPath, "SnakeGame");

            if (!Directory.Exists(gameFolderPath))
            {
                Directory.CreateDirectory(gameFolderPath);
            }

            return Path.Combine(gameFolderPath, "lifetimestats.json");
        }

        private void SaveLifetimeStats()
        {
            string json = JsonConvert.SerializeObject(lifetimeStats);
            File.WriteAllText(GetLifetimeStatsPath(), json);
        }

        private void LoadLifetimeStats()
        {
            string path = GetLifetimeStatsPath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                lifetimeStats = JsonConvert.DeserializeObject<LifetimeStats>(json);
            }
            else
            {
                lifetimeStats = new LifetimeStats();
                SaveLifetimeStats();
            }
        }


        private void StopFlashing()
        {
            isFlashing = false;
            flashText = ""; // Clear the text
                            //      textColor = Color.Transparent; // Hide the text
            elapsedTime = 0f; // Reset elapsed time
                              //        flashTimeElapsed = 0f; // Reset total flashing time
        }





        float textureFadeAlpha = 0.5f; // Start semi-transparent (adjust as needed)
        float minAlpha = 0.2f; // Minimum alpha value (e.g., 0.3 for 30% opacity)
        float maxAlpha = 0.7f; // Maximum alpha value (e.g., 0.8 for 80% opacity)
        float textureFadeSpeed = 01.5f; // Speed of the fade effect (adjust as needed)
        float fadeTime = 0.8f; // Time elapsed for the fade effect
        void UpdateTextureFadeEffect(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fadeTime += elapsedTime * textureFadeSpeed;

            // Smooth transition using a sinusoidal function
            float fadeAmount = (float)(Math.Sin(fadeTime) * 0.4 + 0.5); // Value between 0 and 1

            // Interpolate between minAlpha and maxAlpha
            textureFadeAlpha = MathHelper.Lerp(minAlpha, maxAlpha, fadeAmount);

            // Optional: Reset fadeTime to avoid overflow (if needed)
            if (fadeTime > Math.PI * 2) // Complete cycle (0 to 1 and back to 0)
            {
                fadeTime -= (float)(Math.PI * 1);
            }
        }

        void InitMainMenu()
        {
            isMainMenuVisible = true;
            gameState = GameState.MainMenu;
            isBlackFadeActive = false;
            blackFadeTimer = 0f;
            blackFadeAlpha = 1.0f;


        }


        void initAchievementsAndStats()
        {
            gameState = GameState.AchievementsAndStats;
        }


        void InitControlsMenu()
        {
            gameState = GameState.ControlsMenu;
        }




        void InitOptionsMenu()
        {

            gameState = GameState.OptionsMenu;

        }





        void InitControls2()
        {
            gameState = GameState.Controls2;
        }
        void InitResolutionPicker()
        {
            gameState = GameState.ResolutionPicker;
        }

        void InitColorPicker()
        {
            gameState = GameState.ColorPicker;
        }

        void InitSkinChooser()
        {
            gameState = GameState.SkinChooser;
        }

        void InitBackgroundColorPicker()
        {
            gameState = GameState.BackgroundColorPicker;
        }

        void InitColorConfirm()
        {
            gameState = GameState.ColorConfirm;
        }

        void InitGameOver()
        {
            gameState = GameState.GameOver;
            if (score > highScore)
            {
                highScore = score;
                SaveHighScore();
            }
            // Stop any ongoing fade effects to prevent issues during restart
            isBlackFadeActive = false;
            blackFadeTimer = 0f;
            blackFadeAlpha = 1.0f;

            // Reset flash text and other relevant states if necessary
            flashText = ""; // Clear any previous flash text
            isFlashing = false; // Stop flashing

            // Reset tail visibility and alpha values
            for (int i = 0; i < snake.TailLength; i++)
            {
                snake.TailAlpha[i] = 1.0f; // Ensure all segments are fully visible again
            }

            snake.IsVisible = true; // Start as visible
            TailAlpha = new float[100]; // Same size as Tail
            for (int i = 0; i < TailAlpha.Length; i++)
            {
                TailAlpha[i] = 1.0f; // Set all tail segments to fully opaque
            }
        }

        void SpawnFood(int i)
        {
            food[i].Pos.X = random.Next(0, worldSize.X);
            food[i].Pos.Y = random.Next(0, worldSize.Y);
            food[i].Type = random.Next(0, foodColorList.Length);
        }


        void SpawnDefaultFood()
        {
            defaultFoodPosX = random.Next(0, worldSize.X);
            defaultFoodPosY = random.Next(0, worldSize.Y);
        }






        string[] GetUpdatedMenuOptions()
        {
            string[] updatedMenuOptions = new string[menuOptions.Length];

            for (int i = 0; i < menuOptions.Length; i++)
            {
                if (i == SelectedMainMenuIndex)
                {
                    updatedMenuOptions[i] = $"> {menuOptions[i]} <";
                }
                else
                {
                    updatedMenuOptions[i] = menuOptions[i];
                }
            }

            return updatedMenuOptions;
        }







        string[] GetUpdatedControlsMenuOptions()
        {
            // Create a new array to hold the updated menu options
            string[] updatedControlsMenuOptions = new string[controlsMenuOptions.Length];

            // Iterate over the controlsMenuOptions array
            for (int i = 0; i < controlsMenuOptions.Length; i++)
            {
                // Check if the current index is the selected index
                if (i == selectedControlsMenuIndex)
                {
                    // Add the selected option indicators around the selected option
                    updatedControlsMenuOptions[i] = $"> {controlsMenuOptions[i]} <";
                }
                else
                {
                    // Copy the non-selected option
                    updatedControlsMenuOptions[i] = controlsMenuOptions[i];
                }
            }

            return updatedControlsMenuOptions;
        }



        string[] GetUpdatedOptionsMenuOptions()
        {
            // Create a new array to hold the updated menu options
            string[] updatedOptionsMenuOptions = new string[optionsMenuOptions.Length];

            // Iterate over the optionsMenuOptions array
            for (int i = 0; i < optionsMenuOptions.Length; i++)
            {
                // Check if the current index is the selected index
                if (i == selectedOptionsMenuIndex)
                {
                    // Add the selected option indicators around the selected option
                    updatedOptionsMenuOptions[i] = $"> {optionsMenuOptions[i]} <";
                }
                else
                {
                    // Copy the non-selected option
                    updatedOptionsMenuOptions[i] = optionsMenuOptions[i];
                }
            }

            return updatedOptionsMenuOptions;
        }


        private float scrollVelocity = 0f;
        private const float scrollAcceleration = 0.5f;
        private const float maxScrollVelocity = 10f;
        private const float scrollDeceleration = 0.9f;
        private bool isUpArrowClicked = false;
        private bool isDownArrowClicked = false;
        private bool isHoldingUpArrow = false;
        private bool isHoldingDownArrow = false;
        void HandleInput(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Handle MainMenu
            if (gameState == GameState.MainMenu && isMainMenuVisible)
            {
                if (keyboardState.IsKeyDown(Keys.Up) && prevKeyboardState.IsKeyUp(Keys.Up))
                {
                    SelectedMainMenuIndex = (SelectedMainMenuIndex - 1 + menuOptions.Length) % menuOptions.Length;
                }
                else if (keyboardState.IsKeyDown(Keys.Down) && prevKeyboardState.IsKeyUp(Keys.Down))
                {
                    SelectedMainMenuIndex = (SelectedMainMenuIndex + 1) % menuOptions.Length;
                }

                if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
                {
                    switch (SelectedMainMenuIndex)
                    {
                        case 0:
                            // Start Game
                            gameState = GameState.GamePlay;
                            isMainMenuVisible = false;
                            InitGamePlay();
                            isDebugMenuVisible = false;

                            System.Threading.Thread.Sleep(200);  // Pause briefly
                            isPressKeyPromptVisible = true; // Show the prompt when the game starts
                            isBlackFadeActive = false;
                            blackFadeTimer = 0f;
                            blackFadeAlpha = 1.0f;

                            // Initialize gameplay
                            break;
                        case 1: // Assuming this is the index for Achievements & Stats
                            gameState = GameState.AchievementsAndStats;
                            isMainMenuVisible = false;
                            scrollPosition = 0;
                            InitializeAchievements(); // Reinitialize achievements when entering this state
                            SetupAchievementsUI(); // Setup the UI elements
                            break;
                        case 2:
                            // Start Game
                            gameState = GameState.ControlsMenu;
                            isMainMenuVisible = false;
                            isControlsMenuVisible = true;

                            InitControlsMenu();  // Initialize gameplay
                            break;

                        case 3:
                            // Show Options Menu
                            gameState = GameState.OptionsMenu;

                            isMainMenuVisible = false;
                            isOptionsMenuVisible = true;
                            selectedOptionsMenuIndex = 0;

                            break;
                        case 4:
                            Exit();
                            break;
                    }
                }
            }




            else if (gameState == GameState.AchievementsAndStats)
            {
                MouseState mouseState = Mouse.GetState();
                KeyboardState currentKeyboardState = Keyboard.GetState();
                bool hasPlayedSliderSound = false; // Add this line at the beginning of the method or class


                float scrollTimer = 0f;
                int scrollCounter = 0;

                const float arrowClickScrollSpeed = 8f;
                const float continuousScrollSpeed = 8f;
                const float keyboardScrollSpeed = 600f; // Significantly increased for noticeable effect

                bool hasArrowButtonBeenClicked;
                const int handleClickPadding = 0;
                Rectangle handleClickableArea = new Rectangle(scrollBarHandle.X - handleClickPadding,
                                                               scrollBarHandle.Y - handleClickPadding,
                                                               scrollBarHandle.Width + handleClickPadding * 2,
                                                               scrollBarHandle.Height + handleClickPadding * 2);

                bool isLeftMouseButtonPressed = mouseState.LeftButton == ButtonState.Pressed;
                bool isMouseOverUpArrowButton = upArrowButton.Contains(mouseState.Position);
                bool isMouseOverDownArrowButton = downArrowButton.Contains(mouseState.Position);
                bool isMouseOverScrollbarHandle = handleClickableArea.Contains(mouseState.Position);
                bool isMouseOverScrollbarBounds = scrollBarBounds.Contains(mouseState.Position);

                // Define the scroll bar as a scissor area
                GraphicsDevice.ScissorRectangle = scrollBarBounds;
                GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                Color scrollBarDefaultColor = new Color(200, 200, 200);
                Color scrollBarHoverColor = new Color(220, 220, 220);
                Color scrollBarClickColor = new Color(180, 180, 180);
                Color scrollHandleDefaultColor = Color.White;
                Color scrollHandleHoverColor = new Color(230, 230, 230);
                Color scrollHandleClickColor = new Color(200, 200, 200);
                Color arrowButtonDefaultColor = Color.White;
                Color arrowButtonHoverColor = new Color(230, 230, 230);
                Color arrowButtonClickColor = new Color(180, 180, 180);

                float gradientExtent = 0.5f;
                float gradientIntensity = 0.3f;

                // Determine current colors based on mouse state
                Color currentScrollBarColor = scrollBarDefaultColor;
                Color currentScrollHandleColor = scrollHandleDefaultColor;
                Color currentUpArrowColor = arrowButtonDefaultColor;
                Color currentDownArrowColor = arrowButtonDefaultColor;

                bool isMouseOverScrollHandle = handleClickableArea.Contains(mouseState.Position);
                bool isMouseOverUpArrow = upArrowButton.Contains(mouseState.Position);
                bool isMouseOverDownArrow = downArrowButton.Contains(mouseState.Position);
                bool isMouseOverScrollBar = scrollBarBounds.Contains(mouseState.Position);

                bool isDraggingScrollBar = false;

                ;

                if (isMouseOverScrollBar)
                {
                    currentScrollBarColor = isLeftMouseButtonPressed ? scrollBarClickColor : scrollBarHoverColor;
                }

                if (isMouseOverScrollHandle)
                {
                    currentScrollHandleColor = isLeftMouseButtonPressed ? scrollHandleClickColor : scrollHandleHoverColor;
                }

                if (isMouseOverUpArrow)
                {
                    currentUpArrowColor = isLeftMouseButtonPressed ? arrowButtonClickColor : arrowButtonHoverColor;
                }

                if (isMouseOverDownArrow)
                {
                    currentDownArrowColor = isLeftMouseButtonPressed ? arrowButtonClickColor : arrowButtonHoverColor;
                }

                if (keyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
                {
                    flashText = ""; // Clear any previous flash text
                    isFlashing = false; // Stop flashing
                    gameState = GameState.MainMenu; // Switch to main menu
                    isMainMenuVisible = true; // Show the main menu
                }

                bool isScrollingUp = currentKeyboardState.IsKeyDown(Keys.Up);
                bool isScrollingDown = currentKeyboardState.IsKeyDown(Keys.Down);

                // Continuous scrolling logic (without sound)
                if (isScrollingUp)
                {
                    if (prevKeyboardState.IsKeyUp(Keys.Up))
                    {
                        lowBeep.Play();
                    }
                    currentScrollPosition = MathHelper.Clamp(currentScrollPosition - keyboardScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0, maxScrollPosition);
                }

                if (isScrollingDown)
                {
                    if (prevKeyboardState.IsKeyUp(Keys.Down))
                    {
                        highBeep.Play();
                    }
                    currentScrollPosition = MathHelper.Clamp(currentScrollPosition + keyboardScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0, maxScrollPosition);
                }

                // Keep the sound for mouse clicks on arrow buttons
                if (isLeftMouseButtonPressed && prevMouseState.LeftButton == ButtonState.Released)
                {
                    if (isMouseOverUpArrowButton)
                    {
                        lowBeep.Play();
                    }
                    else if (isMouseOverDownArrowButton)
                    {
                        highBeep.Play();
                    }
                    else if (isMouseOverScrollbarHandle && !hasPlayedSliderSound)
                    {
                        highBeep.Play();
                        hasPlayedSliderSound = true;
                    }
                }
                else if (mouseState.LeftButton == ButtonState.Released)
                {
                    hasPlayedSliderSound = false;
                }

                if (isHoldingUpArrowButton && !isMouseOverUpArrowButton)
                {
                    isHoldingUpArrowButton = false;
                }
                if (isHoldingDownArrowButton && !isMouseOverDownArrowButton)
                {
                    isHoldingDownArrowButton = false;
                }

                if (!isMouseOverUpArrowButton && !isMouseOverDownArrowButton)
                {
                    if (!isLeftMouseButtonPressed)
                    {
                        isHoldingUpArrowButton = false;
                        isHoldingDownArrowButton = false;
                        hasArrowButtonBeenClicked = false;
                    }
                }

                if (!isLeftMouseButtonPressed)
                {
                    isHoldingUpArrowButton = false;
                    isHoldingDownArrowButton = false;
                    hasArrowButtonBeenClicked = false;
                }

                int sideBuffer = 100;
                int verticalBuffer = 17;

                Rectangle scrollbarHitbox = new Rectangle(
                    scrollBarBounds.Left - sideBuffer,
                    scrollBarBounds.Top - verticalBuffer,
                    scrollBarBounds.Width + (2 * sideBuffer),
                    scrollBarBounds.Height + (2 * verticalBuffer)
                );
                bool isMouseOverScrollbarHitbox = scrollbarHitbox.Contains(mouseState.Position);

                if (isLeftMouseButtonPressed && isMouseOverScrollbarHitbox)
                {
                    int clampedMouseY = MathHelper.Clamp(mouseState.Y,
                        scrollBarBounds.Top - verticalBuffer,
                        scrollBarBounds.Bottom + verticalBuffer
                    );

                    Mouse.SetPosition(mouseState.X, clampedMouseY);

                    float scrollAmount = 0f;

                    if (clampedMouseY < scrollBarHandle.Y)
                    {
                        if (currentScrollPosition > 0)
                        {
                            float distanceFromTop = scrollBarHandle.Y - clampedMouseY;
                            float normalizedDistance = MathHelper.Clamp(distanceFromTop / (scrollBarBounds.Height - scrollBarHandle.Height), 1f, 1f);
                            scrollAmount = -continuousScrollSpeed * normalizedDistance;
                        }
                    }
                    else if (clampedMouseY > (scrollBarHandle.Y + scrollBarHandle.Height))
                    {
                        if (currentScrollPosition < maxScrollPosition)
                        {
                            float distanceFromBottom = clampedMouseY - (scrollBarHandle.Y + scrollBarHandle.Height);
                            float normalizedDistance = MathHelper.Clamp(distanceFromBottom / (scrollBarBounds.Height - scrollBarHandle.Height), 1f, 1f);
                            scrollAmount = continuousScrollSpeed * normalizedDistance;
                        }
                    }

                    currentScrollPosition = MathHelper.Clamp(currentScrollPosition + scrollAmount, 0, maxScrollPosition);

                    scrollBarHandle.Y = (int)MathHelper.Lerp(scrollBarBounds.Top, scrollBarBounds.Bottom - scrollBarHandle.Height, currentScrollPosition / (float)maxScrollPosition);

                    scrollBarHandle.Y = MathHelper.Clamp(scrollBarHandle.Y, scrollBarBounds.Top, scrollBarBounds.Bottom - scrollBarHandle.Height);
                }
                else
                {
                    scrollTimer = 0f;
                    isLeftMouseButtonPressed = false;
                    isDraggingScrollBar = false;
                    isDraggingScrollBarFromHandle = false;
                }

                if (isLeftMouseButtonPressed && prevMouseState.LeftButton == ButtonState.Released && isMouseOverScrollbarBounds && isMouseOverScrollbarHandle)
                {
                    isDraggingScrollBar = true;
                    dragStartY = mouseState.Y;
                    dragStartHandleY = scrollBarHandle.Y;
                }
                else if (isDraggingScrollBar)
                {
                    int newMouseY = MathHelper.Clamp(mouseState.Y, scrollBarBounds.Top, scrollBarBounds.Bottom);
                    float dragDistance = newMouseY - dragStartY;
                    int newHandleY = (int)(dragStartHandleY + dragDistance);

                    newHandleY = MathHelper.Clamp(newHandleY, scrollBarBounds.Top, scrollBarBounds.Bottom - scrollBarHandle.Height);
                    scrollBarHandle.Y = newHandleY;
                    float scrollRatio = (float)(scrollBarHandle.Y - scrollBarBounds.Top) / (scrollBarBounds.Height - scrollBarHandle.Height);
                    currentScrollPosition = scrollRatio * maxScrollPosition;

                    // Set the mouse position to stay within the scrollbar bounds
                    if (mouseState.Y != newMouseY)
                    {
                        Mouse.SetPosition(mouseState.X, newMouseY);
                    }
                }

                if (mouseState.LeftButton == ButtonState.Released)
                {
                    isDraggingScrollBar = false;
                }
                else
                {
                    isDraggingScrollBar = false;
                    isDraggingScrollBarFromHandle = false;
                    isLeftMouseButtonPressed = false;
                }




                int scrollWheelValue = mouseState.ScrollWheelValue - prevMouseState.ScrollWheelValue;
                if (scrollWheelValue != 0)
                {
                    float scrollAmount = scrollWheelValue / 120f * continuousScrollSpeed;
                    currentScrollPosition = MathHelper.Clamp(currentScrollPosition - scrollAmount, 0, maxScrollPosition);
                }

                float handleRatio = currentScrollPosition / maxScrollPosition;
                int scrollBarContentHeight = scrollBarBounds.Height - scrollBarHandle.Height;
                scrollBarHandle.Y = (int)(scrollBarBounds.Y + (scrollBarContentHeight * handleRatio));

                spriteBatch.Begin();

                int startIndex = (int)(currentScrollPosition / itemHeight);
                float offsetY = -(currentScrollPosition % itemHeight);

                for (int i = 0; i < maxVisibleItems + 1; i++)
                {
                    int index = startIndex + i;
                    if (index >= achievementStats.Count) break;

                    Vector2 position = new Vector2(100, 100 + (i * itemHeight) + offsetY);
                    spriteBatch.DrawString(spriteFont, $"{achievementStats[index].Name}: {achievementStats[index].Value}", position, Color.White);
                }

                // Define colors for different states



                if (isMouseOverScrollbarHandle)
                {
                    currentScrollHandleColor = isLeftMouseButtonPressed ? scrollHandleClickColor : scrollHandleHoverColor;
                }
                else
                {
                    currentScrollHandleColor = scrollHandleDefaultColor;
                }


                // Draw scroll bar background
                Color baseGradientColor = currentScrollBarColor;
                Color scrollBarTopColor = Color.Lerp(currentScrollBarColor, Color.DarkGray, gradientIntensity);
                for (int y = 0; y < scrollBarBounds.Height; y++)
                {
                    float gradientProgress = y / (float)(scrollBarBounds.Height * gradientExtent);
                    gradientProgress = Math.Min(gradientProgress, 1.0f);
                    Color currentGradientColor = Color.Lerp(scrollBarTopColor, baseGradientColor, gradientProgress);

                    Rectangle gradientRect = new Rectangle(scrollBarBounds.X, scrollBarBounds.Y + y, scrollBarBounds.Width, 1);
                    spriteBatch.Draw(texture, gradientRect, currentGradientColor);
                }

                // Add highlight effect
                if (isMouseOverUpArrow || (isMouseOverScrollBar && mouseState.Y < scrollBarBounds.Center.Y))
                {
                    for (int y = 0; y < scrollBarBounds.Height / 3; y++)
                    {
                        float highlightFactor = 1 - (y / (float)(scrollBarBounds.Height / 3));
                        Color highlightColor = Color.Lerp(baseGradientColor, Color.White, highlightFactor * 0.2f);
                        Rectangle highlightRect = new Rectangle(scrollBarBounds.X, scrollBarBounds.Y + y, scrollBarBounds.Width, 1);
                        spriteBatch.Draw(texture, highlightRect, highlightColor * 0.5f);
                    }
                }
                else if (isMouseOverDownArrow || (isMouseOverScrollBar && mouseState.Y >= scrollBarBounds.Center.Y))
                {
                    for (int y = scrollBarBounds.Height * 2 / 3; y < scrollBarBounds.Height; y++)
                    {
                        float highlightFactor = (y - scrollBarBounds.Height * 2 / 3) / (float)(scrollBarBounds.Height / 3);
                        Color highlightColor = Color.Lerp(baseGradientColor, Color.White, highlightFactor * 0.2f);
                        Rectangle highlightRect = new Rectangle(scrollBarBounds.X, scrollBarBounds.Y + y, scrollBarBounds.Width, 1);
                        spriteBatch.Draw(texture, highlightRect, highlightColor * 0.5f);
                    }
                }

                // Draw scroll handle
                spriteBatch.Draw(texture, scrollBarHandle, currentScrollHandleColor);

                // Draw arrow buttons
                spriteBatch.Draw(texUpArrowButton, new Vector2(upArrowButton.X, upArrowButton.Y), currentUpArrowColor);
                spriteBatch.Draw(texDownArrowButton, new Vector2(downArrowButton.X, downArrowButton.Y), currentDownArrowColor);

                spriteBatch.End();


                prevKeyboardState = currentKeyboardState;
                prevMouseState = mouseState;
            }




            // Handle OptionsMenu
            else if (gameState == GameState.ControlsMenu && isControlsMenuVisible)
            {
                if (keyboardState.IsKeyDown(Keys.Up) && prevKeyboardState.IsKeyUp(Keys.Up))
                {
                    selectedControlsMenuIndex = (selectedControlsMenuIndex - 1 + controlsMenuOptions.Length) % controlsMenuOptions.Length;
                }
                else if (keyboardState.IsKeyDown(Keys.Down) && prevKeyboardState.IsKeyUp(Keys.Down))
                {
                    selectedControlsMenuIndex = (selectedControlsMenuIndex + 1) % controlsMenuOptions.Length;
                }

                if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
                {
                    switch (selectedControlsMenuIndex)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            // Do nothing for these options
                            break;
                        case 5:
                            // Go back to Main Menu
                            isOptionsMenuVisible = false;
                            isMainMenuVisible = true;
                            SelectedMainMenuIndex = 0;
                            gameState = GameState.MainMenu;
                            break;
                    }
                }
            }


            //bool isOptionsMenuVisible = false;
            //string[] optionsMenuOptions = { "Color Picker Options", "Back" };
            //int selectedOptionsMenuIndex = 0;



            else if (gameState == GameState.OptionsMenu && isOptionsMenuVisible)
            {
                if (keyboardState.IsKeyDown(Keys.Up) && prevKeyboardState.IsKeyUp(Keys.Up))
                {
                    selectedOptionsMenuIndex = (selectedOptionsMenuIndex - 1 + optionsMenuOptions.Length) % optionsMenuOptions.Length;
                }
                else if (keyboardState.IsKeyDown(Keys.Down) && prevKeyboardState.IsKeyUp(Keys.Down))
                {
                    selectedOptionsMenuIndex = (selectedOptionsMenuIndex + 1) % optionsMenuOptions.Length;
                }

                if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
                {
                    switch (selectedOptionsMenuIndex)
                    {
                        case 0:
                            // Placeholder action for "Graphics"

                            InitBackgroundColorPicker();
                            break;
                        case 1:
                            // Go back to Main Menu
                            isOptionsMenuVisible = false;
                            isMainMenuVisible = true;
                            SelectedMainMenuIndex = 0;

                            gameState = GameState.MainMenu;
                            break;
                    }
                }
            }
            else if (isPressKeyPromptVisible)
            {
                // Check if F12 is pressed
                if (keyboardState.IsKeyDown(Keys.F12) && prevKeyboardState.IsKeyUp(Keys.F12))
                {
                    // Toggle debug menu without hiding the prompt
                    isDebugMenuVisible = !isDebugMenuVisible;
                    selectedDebugOptionIndex = 0; // Reset selection when opening menu
                    snake.Vel.X = 0;
                    snake.Vel.Y = 0;
                }
                // Check for any other key press to hide the prompt
                else if (keyboardState.GetPressedKeys().Length > 0)
                {
                    System.Threading.Thread.Sleep(20);  // Pause briefly
                    isPressKeyPromptVisible = false; // Hide the prompt
                }
            }

            // Handle gameplay input only when the prompt is not visible
            else if (gameState == GameState.GamePlay)
            {
                // Debug menu toggle (always check this)
                if (keyboardState.IsKeyDown(Keys.F12) && prevKeyboardState.IsKeyUp(Keys.F12))
                {
                    isDebugMenuVisible = !isDebugMenuVisible;
                    selectedDebugOptionIndex = 0; // Reset selection when opening menu
                }

                // Close debug menu with Escape
                if (keyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
                {
                    isDebugMenuVisible = false; // Close the debug menu
                }

                else if (!isDebugMenuVisible) // Game controls when debug menu is not visible
                {
                    if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        snake.Vel.X = -1;
                        snake.Vel.Y = 0;
                    }
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        snake.Vel.X = 1;
                        snake.Vel.Y = 0;
                    }
                    if (keyboardState.IsKeyDown(Keys.Up))
                    {
                        snake.Vel.Y = -1;
                        snake.Vel.X = 0;
                    }
                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        snake.Vel.Y = 1;
                        snake.Vel.X = 0;
                    }

                    // Check for Escape to exit the game
                    if (keyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
                    {
                        flashText = ""; // Clear any previous flash text
                        isFlashing = false; // Stop flashing
                        gameState = GameState.MainMenu; // Switch to main menu
                        isMainMenuVisible = true; // Show the main menu
                    }
                }

                // Debug menu navigation (only when debug menu is visible)
                if (isDebugMenuVisible)
                {
                    if (keyboardState.IsKeyDown(Keys.Up) && prevKeyboardState.IsKeyUp(Keys.Up))
                    {
                        selectedDebugOptionIndex = (selectedDebugOptionIndex - 1 + debugOptions.Length) % debugOptions.Length;
                    }
                    else if (keyboardState.IsKeyDown(Keys.Down) && prevKeyboardState.IsKeyUp(Keys.Down))
                    {
                        selectedDebugOptionIndex = (selectedDebugOptionIndex + 1) % debugOptions.Length;
                    }

                    if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
                    {
                        switch (selectedDebugOptionIndex)
                        {
                            case 0:
                                // Red Food pickup test for increasing snake size by x amount
                                flashText = "Tail Size Increased";

                                AddTailSegments(5);

                                break;

                            case 1:
                                // Orange Food pickup test for decreasing snake size by x amount
                                snake.TailLength = Math.Max(0, snake.TailLength - 5); // Ensure TailLength doesn't go negative
                                break;

                            case 2:
                                // Yellow Food pickup test for Making the snake faster
                                snake.Speed *= 0.8f; // Increase speed by 1.25x
                                break;

                            case 3:
                                // Green Food pickup test for Making the snake faster
                                snake.Speed *= 01.2f; // Decrease speed to 75% of current speed
                                break;

                            case 4:
                                // Blue Food pickup test for adding larger score bonus
                                score += 1100;
                                break;

                            case 5:
                                // Purple Food pickup test for teleporting the snake to a new random x,y

                                int newX = random.Next(0, worldSize.X);
                                int newY = random.Next(0, worldSize.Y);

                                // Set the new position
                                snake.Pos.X = newX;
                                snake.Pos.Y = newY;

                                // Preserve the current tail length
                                int currentTailLength = snake.TailLength;

                                // Reset the tail positions
                                for (int i = 0; i < currentTailLength; i++)
                                {
                                    snake.Tail[i] = new Point(newX, newY);
                                    snake.TailAlpha[i] = 1.0f; // Reset alpha to fully visible
                                }

                                // Ensure TailLength remains the same
                                snake.TailLength = currentTailLength;

                                // Make the snake temporarily invisible
                                snake.IsVisible = false;
                                teleportTimer = teleportDuration;

                                // Add a brief invisibility period after teleportation
                                snake.IsVisible = false;
                                teleportTimer = teleportDuration;
                                break;

                            case 6:
                                // Brown Food pickup test for making the snake tail fuilly transparent/invisible, but not the snake head)

                                isFadingTail = true; // Start the fading process
                                break;

                            case 7:
                                // Black Food pickup test for turning the whole screen black for a short period

                                isBlackFadeActive = true;
                                blackFadeTimer = 0f; // Reset the timer
                                blackFadeAlpha = 1.0f; // Start from fully visible
                                break;

                            case 8:
                                // Food munch sfx test
                                foodMunch.Play();


                                break;

                            case 9:
                                // score increase test
                                score += 100;


                                break;

                            case 10:
                                // Score Decrease test
                                score -= 100;


                                break;

                            case 11:
                                // Tail Size Increase test
                                snake.TailLength++;

                                break;

                            case 12:
                                // Tail Size Decrease test
                                snake.TailLength = Math.Max(0, snake.TailLength - 1); // Ensure TailLength doesn't go negative

                                break;

                            case 13:
                                // Assuming you want to respawn the first food item for demonstration
                                SpawnFood(0); // Change the index as necessary
                                break;

                            case 14:
                                SpawnDefaultFood(); // Ensure this method is accessible
                                break;





                            case 15:
                                // Back (close debug menu)
                                isDebugMenuVisible = false;
                                break;
                        }
                    }
                }
            }






            else if (gameState == GameState.ColorPicker)
            {

                if (Keyboard.GetState().IsKeyDown(Keys.Back))

                {
                    System.Threading.Thread.Sleep(500);  // Pause briefly
                    InitMainMenu();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.B))
                {
                    InitBackgroundColorPicker();
                }
            }
            else if (gameState == GameState.SkinChooser)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    System.Threading.Thread.Sleep(500);  // Pause briefly
                    InitControlsMenu();
                }
            }
            else if (gameState == GameState.ColorConfirm)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    System.Threading.Thread.Sleep(500);  // Pause briefly
                    InitMainMenu();  // Go back to main menu
                }
            }
            else if (gameState == GameState.BackgroundColorPicker)
            {

                if (keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
                {
                    // Confirm and apply the background color
                    backgroundColor = background;

                    System.Threading.Thread.Sleep(500);  // Pause briefly
                    gameState = GameState.ColorConfirm; // Transition to the desired state
                }
                else if (keyboardState.IsKeyDown(Keys.Back) && prevKeyboardState.IsKeyUp(Keys.Back))
                {
                    System.Threading.Thread.Sleep(500);  // Pause briefly
                    selectedOptionsMenuIndex = 0;

                    InitOptionsMenu();
                }

                // Handle slider selection with clamping
                if (keyboardState.IsKeyDown(Keys.Up) && prevKeyboardState.IsKeyUp(Keys.Up))
                {
                    selectedSlider = (selectedSlider - 1 + 3) % 3; // Cycling through 0 to 2
                }
                else if (keyboardState.IsKeyDown(Keys.Down) && prevKeyboardState.IsKeyUp(Keys.Down))
                {
                    selectedSlider = (selectedSlider + 1) % 3; // Cycling through 0 to 2
                }
                prevKeyboardState = keyboardState;

                // Update the previous keyboard state at the end of the update method                


                // Handle value adjustment for the selected slider
                if (selectedSlider == 0) // Red Slider
                {
                    if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        xRed = Math.Max(xRed - xRedIncrement, minRed); // Clamp Red value
                        xPosition = Math.Max(xPosition - xPositionIncrement, minX); // Clamp Red slider position
                    }
                    else if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        xRed = Math.Min(xRed + xRedIncrement, maxRed); // Clamp Red value
                        xPosition = Math.Min(xPosition + xPositionIncrement, maxX); // Clamp Red slider position
                    }
                }
                else if (selectedSlider == 1) // Green Slider
                {
                    if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        xGreen = Math.Max(xGreen - xGreenIncrement, minGreen); // Clamp Green value
                        yPosition = Math.Max(yPosition - YPositionIncrement, minY); // Clamp Green slider position
                    }
                    else if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        xGreen = Math.Min(xGreen + xGreenIncrement, maxGreen); // Clamp Green value
                        yPosition = Math.Min(yPosition + YPositionIncrement, maxY); // Clamp Green slider position
                    }
                }
                else if (selectedSlider == 2) // Blue Slider
                {
                    if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        xBlue = Math.Max(xBlue - xBlueIncrement, minBlue); // Clamp Blue value
                        zPosition = Math.Max(zPosition - ZPositionIncrement, minZ); // Clamp Blue slider position
                    }
                    else if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        xBlue = Math.Min(xBlue + xBlueIncrement, maxBlue); // Clamp Blue value
                        zPosition = Math.Min(zPosition + ZPositionIncrement, maxZ); // Clamp Blue slider position
                    }
                }

                // Update the background color based on the slider values
                background = new Color((int)xRed, (int)xGreen, (int)xBlue);
                redBoxPreview = new Color((int)xRed, 0, 0);
                greenBoxPreview = new Color(0, (int)xGreen, 0);
                blueBoxPreview = new Color(0, 0, (int)xBlue);

                // Handle other key presses for navigation or additional functionality
                if (keyboardState.IsKeyDown(Keys.Back) && prevKeyboardState.IsKeyUp(Keys.Back))
                {
                    InitMainMenu();
                }
                if (keyboardState.IsKeyDown(Keys.B) && prevKeyboardState.IsKeyUp(Keys.B))
                {
                    // Add functionality for 'B' key if needed
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                {
                    System.Threading.Thread.Sleep(250); // Pause briefly
                    InitMainMenu(); // Go back to main menu
                }

                if (keyboardState.IsKeyDown(Keys.Enter) && !prevKeyboardState.IsKeyDown(Keys.Enter))
                {
                    // Apply the selected background color
                    backgroundColor = background;
                    gameState = GameState.GamePlay; // Transition to the desired state
                }

                if (keyboardState.IsKeyDown(Keys.Back) && !prevKeyboardState.IsKeyDown(Keys.Back))
                {
                    // Transition back without applying the background color
                    gameState = GameState.MainMenu; // Or appropriate action
                }

                if (keyboardState.IsKeyDown(Keys.Escape) && !prevKeyboardState.IsKeyDown(Keys.Escape))
                {
                    // Handle escape key action
                    gameState = GameState.MainMenu; // Or appropriate action
                }
            }
            else if (gameState == GameState.GameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Y))
                {
                    InitGamePlay();  // Start gameplay
                    flashText = ""; // Clear flash text for a new game
                }
                if (Keyboard.GetState().IsKeyDown(Keys.N))
                {
                    System.Threading.Thread.Sleep(500);  // Pause briefly
                    InitMainMenu();  // Go back to main menu
                }
            }
            prevKeyboardState = Keyboard.GetState();
        }



        protected override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);

            if (isPressKeyPromptVisible)
            {
                elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds; // Update elapsed time

                // Handle flashing logic
                if (elapsedTime >= flashDuration)
                {
                    isTextVisible = !isTextVisible; // Toggle visibility
                    elapsedTime = 0f; // Reset timer
                }
            }

            else if (gameState == GameState.MainMenu)
            {
                UpdateMainMenu(gameTime);
            }
            if (gameState == GameState.AchievementsAndStats)
            {
                HandleScrolling(gameTime);
            }
            else if (gameState == GameState.GamePlay)
            {
                UpdateGamePlay(gameTime);
            }
            else if (gameState == GameState.ControlsMenu)
            {
                UpdateControlsMenu(gameTime);
            }
            else if (gameState == GameState.OptionsMenu)
            {
                UpdateOptionsMenu(gameTime);
            }
            else if (gameState == GameState.Controls2)
            {
                UpdateControls2(gameTime);
            }
            else if (gameState == GameState.ResolutionPicker)
            {
                UpdateResolutionPicker(gameTime);
            }
            else if (gameState == GameState.ColorPicker)
            {
                UpdateColorPicker(gameTime);
            }
            else if (gameState == GameState.SkinChooser)
            {
                UpdateSkinChooser(gameTime);
            }
            else if (gameState == GameState.BackgroundColorPicker)
            {
                UpdateBackgroundColorPicker(gameTime);
            }
            else if (gameState == GameState.ColorConfirm)
            {
                UpdateColorConfirm(gameTime);
            }
            else if (gameState == GameState.GameOver)
            {
                UpdateGameOver(gameTime);
            }

            base.Update(gameTime);
        }

        void UpdateMainMenu(GameTime gameTime)
        {
        }

        private float shuffleIntervalTime = 05f; // Shuffle every 0.1 seconds
        private float teleportDuration = 0.0f; // Duration to stay invisible
        private float teleportTimer = 0f; // Timer for teleporting
        private bool isFadingTail = false;
        private float fadeDuration = 5.0f; // Duration for fading out
        private float invisibleDuration = 5.0f; // Duration for invisibility
        private float fadeInDuration = 5.0f; // Duration for fading in
        private float fadeTimer = 0f; // General timer for handling fade effects
        private bool isTailVisible = true; // Track visibility of the tail

        private float blackFadeAlpha = 1.0f; // Current screen alpha for the black fade
        private float blackFadeDuration = 3.5f; // Duration for fading out (seconds)
        private float blackHoldDuration = 7.0f; // Duration to hold the fade (seconds)
        private float blackFadeInDuration = 3.5f; // Duration for fading back in (seconds)
        private float blackFadeTimer = 0f; // Timer for handling fade effects
        private bool isBlackFadeActive = false; // Flag to check if the fade effect is active



        void UpdateGamePlay(GameTime gameTime)
        {
            // Update food shuffling
            for (int i = 0; i < food.Length; ++i)
            {
                if (food[i].IsShuffling)
                {
                    // Increment the total elapsed time
                    food[i].totalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // If the elapsed time exceeds the shuffle interval, change the color
                    if (food[i].totalElapsedTime >= shuffleIntervalTime)
                    {
                        // Randomly select a new color from the available colors
                        food[i].Type = random.Next(0, foodColorList.Length);
                        food[i].totalElapsedTime = 0f; // Reset the elapsed time
                    }
                }
            }

            // Check if the snake is fading its tail
            if (isFadingTail)
            {
                fadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Handle fade-out
                if (fadeTimer < fadeDuration)
                {
                    // During fade-out, reduce the alpha of the tail segments
                    float alpha = 1.0f - (fadeTimer / fadeDuration);
                    for (int i = 0; i < snake.TailLength; i++)
                    {
                        snake.TailAlpha[i] = alpha; // Set alpha based on fade duration
                    }
                }
                else if (fadeTimer < fadeDuration + invisibleDuration)
                {
                    // Tail is invisible, set alpha to 0 for all segments
                    for (int i = 0; i < snake.TailLength; i++)
                    {
                        snake.TailAlpha[i] = 0.0f; // Set to invisible
                    }
                }
                else if (fadeTimer < fadeDuration + invisibleDuration + fadeInDuration)
                {
                    // Handle fade-in
                    float alpha = (fadeTimer - (fadeDuration + invisibleDuration)) / fadeInDuration;
                    for (int i = 0; i < snake.TailLength; i++)
                    {
                        snake.TailAlpha[i] = alpha; // Restore fading in
                    }
                }
                else
                {
                    // Reset the fade effect
                    isFadingTail = false;
                    fadeTimer = 0f;

                    // Reset alpha values to fully visible
                    for (int i = 0; i < snake.TailLength; i++)
                    {
                        snake.TailAlpha[i] = 1.0f; // Ensure all segments are fully visible again
                    }
                }
            }

            // Update snake movement
            snake.StepTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (snake.StepTimer >= snake.Speed)
            {
                snake.StepTimer = 0;

                // Shift all tail pieces up one in the array so we can insert the new piece at the beginning
                for (int i = snake.TailLength; i > 0; --i)
                {
                    snake.Tail[i] = snake.Tail[i - 1];
                }
                snake.Tail[0] = snake.Pos; // Add new head position to the tail

                // Update snake position
                snake.Pos.X += snake.Vel.X;
                snake.Pos.Y += snake.Vel.Y;

                // Test for snake head colliding with snake tail
                for (int i = 0; i < snake.TailLength; ++i)
                {
                    if (snake.Pos.X == snake.Tail[i].X && snake.Pos.Y == snake.Tail[i].Y)
                    {
                        System.Threading.Thread.Sleep(2000);
                        InitGameOver();
                        return;
                    }
                }

                // For snake eating default food
                for (int i = 0; i < food.Length; ++i)
                {
                    if (snake.Pos.X == defaultFoodPosX && snake.Pos.Y == defaultFoodPosY)
                    {
                        SpawnDefaultFood();
                        snake.Speed *= 0.99;
                        snake.TailLength++;
                        score += 100;
                        targetCellSize = 8f;

                        foodMunch.Play();

                        if (score > highScore)
                        {
                            highScore = score;
                        }
                    }
                }

                // Test for snake head colliding with food
                for (int i = 0; i < food.Length; ++i)
                {
                    if (snake.Pos.X == food[i].Pos.X && snake.Pos.Y == food[i].Pos.Y)
                    {
                        int foodType = food[i].Type;

                        // Handle food consumption
                        SpawnFood(i); // Respawn the food at a new location
                        switch (foodType)
                        {
                            case 0: // Red Food
                                RedFoodEffect();
                                break;
                            case 1: // Orange Food
                                OrangeFoodEffect();
                                break;
                            case 2: // Yellow Food
                                YellowFoodEffect();
                                break;
                            case 3: // Green Food
                                GreenFoodEffect();
                                break;
                            case 4: // Blue Food
                                BlueFoodEffect();
                                break;
                            case 5: // Purple Food
                                PurpleFoodEffect();
                                break;
                            case 6: // Brown Food
                                BrownFoodEffect();
                                break;
                            case 7: // Black Food
                                BlackFoodEffect();
                                break;
                            default:
                                flashText = "Unknown Food Eaten!";
                                break;
                        }

                        // Play the munch sound effect
                        foodMunch.Play();

                        // Reset flashing timers for the new text
                        elapsedTime = 0f; // Reset timer for flashing cycle
                        flashTimeElapsed = 0f; // Reset total flashing time
                        isFlashing = true; // Start flashing effect
                    }
                }

                // Manage flashing text visibility duration
                if (isFlashing)
                {
                    elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (elapsedTime >= flashDuration)
                    {
                        isFlashing = false; // Stop flashing after the duration
                        flashText = ""; // Clear the text after it's displayed
                    }
                }
            }

            if (isBlackFadeActive)
            {
                blackFadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Fade to black over the specified duration
                if (blackFadeTimer < blackFadeDuration)
                {
                    blackFadeAlpha = MathHelper.Lerp(1.0f, 0.1f, blackFadeTimer / blackFadeDuration); // Fade to 0.1f
                }
                else if (blackFadeTimer < blackFadeDuration + blackHoldDuration)
                {
                    // Hold the fade at 0.1f for the specified duration
                    blackFadeAlpha = 0.0f; // Remain at 0.0f
                }
                else if (blackFadeTimer < blackFadeDuration + blackHoldDuration + blackFadeInDuration)
                {
                    // Fade back in
                    blackFadeAlpha = MathHelper.Lerp(0.1f, 1.0f, (blackFadeTimer - (blackFadeDuration + blackHoldDuration)) / blackFadeInDuration);
                }
                else
                {
                    // Reset the fade effect and timer
                    isBlackFadeActive = false; // Reset the fade effect
                    blackFadeTimer = 0f; // Reset the timer
                    blackFadeAlpha = 1.0f; // Reset to fully visible
                }
            }

            if (!snake.IsVisible)
            {
                teleportTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (teleportTimer <= 0)
                {
                    snake.IsVisible = true;
                }
            }


            void RegrowTail()
            {
                // Keep the tail length the same, but reset the tail segments
                // We can set the tail segments based on the new position and the current length
                for (int i = 0; i < snake.TailLength; i++)
                {
                    // Regrow the tail segments from the new head position
                    snake.Tail[i] = new Point(snake.Pos.X, snake.Pos.Y);
                }

                // Make the snake visible again
                snake.IsVisible = true;
            }


            for (int i = 0; i < food.Length; ++i)
            {
                if (snake.Pos.X == food[i].Pos.X && snake.Pos.Y == food[i].Pos.Y)
                {
                    // Handle food consumption
                    SpawnFood(i);

                    snake.TailLength++;
                    // foodMunch.Play();


                    float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;


                    // Calculate alpha using a sine wave function
                    float alpha = (float)(Math.Sin(elapsedTime * Math.PI * 2 / flashDuration) * 0.5 + 0.5);
                    textColor = new Color(1f, 1f, 1f, alpha); // White color with oscillating alpha

                    // Reset elapsedTime to ensure continuous flashing
                    if (elapsedTime > flashDuration)
                    {
                        isTextVisible = !isTextVisible;
                        elapsedTime -= flashDuration; // Keeps the flashing cycle going
                    }

                    if (flashTimeElapsed >= totalFlashTime)
                    {
                        isFlashing = false;
                        flashText = ""; // Optionally clear the text
                        textColor = Color.Transparent; // Ensure text is hidden
                    }

                    if (isFading)
                    {
                        if (fadingIn)
                        {
                            fadeAlpha -= fadeSpeed * elapsed;
                            if (fadeAlpha <= 0.0f)
                            {
                                fadeAlpha = 0.0f;
                                isFading = false; // Stop fading
                            }
                        }
                        else
                        {
                            fadeAlpha += fadeSpeed * elapsed;
                            if (fadeAlpha >= 1.0f)
                            {
                                fadeAlpha = 1.0f;
                                fadingIn = true; // Set to fade back in after fading out
                            }
                        }
                    }

                    if (isFlashing)
                    {
                        // Update elapsed time for current flash cycle
                        elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        // Update total time for the flashing effect
                        flashTimeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Toggle text visibility based on elapsed time
                        if (elapsedTime >= flashDuration)
                        {
                            isTextVisible = !isTextVisible;
                            elapsedTime = 0f; // Reset timer for the flashing cycle
                        }

                        textColor = isTextVisible ? Color.White : Color.Transparent; // Toggle between visible and transparent

                        // Stop flashing after totalFlashTime has elapsed
                        if (flashTimeElapsed >= totalFlashTime)
                        {
                            isFlashing = false;
                            flashText = ""; // Optionally clear the text
                            textColor = Color.Transparent; // Ensure text is hidden
                        }
                    }
                }
                // handle snake teleporting over edges
                if (snake.Pos.X < 0)
                {
                    snake.Pos.X = worldSize.X - 1;
                }
                if (snake.Pos.Y < 0)
                {
                    snake.Pos.Y = worldSize.Y - 1;
                }
                if (snake.Pos.X >= worldSize.X)
                {
                    snake.Pos.X = 0;
                }
                if (snake.Pos.Y >= worldSize.Y)
                {
                    snake.Pos.Y = 0;
                }
            }
        }

        void UpdateControlsMenu(GameTime gameTime)
        {
        }

        void UpdateOptionsMenu(GameTime gameTime)
        {
        }

        void UpdateControls2(GameTime gameTime)
        {
        }

        void UpdateResolutionPicker(GameTime gameTime)
        {
        }

        void UpdateColorPicker(GameTime gameTime)
        {
            backgroundColor = background;

            int red = (int)xPosition;

            // Convert the integer to a string
            string redString = red.ToString();

            xRed = MathHelper.Clamp(xRed, minRed, maxRed);
            base.Update(gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                backgroundColor = background;
            }
        }

        void UpdateSkinChooser(GameTime gameTime)
        {
        }

        void UpdateBackgroundColorPicker(GameTime gameTime)
        {
        }

        void UpdateColorConfirm(GameTime gameTime)
        {
        }

        void UpdateGameOver(GameTime gameTime)
        {
            if (gameState == GameState.GamePlay)

            {
                StopFlashing();
                // Additional game over logic here
            }
        }


        void AddTailSegments(int segments)
        {
            // Ensure the Tail array can accommodate the new segments
            if (snake.TailLength + segments > snake.Tail.Length)
            {
                Array.Resize(ref snake.Tail, snake.TailLength + segments);
                Array.Resize(ref snake.TailAlpha, snake.TailLength + segments); // Resize TailAlpha array
            }

            // Start adding segments from the current tail length to the end
            for (int j = 0; j < segments; j++)
            {
                // New segments take the position of the second-to-last segment
                if (snake.TailLength > 1)
                {
                    snake.Tail[snake.TailLength + j] = snake.Tail[snake.TailLength]; // Copy the position of the second-to-last segment
                }
                else
                {
                    // If there's only one segment or less, use the head's current position
                    snake.Tail[snake.TailLength + j] = new Point(snake.Pos.X, snake.Pos.Y);
                }
                snake.TailAlpha[snake.TailLength + j] = 1.0f; // Set alpha to fully visible for new segments
            }

            // Update the TailLength to account for the newly added segments
            snake.TailLength += segments;
        }

        void TeleportSnake()
        {
            // Generate new random coordinates for the snake's position
            int newX = random.Next(0, worldSize.X);
            int newY = random.Next(0, worldSize.Y);

            // Set the new position
            snake.Pos.X = newX;
            snake.Pos.Y = newY;

            // Preserve the current tail length
            int currentTailLength = snake.TailLength;

            // Reset the tail positions
            for (int i = 0; i < currentTailLength; i++)
            {
                snake.Tail[i] = new Point(newX, newY);
                snake.TailAlpha[i] = 1.0f; // Reset alpha to fully visible
            }

            // Ensure TailLength remains the same
            snake.TailLength = currentTailLength;

            // Make the snake temporarily invisible
            snake.IsVisible = false;
            teleportTimer = teleportDuration;
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor); // Clear with the current backgroundColor

            switch (gameState)
            {
                case GameState.MainMenu:
                    DrawMainMenu(gameTime);
                    break;
                case GameState.AchievementsAndStats:
                    DrawAchievementsAndStats(gameTime);
                    break;
                case GameState.GamePlay:
                    DrawGamePlay(gameTime);
                    break;
                case GameState.ControlsMenu:
                    DrawControlsMenu(gameTime);
                    break;
                case GameState.OptionsMenu:
                    DrawOptionsMenu(gameTime);
                    break;
                case GameState.Controls2:
                    DrawControls2(gameTime);
                    break;
                case GameState.ColorPicker:
                    DrawColorPicker(gameTime);
                    break;
                case GameState.SkinChooser:
                    DrawSkinChooser(gameTime);
                    break;
                case GameState.BackgroundColorPicker:
                    DrawBackgroundColorPicker(gameTime);
                    break;
                case GameState.ColorConfirm:
                    DrawColorConfirm(gameTime);
                    break;
                case GameState.GameOver:
                    DrawGameOver(gameTime);
                    break;
                default:
                    // Optionally handle unexpected game states
                    break;
            }

            base.Draw(gameTime);
        }

        private void CreateGradientTexture()
        {
            gradientTexture = new Texture2D(GraphicsDevice, 1, 256);
            Color[] colorData = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                colorData[i] = new Color(100, 100, 100, i);
            }
            gradientTexture.SetData(colorData);
        }


        private void DrawPressAnyKeyPrompt(GameTime gameTime)
        {
            if (!isPressKeyPromptVisible)
                return; // Exit the method if the prompt is not visible


            // Define box dimensions
            int boxWidth = GraphicsDevice.Viewport.Width; // Leave some margin
            int boxHeight = 50; // Height of the box
            int boxX = (GraphicsDevice.Viewport.Width - boxWidth) / 2; // Center the box horizontally
            int boxY = (GraphicsDevice.Viewport.Height - boxHeight) / 2 - 75; // Center the box vertically

            // Draw the semi-transparent box
            spriteBatch.Draw(texture, new Rectangle(boxX, boxY, boxWidth, boxHeight), Color.Black * 0.5f); // Adjust color and transparency as needed

            // Flashing text logic
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds; // Update elapsed time
            Color flashingColor = GetFlashingColor(gameTime, Color.White, Color.Gray); // Get the flashing color

            // Draw the text
            string text = "Press Any Key";
            Vector2 textSize = spriteFont.MeasureString(text);
            Vector2 textPosition = new Vector2(boxX + (boxWidth - textSize.X) / 2 - 20, boxY + (boxHeight - textSize.Y) / 2 - 5); // Center the text in the box

            // Set the scale and thickness for bold effect
            float textScale = 1.5f;
            float thickness = 1.5f; // Thickness for the bold effect
            Vector2 shadowOffset = new Vector2(3f, 3f); // Shadow offset

            // Draw the shadow
            spriteBatch.DrawString(spriteFont, text, textPosition + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

            // Draw the main text with thickness for bold effect
            for (float offset = -thickness; offset <= thickness; offset += thickness)
            {
                if (offset != 0) // Skip the center position
                {
                    spriteBatch.DrawString(spriteFont, text, textPosition + new Vector2(offset, 0), flashingColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(spriteFont, text, textPosition + new Vector2(0, offset), flashingColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                }
            }

            // Finally, draw the main text over the shadow to complete the bold effect
            spriteBatch.DrawString(spriteFont, text, textPosition, flashingColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }

        void DrawMainMenu(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (isMainMenuVisible)
            {
                UpdateTextureFadeEffect(gameTime);

                // Draw menu background with a semi-transparent black rectangle
                spriteBatch.Draw(texture, new Rectangle(435, 355, 475, 335), Color.Black * 0.4f);

                // Calculate the flashing color for the selected menu option
                Color flashingColor = GetFlashingColor(gameTime, Color.Green, Color.DarkGreen);

                // Get the updated menu options with indicators
                string[] updatedMenuOptions = GetUpdatedMenuOptions();

                // Loop through each updated menu option to draw it on the screen
                for (int i = 0; i < updatedMenuOptions.Length; i++)
                {
                    // Determine the color of the menu option:
                    // If it's the selected option, use the flashing color, otherwise, use a darker green
                    Color color = i == SelectedMainMenuIndex ? flashingColor : Color.DarkOliveGreen;

                    // Set the position where the text will be drawn
                    Vector2 textPosition = new Vector2(473, 375 + i * 50);

                    // Set the scale to make the text larger; 1.5 times the original size
                    float textScale = 1.5f;

                    // Set the thickness for the bold effect by drawing multiple text layers slightly offset
                    float thickness = 1.5f;

                    // Set the offset for creating the 3D shadow effect
                    Vector2 shadowOffset = new Vector2(3f, 3f);

                    // Draw the shadow by drawing the text slightly offset to the bottom-right
                    spriteBatch.DrawString(mainMenuTextFont, updatedMenuOptions[i], textPosition + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                    // Draw the text multiple times with slight offsets to create a thicker, bolder effect
                    spriteBatch.DrawString(mainMenuTextFont, updatedMenuOptions[i], textPosition + new Vector2(-thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedMenuOptions[i], textPosition + new Vector2(thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedMenuOptions[i], textPosition + new Vector2(0, -thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedMenuOptions[i], textPosition + new Vector2(0, thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                    // Draw the main text in the center to complete the bold effect
                    spriteBatch.DrawString(mainMenuTextFont, updatedMenuOptions[i], textPosition, color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    // Draw additional textures



                    Color textureColor = Color.White * textureFadeAlpha; // Apply fade effect
                    spriteBatch.Draw(texPressEnter, new Vector2(515, 635), textureColor);


                    spriteBatch.Draw(texSnakeArt, new Vector2(450, -10), Color.White);
                    spriteBatch.Draw(texSnakeGameText, new Vector2(435, 225), Color.White);

                }

                // End the sprite batch
                spriteBatch.End();
            }
        }

        private Rectangle upArrowButton;
        private Rectangle downArrowButton;

        void DrawAchievementsAndStats(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw achievements box
            spriteBatch.Draw(texture, achievementsBox, new Color(120, 120, 120));

            // Draw box border
            spriteBatch.Draw(texture, new Rectangle(achievementsBox.X, achievementsBox.Y, achievementsBox.Width, 2), Color.White);
            spriteBatch.Draw(texture, new Rectangle(achievementsBox.X, achievementsBox.Y, 2, achievementsBox.Height), Color.White);
            spriteBatch.Draw(texture, new Rectangle(achievementsBox.Right - 2, achievementsBox.Y, 2, achievementsBox.Height), Color.White);
            spriteBatch.Draw(texture, new Rectangle(achievementsBox.X, achievementsBox.Bottom - 2, achievementsBox.Width, 2), Color.White);

            // Calculate the start index and offset based on the current scroll position
            int startIndex = (int)(currentScrollPosition / itemHeight);
            float offsetY = -(currentScrollPosition % itemHeight);

            // Set the scissor rectangle to clip content within the achievements box
            Rectangle scissorRectangle = new Rectangle(
       achievementsBox.X + 2,
       achievementsBox.Y + 2,
       achievementsBox.Width - scrollBarBounds.Width - 7,
       achievementsBox.Height - 4
   );

            // Begin a new SpriteBatch with scissor testing enabled
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, new RasterizerState { ScissorTestEnable = true });
            GraphicsDevice.ScissorRectangle = scissorRectangle;

            // Add some padding to the top
            float topPadding = 25f; // Adjust this value to move text further down

            // Draw achievements
            for (int i = 0; i < maxVisibleItems + 3; i++) // Add one extra item for smooth scrolling
            {
                int index = startIndex + i;
                if (index >= achievementStats.Count) break;

                var stat = achievementStats[index];
                Vector2 position = new Vector2(
                    achievementsBox.X + 50,
                    achievementsBox.Y + topPadding + (i * itemHeight) + offsetY
                );

                spriteBatch.DrawString(spriteFont, $"{stat.Name}: {stat.Value}", position, Color.White);
            }

            // End the clipped SpriteBatch and start a new one for drawing outside the clipped area
            spriteBatch.End();
            spriteBatch.Begin();

            // Draw scroll bar if necessary
            if (achievementStats.Count > maxVisibleItems)
            {
                MouseState mouseState = Mouse.GetState();
                bool isMouseOverScrollBar = scrollBarBounds.Contains(mouseState.Position);
                bool isMouseOverUpArrow = upArrowButton.Contains(mouseState.Position);
                bool isMouseOverDownArrow = downArrowButton.Contains(mouseState.Position);
                bool isMouseOverScrollbarHandle = scrollBarHandle.Contains(mouseState.Position);
                bool isLeftMouseButtonPressed = mouseState.LeftButton == ButtonState.Pressed;

                // Define colors for different states
                Color scrollBarDefaultColor = Color.White;
                Color scrollBarHoverColor = new Color(235, 235, 235);
                Color scrollBarClickColor = new Color(215, 215, 215);
                Color scrollHandleDefaultColor = new Color(100, 100, 100);
                Color scrollHandleHoverColor = new Color(125, 125, 125);
                Color scrollHandleClickColor = new Color(75, 75, 75);
                Color arrowButtonDefaultColor = new Color(235, 235, 235);
                Color arrowButtonHoverColor = new Color(200, 200, 200);
                Color arrowButtonClickColor = new Color(150, 150, 150);

                // Determine current colors based on mouse state
                Color currentScrollBarColor = scrollBarDefaultColor;
                Color currentScrollHandleColor = scrollHandleDefaultColor;
                Color currentUpArrowColor = arrowButtonDefaultColor;
                Color currentDownArrowColor = arrowButtonDefaultColor;

                if (isMouseOverScrollBar)
                {
                    currentScrollBarColor = isLeftMouseButtonPressed ? scrollBarClickColor : scrollBarHoverColor;
                }

                if (isMouseOverScrollbarHandle)
                {
                    currentScrollHandleColor = isLeftMouseButtonPressed ? scrollHandleClickColor : scrollHandleHoverColor;
                }

                if (isMouseOverUpArrow)
                {
                    currentUpArrowColor = isLeftMouseButtonPressed ? arrowButtonClickColor : arrowButtonHoverColor;
                }

                if (isMouseOverDownArrow)
                {
                    currentDownArrowColor = isLeftMouseButtonPressed ? arrowButtonClickColor : arrowButtonHoverColor;
                }


                // Draw scrollbar background
                spriteBatch.Draw(texture, scrollBarBounds, currentScrollBarColor);

                // Apply gradient effect
                float gradientExtent = 03f;
                float gradientIntensity = 0.3f;
                Color scrollBarTopColor = Color.Lerp(currentScrollBarColor, Color.DarkGray, gradientIntensity);
                for (int y = 0; y < scrollBarBounds.Height; y++)
                {
                    float gradientProgress = y / (float)(scrollBarBounds.Height * gradientExtent);
                    gradientProgress = Math.Min(gradientProgress, 3.0f);
                    Color currentGradientColor = Color.Lerp(scrollBarTopColor, currentScrollBarColor, gradientProgress);

                    Rectangle gradientRect = new Rectangle(scrollBarBounds.X, scrollBarBounds.Y + y, scrollBarBounds.Width, 1);
                    spriteBatch.Draw(texture, gradientRect, currentGradientColor);
                }

                // Draw scrollbar handle
                spriteBatch.Draw(texture, scrollBarHandle, currentScrollHandleColor);

                // Draw up arrow button
                spriteBatch.Draw(texUpArrowButton, new Vector2(upArrowButton.X, upArrowButton.Y), currentUpArrowColor);

                // Draw down arrow button
                spriteBatch.Draw(texDownArrowButton, new Vector2(downArrowButton.X, downArrowButton.Y), currentDownArrowColor);

                if (isMouseOverUpArrow || isMouseOverDownArrow)
                {
                    int gradientHeight = scrollBarBounds.Height / 1; // Half the scrollbar height for each gradient

                    Rectangle destRect;
                    if (isMouseOverUpArrow)
                    {
                        // For the top button, start the gradient just below the button on the scrollbar
                        destRect = new Rectangle(
                            scrollBarBounds.X,
                            upArrowButton.Bottom,
                            scrollBarBounds.Width,
                            gradientHeight
                        );
                    }
                    else
                    {
                        // For the bottom button, end the gradient just above the button on the scrollbar
                        destRect = new Rectangle(
                            scrollBarBounds.X,
                            downArrowButton.Y - gradientHeight,
                            scrollBarBounds.Width,
                            gradientHeight
                        );
                    }


                    //Color arrowButtonHoverColor = new Color(220, 220, 220);
                    //Color arrowButtonClickColor = new Color(150, 150, 150);


                    Color startColor = isLeftMouseButtonPressed
                        ? new Color((byte)125, (byte)125, (byte)125, (byte)250)  // Darker clicked color
                        : new Color((byte)175, (byte)175, (byte)175, (byte)250);  // Darker hover color


                    Color endColor = new Color(startColor.R, startColor.G, startColor.B, (byte)0);  // Fade to transparent

                    // Draw the gradient
                    for (int y = 0; y < gradientHeight; y++)
                    {
                        float progress = y / (float)gradientHeight;
                        Color currentColor;

                        if (isMouseOverUpArrow)
                        {
                            currentColor = Color.Lerp(startColor, endColor, progress);
                        }
                        else
                        {
                            currentColor = Color.Lerp(endColor, startColor, progress);
                        }

                        Rectangle currentRect = new Rectangle(
                            destRect.X,
                            destRect.Y + y,
                            destRect.Width,
                            1
                        );

                        spriteBatch.Draw(texture, currentRect, currentColor);
                    }
                }


                // Draw scrollbar background
                spriteBatch.Draw(texture, scrollBarHandle, currentScrollHandleColor);

            }
            spriteBatch.End();
        }



        void DrawControlsMenu(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (isControlsMenuVisible)
            {
                UpdateTextureFadeEffect(gameTime);

                // Draw menu background with a semi-transparent black rectangle
                spriteBatch.Draw(texture, new Rectangle(450, 285, 375, 415), Color.Black * 0.4f);

                // Calculate the flashing color for the selected menu option
                Color flashingColor = GetFlashingColor(gameTime, Color.Green, Color.DarkGreen);

                // Get the updated menu options with indicators
                string[] updatedControlsMenuOptions = GetUpdatedControlsMenuOptions();

                // Loop through each updated menu option to draw it on the screen
                for (int i = 0; i < updatedControlsMenuOptions.Length; i++)
                {
                    // Determine the color of the menu option:
                    // If it's the selected option, use the flashing color, otherwise, use a darker green
                    Color color = i == selectedControlsMenuIndex ? flashingColor : Color.DarkOliveGreen;

                    // Set the position where the text will be drawn
                    Vector2 textPosition = new Vector2(490, 300 + i * 60);

                    // Set the scale to make the text larger; 1.5 times the original size
                    float textScale = 1.5f;

                    // Set the thickness for the bold effect by drawing multiple text layers slightly offset
                    float thickness = 1.5f;

                    // Set the offset for creating the 3D shadow effect
                    Vector2 shadowOffset = new Vector2(3f, 3f);

                    // Draw the shadow by drawing the text slightly offset to the bottom-right
                    spriteBatch.DrawString(mainMenuTextFont, updatedControlsMenuOptions[i], textPosition + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                    // Draw the text multiple times with slight offsets to create a thicker, bolder effect
                    spriteBatch.DrawString(mainMenuTextFont, updatedControlsMenuOptions[i], textPosition + new Vector2(-thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedControlsMenuOptions[i], textPosition + new Vector2(thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedControlsMenuOptions[i], textPosition + new Vector2(0, -thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedControlsMenuOptions[i], textPosition + new Vector2(0, thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                    // Draw the main text in the center to complete the bold effect
                    spriteBatch.DrawString(mainMenuTextFont, updatedControlsMenuOptions[i], textPosition, color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                }

                // Draw the "Press Enter" texture outside of the loop to ensure it's only drawn once
                Color textureColor = Color.White * textureFadeAlpha; // Apply fade effect
                spriteBatch.Draw(texPressEnter, new Vector2(515, 650), textureColor);

                // You can add additional textures here if needed
                // For example:
                // spriteBatch.Draw(texSomeOtherTexture, new Vector2(x, y), Color.White);

                spriteBatch.End();
            }
        }
        void DrawControls2(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(mainMenuTextFont, "Resolution Picker", new Vector2(375, 100), Color.White);
            spriteBatch.DrawString(mainMenuTextFont, "Previous Page", new Vector2(375, 585), Color.White);
            spriteBatch.DrawString(mainMenuTextFont, "Main Menu", new Vector2(375, 675), Color.White);

            spriteBatch.Draw(texControlsText, new Vector2(550, 0), Color.White);

            spriteBatch.Draw(texR, new Vector2(674, 80), Color.White);
            spriteBatch.Draw(texBack, new Vector2(665, 570), Color.White);
            spriteBatch.Draw(texEsc, new Vector2(674, 660), Color.White);

            spriteBatch.Draw(texture, new Rectangle(673, worldScreenTopY, 1, GraphicsDevice.Viewport.Height), Color.Black);
            spriteBatch.Draw(texture, new Rectangle(374, worldScreenTopY, 1, GraphicsDevice.Viewport.Height), Color.Black);

            spriteBatch.End();
        }

        void DrawOptionsMenu(GameTime gameTime)
        {
            spriteBatch.Begin();
            if (isOptionsMenuVisible)
            {
                UpdateTextureFadeEffect(gameTime);

                // Draw menu background with a semi-transparent black rectangle
                spriteBatch.Draw(texture, new Rectangle(375, 400, 500, 250), Color.Black * 0.4f);

                // Get the updated menu options with indicators
                string[] updatedOptionsMenu = GetUpdatedOptionsMenuOptions();

                // Calculate the flashing color for the selected menu option
                Color flashingColor = GetFlashingColor(gameTime, Color.Green, Color.DarkGreen);

                // Loop through each menu option to draw it on the screen
                for (int i = 0; i < updatedOptionsMenu.Length; i++)
                {
                    // Determine the color of the menu option:
                    // If it's the selected option, use the flashing color, otherwise, use a darker green
                    Color color = i == selectedOptionsMenuIndex ? flashingColor : Color.DarkOliveGreen;

                    // Set the position where the text will be drawn
                    Vector2 textPosition = new Vector2(425, 425 + i * 60);

                    // Set the scale to make the text larger; 1.5 times the original size
                    float textScale = 1.5f;

                    // Set the thickness for the bold effect by drawing multiple text layers slightly offset
                    float thickness = 1.5f;

                    // Set the offset for creating the 3D shadow effect
                    Vector2 shadowOffset = new Vector2(3f, 3f);

                    Color textureColor = Color.White * textureFadeAlpha; // Apply fade effect
                    spriteBatch.Draw(texPressEnter, new Vector2(500, 600), textureColor);

                    // Draw the shadow by drawing the text slightly offset to the bottom-right
                    spriteBatch.DrawString(mainMenuTextFont, updatedOptionsMenu[i], textPosition + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                    // Draw the text multiple times with slight offsets to create a thicker, bolder effect
                    spriteBatch.DrawString(mainMenuTextFont, updatedOptionsMenu[i], textPosition + new Vector2(-thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedOptionsMenu[i], textPosition + new Vector2(thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedOptionsMenu[i], textPosition + new Vector2(0, -thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, updatedOptionsMenu[i], textPosition + new Vector2(0, thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                }

                // End the sprite batch
                spriteBatch.End();
            }
        }









        void DrawColorPicker(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw the background with the current background color
            spriteBatch.Draw(texture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), backgroundColor);

            // Draw UI elements
            spriteBatch.DrawString(mainMenuTextFont, "Background color", new Vector2(450, 260), Color.White);
            spriteBatch.DrawString(mainMenuTextFont, "Back", new Vector2(500, 660), Color.White);
            spriteBatch.Draw(texColorPickerText, new Vector2(405, 50), Color.White);
            spriteBatch.Draw(texBack, new Vector2(675, 650), Color.White);
            spriteBatch.Draw(texB, new Vector2(700, 240), Color.White);

            spriteBatch.End();
        }


        void DrawSkinChooser(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw skin selector background (assuming texSkinSelector is loaded)
            spriteBatch.Draw(texSkinSelector, new Vector2(300, 50), Color.White);

            // Draw the current texture based on currentTextureIndex
            if (textures.Length > 0 && currentTextureIndex >= 0 && currentTextureIndex < textures.Length)
            {
                spriteBatch.Draw(textures[currentTextureIndex], new Vector2(100, 200), Color.White);
            }

            // Handle key press to change currentTextureIndex
            KeyboardState keyboardState = Keyboard.GetState();

            // Move to the previous texture index (decrement)
            if (keyboardState.IsKeyDown(Keys.Left) && !prevKeyboardState.IsKeyDown(Keys.Left))
            {
                {
                    colorIndex++;
                    if (colorIndex >= textures.Length)
                    {
                        colorIndex = 0;
                    }
                }
                currentTextureIndex--;
                if (currentTextureIndex < 0)
                    currentTextureIndex = textures.Length - 1; // Wrap around to the last texture
            }

            // Move to the next texture index (increment)
            if (keyboardState.IsKeyDown(Keys.Right) && !prevKeyboardState.IsKeyDown(Keys.Right))
            {
                {
                    colorIndex++;
                    if (colorIndex >= textures.Length)
                    {
                        colorIndex = 0;
                    }
                }
                currentTextureIndex++;
                if (currentTextureIndex >= textures.Length)
                    currentTextureIndex = 0; // Wrap around to the first texture
            }

            prevKeyboardState = keyboardState; // Update previous keyboard state

            spriteBatch.DrawString(mainMenuTextFont, "Back", new Vector2(500, 660), Color.White);
            spriteBatch.Draw(texBack, new Vector2(675, 650), Color.White);
            spriteBatch.End();
        }

        void DrawBackgroundColorPicker(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(backgroundColor); // Use the updated backgroundColor

            spriteBatch.Draw(texture, new Rectangle(485, 125, 300, 150), background); // Use the current background color
            spriteBatch.DrawString(mainMenuTextFont, "Back", new Vector2(500, 660), Color.White);
            spriteBatch.DrawString(mainMenuTextFont, "Apply Changes", new Vector2(435, 590), Color.White);

            spriteBatch.Draw(texBackgroundColorText, new Vector2(405, 20), Color.White);
            spriteBatch.Draw(texture, new Rectangle(485, 125, 300, 150), background);

            spriteBatch.Draw(texture, new Rectangle(943, 330, 50, 50), redBoxPreview);
            spriteBatch.Draw(texture, new Rectangle(943, 420, 50, 50), greenBoxPreview);
            spriteBatch.Draw(texture, new Rectangle(943, 510, 50, 50), blueBoxPreview);

            spriteBatch.Draw(texSliderBarLong, new Vector2(374, 335), Color.White);
            spriteBatch.Draw(texSliderBarLong, new Vector2(374, 425), Color.White);
            spriteBatch.Draw(texSliderBarLong, new Vector2(374, 515), Color.White);

            spriteBatch.Draw(texRedSliderBar, new Vector2(xPosition, 334), Color.White);
            spriteBatch.Draw(texGreenSliderBar, new Vector2(yPosition, 425), Color.White);
            spriteBatch.Draw(texBlueSliderBar, new Vector2(zPosition, 515), Color.White);

            Vector2 borderPosition = new Vector2(366, 329); // Default to Red Slider
            if (selectedSlider == 1) // Green Slider
            {
                borderPosition = new Vector2(374, 425);
            }
            else if (selectedSlider == 2) // Blue Slider
            {
                borderPosition = new Vector2(374, 515);
            }

            spriteBatch.Draw(texSlideBarBorder, borderPosition, Color.White);

            spriteBatch.DrawString(mainMenuTextFont, $"Red Value: {xRed}", new Vector2(175, 345), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(mainMenuTextFont, $"Green Value: {xGreen}", new Vector2(170, 435), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(mainMenuTextFont, $"Blue Value: {xBlue}", new Vector2(175, 525), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

            spriteBatch.Draw(texBack, new Vector2(625, 650), Color.White);
            spriteBatch.Draw(texEnter, new Vector2(635, 580), Color.White);
            spriteBatch.End();
        }

        void DrawColorConfirm(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texChangeConfirm, new Vector2((GraphicsDevice.Viewport.Width - texChangeConfirm.Width) / 2, (GraphicsDevice.Viewport.Height - texChangeConfirm.Height) / 2), Color.White);
            spriteBatch.End();
        }

        void DrawGameOver(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw the Game Over text
            spriteBatch.Draw(texGameOverText, new Vector2(375, 320), Color.White);
            spriteBatch.DrawString(mainMenuTextFont, "Retry?", new Vector2(550, 450), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            spriteBatch.DrawString(mainMenuTextFont, "Yes", new Vector2(350, 550), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            spriteBatch.DrawString(mainMenuTextFont, "No", new Vector2(650, 550), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);

            // Draw Y and N indicators
            spriteBatch.Draw(texY, new Vector2(450, 550), Color.White);
            spriteBatch.Draw(texN, new Vector2(750, 550), Color.White);

            // Draw flash text if it exists
            if (isFlashing && !string.IsNullOrEmpty(flashText))
            {
                // Position the flash text appropriately on the Game Over screen
                Vector2 flashTextPosition = new Vector2(400, 600); // Adjust the position as needed
                spriteBatch.DrawString(mainMenuTextFont, flashText, flashTextPosition, Color.White);
            }

            spriteBatch.End();
        }

        void DrawGamePlay(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor); // Clear with the current backgroundColor

            spriteBatch.Begin();

            DrawPressAnyKeyPrompt(gameTime);
            float fadeFactor = isBlackFadeActive ? blackFadeAlpha : 1f;


            // Draw score information
            spriteBatch.DrawString(mainMenuTextFont, $"Score: {score}", new Vector2(10, -2), Color.White);
            spriteBatch.DrawString(mainMenuTextFont, $"High Score: {highScore}", new Vector2(GraphicsDevice.Viewport.Width / 2 + 400, -2), Color.White);
            spriteBatch.Draw(texture, new Rectangle(0, worldScreenTopY, GraphicsDevice.Viewport.Width, 1), Color.White);

            // Draw the snake's head only if it's visible
            if (snake.IsVisible)
            {
                spriteBatch.Draw(texture, new Rectangle((int)(snake.Pos.X * cellSize), (int)(snake.Pos.Y * cellSize + worldScreenTopY), cellSize, cellSize), Color.Yellow);

                // Draw the snake's tail only if it's visible
                if (isTailVisible)
                {
                    for (int i = 0; i < snake.TailLength; i++)
                    {
                        Color tailColor = snakTailColorPresets[currentTextureIndex][i % snakTailColorPresets[currentTextureIndex].Length]; // Use color preset
                        spriteBatch.Draw(texture,
                            new Rectangle(snake.Tail[i].X * cellSize, snake.Tail[i].Y * cellSize + worldScreenTopY, cellSize, cellSize),
                            tailColor * snake.TailAlpha[i]); // Apply alpha value to the color
                    }
                }
            }

            // Draw food items
            for (int i = 0; i < food.Length; ++i)
            {
                Color foodColor = foodColorList[food[i].Type];
                spriteBatch.Draw(texture, new Rectangle(food[i].Pos.X * cellSize, food[i].Pos.Y * cellSize + worldScreenTopY, cellSize, cellSize), foodColor);
            }

            // Draw the default food
            spriteBatch.Draw(texture, new Rectangle(defaultFoodPosX * cellSize, defaultFoodPosY * cellSize + worldScreenTopY, cellSize, cellSize), Color.White);

            // Draw grid lines
            for (int x = 0; x < GraphicsDevice.Viewport.Width; x += cellSize)
            {
                spriteBatch.Draw(texture, new Rectangle(x, worldScreenTopY, 1, GraphicsDevice.Viewport.Height), Color.Black);
            }

            for (int y = worldScreenTopY; y < GraphicsDevice.Viewport.Height; y += cellSize)
            {
                spriteBatch.Draw(texture, new Rectangle(0, y, GraphicsDevice.Viewport.Width, 1), Color.Black);
            }


            Color fadeColor = Color.White;
            if (isBlackFadeActive)
            {
                fadeColor = Color.White * blackFadeAlpha;
            }

            if (isBlackFadeActive)
            {
                spriteBatch.Draw(texture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                                 Color.Black * (1.0f - blackFadeAlpha));
            }

            // Draw fade effect if applicable
            if (isFading)
            {
                spriteBatch.Draw(texture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.Black * fadeAlpha);
            }

            if (snake.IsVisible)
            {
                // Draw the snake's head
                Color headColor = Color.Yellow * fadeFactor;
                spriteBatch.Draw(texture,
                    new Rectangle((int)(snake.Pos.X * cellSize), (int)(snake.Pos.Y * cellSize + worldScreenTopY), cellSize, cellSize),
                    headColor);

                // Draw the snake's tail
                for (int i = 0; i < snake.TailLength; i++)
                {
                    Color tailColor = snakTailColorPresets[currentTextureIndex][i % snakTailColorPresets[currentTextureIndex].Length];
                    tailColor = new Color(
                        (int)(tailColor.R * snake.TailAlpha[i] * fadeFactor),
                        (int)(tailColor.G * snake.TailAlpha[i] * fadeFactor),
                        (int)(tailColor.B * snake.TailAlpha[i] * fadeFactor),
                        (int)(tailColor.A * snake.TailAlpha[i] * fadeFactor)
                    );
                    spriteBatch.Draw(texture,
                        new Rectangle(snake.Tail[i].X * cellSize, snake.Tail[i].Y * cellSize + worldScreenTopY, cellSize, cellSize),
                        tailColor);
                }
            }



            // Draw flashing text if applicable
            if (isFlashing && !string.IsNullOrEmpty(flashText))
            {
                spriteBatch.DrawString(spriteFont, flashText, textPosition, textColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            }


            if (isDebugMenuVisible)
            {
                DrawDebugMenu(gameTime);
            }
            spriteBatch.End();
        }


        private bool isDebugMenuVisible = false;
        private string[] debugOptions =
        {
    "Red Pickup Test",
    "Orange Pickup Test",
    "Yellow Pickup Test",
    "Green Pickup Test",
    "Blue Pickup Test",
    "Indigo Pickup Test",
    "Violet Pickup Test",
    "Black Pickup Test",
    "Food Munch SFX",
    "Score Increase",
    "Score Decrease",
    "Tail Size Increase",
    "Tail Size Decrease",
    "Respawn  Default Food",
    "Respawn Food Pickup",
    "Back"
};
        private int selectedDebugOptionIndex = 0;

        void DrawDebugMenu(GameTime gameTime)
        {
            if (isDebugMenuVisible)
            {
                // Draw semi-transparent background for the debug menu
                spriteBatch.Draw(texture, new Rectangle(850, 25, 400, 675), new Color(0, 0, 0, 200) * 0.6f);

                // Draw the "Debug Menu" title separately
                string title = "Debug Menu";
                Vector2 titlePosition = new Vector2(950, 35); // Position for the title

                // Set title text scale and color
                float titleScale = 1.5f; // Larger scale for the title
                Color titleColor = Color.LightSeaGreen; // Title color

                // Set thickness for bold effect and shadow offset
                float titleThickness = 0.5f;
                Vector2 titleShadowOffset = new Vector2(3f, 3f);

                // Draw shadow for the title
                spriteBatch.DrawString(mainMenuTextFont, title, titlePosition + titleShadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                // Draw text with thickness offsets for bold effect
                spriteBatch.DrawString(mainMenuTextFont, title, titlePosition + new Vector2(-titleThickness, 0), titleColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(mainMenuTextFont, title, titlePosition + new Vector2(titleThickness, 0), titleColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(mainMenuTextFont, title, titlePosition + new Vector2(0, -titleThickness), titleColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(mainMenuTextFont, title, titlePosition + new Vector2(0, titleThickness), titleColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                // Draw the main title text in the center to complete the bold effect
                spriteBatch.DrawString(mainMenuTextFont, title, titlePosition, titleColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                // Define a darker, warmer yellow color
                Color darkYellow = new Color(255, 215, 0); // Golden yellow

                // Loop through each debug option to draw it on the screen
                for (int i = 0; i < debugOptions.Length; i++)
                {
                    // Format the option with > < around it if it's selected
                    string formattedOption = (i == selectedDebugOptionIndex) ? $"> {debugOptions[i]} <" : debugOptions[i];

                    // Calculate the flashing color for the selected option
                    Color color = (i == selectedDebugOptionIndex)
                        ? GetFlashingColor(gameTime, darkYellow, Color.White) // Use darkYellow for flashing
                        : Color.White;

                    // Set the position for the text
                    Vector2 position = new Vector2(950, 105 + i * 37);

                    // Set the scale for larger text
                    float textScale = .85f;

                    // Set thickness for bold effect
                    float thickness = .35f;

                    // Set shadow offset
                    Vector2 shadowOffset = new Vector2(3f, 3f);

                    // Draw shadow text
                    spriteBatch.DrawString(mainMenuTextFont, formattedOption, position + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                    // Draw text with thickness offsets for bold effect
                    spriteBatch.DrawString(mainMenuTextFont, formattedOption, position + new Vector2(-thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, formattedOption, position + new Vector2(thickness, 0), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, formattedOption, position + new Vector2(0, -thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(mainMenuTextFont, formattedOption, position + new Vector2(0, thickness), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                    // Draw the main text in the center to complete the bold effect
                    spriteBatch.DrawString(mainMenuTextFont, formattedOption, position, color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                }
            }


            Color GetFlashingColor(GameTime gameTime, Color colorA, Color colorB)
            {
                // Example flashing effect based on time
                float t = (float)((Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) + 1) / 2); // Adjusted to range from 0 to 1
                return Color.Lerp(colorA, colorB, t * 0.3f); // Make the transition less pronounced
            }
        }







        void RedFoodEffect()
        {
            flashText = "Tail Size Increased";
            AddTailSegments(5);
            snake.Speed *= 0.95f; // Increase speed
        }

        void OrangeFoodEffect()
        {
            flashText = "Tail size decreased!";
            snake.TailLength = Math.Max(0, snake.TailLength - 5); // Ensure TailLength doesn't go negative
            snake.Speed *= 1.05f; // Increase speed
        }

        void YellowFoodEffect()
        {
            flashText = "Increased speed!";
            snake.Speed *= 0.8f; // Increase speed by 1.25x
            snake.TailLength++;
            AddTailSegments(5);
        }

        void GreenFoodEffect()
        {
            flashText = "Decreased speed!";
            snake.TailLength++;
            snake.Speed *= 1.2f; // Decrease speed to 75% of current speed
        }

        void BlueFoodEffect()
        {
            flashText = "Bonus Points";
            snake.TailLength++;
            score += 1100;
        }

        void PurpleFoodEffect()
        {
            flashText = "Purple Food Eaten!";
            TeleportSnake();
            // Add a brief invisibility period after teleportation
            snake.IsVisible = false;
            teleportTimer = teleportDuration;
        }

        void BrownFoodEffect()
        {
            flashText = "Brown Food Eaten!";
            isFadingTail = true; // Start the fading process
        }

        void BlackFoodEffect()
        {
            flashText = "Black Food Eaten!";
            isBlackFadeActive = true;
            blackFadeTimer = 0f; // Reset the timer
            blackFadeAlpha = 1.0f; // Start from fully visible
        }


        private float scrollSpeed = 0.1f; // Adjust this value to change scroll smoothness
        private MouseState prevMouseState;
        private bool isMouseOverHandle = false;

        private float dragStartY;
        private float dragStartHandleY;

        // Track dragging state
        private bool isDraggingScrollBar = false;
        private float dragDelay = 0.1f; // Delay in seconds before moving the scrollbar handle
        private float dragDelayTimer = 0f; // Timer to track the delay

        void HandleScrolling(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            // Check if mouse is over the handle
            isMouseOverHandle = scrollBarHandle.Contains(mouseState.Position);

            if (mouseState.LeftButton == ButtonState.Pressed && !isDraggingScrollBar && isMouseOverHandle)
            {
                // Start dragging
                isDraggingScrollBar = true;
                dragStartY = mouseState.Y; // Store the initial Y position of the mouse
                dragStartHandleY = scrollBarHandle.Y; // Store the initial position of the scrollbar handle
            }
            else if (mouseState.LeftButton == ButtonState.Released)
            {
                isDraggingScrollBar = false;
                dragDelayTimer = 0f; // Reset the timer when dragging stops
            }

            if (isDraggingScrollBar)
            {
                // Update the delay timer
                dragDelayTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Only move the scrollbar handle after the delay
                if (dragDelayTimer >= dragDelay)
                {
                    // Calculate the drag distance based on the difference from the initial click position
                    float dragDistance = mouseState.Y - dragStartY; // This gives you the movement from where the drag started

                    // Calculate new handle position without jumping to the mouse cursor
                    int newHandleY = (int)(dragStartHandleY + dragDistance); // Move the handle based on the drag distance

                    // Clamp the new Y position within the scrollbar bounds
                    newHandleY = MathHelper.Clamp(newHandleY, scrollBarBounds.Y, scrollBarBounds.Bottom - scrollBarHandle.Height);

                    // Update handle position
                    scrollBarHandle.Y = newHandleY;

                    // Calculate and update the scroll position based on the handle position
                    float scrollRatio = (float)(scrollBarHandle.Y - scrollBarBounds.Y) / (scrollBarBounds.Height - scrollBarHandle.Height);
                    currentScrollPosition = scrollRatio * maxScrollPosition;
                }
            }

            // Clamp the currentScrollPosition
            currentScrollPosition = MathHelper.Clamp(currentScrollPosition, 0, maxScrollPosition);

            // Only update scrollBarHandle position if not dragging
            if (!isDraggingScrollBar)
            {
                float handleRatio = currentScrollPosition / maxScrollPosition;
                int targetHandleY = (int)(scrollBarBounds.Y + (scrollBarBounds.Height - scrollBarHandle.Height) * handleRatio);
                scrollBarHandle.Y = MathHelper.Clamp(targetHandleY, scrollBarBounds.Y, scrollBarBounds.Bottom - scrollBarHandle.Height);
            }
        }




        public class LifetimeStats
        {
            public int TotalGamesPlayed { get; set; }
            public int TotalScore { get; set; }
            public int TotalFoodEaten { get; set; }
            public Dictionary<int, int> FoodTypeEaten { get; set; }
            public int LongestSnakeLength { get; set; }
            public TimeSpan TotalPlayTime { get; set; }

            public LifetimeStats()
            {
                FoodTypeEaten = new Dictionary<int, int>();
            }
        }

        private void InitializeAchievements()
        {
            achievementStats = new List<AchievementStat>
    {
        new AchievementStat { Name = "High Score", Value = highScore.ToString() },
        new AchievementStat { Name = "Total Games Played", Value = lifetimeStats.TotalGamesPlayed.ToString() },
        new AchievementStat { Name = "Total Score", Value = lifetimeStats.TotalScore.ToString() },
        new AchievementStat { Name = "Total Food Eaten", Value = lifetimeStats.TotalFoodEaten.ToString() },
        new AchievementStat { Name = "Longest Snake Length", Value = lifetimeStats.LongestSnakeLength.ToString() },
        new AchievementStat { Name = "Total Play Time", Value = lifetimeStats.TotalPlayTime.ToString(@"hh\:mm\:ss") }
    };

            // Add more test achievements
            for (int i = 1; i <= 20; i++)
            {
                achievementStats.Add(new AchievementStat { Name = $"Test Achievement {i}", Value = $"Value {i}" });
            }
        }





        void SetupAchievementsUI()
        {
            int boxWidth = 600;
            int boxHeight = 400;
            achievementsBox = new Rectangle(
                (GraphicsDevice.Viewport.Width - boxWidth) / 2,
                (GraphicsDevice.Viewport.Height - boxHeight) / 2,
                boxWidth,
                boxHeight
            );

            int scrollBarWidth = 20;
            int arrowHeight = texUpArrowButton.Height; // Use the actual height of the texture
            int scrollBarPadding = 2; // Padding from the right edge of the box

            // Adjust the scrollbar bounds to leave space for both arrows
            scrollBarBounds = new Rectangle(
                achievementsBox.Right - scrollBarWidth - scrollBarPadding,
                achievementsBox.Y + arrowHeight,
                scrollBarWidth,
                achievementsBox.Height - (2 * arrowHeight)
            );

            float visibleRatio = (float)achievementsBox.Height / (achievementStats.Count * itemHeight);
            int handleHeight = Math.Max((int)(scrollBarBounds.Height * visibleRatio), 40); // Minimum handle height of 40
            scrollBarHandle = new Rectangle(
                scrollBarBounds.X,
                scrollBarBounds.Y,
                scrollBarBounds.Width,
                handleHeight
            );

            // Define up arrow button position
            Vector2 upArrowPosition = new Vector2(
                scrollBarBounds.X,
                achievementsBox.Y
            );

            // Define down arrow button position
            Vector2 downArrowPosition = new Vector2(
                scrollBarBounds.X,
                achievementsBox.Bottom - arrowHeight
            );

            // Store these positions for use in drawing
            upArrowButton = new Rectangle((int)upArrowPosition.X, (int)upArrowPosition.Y, texUpArrowButton.Width, texUpArrowButton.Height);
            downArrowButton = new Rectangle((int)downArrowPosition.X, (int)downArrowPosition.Y, texDownArrowButton.Width, texDownArrowButton.Height);

            maxScrollPosition = Math.Max(0, (achievementStats.Count * itemHeight) - (achievementsBox.Height - 40));
            currentScrollPosition = 0;
            targetScrollPosition = 0;
        }


    }
}

