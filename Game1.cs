using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game1
{

    public enum GameState
    {
        mainMenu,
        levelSelectScreen,
        helpScreen,
        levelPackScreen,
        gameInProgress,
        exitgame
    };

    public struct EnemyStruct
    {
        public EnemyClassification classification;
        public ActorModel model;
        public int moveSpeed;
        public int visionRange;
    }

    struct ProjectileStruct
    {
        public ProjectileClassification classification;
        public ActorModel model;
        public int moveSpeed;
        public int index;
        public bool allowedThisLevel;
        public bool hasSecondaryFire;
        public bool secondaryFireAvailable;
        public Texture2D uiTextTexture;
        public UI uiBackground;
        public UI uiText;
        public UI uiMouseButtons;
    }

    public struct Tile
    {
        public Vector2 coordinates;
        public Vector3 centre;
    }

    public class Game1 : Game
    {
        GameState currentGameState = GameState.mainMenu;

        MainMenu mainMenu;
        HelpScreen helpScreen;
        LevelSelect levelSelectScreen;
        LevelPackSelect levelPackSelectScreen;

        bool isLevelOver;
        double levelResetTimer;
        double levelEndTime;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameTime gameTime;
        Random rng;

        //camera
        Vector3 overheadCamTarget, overheadCamPosition;
        Matrix overheadProjectionMatrix, overheadViewMatrix;
        Vector3 firstPersonCamTarget, firstPersonCamPosition;
        Matrix firstPersonProjectionMatrix, firstPersonViewMatrix;

        Matrix projectionMatrix, viewMatrix, worldMatrix;

        float screenCentreX;
        float screenCentreY;

        bool gunLoaded;
        bool rightMouseButtonDown;

        Texture2D unselectedProjectileUITexture;
        Texture2D selectedProjectileUITexture;
        Texture2D unavailableProjectileUITexture;

        Texture2D primaryFireAvailableUITexture;
        Texture2D primaryFireUsedUITexture;
        Texture2D secondaryFireAvailableUITexture;
        Texture2D secondayFireUsedUITexture;

        Texture2D viewUIFirstPersonTexture;
        Texture2D viewUIBirdsEyeTexture;
        Texture2D gameOverTexture;
        Texture2D goalFoundTexture;

        public static Texture2D genericButtonSprite;
        public static SpriteFont buttonText;

        ActorModel cube;
        ActorModel playerModel;
        ActorModel goalModel;
        ActorModel wall;
        ActorModel electricBeams;
        ActorModel dart;
        ActorModel dartWalls;
        ActorModel gateWalls;
        ActorModel spikePit;
        ActorModel floorModel;
        ActorModel floorSegmentModel;
        ActorModel movementTriggerModel;
        ActorModel projectileTriggerModel;

        ProjectileStruct[] projectileTypes;
       
        int numberOfProjectileTypes;
        ProjectileStruct none;
        ProjectileStruct shock;
        ProjectileStruct pull;
        ProjectileStruct teleport;

        EnemyStruct mummy;
        EnemyStruct boulder;

        Player player;
        Actor goal;

        static string levelPacksFileLocation;

        List<Tile> levelTiles;
        float tileSize;

        List<Actor> terrain;
        List<Actor> floor;
        List<Actor> roof;
        List<Actor> staticActors;
        List<Projectile> allProjectiles;
        List<NPC> guards;
        List<Hazard> hazards;
        List<DartSpawner> dartSpawners;
        List<ProjectileActivatedTrigger> projectileActivatedTriggers;
        List<TimeActivatedTrigger> timeActivatedTriggers;
        List<MovementActivatedTrigger> movementActivatedTriggers;

        int loadedProjectileIndex;

        UI viewUI;
        UI gameOverUI;
        UI goalFoundUI;

        Song bgm;

        public static List<Level> currentLevelPack;
        static int currentLevelNumber;
        int numberOfLevelsInLevelPack;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            gameTime = new GameTime();
            graphics.PreferredBackBufferWidth = 1550;
            graphics.PreferredBackBufferHeight = 900;
        }

        protected override void Initialize()
        {
            tileSize = 150f;

            //set up camera
            overheadCamTarget = new Vector3(0f, 0f, 0f);
            overheadCamPosition = new Vector3(0f, 1500f, 750f);

            overheadProjectionMatrix = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width * 2, GraphicsDevice.Viewport.Height * 2, 0f, 3000f);
            overheadViewMatrix = Matrix.CreateLookAt(overheadCamPosition, overheadCamTarget, new Vector3(0f, 1f, 0f));

            firstPersonCamTarget = new Vector3(0f, 0f, 1f);
            firstPersonCamPosition = new Vector3(0f, 0f, 0f);
            firstPersonProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(135), graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight, 1f, 35000f);
            firstPersonViewMatrix = Matrix.CreateLookAt(firstPersonCamPosition, firstPersonCamTarget, new Vector3(0f, 1f, 0f));

            worldMatrix = Matrix.CreateWorld(firstPersonCamTarget, Vector3.Forward, Vector3.Up);

            screenCentreX = GraphicsDevice.Viewport.Width / 2;
            screenCentreY = GraphicsDevice.Viewport.Height / 2;

            rng = new Random();

            mummy.classification = EnemyClassification.mummy;
            mummy.moveSpeed = 4;
            mummy.visionRange = 8;

            boulder.classification = EnemyClassification.boulder;
            boulder.moveSpeed = 7;
            boulder.visionRange = 1;

            none.classification = ProjectileClassification.none;
            none.moveSpeed = 0;
            none.allowedThisLevel = true;

            shock.classification = ProjectileClassification.power;
            shock.moveSpeed = 16;
            shock.allowedThisLevel = false;
            shock.hasSecondaryFire = false;
            
            pull.classification = ProjectileClassification.pull;
            pull.moveSpeed = 12;
            pull.allowedThisLevel = false;
            pull.hasSecondaryFire = true;
            pull.secondaryFireAvailable = false;

            teleport.classification = ProjectileClassification.teleport;
            teleport.moveSpeed = 8;
            teleport.allowedThisLevel = false;
            teleport.hasSecondaryFire = true;
            teleport.secondaryFireAvailable = false;

            levelPacksFileLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            levelPacksFileLocation += "..\\..\\..\\..\\..\\LevelPacks";

            Mouse.SetPosition((int)screenCentreX, (int)screenCentreY);
            this.IsMouseVisible = false;
            
            base.Initialize();

            currentLevelPack = new List<Level>();

            mainMenu = new MainMenu(GraphicsDevice, unselectedProjectileUITexture, selectedProjectileUITexture);
            helpScreen = new HelpScreen(GameState.mainMenu, GraphicsDevice);
            levelSelectScreen = new LevelSelect(GraphicsDevice);
            levelPackSelectScreen = new LevelPackSelect(GraphicsDevice);

            player = new Player(playerModel, new Vector3(0f, 0f, 0f));
            goal = new Actor(goalModel, new Vector3(0f, 0f, 150f));

            numberOfProjectileTypes = Enum.GetNames(typeof(ProjectileClassification)).Length;
            projectileTypes = new ProjectileStruct[numberOfProjectileTypes];

            none.index = 0;
            projectileTypes[0] = none;
            shock.index = 1;
            projectileTypes[1] = shock;
            pull.index = 2;
            projectileTypes[2] = pull;
            teleport.index = 3;
            projectileTypes[3] = teleport;

            levelResetTimer = 2;

            viewUI = new UI(viewUIFirstPersonTexture, new Vector2(viewUIFirstPersonTexture.Width * (2), viewUIFirstPersonTexture.Height * (2)), new Vector2(3f, 3f), true);
            gameOverUI = new UI(gameOverTexture, new Vector2(screenCentreX, screenCentreY), new Vector2(2f, 2f), false);
            goalFoundUI = new UI(goalFoundTexture, new Vector2(screenCentreX, screenCentreY), new Vector2(2f, 2f), false);

            currentLevelPack = loadLevelPack("Default.txt");
            numberOfLevelsInLevelPack = currentLevelPack.Count;

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(bgm);
        }

        protected override void LoadContent()
        {
            // To draw 2D UI elements
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ActorModel.setWorldMatrix(worldMatrix);

            cube = new ActorModel(Content.Load<Model>("Cube"), true, true, ActorModel.ModelEffect.Fog);
            playerModel = new ActorModel(Content.Load<Model>("PlayerHat"), false, false, ActorModel.ModelEffect.DefaultLighting);
            goalModel = new ActorModel(Content.Load<Model>("Goal"), false, false, ActorModel.ModelEffect.Fog);
            wall = new ActorModel(Content.Load<Model>("WallSegment"), true, true, ActorModel.ModelEffect.Fog);
            gateWalls = new ActorModel(Content.Load<Model>("ElectricGateWall"), true, true, ActorModel.ModelEffect.Fog);
            floorModel = new ActorModel(Content.Load<Model>("Floor"), false, false, ActorModel.ModelEffect.Fog);
            floorSegmentModel = new ActorModel(Content.Load<Model>("FloorSegment"), false, false, ActorModel.ModelEffect.Fog);
            movementTriggerModel = new ActorModel(Content.Load<Model>("FloorTrigger"), false, false, ActorModel.ModelEffect.Fog);
            projectileTriggerModel = new ActorModel(Content.Load<Model>("ProjectileTrigger"), true, true, ActorModel.ModelEffect.Fog);

            electricBeams = new ActorModel(Content.Load<Model>("ElectricGateBeams"), false, false, ActorModel.ModelEffect.Fog);
            dart = new ActorModel(Content.Load<Model>("Dart"), false, false, ActorModel.ModelEffect.Fog);
            dartWalls = new ActorModel(Content.Load<Model>("DartWall"), true, true, ActorModel.ModelEffect.Fog);
            spikePit = new ActorModel(Content.Load<Model>("HoleHazard"), false, false, ActorModel.ModelEffect.Fog);

            shock.model = new ActorModel(Content.Load<Model>("ShockProjectile"), false, false, ActorModel.ModelEffect.DefaultLighting);
            pull.model = new ActorModel(Content.Load<Model>("PullProjectile"), false, false, ActorModel.ModelEffect.DefaultLighting);
            teleport.model = new ActorModel(Content.Load<Model>("PortProjectile"), false, false, ActorModel.ModelEffect.DefaultLighting);

            mummy.model = new ActorModel(Content.Load<Model>("Mummy"), true, true, ActorModel.ModelEffect.Fog);
            boulder.model = new ActorModel(Content.Load<Model>("Boulder"), true, true, ActorModel.ModelEffect.Fog);

            shock.uiTextTexture = Content.Load<Texture2D>("UIPowerText");
            pull.uiTextTexture = Content.Load<Texture2D>("UIPullText");
            teleport.uiTextTexture = Content.Load<Texture2D>("UIPortText");

            unselectedProjectileUITexture = Content.Load<Texture2D>("UIUnselectedProjectile");
            selectedProjectileUITexture = Content.Load<Texture2D>("UISelectedProjectile");
            unavailableProjectileUITexture = Content.Load<Texture2D>("UIUnavailableProjectile");

            primaryFireAvailableUITexture = Content.Load<Texture2D>("UIPrimaryFireAvailable");
            primaryFireUsedUITexture = Content.Load<Texture2D>("UIPrimaryFireUsed");
            secondaryFireAvailableUITexture = Content.Load<Texture2D>("UISecondaryFireAvailable");
            secondayFireUsedUITexture = Content.Load<Texture2D>("UISecondaryFireUsed");

            viewUIFirstPersonTexture = Content.Load<Texture2D>("UIFirstPerson");
            viewUIBirdsEyeTexture = Content.Load<Texture2D>("UIThirdPerson");
            gameOverTexture = Content.Load<Texture2D>("UIGameOver");
            goalFoundTexture = Content.Load<Texture2D>("UIGoal");

            genericButtonSprite = Content.Load < Texture2D>("UIPurpleBox");
            buttonText = Content.Load<SpriteFont>("ButtonText");

            bgm = Content.Load<Song>("Mystery Sax");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            switch (currentGameState)
            {
                case GameState.gameInProgress:

                    Vector3 playerDisplacement = new Vector3(0f, 0f, 0f);
                    bool onFloor;

                    List<Actor> enemyVisionBlockers = new List<Actor>();
                    List<Projectile> activeProjectiles = new List<Projectile>();
                    
                    Projectile activePullProjectile = null;
                    this.IsMouseVisible = false;

                    hazards.Clear();

                    // check for exit
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape) || currentGameState == GameState.exitgame)
                        Exit();

                    // if level is over, check timer until next level / level reset
                    updateLevelOverState(goalFoundUI.isActive());

                    // update list of enemy vision blockers
                    foreach (Actor t in terrain)
                    {
                        t.updateHitboxes();
                        enemyVisionBlockers.Add(t);
                    }
                    foreach (NPC guard in guards)
                    {
                        enemyVisionBlockers.Add(guard);
                    }

                    foreach (DartSpawner ds in dartSpawners)
                    {
                        foreach (Hazard dart in ds.getDartList())
                        {
                            hazards.Add(dart);
                        }
                    }

                    // player updates

                    onFloor = false;

                    // player / end goal collision
                    if (player.collidesWith(goal))
                    {
                        endLevel(goal);
                        goalFoundUI.setActive(true);
                    }

                    player.updateHitboxes();
                    foreach (Hazard h in hazards)
                    {
                        // hazard / player collision
                        if (h.collidesWith(player) && !isLevelOver)
                        {
                            endLevel(h.getParentActor());
                            gameOverUI.setActive(true);
                        }
                    }

                    foreach (Actor f in floor)
                    {
                        if (f.collisionHitbox.Intersects(player.underfootHitbox))
                        {
                            onFloor = true;
                            break;
                        }
                    }

                    if (!onFloor && !isLevelOver)
                    {
                        player.Falling = true;
                        endLevel(null);
                        gameOverUI.setActive(true);
                    }

                    // guard updates
                    foreach (NPC g in guards)
                    {
                        if (!g.isDead())
                        {
                            onFloor = false;

                            foreach (Actor f in floor)
                            {
                                if (f.collisionHitbox.Intersects(g.underfootHitbox))
                                {
                                    onFloor = true;
                                    break;
                                }
                            }

                            if (!onFloor)
                            {
                                g.Falling = true;
                                g.kill();
                            }

                            if (!isLevelOver)
                            {
                                g.update(enemyVisionBlockers);

                                // detect player if collide with enemy
                                if (player.collidesWith(g))
                                {
                                    g.detectPlayer();
                                    endLevel(g);
                                    gameOverUI.setActive(true);
                                }
                                // detect player if collides with area in front of enemy
                                else
                                {
                                    foreach (BoundingSphere b in g.detectionArea)
                                    {
                                        if (player.collisionHitbox.Intersects(b))
                                        {
                                            g.detectPlayer();
                                            endLevel(g);
                                            gameOverUI.setActive(true);
                                        }
                                    }
                                }

                                // kill guards who collide with hazards
                                foreach (Hazard h in hazards)
                                {
                                    if (h.collidesWith(g) && g.getClassification() != EnemyClassification.boulder)
                                    {
                                        g.kill();
                                    }
                                }
                            }
                        }
                        else if (g.Falling)
                        {
                            g.currentlyPulledDownByGravity();
                        }
                    }


                    // hazard and variable obstacles updates
                    foreach (DartSpawner ds in dartSpawners)
                    {
                        ds.update();
                    }


                    // projectile updates

                    // look for an active pull projectile
                    foreach (Projectile pj in allProjectiles)
                    {
                        if (pj.getClassification() == ProjectileClassification.pull)
                        {
                            activePullProjectile = pj;
                            break;
                        }
                    }

                    foreach (Projectile pj in allProjectiles)
                    {
                        bool destroyProjectile = false;

                        if (activePullProjectile == null)
                        {
                            pj.move();
                        }
                        else
                        {
                            // destory projectiles that collide with active pull projectile
                            if (!pj.Equals(activePullProjectile) && pj.collidesWith(activePullProjectile))
                            {
                                destroyProjectile = true;
                            }
                            // move projectiles towards active pull projectile if it is being used
                            else if (!pj.Equals(activePullProjectile) && activePullProjectile.hasActionStarted() && activePullProjectile.actorInActionRadius(pj.collisionHitbox))
                            {
                                pj.move(new Vector3(activePullProjectile.position.X - pj.position.X, 0f, activePullProjectile.position.Z - pj.position.Z) / 60);
                            }
                            // normal projectile movement
                            else
                            {
                                pj.move();
                            }
                        }

                        pj.updateHitboxes();

                        // projectile / guard collision
                        foreach (NPC g in guards)
                        {
                            if (pj.collidesWith(g) && !g.isDead())
                            {
                                if (pj.getClassification() == ProjectileClassification.pull && !pj.hasActionStarted())
                                {
                                    // pull projectiles attach to guards
                                    if (pj.hasNoParentActor())
                                    {
                                        g.attachNewActor(pj);
                                    }
                                    pj.MovementBlocked = true;
                                }
                                else
                                {
                                    // other projectiles are destroyed
                                    destroyProjectile = true;
                                }

                                break;
                            }

                            // PULL PROJECTILE SPECIFICS
                            if (pj.getClassification() == ProjectileClassification.pull && g.getClassification() == EnemyClassification.mummy && pj.hasActionStarted())
                            {
                                // in range of enemy when activated
                                if (pj.actorInActionRadius(g.collisionHitbox))
                                {
                                    g.move(new Vector3(pj.position.X - g.position.X, 0f, pj.position.Z - g.position.Z) / 60);
                                }
                            }
                        }

                        // projectile / terrain collision
                        foreach (Actor t in terrain)
                        {
                            if (pj.collidesWith(t))
                            {
                                // attach pull proejctile to terrain
                                if (pj.getClassification() == ProjectileClassification.pull)
                                {
                                    if (pj.hasNoParentActor())
                                    {
                                        t.attachNewActor(pj);
                                    }
                                    pj.MovementBlocked = true;
                                }

                                // destory other projectile types
                                else
                                {
                                    destroyProjectile = true;
                                }

                                break;
                            }
                        }

                        // projectile / projectile activated trigger collision
                        foreach (ProjectileActivatedTrigger pat in projectileActivatedTriggers)
                        {
                            if (pj.collidesWith(pat))
                            {
                                if (pat.affectedByProjectile(pj.getClassification()))
                                {
                                    destroyProjectile = true;
                                }
                            }
                        }

                        if (pj.requiresDeletion)
                        {
                            destroyProjectile = true;
                        }

                        // list of projectiles that don't need destroying
                        if (destroyProjectile == false)
                        {
                            activeProjectiles.Add(pj);
                        }
                        // remove projectiles
                        else
                        {
                            pj.detachFromParentActor();
                            for (int i = 1; i < numberOfProjectileTypes; i++)
                            {
                                if (pj.getClassification() == projectileTypes[i].classification)
                                {
                                    if (projectileTypes[i].secondaryFireAvailable)
                                    {
                                        projectileTypes[i].secondaryFireAvailable = false;
                                        if (loadedProjectileIndex == projectileTypes[i].index)
                                        {
                                            projectileTypes[i].uiMouseButtons.setSprite(primaryFireAvailableUITexture);
                                        }
                                        else
                                        {
                                            projectileTypes[i].uiMouseButtons.setSprite(null);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    // replace list of projectiles with list of projectiles that didn't collide with anything
                    allProjectiles.Clear();
                    allProjectiles = activeProjectiles;

                    // check triggers

                    foreach (MovementActivatedTrigger mat in movementActivatedTriggers)
                    {
                        mat.checkCurrentlyCollidingCharacter();
                        mat.checkResetTimer();

                        if (mat.collidesWith(player))
                        {
                            mat.collisionWithCharacter(player);
                        }

                        foreach (NPC g in guards)
                        {
                            if (mat.collidesWith(g))
                            {
                                mat.collisionWithCharacter(g);
                            }
                        }
                    }

                    foreach (ProjectileActivatedTrigger pat in projectileActivatedTriggers)
                    {
                        pat.checkResetTimer();
                    }

                    //projectile action button
                    if (!isLevelOver)
                    {
                        if (mouse.RightButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
                        {
                            rightMouseButtonDown = true;
                            for (int i = 1; i < numberOfProjectileTypes; i++)
                            {
                                if (projectileTypes[i].uiMouseButtons.currentlyUsesSprite(secondaryFireAvailableUITexture))
                                {
                                    projectileTypes[i].secondaryFireAvailable = false;
                                    projectileTypes[i].uiMouseButtons.setSprite(secondayFireUsedUITexture);
                                }
                            }

                            foreach (Projectile p in allProjectiles)
                            {
                                if (!p.Equals(none))
                                {
                                    p.startAction();
                                }

                                if (p.getClassification() == ProjectileClassification.teleport)
                                {
                                    player.setPosition(new Vector3(p.position.X, player.position.Y, p.position.Z));
                                }
                            }
                        }

                        if (mouse.RightButton == ButtonState.Released && rightMouseButtonDown)
                        {
                            rightMouseButtonDown = false;
                            for (int i = 1; i < numberOfProjectileTypes; i++)
                            {
                                if (projectileTypes[i].uiMouseButtons.currentlyUsesSprite(secondayFireUsedUITexture))
                                {
                                    if (i == loadedProjectileIndex)
                                    {
                                        projectileTypes[i].uiMouseButtons.setSprite(primaryFireAvailableUITexture);
                                    }
                                    else
                                    {
                                        projectileTypes[i].uiMouseButtons.setSprite(null);
                                    }
                                }
                            }
                        }
                    }

                    // BIRDS EYE VIEW
                    if (keyboard.IsKeyDown(Keys.Tab) && !isLevelOver)
                    {
                        viewUI.setSprite(viewUIBirdsEyeTexture);

                        overheadCamTarget = player.position;
                        overheadCamPosition = new Vector3(player.position.X, 350f, player.position.Z + 1f);
                        overheadProjectionMatrix = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width * 2, GraphicsDevice.Viewport.Height * 2, 0f, 3000f);
                        overheadViewMatrix = Matrix.CreateLookAt(overheadCamPosition, overheadCamTarget, new Vector3(0f, 1f, 0f));

                        projectionMatrix = overheadProjectionMatrix;
                        viewMatrix = overheadViewMatrix;
                    }

                    // FIRST PERSON VIEW
                    else
                    {
                        bool currentAmmoChanged = false;

                        viewUI.setSprite(viewUIFirstPersonTexture);

                        //set up first person camera
                        firstPersonCamPosition = new Vector3(player.position.X, player.collisionHitbox.Max.Y, player.position.Z);
                        firstPersonCamTarget = firstPersonCamPosition + Vector3.Transform(new Vector3(0f, 0f, 1f), player.rotation);

                        // set up first person matrices
                        firstPersonProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(135), graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight, playerModel.boxExtents.X, 3000f);
                        firstPersonViewMatrix = Matrix.CreateLookAt(firstPersonCamPosition, firstPersonCamTarget, new Vector3(0f, 1f, 0f));
                        worldMatrix = Matrix.CreateWorld(firstPersonCamTarget, Vector3.Forward, Vector3.Up);

                        projectionMatrix = firstPersonProjectionMatrix;
                        viewMatrix = firstPersonViewMatrix;

                        if (!isLevelOver)
                        {
                            // MOUSE INPUT
                            if (mouse.X != screenCentreX)
                            {
                                player.changeYaw(MathHelper.ToRadians(screenCentreX - mouse.X) / 3);
                            }

                            Mouse.SetPosition((int)screenCentreX, (int)screenCentreY); // set mouse to centre


                            // fire gun
                            if (mouse.LeftButton == ButtonState.Pressed && gunLoaded && !projectileTypes[loadedProjectileIndex].Equals(none))
                            {
                                if (projectileTypes[loadedProjectileIndex].hasSecondaryFire && projectileTypes[loadedProjectileIndex].secondaryFireAvailable)
                                {
                                    // only one pull projectile allowed to be active at any one time
                                }
                                else
                                {
                                    for (int i = 1; i < numberOfProjectileTypes; i++)
                                    {
                                        if (projectileTypes[i].uiMouseButtons.currentlyUsesSprite(primaryFireAvailableUITexture))
                                        {
                                            projectileTypes[i].uiMouseButtons.setSprite(null);
                                        }
                                    }
                                    projectileTypes[loadedProjectileIndex].uiMouseButtons.setSprite(primaryFireUsedUITexture);
                                    gunLoaded = false;
                                    allProjectiles.Add(createNewProjectile(projectileTypes[loadedProjectileIndex].classification));
                                    if (projectileTypes[loadedProjectileIndex].hasSecondaryFire)
                                    {
                                        projectileTypes[loadedProjectileIndex].secondaryFireAvailable = true;
                                    }
                                }
                            }
                        
                            // mouse button must be released in between each shot
                            if (mouse.LeftButton == ButtonState.Released && !gunLoaded && !projectileTypes[loadedProjectileIndex].Equals(none))
                            {
                                gunLoaded = true;
                                for (int i = 1; i < numberOfProjectileTypes; i++)
                                {
                                    if (projectileTypes[i].secondaryFireAvailable)
                                    {
                                        projectileTypes[i].uiMouseButtons.setSprite(secondaryFireAvailableUITexture);
                                    }
                                    else
                                    {
                                        if (loadedProjectileIndex == i)
                                        {
                                            projectileTypes[loadedProjectileIndex].uiMouseButtons.setSprite(primaryFireAvailableUITexture);
                                        }
                                        else
                                        {
                                            projectileTypes[i].uiMouseButtons.setSprite(null);
                                        }
                                    }
                                }
                            }

                            // KEYBOARD INPUT
                            // player move
                            if ((keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) && (keyboard.IsKeyUp(Keys.Right) && keyboard.IsKeyUp(Keys.D)))
                            {
                                playerDisplacement += -(new Vector3(2f, 2f, 2f) * new Vector3((float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg - 180)), 0f, (float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg))));
                            }
                            if ((keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) && (keyboard.IsKeyUp(Keys.Left) && keyboard.IsKeyUp(Keys.A)))
                            {
                                playerDisplacement += new Vector3(2f, 2f, 2f) * new Vector3((float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg - 180)), 0f, (float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg)));
                            }

                            if ((keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W)) && (keyboard.IsKeyUp(Keys.Down) && keyboard.IsKeyUp(Keys.S)))
                            {
                                playerDisplacement += new Vector3((float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg)), 0f, (float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg)));
                            }

                            if ((keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S)) && (keyboard.IsKeyUp(Keys.Up) && keyboard.IsKeyUp(Keys.W)))
                            {
                                playerDisplacement += -(new Vector3((float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg)), 0f, (float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg))));
                            }
                            
                            // choose ammo

                            if ((keyboard.IsKeyDown(Keys.D1) || keyboard.IsKeyDown(Keys.NumPad1)) && projectileTypes[shock.index].allowedThisLevel)
                            {
                                currentAmmoChanged = true;
                                loadedProjectileIndex = shock.index;
                            }
                            if ((keyboard.IsKeyDown(Keys.D2) || keyboard.IsKeyDown(Keys.NumPad2)) && projectileTypes[pull.index].allowedThisLevel)
                            {
                                currentAmmoChanged = true;
                                loadedProjectileIndex = pull.index;
                            }

                            if ((keyboard.IsKeyDown(Keys.D3) || keyboard.IsKeyDown(Keys.NumPad3)) && projectileTypes[teleport.index].allowedThisLevel)
                            {
                                currentAmmoChanged = true;
                                loadedProjectileIndex = teleport.index;
                            }
                        }

                        if (currentAmmoChanged)
                        {
                            projectileTypes[loadedProjectileIndex].uiBackground.setSprite(selectedProjectileUITexture);
                            if (!projectileTypes[loadedProjectileIndex].secondaryFireAvailable)
                            {
                                projectileTypes[loadedProjectileIndex].uiMouseButtons.setSprite(primaryFireAvailableUITexture);
                            }
                            for (int i = 1; i < numberOfProjectileTypes; i++)
                            {
                                if (!projectileTypes[i].Equals(projectileTypes[loadedProjectileIndex]))
                                {
                                    if (projectileTypes[i].uiBackground.currentlyUsesSprite(selectedProjectileUITexture))
                                    {
                                        projectileTypes[i].uiBackground.setSprite(unselectedProjectileUITexture);
                                    }
                                    if (!projectileTypes[i].secondaryFireAvailable)
                                    {
                                        projectileTypes[i].uiMouseButtons.setSprite(null);
                                    }
                                }
                            }
                        }

                        player.move(playerDisplacement);
                    }
                    break;

                case GameState.mainMenu:
                    this.IsMouseVisible = true;
                    currentGameState = mainMenu.update(mouse);
                    break;

                case GameState.helpScreen:
                    this.IsMouseVisible = true;
                    currentGameState = helpScreen.update(mouse);
                    break;

                case GameState.levelSelectScreen:
                    this.IsMouseVisible = true;
                    currentGameState = levelSelectScreen.update(mouse);
                    if (currentGameState == GameState.gameInProgress)
                    {
                        resetLevel(currentLevelNumber);
                    }
                    break;

                case GameState.levelPackScreen:
                    this.IsMouseVisible = true;
                    currentGameState = levelPackSelectScreen.update(mouse);
                    break;

                default:
                    if (currentGameState == GameState.exitgame)
                    {
                        Exit();
                    }
                    break;

            }
            base.Update(gameTime);
        
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (currentGameState)
            {
            case GameState.gameInProgress:
                GraphicsDevice.Clear(Color.CornflowerBlue);

                GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true }; // allows drawing in 3D

                // DRAW 3D OBJECTS
                foreach (Actor t in terrain)
                {
                    t.draw(viewMatrix, projectionMatrix);
                }

                foreach (Actor f in floor)
                {
                    f.draw(viewMatrix, projectionMatrix);
                }
                
                foreach(Actor a in staticActors)
                {
                    a.draw(viewMatrix, projectionMatrix);
                }

                foreach (Actor h in hazards)
                {
                    h.draw(viewMatrix, projectionMatrix);
                }

                foreach (DartSpawner ds in dartSpawners)
                {
                    List<Hazard> darts = ds.getDartList();

                    foreach (Hazard dart in darts)
                    {
                        dart.draw(viewMatrix, projectionMatrix);
                    }
                }

                foreach (Projectile p in allProjectiles)
                {
                    p.draw(viewMatrix, projectionMatrix);
                }

                foreach (NPC g in guards)
                {
                    g.draw(viewMatrix, projectionMatrix);
                }

                foreach (ProjectileActivatedTrigger pat in projectileActivatedTriggers)
                {
                    pat.draw(viewMatrix, projectionMatrix);
                }

                foreach (TimeActivatedTrigger tat in timeActivatedTriggers)
                {
                    tat.draw(viewMatrix, projectionMatrix);
                }

                foreach (MovementActivatedTrigger mat in movementActivatedTriggers)
                {
                    mat.draw(viewMatrix, projectionMatrix);
                }

                // only draw player in birds eye view
                if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                {
                    player.draw(viewMatrix, projectionMatrix);
                }
                else // only draw roof in first person view
                {
                    foreach (Actor r in roof)
                    {
                        r.draw(viewMatrix, projectionMatrix);
                    }
                }

                goal.draw(viewMatrix, projectionMatrix);

                // DRAW UI
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                for (int i = 1; i < numberOfProjectileTypes; i++)
                {
                    projectileTypes[i].uiBackground.draw(spriteBatch);
                    if (!projectileTypes[i].uiMouseButtons.currentlyUsesSprite(null))
                    {
                        projectileTypes[i].uiMouseButtons.draw(spriteBatch);
                    }
                    projectileTypes[i].uiText.draw(spriteBatch);
                }

                viewUI.draw(spriteBatch);
                gameOverUI.draw(spriteBatch);
                goalFoundUI.draw(spriteBatch);

                spriteBatch.End();
                break;

            case GameState.mainMenu:
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                mainMenu.drawMainMenu(gameTime, spriteBatch);
                spriteBatch.End();
                break;

            case GameState.helpScreen:
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                helpScreen.drawHelpScreen(gameTime, spriteBatch);
                spriteBatch.End();
                break;

            case GameState.levelSelectScreen:
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                levelSelectScreen.drawLevelSelect(gameTime, spriteBatch);
                spriteBatch.End();
                break;

            case GameState.levelPackScreen:
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                levelPackSelectScreen.drawLevelPackScreen(gameTime, spriteBatch);
                spriteBatch.End();
                break;

            default:
                GraphicsDevice.Clear(Color.SandyBrown);
                break;
            }

            base.Draw(gameTime);
        }

        public void updateNPCInstructions(NPC npc, string instructionList)
        {
            int numOfInstructions = 0;
            string[] individualInstructions;
            List<Instruction> npcInstructionList = new List<Instruction>();
            List<Tile> npcPatrolPath = new List<Tile>();
            Vector2 lastTileCoordinates = npc.currentTile.coordinates;

            // if the last char of instructions isn't a comma then add one
            // needs to be this way for parsing reasons
            if (instructionList[instructionList.Length - 1] != ',')
            {
                instructionList += ",";
            }

            for (int i = 0; i < instructionList.Length; i++)
            {
                char c = instructionList[i];
                if (c == ',')
                {
                    numOfInstructions++;
                }
            }

            individualInstructions = instructionList.Split(',');

            for (int i = 0; i < numOfInstructions; i++)
            {
                Instruction newInstruction;
                newInstruction.type = individualInstructions[i][0];
                newInstruction.factor = int.Parse(individualInstructions[i].Substring(1));

                if (newInstruction.type == 'X' || newInstruction.type == 'Z')
                {
                    foreach(Tile t in levelTiles)
                    {
                        if ((newInstruction.type == 'X' && t.coordinates == new Vector2(lastTileCoordinates.X + newInstruction.factor, lastTileCoordinates.Y)) ||
                            (newInstruction.type == 'Z' && t.coordinates == new Vector2(lastTileCoordinates.X, lastTileCoordinates.Y + newInstruction.factor)))
                        {
                            npcPatrolPath.Add(t);
                            lastTileCoordinates = t.coordinates;
                            break;
                        }
                    }
                }
                npcInstructionList.Add(newInstruction);
            }

            npc.createInstructionList(npcInstructionList);
            npc.createPatrolPath(npcPatrolPath);
        }

        public void createDartSpawner(Vector3 position, bool initiallyActive, double? intervalTimer = null, float rotation = 0)
        {
            Actor dartSpawningWall = new Actor(dartWalls, new Vector3(position.X + (tileSize / 2), 0f, position.Z));
            Actor otherWall = new Actor(dartWalls, new Vector3(position.X - (tileSize / 2), 0f, position.Z));
            dartSpawningWall.changeYaw(MathHelper.ToRadians(180));

            dartSpawningWall.getModelData().resizeHitbox(new Vector3(0.5f, 1f, 1f));
            otherWall.getModelData().resizeHitbox(new Vector3(0.5f, 1f, 1f));

            terrain.Add(dartSpawningWall);
            terrain.Add(otherWall);

            DartSpawner newDartSpawner = new DartSpawner(floorModel, position, dartSpawningWall, otherWall, dart, initiallyActive, intervalTimer);

            newDartSpawner.changeYaw(MathHelper.ToRadians(rotation));

            dartSpawners.Add(newDartSpawner);
        }

        public Projectile createNewProjectile(ProjectileClassification classToAdd)
        {
            Projectile newProjectile;

            Vector3 projectilePos = player.position + new Vector3(0f, 100f, 0f);
            float projectileAngle = player.currentYawAngleDeg;

            switch (classToAdd)
            {
                case ProjectileClassification.pull:
                    newProjectile = new PullProjectile(pull.model, projectilePos, pull.moveSpeed, projectileAngle);
                    break;

                case ProjectileClassification.teleport:
                    newProjectile = new TeleportProjectile(teleport.model, projectilePos, teleport.moveSpeed, projectileAngle);
                    break;

                default:
                    newProjectile = new Projectile(ProjectileClassification.power, shock.model, projectilePos, shock.moveSpeed, projectileAngle);
                    break;
            }

            return newProjectile;
        }

        public static List<Level> loadLevelPack(string levelPackName)
        {
            List<Level> levelPack = new List<Level>();
            Level level = new Level();
            bool isFirstLevelOfPack = true;

            try
            {
                using (var stream = new FileStream(levelPacksFileLocation + "\\" + levelPackName, FileMode.Open))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

                            // start of a new level
                            if (line[0] == '/')
                            {
                                string levelName = line.Remove(0, 1);
                                level.name = levelName;

                                if (isFirstLevelOfPack)
                                {
                                    level.unlocked = true;
                                    isFirstLevelOfPack = false;
                                }

                                levelPack.Add(level);
                                level = new Level();
                            }

                            // line is part of same level
                            else
                            {
                                if (line[0] == '#')
                                {
                                    // # adds an actor to the level
                                    string levelActor = line.Remove(0, 1);
                                    level.actors.Add(levelActor);
                                }
                                else if (line[0] == '~')
                                {
                                    // ~ sets allowed projectiles
                                    string[] individualInstructions;
                                    string instructionLine = line.Remove(0, 1);

                                    for (int i = 0; i < instructionLine.Length; i++)
                                    {
                                        char c = instructionLine[i];
                                        if (c == ';')
                                        {
                                            level.numberOfProjectilesAllowed++;
                                        }
                                    }

                                    individualInstructions = instructionLine.Split(';');

                                    for (int i = 0; i < level.numberOfProjectilesAllowed; i++)
                                    {
                                        int projectileID = Convert.ToInt32(individualInstructions[i]);
                                        level.projectilesAllowed.Add(projectileID);
                                    }
                                }
                                else if (line[0] == '%')
                                {
                                    if (line[1] == 't')
                                    {
                                        level.unlocked = true;
                                    }
                                    else if (line[1] == 'f')
                                    {
                                        level.unlocked = false;
                                    }
                                }
                                else
                                {
                                    level.layout.Add(line);
                                }
                            }
                        }
                    }
                }

                currentLevelNumber = 0;
                currentLevelPack = levelPack;
                LevelSelect.changeLevelPack();
                
            }
            catch(FileNotFoundException)
            {
                levelPack = loadLevelPack("Default.txt");
            }
            return levelPack;
        }

        public void resetLevel(int levelNumber)
        {
            Mouse.SetPosition((int)screenCentreX, (int)screenCentreY);
            Level thisLevel = currentLevelPack[levelNumber];

            // reset projectiles allowed on level
            for (int i = 1; i < numberOfProjectileTypes; i++)
            {
                projectileTypes[i].allowedThisLevel = false;
            }
            foreach (int pa in thisLevel.projectilesAllowed)
            {
                projectileTypes[pa].allowedThisLevel = true;
            }
            player = new Player(playerModel, Vector3.Zero);
            terrain = new List<Actor>();
            floor = new List<Actor>();
            roof = new List<Actor>();
            staticActors = new List<Actor>();
            guards = new List<NPC>();
            hazards = new List<Hazard>();
            dartSpawners = new List<DartSpawner>();
            levelTiles = new List<Tile>();
            projectileActivatedTriggers = new List<ProjectileActivatedTrigger>();
            timeActivatedTriggers = new List<TimeActivatedTrigger>();
            movementActivatedTriggers = new List<MovementActivatedTrigger>();

            allProjectiles = new List<Projectile>();

            isLevelOver = false;
            gameOverUI.setActive(false);
            goalFoundUI.setActive(false);

            createLevel(thisLevel);

            for (int i = 1; i < numberOfProjectileTypes; i++)
            {
                Vector2 position = new Vector2(GraphicsDevice.Viewport.Width * ((float)(i) / (float)(numberOfProjectileTypes)), GraphicsDevice.Viewport.Height - 100f);

                projectileTypes[i].uiBackground = new UI(unselectedProjectileUITexture, position, new Vector2(3f, 3f), true);
                projectileTypes[i].uiMouseButtons = new UI(null, position, new Vector2(3f, 3f), true);
                if (projectileTypes[i].allowedThisLevel)
                {
                    projectileTypes[i].uiText = new UI(projectileTypes[i].uiTextTexture, position, new Vector2(3f, 3f), true);
                }
                else
                {
                    projectileTypes[i].uiText = new UI(unavailableProjectileUITexture, position, new Vector2(3f, 3f), true);
                }
            }

            gunLoaded = true;
            loadedProjectileIndex = 0;
            rightMouseButtonDown = false;

            player.setYawAngle(MathHelper.ToRadians(thisLevel.playerStartingAngle));
            goal.setYawAngle(MathHelper.ToRadians(thisLevel.goalAngle));
            Character.setMovementBlockers(terrain);
        }

        public void createLevel(Level level)
        {
            Tile newTile;
            float zPosition = 0f;

            List<string> levelTextRepresentation = level.layout;
            List<string> levelActorsTextRepresentation = level.actors;

            // check each row of level
            for (int i = 0; i < levelTextRepresentation.Count; i++)
            {
                // check each tile in the row
                for (int j = 0; j < levelTextRepresentation[i].Length; j++)
                {
                    float xPosition = (float)(tileSize * j);
                    Vector3 tilePosition = new Vector3(xPosition, 0f, zPosition);
                    char tileContents = levelTextRepresentation[i][j];

                    newTile.coordinates.X = j;
                    newTile.coordinates.Y = zPosition / tileSize;
                    newTile.centre = new Vector3(xPosition, 0f, zPosition);

                    roof.Add(new Actor(floorSegmentModel, new Vector3(tilePosition.X, tilePosition.Y + 300f, tilePosition.Z)));

                    if (tileContents != 'x') // x = hole in floor
                    {
                        floor.Add(new Actor(floorSegmentModel, tilePosition));
                        
                        switch (tileContents)
                        {
                            case 'O':
                                terrain.Add(new Actor(wall, tilePosition));
                                break;

                            //case 'E':
                            //    goal.setPosition(tilePosition);
                            //    break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        staticActors.Add(new Actor(spikePit, tilePosition));
                    }

                    levelTiles.Add(newTile);
                }

                zPosition += tileSize;
            }

            // check each actor
            for (int i = 0; i < levelActorsTextRepresentation.Count; i++)
            {
                string[] splitLine;
                Vector2 actorCoordinates;
                string[] actorCoordinatesString;
                char initialDirection;
                float initialAngle;
                Tile actorStartingTile = levelTiles[0];

                splitLine = levelActorsTextRepresentation[i].Split(';');

                actorCoordinatesString = splitLine[1].Split(',');
                actorCoordinates = new Vector2(Int32.Parse(actorCoordinatesString[0]), Int32.Parse(actorCoordinatesString[1]));

                foreach(Tile t in levelTiles)
                {
                    if (t.coordinates == actorCoordinates)
                    {
                        actorStartingTile = t;
                        break;
                    }
                }

                initialDirection = splitLine[2].ToCharArray()[0];

                // convert initial facing direction to an angle
                switch (initialDirection)
                {
                    case 'u':
                        initialAngle = 180;
                        break;
                    case 'l':
                        initialAngle = -90;
                        break;
                    case 'r':
                        initialAngle = 90;
                        break;
                    default:
                        //also case 'd'
                        initialAngle = 0;
                        break;
                }

                switch (splitLine[0].ToCharArray()[0])
                {
                    case 'P':
                        player.setPosition(actorStartingTile.centre);
                        level.playerStartingAngle = initialAngle;
                        break;

                    case 'E':
                        goal.setPosition(actorStartingTile.centre);
                        level.goalAngle = initialAngle;
                        break;

                    case 'H':
                        // Line Contents: (Type, Tile, Direction, Trigger Type, Trigger Tile, InitiallyActive, CanBeReactivated, ResetTimer, IntervalTimer)
                        bool initiallyActive;
                        bool canBeReactivated;
                        double? resetTimer, intervalTimer;
                        Tile triggerTile;
                        Vector2 triggerCoordinates;
                        string[] triggerCoordinatesString;

                        if (splitLine[5] == "T")
                        {
                            initiallyActive = true;
                        }
                        else
                        {
                            initiallyActive = false;
                        }

                        if (Double.Parse(splitLine[8]) == 0)
                        {
                            intervalTimer = null;
                        }
                        else
                        {
                            intervalTimer = Double.Parse(splitLine[8]);
                        }

                        createDartSpawner(actorStartingTile.centre, initiallyActive, intervalTimer, initialAngle);

                        if (splitLine[3] != "N")
                        {
                            triggerCoordinatesString = splitLine[4].Split(',');
                            triggerCoordinates = new Vector2(Int32.Parse(triggerCoordinatesString[0]), Int32.Parse(triggerCoordinatesString[1]));
                            triggerTile = levelTiles[0];

                            if (splitLine[6] == "T")
                            {
                                canBeReactivated = true;
                            }
                            else
                            {
                                canBeReactivated = false;
                            }

                            if (Double.Parse(splitLine[7]) == 0)
                            {
                                resetTimer = null;
                            }
                            else
                            {
                                resetTimer = Double.Parse(splitLine[7]);
                            }

                            foreach (Tile t in levelTiles)
                            {
                                if (t.coordinates == triggerCoordinates)
                                {
                                    triggerTile = t;
                                    break;
                                }
                            }

                            if (splitLine[3] == "P")
                            {
                                ProjectileActivatedTrigger pat = new ProjectileActivatedTrigger(projectileTriggerModel, triggerTile.centre, dartSpawners[dartSpawners.Count - 1], canBeReactivated, ProjectileClassification.power, intervalTimer, resetTimer);
                                projectileActivatedTriggers.Add(pat);
                            }
                            else if (splitLine[3] == "M")
                            {
                                MovementActivatedTrigger mat = new MovementActivatedTrigger(movementTriggerModel, triggerTile.centre, dartSpawners[dartSpawners.Count - 1], canBeReactivated, intervalTimer, resetTimer);
                                movementActivatedTriggers.Add(mat);
                            }
                        }
                        break;

                    case 'G':
                        // Line: (Type, Tile, Direction, Instruction List)
                        string mummyInstuctions;
                        NPC newMummy = new NPC(mummy, actorStartingTile, mummy.moveSpeed, initialAngle);
                        
                        if (splitLine.Length < 4)
                        {
                            mummyInstuctions = "W10,";
                        }
                        else if (splitLine[3] == "")
                        {
                            mummyInstuctions = "W10,";
                        }
                        else
                        {
                            mummyInstuctions = splitLine[3];
                        }

                        updateNPCInstructions(newMummy, mummyInstuctions);
                        guards.Add(newMummy);
                        break;

                    case 'B':
                        // Line: (Type, Tile, Direction, Instruction List)
                        string boulderInstuctions;
                        NPC newBoulder = new NPC(boulder, actorStartingTile, boulder.moveSpeed, initialAngle);
                        
                        if (splitLine.Length < 4)
                        {
                            boulderInstuctions = "W10,";
                        }
                        else if (splitLine[3] == "")
                        {
                            boulderInstuctions = "W10,";
                        }
                        else
                        {
                            boulderInstuctions = splitLine[3];
                        }

                        updateNPCInstructions(newBoulder, boulderInstuctions);
                        guards.Add(newBoulder);
                        break;
                }
            }

        }

        private void endLevel(Actor triggeringActor)
        {
            if (!isLevelOver)
            {
                isLevelOver = true;
                levelEndTime = DateTime.Now.TimeOfDay.TotalSeconds;

                if (triggeringActor != null)
                {
                    float targetAngleFromZero;
                    float newLookAtAngle;

                    Vector2 playerToActorVector = new Vector2(triggeringActor.position.X - player.position.X, triggeringActor.position.Z - player.position.Z);

                    targetAngleFromZero = MathHelper.ToDegrees((float)Math.Atan((double)Math.Abs(playerToActorVector.X) / (double)Math.Abs(playerToActorVector.Y)));

                    if (playerToActorVector.Y > 0)
                    {
                        if (playerToActorVector.X > 0)
                        {
                            newLookAtAngle = targetAngleFromZero;
                        }
                        else
                        {
                            newLookAtAngle = -targetAngleFromZero;
                        }
                    }
                    else
                    {
                        if (playerToActorVector.X > 0)
                        {
                            newLookAtAngle = 180 - targetAngleFromZero;
                        }
                        else
                        {
                            newLookAtAngle = -180 + targetAngleFromZero;
                        }
                    }
                    
                    player.normaliseAngle(ref newLookAtAngle);
                    player.AutoRotationTargetAngle = newLookAtAngle;
                }
                else
                {
                    player.AutoRotationTargetAngle = player.currentYawAngleDeg;
                }
            }
        }

        private void updateLevelOverState(bool goToNextLevel = false)
        {
            if (isLevelOver)
            {
                if (DateTime.Now.TimeOfDay.TotalSeconds > levelEndTime + levelResetTimer)
                {
                    if (goToNextLevel)
                    {
                        currentLevelNumber++;
                        if (currentLevelNumber >= currentLevelPack.Count)
                        {
                            currentGameState = GameState.mainMenu;
                            currentLevelNumber = 0;
                            return;
                        }

                        currentLevelPack[currentLevelNumber].unlocked = true;
                        LevelSelect.changeLevelPack();
                        resetLevel(currentLevelNumber);
                    }
                    else
                    {
                        resetLevel(currentLevelNumber);
                    }
                }
                else
                {
                    if (player.currentYawAngleDeg - player.AutoRotationTargetAngle <= -Actor.rotationSpeed || player.currentYawAngleDeg - player.AutoRotationTargetAngle >= Actor.rotationSpeed)
                    {
                        // new and current angle both on right of screen, or both on left of screen
                        if ((player.AutoRotationTargetAngle > 0 && player.currentYawAngleDeg > 0) || (player.AutoRotationTargetAngle <= 0 && player.currentYawAngleDeg <= 0))
                        {
                            // counter clockwise rotation
                            if (player.AutoRotationTargetAngle > player.currentYawAngleDeg)
                            {
                                player.changeYaw(MathHelper.ToRadians(Actor.rotationSpeed));
                            }
                            // clockwise rotation
                            else
                            {
                                player.changeYaw(MathHelper.ToRadians(-Actor.rotationSpeed));
                            }
                        }

                        // new angle on left, current on right
                        else if (player.AutoRotationTargetAngle <= 0 && player.currentYawAngleDeg > 0)
                        {
                            // clockwise if angle difference > 180, else counterclockwise
                            if (Math.Abs(player.AutoRotationTargetAngle) + player.currentYawAngleDeg >= 180)
                            {
                                player.changeYaw(MathHelper.ToRadians(Actor.rotationSpeed));
                            }
                            else
                            {
                                player.changeYaw(MathHelper.ToRadians(-Actor.rotationSpeed));
                            }
                        }
                        // new angle on right, current on left
                        else if (player.AutoRotationTargetAngle > 0 && player.currentYawAngleDeg <= 0)
                        {
                            // counter if angle difference > 180, else clockwise
                            if (Math.Abs(player.currentYawAngleDeg) + player.AutoRotationTargetAngle >= 180)
                            {
                                player.changeYaw(MathHelper.ToRadians(-Actor.rotationSpeed));
                            }
                            else
                            {
                                player.changeYaw(MathHelper.ToRadians(Actor.rotationSpeed));
                            }
                        }
                        player.normaliseAngle(ref player.currentYawAngleDeg);

                    }
                    else
                    {
                        
                    }
                }
            }
        }

        public static void setCurrentLevel(int levelNum, int numberOfLevels)
        {
            if (levelNum < numberOfLevels)
            {
                currentLevelNumber = levelNum;
            }
            else
            {
                currentLevelNumber = 0;
            }

            return;
        }

        public static string calculateLineBreaks(string initialString, float textAreaWidth, SpriteFont font)
        {
            string newString = "";
            List<string> allLines = new List<string>();
            string thisLine = "";
            string nextLineReversed = "";
            int numberOfLines = 0;


            foreach (char c in initialString.ToCharArray())
            {
                if (font.MeasureString(thisLine).X >= textAreaWidth)
                {
                    while (thisLine.ToCharArray()[thisLine.Length - 1] != ' ')
                    {
                        nextLineReversed += thisLine.ToCharArray()[thisLine.Length - 1];
                        thisLine = thisLine.Remove(thisLine.Length - 1, 1);
                    }
                    thisLine += "\n";
                    numberOfLines++;
                    allLines.Add(thisLine);
                    thisLine = "";

                    for (int i = nextLineReversed.Length - 1; i >= 0; i--)
                    {
                        thisLine += nextLineReversed.ToCharArray()[i];
                    }
                    nextLineReversed = "";
                }

                thisLine += c.ToString();
            }

            if (thisLine != "")
            {
                allLines.Add(thisLine);
            }

            foreach (string line in allLines)
            {
                newString += line;
            }

            return newString;
        }
    }
}
