using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


// TODO
// FIX PULL PROJECTILES STAYING IN PLAY AFTER USE
// Fix: Enemies die when colliding with the sides of a hazard - hitbox too big
// Level creation


namespace Game1
{
    // struct applied to each enemy type, and dictates which projectile types affect them
    struct ProjectileEffectiveness
    {
        public bool shock;
        public bool pull;
    }

    struct EnemyStruct
    {
        public EnemyClassification classification;
        public ActorModel unalertModel;
        public ActorModel alertModel;
        public ProjectileEffectiveness projectileEffectiveness;
        public int moveSpeed;
    }

    struct ProjectileStruct
    {
        public ProjectileClassification classification;
        public ActorModel model;
        public int moveSpeed;
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

        public static Direction up, down, left, right, upLeft, upRight, downLeft, downRight;

        ActorModel cube;
        ActorModel playerModel;
        ActorModel wall;
        ActorModel electricBeams;
        ActorModel gateWalls;
        ActorModel floorModel;

        Texture2D shockUnselectedTexture;
        Texture2D shockSelectedTexture;
        Texture2D pullUnselectedTexture;
        Texture2D pullSelectedTexture;
        Texture2D gameOverTexture;

        ProjectileStruct shock;
        ProjectileStruct pull;

        EnemyStruct pawn;
        EnemyStruct armoured;

        Player player;
        List<Actor> terrain;
        List<Projectile> allProjectiles;
        List<NPC> guards;
        List<Hazard> hazards;

        Actor floor;

        ProjectileStruct loadedProjectile;

        UI shockUI;
        UI pullUI;
        UI gameOverUI;
        
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

            upLeft = new Direction(-135f, new Vector3(1f, 0f, -1f));
            left = new Direction(-90f, new Vector3(1f, 0f, 0f));
            downLeft = new Direction(-45f, new Vector3(1f, 0f, 1f));
            down = new Direction(0f, new Vector3(0f, 0f, 1f));
            downRight = new Direction(45f, new Vector3(-1f, 0f, 1f));
            right = new Direction(90f, new Vector3(-1f, 0f, 0f));
            upRight = new Direction(135f, new Vector3(-1f, 0f, -1f));
            up = new Direction(180f, new Vector3(0f, 0f, -1f));

            Mouse.SetPosition((int)screenCentreX, (int)screenCentreY);
            this.IsMouseVisible = false;

            down.setAdjacentDirections(right, downRight, downLeft, left, up);
            downRight.setAdjacentDirections(upRight, right, down, downLeft, upLeft);
            right.setAdjacentDirections(up, upRight, downRight, down, left);
            upRight.setAdjacentDirections(upLeft, up, right, downRight, downLeft);
            up.setAdjacentDirections(left, upLeft, upRight, right, down);
            upLeft.setAdjacentDirections(downLeft, left, up, upRight, downRight);
            left.setAdjacentDirections(down, downLeft, upLeft, up, right);
            downLeft.setAdjacentDirections(downRight, down, left, upLeft, upRight);
            
            base.Initialize();

            player = new Player(playerModel, new Vector3(0f, 0f, 0f), 6);

            terrain = new List<Actor>();

            guards = new List<NPC>();

            hazards = new List<Hazard>();

            pawn.classification = EnemyClassification.pawn;
            pawn.projectileEffectiveness.shock = true;
            pawn.projectileEffectiveness.pull = true;
            pawn.moveSpeed = 6;

            armoured.classification = EnemyClassification.armoured;
            armoured.projectileEffectiveness.shock = false;
            armoured.projectileEffectiveness.pull = true;
            armoured.moveSpeed = 4;

            shock.classification = ProjectileClassification.shock;
            shock.moveSpeed = 16;
            shockUI = new UI(shockSelectedTexture, new Vector2(screenCentreX - 100f, GraphicsDevice.Viewport.Height - 100f), 3f, true);

            pull.classification = ProjectileClassification.pull;
            pull.moveSpeed = 12;
            pullUI = new UI(pullUnselectedTexture, new Vector2(screenCentreX + 100f, GraphicsDevice.Viewport.Height - 100f), 3f, true);

            gameOverUI = new UI(gameOverTexture, new Vector2(screenCentreX, screenCentreY), 2f, false);

            gunLoaded = true;
            loadedProjectile = shock;

            allProjectiles = new List<Projectile>();

            levelBounds.top = -GraphicsDevice.Viewport.Height + wall.boxSize.Y;
            levelBounds.bottom = GraphicsDevice.Viewport.Height - wall.boxSize.Y;
            levelBounds.left = -GraphicsDevice.Viewport.Width + wall.boxSize.X;
            levelBounds.right = GraphicsDevice.Viewport.Height - wall.boxSize.X;

            buildStandardLevel();
            buildTestLevel();
        }

        protected override void LoadContent()
        {
            // To draw 2D UI elements
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ActorModel.setWorldMatrix(worldMatrix);

            //cube = new ActorType(Content.Load<Model>("Cube"));
            playerModel = new ActorModel(Content.Load<Model>("PlayerTemp"), false, false);
            wall = new ActorModel(Content.Load<Model>("WallSegment"), true, true);
            gateWalls = new ActorModel(Content.Load<Model>("ElectricGateWall"), true, true);
            floorModel = new ActorModel(Content.Load<Model>("Floor"), false, false);

            electricBeams = new ActorModel(Content.Load<Model>("ElectricGateBeams"), false, false);

            shock.model = new ActorModel(Content.Load<Model>("ShockProjectile"), false, false);
            pull.model = new ActorModel(Content.Load<Model>("PullProjectile"), false, false);

            pawn.unalertModel = new ActorModel(Content.Load<Model>("RobotUnalert"), true, true);
            pawn.alertModel = new ActorModel(Content.Load<Model>("RobotAlert"), true, true);

            armoured.unalertModel = new ActorModel(Content.Load<Model>("ArmouredRobotUnalert"), true, true);
            armoured.alertModel = new ActorModel(Content.Load<Model>("ArmouredRobotAlert"), true, true);

            shockSelectedTexture = Content.Load<Texture2D>("UIShockSelected");
            shockUnselectedTexture = Content.Load<Texture2D>("UIShockUnselected");
            pullSelectedTexture = Content.Load<Texture2D>("UIPullSelected");
            pullUnselectedTexture = Content.Load<Texture2D>("UIPullUnselected");
            gameOverTexture = Content.Load<Texture2D>("UIGameOver");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            List<Actor> activeActors = new List<Actor>();
            List<Projectile> activeProjectiles = new List<Projectile>();
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            // check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
                Exit();

            // store all actors that can interact with others in the same list
            foreach (Actor t in terrain)
            {
                t.updateHitboxes();
                activeActors.Add(t);
            }
            foreach(NPC n in guards)
            {
                activeActors.Add(n);
            }
            foreach(Actor h in hazards)
            {
                h.updateHitboxes();
                activeActors.Add(h);
            }

            // player updates
            player.updateHitboxes();
            foreach (Hazard h in hazards)
            {
                // hazard / player collision
                if (h.collisionHitbox.Intersects(player.collisionHitbox))
                {
                    gameOverUI.setActive(true);
                }
            }

            // guard updates
            foreach (NPC g in guards)
            {
                if (!g.isDead())
                {
                    g.update(activeActors);

                    // detect player if collide with enemy
                    if (player.collisionHitbox.Intersects(g.collisionHitbox))
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
                            }
                        }
                    }

                    // kill guards who collide with hazards
                    foreach(Hazard h in hazards)
                    {
                        if (h.collisionHitbox.Intersects(g.collisionHitbox))
                        {
                            g.kill();
                        }
                    }
                }
            }
            
            // projectile updates
            foreach(Projectile pj in allProjectiles)
            {
                bool destroyProjectile = false;

                pj.move();
                pj.updateHitboxes();

                if (pj.requiresDeletion)
                {
                    destroyProjectile = true;
                }

                // projectile / guard collision
                foreach (NPC g in guards)
                {
                    if (pj.collisionHitbox.Intersects(g.collisionHitbox) && !g.isDead())
                    {
                        if (pj.getClassification() == ProjectileClassification.shock && g.isEffectedBy(pj.getClassification()))
                        {
                            g.kill();
                        }
                        destroyProjectile = true;
                        break;
                    }

                    // PULL PROJECTILE SPECIFICS
                    if (pj.getClassification() == ProjectileClassification.pull && g.isEffectedBy(pj.getClassification()) && pj.hasActionStarted())
                    {
                        // in range of enemy when activated
                        if (pj.enemyInActionRadius(g.collisionHitbox))
                        {
                           g.move(new Vector3(pj.position.X - g.position.X, 0f, pj.position.Z - g.position.Z) / 60, terrain);
                        }
                    }

                }

                // projectile / terrain collision
                foreach (Actor t in terrain)
                {
                    if (pj.collisionHitbox.Intersects(t.collisionHitbox))
                    {
                        destroyProjectile = true;
                        break;
                    }
                }

                // list of projectiles that don't need destroying
                if (destroyProjectile == false)
                    activeProjectiles.Add(pj);
            }

            // replace list of projectiles with list of projectiles that didn't collide with anything
            allProjectiles.Clear();
            allProjectiles = activeProjectiles;


            // STRATEGIC VIEW
            if (keyboard.IsKeyDown(Keys.Tab))
            {
                projectionMatrix = overheadProjectionMatrix;
                viewMatrix = overheadViewMatrix;
            }

            // FIRST PERSON VIEW
            else
            {
               // MOUSE INPUT
               if (mouse.X != screenCentreX)
                { 
                    player.changeYaw(MathHelper.ToRadians(screenCentreX - mouse.X) / 3);
                }

                Mouse.SetPosition((int)screenCentreX, (int)screenCentreY); // set mouse to centre


                // fire gun
                if (mouse.LeftButton == ButtonState.Pressed && gunLoaded)
                {
                    gunLoaded = false;
                    allProjectiles.Add(createNewProjectile(loadedProjectile.classification));
                }
                // mouse button must be released in between each shot
                if (mouse.LeftButton == ButtonState.Released && !gunLoaded)
                {
                    gunLoaded = true;
                }

                // action button
                if (mouse.RightButton == ButtonState.Pressed)
                {
                    foreach (Projectile p in allProjectiles)
                    {
                        p.startAction();
                    }
                }

                // KEYBOARD INPUT
                // player move
                if ((keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) && (keyboard.IsKeyUp(Keys.Right) && keyboard.IsKeyUp(Keys.D)))
                {
                    player.move(-(new Vector3(2f, 2f, 2f) * new Vector3((float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg - 180)), 0f, (float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg)))), activeActors);  
                }
                if ((keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) && (keyboard.IsKeyUp(Keys.Left) && keyboard.IsKeyUp(Keys.A)))
                {
                    player.move(new Vector3(2f, 2f, 2f) * new Vector3((float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg - 180)), 0f, (float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg))), activeActors);
                }

                if ((keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W)) && (keyboard.IsKeyUp(Keys.Down) && keyboard.IsKeyUp(Keys.S))) 
                {
                    player.move(new Vector3((float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg)), 0f, (float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg))), activeActors);
                }

                if ((keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S)) && (keyboard.IsKeyUp(Keys.Up) && keyboard.IsKeyUp(Keys.W)))
                {
                    player.move(-(new Vector3((float)Math.Sin(MathHelper.ToRadians(player.currentYawAngleDeg)), 0f, (float)Math.Cos(MathHelper.ToRadians(player.currentYawAngleDeg)))), activeActors);
                }

                // choose ammo

                if (keyboard.IsKeyDown(Keys.D1) || keyboard.IsKeyDown(Keys.NumPad1))
                {
                    loadedProjectile = shock;
                    shockUI.setSprite(shockSelectedTexture);
                    pullUI.setSprite(pullUnselectedTexture);
                }
                if (keyboard.IsKeyDown(Keys.D2) || keyboard.IsKeyDown(Keys.NumPad2))
                {
                    loadedProjectile = pull;
                    shockUI.setSprite(shockUnselectedTexture);
                    pullUI.setSprite(pullSelectedTexture);
                }


                //set up first person camera
                firstPersonCamPosition = new Vector3(player.position.X, playerModel.boxSize.Y * 3/5, player.position.Z);
                firstPersonCamTarget = firstPersonCamPosition + Vector3.Transform(new Vector3(0f, 0f, 1f), player.rotation);

                // set up first person matrices
                firstPersonProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(135), graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight, playerModel.boxExtents.X, 3000f);
                firstPersonViewMatrix = Matrix.CreateLookAt(firstPersonCamPosition, firstPersonCamTarget, new Vector3(0f, 1f, 0f));
                worldMatrix = Matrix.CreateWorld(firstPersonCamTarget, Vector3.Forward, Vector3.Up);

                projectionMatrix = firstPersonProjectionMatrix;
                viewMatrix = firstPersonViewMatrix;
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

            foreach(Actor h in hazards)
            {
                h.draw(viewMatrix, projectionMatrix);
            }

            foreach(Projectile p in allProjectiles)
            {
                p.draw(viewMatrix, projectionMatrix);
            }

            foreach(NPC g in guards)
            {
                g.draw(viewMatrix, projectionMatrix);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Tab))
            {
                player.draw(viewMatrix, projectionMatrix);
            }

            floor.draw(viewMatrix, projectionMatrix);

            // DRAW UI
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            shockUI.draw(spriteBatch);
            pullUI.draw(spriteBatch);
            gameOverUI.draw(spriteBatch);

            spriteBatch.End();

            

            base.Draw(gameTime);
        }

        public void createWall(Vector3 position, float rotation = 0)
        {
            Actor newWall = new Actor(wall, position);

            newWall.changeYaw(MathHelper.ToRadians(rotation));

            terrain.Add(newWall);
        }

        public void createShockGate(Vector3 position, float rotation = 0)
        {
            Hazard newShockGate = new Hazard(electricBeams, position);

            // left wall
            newShockGate.attachNewActor(gateWalls, new Vector3(-newShockGate.getModelData().boxExtents.X, 0f, 0f), 0f);
            // right wall
            newShockGate.attachNewActor(gateWalls, new Vector3(newShockGate.getModelData().boxExtents.X, 0f, 0f), 0f);

            hazards.Add(newShockGate);

            for(int i = 1; i <newShockGate.numberOfAttachedActors() - 1; i++)
            {
                newShockGate.getAttachedActor(i).getModelData().resizeHitbox(new Vector3(0.5f, 1f, 1f));
                terrain.Add(newShockGate.getAttachedActor(i));
            }
        }

        public Projectile createNewProjectile(ProjectileClassification classToAdd)
        {
            Projectile newProjectile;

            Vector3 projectilePos = player.position + new Vector3(0f, playerModel.boxExtents.Y, 0f);
            float projectileAngle = player.currentYawAngleDeg;

            switch (classToAdd)
            {
                case ProjectileClassification.pull:
                    newProjectile = new PullProjectile(pull.model, projectilePos, pull.moveSpeed, projectileAngle);
                    break;

                default:
                    newProjectile = new Projectile(ProjectileClassification.shock, shock.model, projectilePos, shock.moveSpeed, projectileAngle);
                    break;
            }

            return newProjectile;
        }

        public void buildTestLevel()
        {
            createShockGate(new Vector3(500f, 0f, -300f));
            //createWall(new Vector3(500f, 0f, -125f));

            guards.Add(new NPC(pawn, new Vector3(-500f, 0f, -500f), pawn.moveSpeed));
            guards.Add(new NPC(armoured, new Vector3(500f, 0f, -500f), armoured.moveSpeed));
            guards.Add(new NPC(pawn, new Vector3(-800f, 0f, -500f), pawn.moveSpeed));
        }

        public void buildStandardLevel()
        {
            /// Create a blank level with boundary walls and a floor
            // horizontal terrain creation
            for (int i = 0; i < graphics.PreferredBackBufferWidth + wall.boxSize.X; i += (int)wall.boxSize.X)
            {
                createWall(new Vector3((float)i, 0f, GraphicsDevice.Viewport.Height + wall.boxExtents.Y));
                createWall(new Vector3((float)i, 0f, -GraphicsDevice.Viewport.Height));

                // create terrain in negative direction
                // ignore if i==0, otherwise 2 objects created in same place
                if (i != 0)
                {
                    createWall(new Vector3((float)-i, 0f, GraphicsDevice.Viewport.Height + wall.boxExtents.Y));
                    createWall(new Vector3((float)-i, 0f, -GraphicsDevice.Viewport.Height));
                }
            }

            // vertical terrain creation
            for (int i = 0; i < graphics.PreferredBackBufferHeight + wall.boxSize.X; i += (int)wall.boxSize.X)
            {
                createWall(new Vector3(-GraphicsDevice.Viewport.Width, 0f, (float)i), 90);
                createWall(new Vector3(GraphicsDevice.Viewport.Width, 0f, (float)i), 90);

                // create terrain in negative direction
                // ignore if i==0, otherwise 2 objects created in same place
                if (i != 0)
                {
                    createWall(new Vector3(-GraphicsDevice.Viewport.Width, 0f, (float)-i), 90);
                    createWall(new Vector3(GraphicsDevice.Viewport.Width, 0f, (float)-i), 90);
                }
            }

            floor = new Actor(floorModel, new Vector3(0f, 0f, 0f));
        }
    }
}
