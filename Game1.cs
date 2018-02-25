using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;


// TODO

// PRIORITY

// Main menu / Help Screen
// program in game over conditions - reset level
// program in level packs / multiple levels
// make goal send player to next level
// save progress through level pack - just add an 'unlocked' bool to each level
// load new level packs

// art and textures

// Boulder Hazard. Essentially replaces armoured guard. Has a patrol path, kills enemies / players it runs over
// could just make this an enemy with a very short vision radius
// A hazard similar to boulder hazard, but doesn't move unless triggered, then moves until it collides with something then stops. Used to block / unblock passages as well as kill

// SECONDARY
// set pull to pull things to tile it is on, instead of its position
// pulls things on same row/column as it only
// change model to demonstrate better

// NPCs need to find their way back on track after getting pulled away
// After move instruction is complete, check coordinates against where it should be in patrol path

// OTHER
// moving platforms (patrol like enemies)
    // would have to move things on top of them as well
    // if character.underfootHitbox collides with moving platform, then move character when platform moves





namespace Game1
{
    // struct applied to each enemy type, and dictates which projectile types affect them
    public struct ProjectileEffectiveness
    {
        public bool shock;
        public bool pull;
        public bool teleport;
        // swap? swaps two actors around
        // clear fog?
        // drone? overhead camera follows last fired drone projectile
        // power? Turns on electrical hazards or activates consoles - replace shock with this
        // teleport? moves player to projectile's position on right click
    }

    public struct EnemyStruct
    {
        public EnemyClassification classification;
        public ActorModel model;
        public ProjectileEffectiveness projectileEffectiveness;
        public int moveSpeed;
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
        /*
         * change to a class?
        */
        public Vector2 coordinates;
        public Vector3 centre;
        // public list<Actor> thingsOnTile;
        //public Tile nextTileNorth;
        //public Tile nextTileSouth;
        //public Tile nextTileEast;
        //public Tile nextTileWest;
    }

    public class Game1 : Game
    {
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

        public static Direction north, south, west, east, northWest, northEast, southWest, southEast;

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

        ActorModel cube;
        ActorModel playerModel;
        ActorModel goalModel;
        ActorModel wall;
        ActorModel electricBeams;
        ActorModel dart;
        ActorModel dartWalls;
        ActorModel gateWalls;
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

        EnemyStruct pawn;
        EnemyStruct armoured;

        Player player;
        Actor goal;

        List<string> levelLayout;
        List<string> levelActors;
        List<string> NPCInstructions;
        List<int> allowedProjectiles; 

        List<Tile> levelTiles;
        float tileSize;

        List<Actor> terrain;
        List<Actor> floor;
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

        struct Bounds
        {
            public float left;
            public float right;
            public float top;
            public float bottom;
        }

        Bounds levelBounds;

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

            northWest = new Direction(-135f, new Vector3(1f, 0f, -1f));
            west = new Direction(-90f, new Vector3(1f, 0f, 0f));
            southWest = new Direction(-45f, new Vector3(1f, 0f, 1f));
            south = new Direction(0f, new Vector3(0f, 0f, 1f));
            southEast = new Direction(45f, new Vector3(-1f, 0f, 1f));
            east = new Direction(90f, new Vector3(-1f, 0f, 0f));
            northEast = new Direction(135f, new Vector3(-1f, 0f, -1f));
            north = new Direction(180f, new Vector3(0f, 0f, -1f));

            pawn.classification = EnemyClassification.pawn;
            pawn.projectileEffectiveness.shock = true;
            pawn.projectileEffectiveness.pull = true;
            pawn.moveSpeed = 4;

            armoured.classification = EnemyClassification.armoured;
            armoured.projectileEffectiveness.shock = false;
            armoured.projectileEffectiveness.pull = true;
            armoured.moveSpeed = 4;

            none.classification = ProjectileClassification.none;
            none.moveSpeed = 0;
            none.allowedThisLevel = true;

            shock.classification = ProjectileClassification.shock;
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
            
            levelLayout = new List<string>();
            levelActors = new List<string>();
            NPCInstructions = new List<string>();
            allowedProjectiles = new List<int>();

            using (var stream = TitleContainer.OpenStream("LevelFile.txt"))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line[0] == '#')
                        {
                            // remove # sign
                            string levelActor = line.Remove(0, 1);
                            levelActors.Add(levelActor);
                            //NPCInstructions.Add(instructionLine);
                        }
                        else if (line[0] == '~')
                        {
                            int numOfProjectilesAllowed = 0;
                            string[] individualInstructions;
                            string instructionLine = line.Remove(0, 1);

                            allowedProjectiles.Clear();

                            for (int i = 0; i < instructionLine.Length; i++)
                            {
                                char c = instructionLine[i];
                                if (c == ';')
                                {
                                    numOfProjectilesAllowed++;
                                }
                            }

                            individualInstructions = instructionLine.Split(';');

                            for (int i = 0; i < numOfProjectilesAllowed; i++)
                            {
                                int projectileID = Convert.ToInt32(individualInstructions[i]);

                                switch(projectileID)
                                {
                                    case 1:
                                        shock.allowedThisLevel = true;
                                        break;
                                    case 2:
                                        pull.allowedThisLevel = true;
                                        break;
                                    case 3:
                                        teleport.allowedThisLevel = true;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            levelLayout.Add(line);
                        }
                    }
                }
            }

            Mouse.SetPosition((int)screenCentreX, (int)screenCentreY);
            this.IsMouseVisible = false;

            south.setAdjacentDirections(east, southEast, southWest, west, north);
            southEast.setAdjacentDirections(northEast, east, south, southWest, northWest);
            east.setAdjacentDirections(north, northEast, southEast, south, west);
            northEast.setAdjacentDirections(northWest, north, east, southEast, southWest);
            north.setAdjacentDirections(west, northWest, northEast, east, south);
            northWest.setAdjacentDirections(southWest, west, north, northEast, southEast);
            west.setAdjacentDirections(south, southWest, northWest, north, east);
            southWest.setAdjacentDirections(southEast, south, west, northWest, northEast);
            
            base.Initialize();

            player = new Player(playerModel, new Vector3(0f, 0f, 0f), 6);
            goal = new Actor(goalModel, new Vector3(0f, 0f, 150f));

            terrain = new List<Actor>();
            floor = new List<Actor>();
            guards = new List<NPC>();
            hazards = new List<Hazard>();
            dartSpawners = new List<DartSpawner>();
            levelTiles = new List<Tile>();
            projectileActivatedTriggers = new List<ProjectileActivatedTrigger>();
            timeActivatedTriggers = new List<TimeActivatedTrigger>();
            movementActivatedTriggers = new List<MovementActivatedTrigger>();

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

            for (int i = 1; i < numberOfProjectileTypes; i++)
            {
                Vector2 position = new Vector2(GraphicsDevice.Viewport.Width * ((float)(i) / (float)(numberOfProjectileTypes)), GraphicsDevice.Viewport.Height - 100f);
                projectileTypes[i].uiBackground = new UI(unselectedProjectileUITexture, position, 3f, true);
                projectileTypes[i].uiMouseButtons = new UI(null, position, 3f, true);
                if (projectileTypes[i].allowedThisLevel)
                {
                    projectileTypes[i].uiText = new UI(projectileTypes[i].uiTextTexture, position, 3f, true);
                }
                else
                {
                    projectileTypes[i].uiText = new UI(unavailableProjectileUITexture, position, 3f, true);
                }
            }

            viewUI = new UI(viewUIFirstPersonTexture, new Vector2(viewUIFirstPersonTexture.Width * (2), viewUIFirstPersonTexture.Height * (2)), 3f, true);
            gameOverUI = new UI(gameOverTexture, new Vector2(screenCentreX, screenCentreY), 2f, false);
            goalFoundUI = new UI(goalFoundTexture, new Vector2(screenCentreX, screenCentreY), 2f, false);

            gunLoaded = true;
            loadedProjectileIndex = 0;
            rightMouseButtonDown = false;

            allProjectiles = new List<Projectile>();

            levelBounds.top = -GraphicsDevice.Viewport.Height + wall.boxSize.Y;
            levelBounds.bottom = GraphicsDevice.Viewport.Height - wall.boxSize.Y;
            levelBounds.left = -GraphicsDevice.Viewport.Width + wall.boxSize.X;
            levelBounds.right = GraphicsDevice.Viewport.Height - wall.boxSize.X;

            createLevel(levelLayout, levelActors);

            // if less instructions than guards, add instructions to stand still
            //while (NPCInstructions.Count < guards.Count)
            //{
            //    NPCInstructions.Add("W10;");
            //}

            //// add instructions to guards
            //for (int i = 0; i < guards.Count; i++)
            //{
            //    if (NPCInstructions[i] == "")
            //    {
            //        NPCInstructions[i] = "W10;";
            //    }
            //    updateNPCInstructions(guards[i], NPCInstructions[i]);
                
            //}
            // send terrain data to characters
            Character.setMovementBlockers(terrain);
        }

        protected override void LoadContent()
        {
            // To draw 2D UI elements
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ActorModel.setWorldMatrix(worldMatrix);

            cube = new ActorModel(Content.Load<Model>("Cube"), true, true);
            playerModel = new ActorModel(Content.Load<Model>("PlayerHat"), false, false);
            goalModel = new ActorModel(Content.Load<Model>("Goal"), false, false);
            wall = new ActorModel(Content.Load<Model>("WallSegment"), true, true);
            gateWalls = new ActorModel(Content.Load<Model>("ElectricGateWall"), true, true);
            floorModel = new ActorModel(Content.Load<Model>("Floor"), false, false);
            floorSegmentModel = new ActorModel(Content.Load<Model>("FloorSegment"), false, false);
            movementTriggerModel = new ActorModel(Content.Load<Model>("FloorTrigger"), false, false);
            projectileTriggerModel = new ActorModel(Content.Load<Model>("ProjectileTrigger"), true, true);

            electricBeams = new ActorModel(Content.Load<Model>("ElectricGateBeams"), false, false);
            dart = new ActorModel(Content.Load<Model>("Dart"), false, false);
            dartWalls = new ActorModel(Content.Load<Model>("DartWall"), true, true);

            shock.model = new ActorModel(Content.Load<Model>("ShockProjectile"), false, false);
            pull.model = new ActorModel(Content.Load<Model>("PullProjectile"), false, false);
            teleport.model = new ActorModel(Content.Load<Model>("PortProjectile"), false, false);

            pawn.model = new ActorModel(Content.Load<Model>("Mummy"), true, true);

            armoured.model = new ActorModel(Content.Load<Model>("ArmouredRobotUnalert"), true, true);

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
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            Vector3 playerDisplacement = new Vector3(0f, 0f, 0f);
            bool onFloor;

            List<Actor> enemyVisionBlockers = new List<Actor>();
            List<Projectile> activeProjectiles = new List<Projectile>();
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            Projectile activePullProjectile = null;

            hazards.Clear();

            // check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
                Exit();

            // update list of enemy vision blockers
            foreach (Actor t in terrain)
            {
                t.updateHitboxes();
                enemyVisionBlockers.Add(t);
            }
            foreach(NPC guard in guards)
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
                goalFoundUI.setActive(true);
            }

            player.updateHitboxes();
            foreach (Hazard h in hazards)
            {
                // hazard / player collision
                if (h.collidesWith(player))
                {
                    gameOverUI.setActive(true);
                }
            }

            foreach(Actor f in floor)
            {
                if (f.collisionHitbox.Intersects(player.underfootHitbox))
                {
                    onFloor = true;
                    break;
                }
            }

            if (!onFloor)
            {
                player.Falling = true;
            }

            // guard updates
            foreach (NPC g in guards)
            {
                if (!g.isDead())
                {
                    onFloor = false;

                    foreach(Actor f in floor)
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
                    }

                    g.update(enemyVisionBlockers);

                    // detect player if collide with enemy
                    if (player.collidesWith(g))
                    {
                        g.detectPlayer();
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
                                
                                gameOverUI.setActive(true);

                                /*
                                 * 
                                 * 
                                 * 
                                 * 
                                 * 
                                 * npc reaction to seeing player
                                 * 
                                 * 
                                 * 
                                 * 
                                 * 
                                 */
                            }
                        }
                    }

                    // kill guards who collide with hazards
                    foreach(Hazard h in hazards)
                    {
                        if (h.collidesWith(g))
                        {
                            g.kill();
                        }
                    }
                }
            }


            // hazard and variable obstacles updates
            foreach(DartSpawner ds in dartSpawners)
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

                //if (pj.getClassification() == ProjectileClassification.pull)
                //{
                //    foreach(Projectile otherPj in allProjectiles)
                //    {
                //        if (otherPj.Equals(pj))
                //        {
                //            continue;
                //        }

                //        if (otherPj.collisionHitbox.Intersects(pj.collisionHitbox))
                //        {
                //            otherPj.requiresDeletion = true;
                //            continue;
                //        }

                //        if (pj.hasActionStarted() && pj.actorInActionRadius(otherPj.collisionHitbox))
                //        {
                //            otherPj.move(new Vector3(pj.position.X - otherPj.position.X, 0f, pj.position.Z - otherPj.position.Z) / 60);
                //        }
                //    }
                //}

                if (activePullProjectile == null)
                {
                    pj.move();
                }
                else
                {
                    if (!pj.Equals(activePullProjectile) && pj.collidesWith(activePullProjectile))
                    {
                        destroyProjectile = true;
                    }
                    else if (!pj.Equals(activePullProjectile) && activePullProjectile.hasActionStarted() && activePullProjectile.actorInActionRadius(pj.collisionHitbox))
                    {
                        /*
                         * 
                         * 
                         * 
                         * don't move at all if in contact with minimum range, or delete immediately
                         * 
                         * 
                         * 
                         */
                        pj.move(new Vector3(activePullProjectile.position.X - pj.position.X, 0f, activePullProjectile.position.Z - pj.position.Z) / 60);
                    }
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
                        if (pj.getClassification() == ProjectileClassification.shock && g.isEffectedBy(pj.getClassification()))
                        {
                            //g.kill();
                            destroyProjectile = true;
                        }

                        if (pj.getClassification() == ProjectileClassification.pull && !pj.hasActionStarted())
                        {
                            if (pj.hasNoParentActor())
                            {
                                g.attachNewActor(pj);
                            }
                            pj.MovementBlocked = true;
                        }

                        break;
                    }

                    // PULL PROJECTILE SPECIFICS
                    if (pj.getClassification() == ProjectileClassification.pull && g.isEffectedBy(pj.getClassification()) && pj.hasActionStarted())
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
                        if (pj.getClassification() == ProjectileClassification.pull)
                        {
                            if (pj.hasNoParentActor())
                            {
                                t.attachNewActor(pj);
                            }
                            pj.MovementBlocked = true;
                        }

                        else
                        {
                            destroyProjectile = true;
                        }

                        break;
                    }
                }

                foreach (ProjectileActivatedTrigger pat in projectileActivatedTriggers)
                {
                    if (pj.collidesWith(pat))
                    {
                        pat.hitByProjectile(pj.getClassification());
                        destroyProjectile = true;
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
                else
                {
                    pj.detachFromParentActor();
                }
            }

            // replace list of projectiles with list of projectiles that didn't collide with anything
            allProjectiles.Clear();
            allProjectiles = activeProjectiles;

            // check triggers

            foreach(MovementActivatedTrigger mat in movementActivatedTriggers)
            {
                mat.checkCurrentlyCollidingCharacter();
                mat.checkResetTimer();

                if (mat.collidesWith(player))
                {
                    mat.collisionWithCharacter(player);
                }

                foreach(NPC g in guards)
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

            // STRATEGIC VIEW
            if (keyboard.IsKeyDown(Keys.Tab))
            {
                viewUI.setSprite(viewUIBirdsEyeTexture);

                overheadCamTarget = player.position;
                overheadCamPosition = new Vector3(player.position.X, 1000f, player.position.Z + 1f);
                overheadProjectionMatrix = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width * 2, GraphicsDevice.Viewport.Height * 2, 0f, 3000f);
                overheadViewMatrix = Matrix.CreateLookAt(overheadCamPosition, overheadCamTarget, new Vector3(0f, 1f, 0f));

                projectionMatrix = overheadProjectionMatrix;
                viewMatrix = overheadViewMatrix;
                

                /* 
                 * 
                 * 
                 * shrink Y-scale of walls to help player visibility
                 * 
                 * 
                 */
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

                player.move(playerDisplacement);

                // choose ammo

                if ((keyboard.IsKeyDown(Keys.D1) || keyboard.IsKeyDown(Keys.NumPad1)) && shock.allowedThisLevel)
                {
                    currentAmmoChanged = true;
                    loadedProjectileIndex = shock.index;
                }
                if ((keyboard.IsKeyDown(Keys.D2) || keyboard.IsKeyDown(Keys.NumPad2)) && pull.allowedThisLevel)
                {
                    currentAmmoChanged = true;
                    loadedProjectileIndex = pull.index;
                }

                if((keyboard.IsKeyDown(Keys.D3) || keyboard.IsKeyDown(Keys.NumPad3)) && teleport.allowedThisLevel)
                {
                    currentAmmoChanged = true;
                    loadedProjectileIndex = teleport.index;
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
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true }; // allows drawing in 3D

            // DRAW 3D OBJECTS
            foreach (Actor t in terrain)
            {
                t.draw(viewMatrix, projectionMatrix);
            }

            foreach(Actor f in floor)
            {
                f.draw(viewMatrix, projectionMatrix);
            }

            foreach(Actor h in hazards)
            {
                h.draw(viewMatrix, projectionMatrix);
            }

            foreach(DartSpawner ds in dartSpawners)
            {
                List<Hazard> darts = ds.getDartList();

                foreach(Hazard dart in darts)
                {
                    dart.draw(viewMatrix, projectionMatrix);
                }
            }

            foreach(Projectile p in allProjectiles)
            {
                p.draw(viewMatrix, projectionMatrix);
            }

            foreach(NPC g in guards)
            {
                g.draw(viewMatrix, projectionMatrix);
            }

            foreach(ProjectileActivatedTrigger pat in projectileActivatedTriggers)
            {
                pat.draw(viewMatrix, projectionMatrix);
            }

            foreach(TimeActivatedTrigger tat in timeActivatedTriggers)
            {
                tat.draw(viewMatrix, projectionMatrix);
            }

            foreach(MovementActivatedTrigger mat in movementActivatedTriggers)
            {
                mat.draw(viewMatrix, projectionMatrix);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Tab))
            {
                player.draw(viewMatrix, projectionMatrix);
            }

            goal.draw(viewMatrix, projectionMatrix);


            // DRAW UI
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            for (int i = 1; i < numberOfProjectileTypes; i++)
            {
                projectileTypes[i].uiBackground.draw(spriteBatch);
                projectileTypes[i].uiText.draw(spriteBatch);
                if (!projectileTypes[i].uiMouseButtons.currentlyUsesSprite(null))
                {
                    projectileTypes[i].uiMouseButtons.draw(spriteBatch);
                }
            }

            viewUI.draw(spriteBatch);
            gameOverUI.draw(spriteBatch);
            goalFoundUI.draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void updateNPCInstructions(NPC npc, string instructionList)
        {
            int numOfInstructions = 0;
            string[] individualInstructions;
            List<Instruction> npcInstructionList = new List<Instruction>();
            List<Tile> npcPatrolPath = new List<Tile>();
            Vector2 lastTileCoordinates = npc.currentTile.coordinates;

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

        //public Tile getTileData(Vector2 tileCoordinates)
        //{
        //    foreach(Tile t in levelTiles)
        //    {
        //        if (t.coordinates.X == tileCoordinates.X && t.coordinates.Y == tileCoordinates.Y)
        //        {
        //            return t;
        //        }
        //    }

        //    /*
        //     * 
        //     * 
        //     * 
        //     * change this to return tile that the relevant character is standing on
        //     * 
        //     * 
        //     * 
        //     */
        //    return levelTiles[0];
        //}

        // walls have square bases so don't need rotation 

        //public void createWall(Vector3 position, float rotation = 0)
        //{
        //    Actor newWall = new Actor(wall, position);

        //    newWall.changeYaw(MathHelper.ToRadians(rotation));

        //    terrain.Add(newWall);
        //}

        public void createDartSpawner(Vector3 position, bool initiallyActive, double? intervalTimer = null, float rotation = 0)
        {
            Actor dartSpawningWall = new Actor(dartWalls, new Vector3(position.X + (tileSize / 2), 0f, position.Z));
            Actor otherWall = new Actor(dartWalls, new Vector3(position.X - (tileSize / 2), 0f, position.Z));
            dartSpawningWall.changeYaw(MathHelper.ToRadians(180));

            dartSpawningWall.getModelData().resizeHitbox(new Vector3(0.5f, 1f, 1f));
            otherWall.getModelData().resizeHitbox(new Vector3(0.5f, 1f, 1f));

            terrain.Add(dartSpawningWall);
            terrain.Add(otherWall);

            DartSpawner newDartSpawner = new DartSpawner(floorModel, position, dartSpawningWall, otherWall, dart);

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
                    newProjectile = new Projectile(ProjectileClassification.shock, shock.model, projectilePos, shock.moveSpeed, projectileAngle);
                    break;
            }

            return newProjectile;
        }

        public void createLevel(List<string> levelTextRepresentation, List<string> levelActorsTextRepresentation)
        {
            Tile newTile;
            float zPosition = 0f;

            for (int i = 0; i < levelTextRepresentation.Count; i++)
            {
                //if (levelTextRepresentation[i].Length % 2 != 0)
                //{
                //    levelTextRepresentation[i] += " ";
                //}

                for (int j = 0; j < levelTextRepresentation[i].Length; j++)
                {
                    float xPosition = (float)(tileSize * j);
                    Vector3 tilePosition = new Vector3(xPosition, 0f, zPosition);
                    char tileContents = levelTextRepresentation[i][j];
                    //char initialDirection = levelTextRepresentation[i][j + 1];
                    //float initialAngle;

                    newTile.coordinates.X = j;
                    newTile.coordinates.Y = zPosition / tileSize;
                    newTile.centre = new Vector3(xPosition, 0f, zPosition);

                    

                    if (tileContents != 'x') // x = hole in floor
                    {
                        floor.Add(new Actor(floorSegmentModel, tilePosition));

                        switch (tileContents)
                        {
                            case 'O':
                                terrain.Add(new Actor(wall, tilePosition));
                                break;

                            //case 'H':
                            //    createShockGate(tilePosition, true, initialAngle);
                            //    break;

                            //case 'h':
                            //    createShockGate(tilePosition, false, initialAngle);
                            //    break;

                            //case 'P':
                            //    player.setPosition(tilePosition);
                            //    player.changeYaw(MathHelper.ToRadians(initialAngle));
                            //    break;

                            //case 'G':
                            //    guards.Add(new NPC(pawn, newTile, pawn.moveSpeed, initialAngle));
                            //    break;

                            case 'E':
                                goal.setPosition(tilePosition);
                                break;

                            default:
                                break;
                        }
                    }

                    levelTiles.Add(newTile);
                }

                zPosition += tileSize;
            }

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
                        player.changeYaw(MathHelper.ToRadians(initialAngle));
                        break;

                    case 'H':
                        // Line: (Type, Tile, Direction, Trigger Type, Trigger Tile, InitiallyActive, CanBeReactivated, ResetTimer, IntervalTimer)
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

                        /*
                         * 
                         * 
                         * change this to dart gate
                         * 
                         * 
                         */
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
                                ProjectileActivatedTrigger pat = new ProjectileActivatedTrigger(projectileTriggerModel, triggerTile.centre, dartSpawners[dartSpawners.Count - 1], canBeReactivated, ProjectileClassification.shock, intervalTimer, resetTimer);
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
                        string newGuardInstuctions;
                        NPC newPawnGuard = new NPC(pawn, actorStartingTile, pawn.moveSpeed, initialAngle);
                        guards.Add(newPawnGuard);
                        if (splitLine.Length < 4)
                        {
                            newGuardInstuctions = "W10,";
                        }
                        else if (splitLine[3] == "")
                        {
                            newGuardInstuctions = "W10,";
                        }
                        else
                        {
                            newGuardInstuctions = splitLine[3];
                        }

                        updateNPCInstructions(newPawnGuard, newGuardInstuctions);
                        break;
                }
            }

        }

        //public void buildTestLevel()
        //{
        //    createShockGate(new Vector3(500f, 0f, -300f));
        //    //createWall(new Vector3(500f, 0f, -125f));

        //    guards.Add(new NPC(pawn, new Vector3(-500f, 0f, -500f), pawn.moveSpeed));
        //    guards.Add(new NPC(armoured, new Vector3(500f, 0f, -500f), armoured.moveSpeed));
        //    guards.Add(new NPC(pawn, new Vector3(-800f, 0f, -500f), pawn.moveSpeed));
        //}

        //public void buildStandardLevel()
        //{
        //    /// Create a blank level with boundary walls and a floor
        //    // horizontal terrain creation
        //    for (int i = 0; i < graphics.PreferredBackBufferWidth + wall.boxSize.X; i += (int)wall.boxSize.X)
        //    {
        //        createWall(new Vector3((float)i, 0f, GraphicsDevice.Viewport.Height + wall.boxExtents.Y));
        //        createWall(new Vector3((float)i, 0f, -GraphicsDevice.Viewport.Height));

        //        // create terrain in negative direction
        //        // ignore if i==0, otherwise 2 objects created in same place
        //        if (i != 0)
        //        {
        //            createWall(new Vector3((float)-i, 0f, GraphicsDevice.Viewport.Height + wall.boxExtents.Y));
        //            createWall(new Vector3((float)-i, 0f, -GraphicsDevice.Viewport.Height));
        //        }
        //    }

        //    // vertical terrain creation
        //    for (int i = 0; i < graphics.PreferredBackBufferHeight + wall.boxSize.X; i += (int)wall.boxSize.X)
        //    {
        //        createWall(new Vector3(-GraphicsDevice.Viewport.Width, 0f, (float)i), 90);
        //        createWall(new Vector3(GraphicsDevice.Viewport.Width, 0f, (float)i), 90);

        //        // create terrain in negative direction
        //        // ignore if i==0, otherwise 2 objects created in same place
        //        if (i != 0)
        //        {
        //            createWall(new Vector3(-GraphicsDevice.Viewport.Width, 0f, (float)-i), 90);
        //            createWall(new Vector3(GraphicsDevice.Viewport.Width, 0f, (float)-i), 90);
        //        }
        //    }

        //    floor = new Actor(floorModel, new Vector3(0f, 0f, 0f));
        //}
    }
}
