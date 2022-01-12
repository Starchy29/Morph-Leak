using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public class Player : Entity
    {
        public enum PlayerState {
            Aerial,
            Grounded,
            Crouch,
            Walled, // Up against a wall midair, holding toward wall
            // Slippery
            Dash,
            WallClimb,
            // Sticky
            GrapplePull,
            GrappleSwing,
            Hang, // Stuck to a wall or ceiling
            // Bouncy
            WallJump, // First half of the arc

            Dead
        }

        #region Constants
        private const int COYOTE_TIME = 5;

        // Air
        private const float JUMP_ACCEL = 0.4f;
        private const float JUMP_VEL = -12f;
        private const float SHORT_JUMP_VEL = -10f;
        private const float DRAG = 0.3f;
        private const float DRIFT_ACCEL = 1.5f;
        private const float FAST_FALL_ACCEL = 1.2f;
        private const float GLIDE_VEL = 4f;
        protected const float GRAVITY = 0.8f;

        // Ground
        private const float WALK_ACCEL = 2f;
        private const float MAX_WALK_SPEED = 7f;
        private const float FRICTION = .6f;
        private const float SLIDE_FRICTION = 0.2f;

        // Abilities
        private const float DASH_SPEED = 20f;
        private const int DASH_DURATION = 10;
        private const int CLIMB_DURATION = 10;
        private const int COOLDOWN = -10;
        private const float GRAPPLE_SPEED = 25f;
        private const float MAX_GRAPPLE_LENGTH = 330f;

        #endregion

        // Fields
        private Type form;
        private PlayerState state;
        private Direction lastWall; // For slippery wall climbing and sticky pull grappling, tracks which wall was last used
        private Vector2 grapple;
        private Wall grappledWall;
        private float bounceVelocity; // Holds the aerial velocity to allow bouncing
        private int coyoteFrames; // Time when plyer can jump after walking off a ledge
        private int abilityFrames; // Tracks duration and cooldown of abilities. For bouncy, it is used as a buffer after landing or hitting a ceiling
        private bool abilityReset; // Tracks if the ability has already been used in the air
        private bool jumpReset; // Tracks if double jump has already been used in the air. Also used for jump cancelling out of dash.
        private bool grappleExtending; // Tells whether the grapple is extending or contracting

        private Vector2 velocity;
        private Vector2 acceleration;
        private Vector2 previousSize;
        private Vector2 previousPos;
        private bool facingRight; // false means facing left

        // Animations
        private Animation deathAnimation;
        private Animation bounceAnimation;
        private Animation specialAnimation; // the animation that plays instead of the normal animation
        private Animation glideAnimation;

        // Properties
        public Type Form { get { return form; } }
        public bool Alive { get; set; }
        public float Speed { get { return Math.Abs(velocity.X); } } // Horizontal velocity only
        public Vector2 Velocity { get { return velocity; } set { velocity = value; } } // walls need to set the player's velocity sometimes
        public Wall GrappledWall { get { return grappledWall; } }
        public PlayerState State { get { return state; } }

        // Constructor
        public Player() : base(0, 0, 50, 50) {
            state = PlayerState.Aerial;
            form = Type.Slippery;
            abilityReset = true;
            facingRight = true;
            previousPos = position;
            velocity = Vector2.Zero;
            acceleration = Vector2.Zero;
            Alive = true;

            deathAnimation = null;
            bounceAnimation = new Animation(Game1.PlayerBounce, AnimationType.Rebound, 1);
            glideAnimation = new Animation(Game1.PlayerGlide, AnimationType.Hold, 2);
        }

        public override void Update() {
            List<Direction> collisions = null;
            Vector2 previousMidPoint;

            switch(state) {
                case PlayerState.Dead:
                    if(grapple.Length() > 0) {
                        ExtendGrapple(-GRAPPLE_SPEED);
                    }

                    // allow moving walls to move the player during the death animation
                    switch(lastWall) {
                        case Direction.Up:
                            position.Y -= 0.1f;
                            break;

                        case Direction.Down:
                            position.Y += 0.1f;
                            break;

                        case Direction.Left:
                            position.X -= 0.1f;
                            break;

                        case Direction.Right:
                            position.X += 0.1f;
                            break;

                        case Direction.None:
                            return; // don't check collisions
                    }
                    CheckCollisions();
                    return;

                case PlayerState.Grounded:                   
                    Walk(WALK_ACCEL, FRICTION);

                    position += velocity;
                    position.Y += 0.1f; // Make sure player is still grounded

                    collisions = CheckCollisions();

                    if(Input.BufferInput == Inputs.Jump) { // Jump
                        Jump(true);

                        coyoteFrames = 0;
                        if(form == Type.Bouncy) { // Reset bounce and double jump
                            abilityReset = true;
                            jumpReset = true;
                        }
                    }
                    else if(Input.BufferInput == Inputs.Ability && IsOffCooldown()) { // Use ability
                        switch(form) {
                            case Type.Slippery: // Dash
                                Dash(true); // Allow jump cancel
                                break;

                            case Type.Bouncy: // Bounce
                                if(abilityFrames < 0) { // Regular bounce
                                    Jump(false, -bounceVelocity);

                                    abilityFrames = 0; // Reset buffer to prepare for next landing
                                    abilityReset = false;
                                }
                                else { // Mini bounce
                                    Jump(false, SHORT_JUMP_VEL);
                                    jumpReset = true;
                                }
                                bounceAnimation.Restart();
                                specialAnimation = bounceAnimation;
                                break;

                            case Type.Sticky: // Pull grapple
                                Grapple(Direction.Down);
                                break;
                        }                       
                    }
                    else if(!collisions.Contains(Direction.Down)) { // Fall
                        state = PlayerState.Aerial;
                        coyoteFrames = COYOTE_TIME;

                        if(form == Type.Bouncy) {
                            abilityReset = true;
                            jumpReset = true;
                        }
                    }
                    else if(Input.IsPressed(Inputs.Down)) { // Start crouch                       
                        Crouch();

                        if(form != Type.Slippery) { // Slide if slippery
                            velocity.X = 0;
                        }

                        state = PlayerState.Crouch;
                    }                       

                    if(form != Type.Bouncy) {
                        abilityReset = true;
                    }
                    if(form == Type.Slippery) { // Don't impact sticky grappling
                        lastWall = Direction.None;
                    } 
                    break;

                case PlayerState.Aerial:
                    Walk(DRIFT_ACCEL, DRAG);

                    if(velocity.Y < 0) { // Jumping
                        if(Input.IsUnpressed(Inputs.Jump)) {
                            acceleration.Y = GRAVITY; // Extend jump height while button still held
                        }
                    } 
                    else { // Falling
                        if(form == Type.Sticky && Input.IsPressed(Inputs.Jump) && velocity.Y >= GLIDE_VEL) { // Glide
                            acceleration.Y = 0;
                            velocity.Y = GLIDE_VEL;
                        }
                        else {
                            acceleration.Y = GRAVITY;
                        }
                    }

                    velocity += acceleration;
                    position += velocity;

                    bounceVelocity = velocity.Y;

                    collisions = CheckCollisions();

                    if(Input.JustPressed(Inputs.Jump) && form == Type.Bouncy && coyoteFrames < 0) {
                        // wall jump after just falling off wall
                        Jump(true, SHORT_JUMP_VEL);
                                
                        if(facingRight) {
                            velocity.X = 7f;
                        } else {
                            velocity.X = -7f;
                        }

                        state = PlayerState.WallJump; // Override aerial state
                    }
                    else if(Input.JustPressed(Inputs.Jump) && coyoteFrames > 0) { // Jump if within coyote time
                        Jump(true);
                        coyoteFrames = 0;
                    }
                    else if(form == Type.Bouncy && jumpReset && Input.JustPressed(Inputs.Jump)) { // Double jump
                        AirJump();
                    }
                    else if( (collisions.Contains(Direction.Left) && Input.IsPressed(Inputs.Left) 
                        || collisions.Contains(Direction.Right) && Input.IsPressed(Inputs.Right))
                        && !(form == Type.Sticky && Input.IsPressed(Inputs.Jump)) // don't stick while holding jump
                    ) { // Hit a wall
                        state = PlayerState.Walled;
                        facingRight = !facingRight;
                    }
                    else if(collisions.Contains(Direction.Down)) { // Land
                        state = PlayerState.Grounded;

                        if(form == Type.Bouncy) { // Set up bounce buffer
                            if(abilityReset) {
                                abilityFrames = -COYOTE_TIME; // Apply buffer window
                            }
                            else { // Prevent regular bounce because player already used it
                                abilityReset = true;
                                abilityFrames = 0;
                            }
                        }
                    }
                    else if(Input.JustPressed(Inputs.Ability) && IsOffCooldown()) { // Use ability                      
                        if(form == Type.Slippery) { // Dash
                            abilityReset = false; // Can't use dash mutliple times midair

                            if(Input.IsPressed(Inputs.Down)) {
                                Dash(false, true);
                            } else {
                                Dash();
                            }
                        }
                        else if(form == Type.Sticky) { // Swing grapple
                            Grapple(Direction.None);
                        }
                        else if(form == Type.Bouncy) {
                            // Animate
                            bounceAnimation.Restart();
                            specialAnimation = bounceAnimation;
                        }
                    }

                    if(coyoteFrames > 0) {
                        coyoteFrames--;
                    }
                    else if(coyoteFrames < 0) {
                        coyoteFrames++;
                    }
                    break;

                case PlayerState.Crouch:
                    AccerlerateTo(0, SLIDE_FRICTION);

                    position += velocity;

                    position.Y += 0.1f; // Make sure player is still grounded 

                    collisions = CheckCollisions();

                    if(Input.BufferInput == Inputs.Jump) { // Jump
                        state = PlayerState.Aerial;
                        acceleration.Y = GRAVITY;
                        coyoteFrames = 0;

                        Uncrouch();

                        // Nerf jump cancel distance when sliding
                        if(Speed > DASH_SPEED - 2f) {
                            if(velocity.X > 0) {
                                velocity.X = DASH_SPEED - 2f;
                            } else {
                                velocity.X = -DASH_SPEED + 2f;
                            }
                        }

                        // Because of jump buffer, if the player is on the ground, they immediately land then jump from grounded state.
                        // If they are on a platform, they begin falling through
                    }
                    else if(Input.JustPressed(Inputs.Ability) && IsOffCooldown() && form == Type.Sticky) { // Use ability                      
                        // pull grapple from crouch
                        Grapple(Direction.None);
                    }
                    else if(!collisions.Contains(Direction.Down)) { // Fall
                        state = PlayerState.Aerial;
                        coyoteFrames = COYOTE_TIME;
                        Uncrouch();

                        // Nerf jump cancel distance when sliding
                        if(Speed > DASH_SPEED - 4f) {
                            coyoteFrames = 3; // less coyote time because travelling further
                            if(velocity.X > 0) {
                                velocity.X = DASH_SPEED - 4f;
                            } else {
                                velocity.X = -DASH_SPEED + 4f;
                            }
                        }
                    }
                    else if(Input.IsUnpressed(Inputs.Down)) { // Stop crouching                       
                        state = PlayerState.Grounded;
                        Uncrouch();
                        
                        // Nerf jump cancel distance when sliding
                        if(Speed > DASH_SPEED - 2f) {
                            if(velocity.X > 0) {
                                velocity.X = DASH_SPEED - 2f;
                            } else {
                                velocity.X = -DASH_SPEED + 2f;
                            }
                        }
                    }

                    if(form != Type.Bouncy) {
                        abilityReset = true;
                    }
                    jumpReset = true;
                    if(form == Type.Slippery) { // Don't impact sticky grappling
                        lastWall = Direction.None;
                    }
                    break;

                case PlayerState.Walled:
                    switch(form) {
                        case Type.Slippery:
                            // Normal aerial movement
                            if(velocity.Y < 0) { // Jumping
                                if(Input.IsUnpressed(Inputs.Jump)) {
                                    acceleration.Y = GRAVITY; // Extend jump height while button still held
                                }
                            } else { // Falling
                                acceleration.Y = GRAVITY;
                            }                                                  
                            break;

                        case Type.Sticky: // Wall scrape
                            if(velocity.Y < 0 ) {
                                acceleration.Y = FAST_FALL_ACCEL; // Slide a little bit
                            } else {
                                acceleration.Y = JUMP_ACCEL;
                            } 
                            break;

                        case Type.Bouncy:                              
                            if(velocity.Y < 3f) { // Jumping, first part of fall
                                if(Input.IsUnpressed(Inputs.Jump)) {
                                    acceleration.Y = GRAVITY; // Extend jump height while button still held
                                }
                            } else { // Wall Cling
                                acceleration.Y = 0;
                                velocity.Y = 3f;
                            }
                            break;
                    }

                    velocity += acceleration;
                    position += velocity;

                    // Make sure still next to a wall
                    if(facingRight) {
                        position.X -= 0.1f;
                    } else {
                        position.X += 0.1f;
                    }

                    collisions = CheckCollisions();

                    if(form == Type.Bouncy && Input.JustPressed(Inputs.Jump)) { // Wall jump
                        Jump(true, SHORT_JUMP_VEL);
                                
                        if(facingRight) {
                            velocity.X = 7f;
                        } else {
                            velocity.X = -7f;
                        }

                        state = PlayerState.WallJump; // Override aerial state
                    }
                    else if(form == Type.Slippery && Input.BufferInput == Inputs.Jump && velocity.Y >= 0 && (facingRight && lastWall != Direction.Left || !facingRight && lastWall != Direction.Right)) { // Wall Climb
                        WallClimb();
                    }
                    else if(Input.JustPressed(Inputs.Ability)) { // Use ability
                        switch(form) {
                            case Type.Slippery:                     
                                abilityReset = false; // Uses midair dash

                                if(Input.IsPressed(Inputs.Down)) {
                                    Dash(false, true);
                                } else {
                                    Dash();
                                }
                                break;

                            case Type.Bouncy:
                                if(facingRight) { // Apply high horizontal velocity
                                    velocity.X = 20f;
                                } else {
                                    velocity.X = -20f;
                                }

                                Jump(false, -8f); // Create small upward arc

                                state = PlayerState.WallJump;
                                break;
                        }
                    }
                    else if(form == Type.Sticky && velocity.Y >= 0 && Input.BufferInput != Inputs.Jump) { // Wall Stick
                        state = PlayerState.Hang;
                        velocity = Vector2.Zero;
                        acceleration = Vector2.Zero;
                        if(collisions.Contains(Direction.Right)) {
                            lastWall = Direction.Right;
                        }
                        else if(collisions.Contains(Direction.Left)) {
                            lastWall = Direction.Left;
                        }
                    }
                    else if(collisions.Contains(Direction.Down)) { // Land
                        state = PlayerState.Grounded;
                    }
                    else if(facingRight && Input.IsUnpressed(Inputs.Left) || !facingRight && Input.IsUnpressed(Inputs.Right) || !collisions.Contains(Direction.Left) && !collisions.Contains(Direction.Right)) { // Fall off wall
                        state = PlayerState.Aerial;
                        if(form == Type.Bouncy) {
                            coyoteFrames = -5;
                        }
                    }

                    if(form != Type.Bouncy) { // Walls don't reset bounce
                        abilityReset = true;
                    }
                    break;

                case PlayerState.Dash: // Slippery only
                    if(velocity.Y > 0) {
                        // apply gravity if dashing down
                        velocity.Y += GRAVITY;
                    }

                    position += velocity;

                    if(jumpReset) { // Make sure still on the ground
                        position.Y += 0.1f;
                    }
                    abilityFrames--;

                    collisions = CheckCollisions();

                    if(!collisions.Contains(Direction.Down)) {
                        position.Y -= 0.1f; // correct ground check
                    }

                    if((collisions.Contains(Direction.Right) || collisions.Contains(Direction.Left)) && abilityFrames < 5 && abilityFrames > 0 && previousPos == position) {
                        // If player hits a wall at the end half of the dash, give a bigger window to wall climb
                        abilityFrames = DASH_DURATION / 2; // 5 frames
                    }
                    if(jumpReset && !collisions.Contains(Direction.Down) && abilityFrames < DASH_DURATION / 2 && form != Type.Bouncy) { // allows coyote time
                        // Disable jump cancel
                        jumpReset = false;
                        //position.Y -= 0.1f; // correct ground check
                    }

                    if(Input.BufferInput == Inputs.Jump && velocity == Vector2.Zero && (!facingRight && lastWall != Direction.Left || facingRight && lastWall != Direction.Right)) { // Cancel into wall climb                        
                        facingRight = !facingRight; // Turn around
                        WallClimb();
                    }
                    else if(jumpReset && Input.BufferInput == Inputs.Jump) { // Jump cancel
                        Jump(true, SHORT_JUMP_VEL);
                        coyoteFrames = 0; // Prevent air jump, may not be necessary
                        abilityFrames = COOLDOWN;
                        jumpReset = false;

                        AccerlerateTo(0, 2f); // Nerf jump cancel distance
                    }
                    else if(Input.IsPressed(Inputs.Down) && velocity.Y == 0) { // Crouch cancel keeps momentum
                        abilityFrames = COOLDOWN;
                        if(jumpReset) {
                            state = PlayerState.Grounded;
                        } else {
                            state = PlayerState.Aerial;
                            AccerlerateTo(0, 2f); // Nerf air cancel distance
                        }
                    }
                    else if(abilityFrames == 0) { // End dash
                        state = PlayerState.Aerial;

                        Y += 0.1f;
                        foreach(Wall wall in Game1.Game.CurrentLevel.Walls) {
                            // If this isn't here, there's a frame of aerial at the end of the dash and it looks weird
                            if(wall.Collides && this.Hitbox.Intersects(wall.Hitbox)) {
                                state = PlayerState.Grounded;
                                break;
                            }
                        }
                        Y -= 0.1f;
                        velocity.X /= 4f; // Slide a little at the end
                        abilityFrames = COOLDOWN;
                    }
                    break;

                case PlayerState.WallClimb: // Slippery only
                    velocity.Y = -12f;                   
                    position += velocity;

                    // Make sure player is still against a wall
                    if(facingRight) {
                        position.X -= 0.1f;
                    } else {
                        position.X += 0.1f;
                    }

                    collisions = CheckCollisions();

                    if(Input.JustPressed(Inputs.Ability)) { // Cancel into dash
                        Dash();
                    }
                    else if(abilityFrames == 0 || collisions.Contains(Direction.Up) || !(collisions.Contains(Direction.Left) || collisions.Contains(Direction.Right)) ) { // Hit a ceiling, or no longer against a wall, or duration runs out
                        // Reset dash
                        abilityReset = true;
                        abilityFrames = 0;

                        acceleration.Y = GRAVITY; // Prevent jump height increasing

                        // Prevent corner landing by undoing the shift that checks for wall collisions
                        if(facingRight && !collisions.Contains(Direction.Left)) {
                            position.X += 0.1f;
                        }
                        else if(!facingRight && !collisions.Contains(Direction.Right)) {
                            position.X -= 0.1f;
                        }

                        state = PlayerState.Aerial;
                    }
                    else {
                        abilityFrames--;
                    }
                    break;

                case PlayerState.WallJump: // Bouncy only
                    bool bounceRight = facingRight;
                    Walk(0.25f, DRAG);
                    facingRight = bounceRight; // keep facing away from wall

                    if(Input.IsUnpressed(Inputs.Jump) || velocity.Y >= 0) {
                        acceleration.Y = GRAVITY; // Extend jump height while button still held
                    }

                    velocity += acceleration;
                    position += velocity;

                    collisions = CheckCollisions();

                    if(Input.JustPressed(Inputs.Jump) && jumpReset) { // Jump if double jump available
                        AirJump();
                    }
                    else if(collisions.Contains(Direction.Down)) {
                        state = PlayerState.Grounded;
                    }
                    else if(collisions.Contains(Direction.Left) && Input.IsPressed(Inputs.Left) || collisions.Contains(Direction.Right) && Input.IsPressed(Inputs.Right)) { // Become walled
                        state = PlayerState.Walled;
                        facingRight = !facingRight;
                    }
                    else if(velocity.Y >= 0) { // End first half of arc
                        state = PlayerState.Aerial;
                    }
                    break;

                case PlayerState.Hang: // Sticky only
                    // make sure wall this is stuck on is still present
                    switch(lastWall) {
                        case Direction.Up:
                            position.Y -= 0.1f;
                            break;

                        case Direction.Left:
                            position.X -= 0.1f;
                            break;

                        case Direction.Right:
                            position.X += 0.1f;
                            break;
                    }

                    collisions = CheckCollisions();

                    if(Input.JustPressed(Inputs.Ability) && IsOffCooldown()) { // Grapple pull
                        Grapple(lastWall);
                        coyoteFrames = 0;
                    }
                    else if(form != Type.Sticky || Input.BufferInput == Inputs.Jump || collisions.Count <= 0) { // Detach
                        state = PlayerState.Aerial;
                        coyoteFrames = 0;
                    }
                    break;

                case PlayerState.GrapplePull: // Sticky only
                    position += velocity;
                    ExtendGrapple(-GRAPPLE_SPEED);

                    collisions = CheckCollisions();
                    Direction wallCollide = Direction.None;
                    if(collisions.Count == 1) {
                        wallCollide = collisions[0];
                    }

                    if(collisions.Contains(Direction.Down)) { // Land on ground
                        state = PlayerState.Grounded;
                        grapple = Vector2.Zero;
                        velocity = Vector2.Zero;
                        acceleration = Vector2.Zero;
                        grappledWall = null;
                    }
                    else if(collisions.Count > 0 && lastWall != wallCollide) { // Hit a wall or ceiling
                        state = PlayerState.Hang;
                        grapple = Vector2.Zero;
                        velocity = Vector2.Zero;
                        acceleration = Vector2.Zero;
                        grappledWall = null;

                        // Set last wall to which side the player hit
                        if(collisions.Contains(Direction.Left)) {
                            lastWall = Direction.Left;
                            facingRight = true;
                        }
                        else if(collisions.Contains(Direction.Right)) {
                            lastWall = Direction.Right;
                            facingRight = false;
                        }
                        else if(collisions.Contains(Direction.Up)) {
                            lastWall = Direction.Up;
                        }
                    }
                    else if(grappledWall != null && !grappledWall.Intersects(Midpoint + grapple) || form != Type.Sticky) { // Release early
                        state = PlayerState.Aerial;
                        acceleration.Y = GRAVITY;
                        velocity /= 1.1f;
                        grappledWall = null;
                    }
                    break;

                case PlayerState.GrappleSwing: // Sticky only
                    Vector2 prevVel = velocity;
                    previousMidPoint = Midpoint;
                    
                    if(velocity.X == 0 && grapple.X == 0 && grapple.Y < 0) { // Hang still
                        velocity = Vector2.Zero;
                        abilityReset = false;
                    }
                    else if(grapple.Y > 0 || abilityReset && velocity.Y < 0) { // Above grapple point or still moving up before swinging: normal fall
                        if(abilityReset) { // Still allow movement if above grapple point and haven't swung yet
                            Walk(DRIFT_ACCEL, FRICTION);
                        } else {
                            // Input does not affect
                            velocity.X = 0;
                        }
                        velocity.Y += GRAVITY; 
                        position += velocity;

                        grapple += previousMidPoint - Midpoint;

                        if(grapple.Y <= 0 && (grapple.X > 0 && velocity.X < 0 || grapple.X < 0 && velocity.X > 0)) { // Crossed into swing zone                          
                            // Correct velocity direction
                            velocity.X = -velocity.X;
                        }
                    } 
                    else { // Swing
                        abilityReset = false;
                        // Calculate energy
                        float energy = velocity.LengthSquared() / 2 + GRAVITY * (grapple.Length() + grapple.Y);

                        // Adjust velocity angle perpendicular to grapple
                        Vector2 newVelocity;
                        if(velocity.X > 0) { // Moving clockwise
                            newVelocity = new Vector2(-grapple.Y, grapple.X);
                        }
                        else if(velocity.X < 0) { // Moving counter-clockwise
                            newVelocity = new Vector2(grapple.Y, -grapple.X);
                        }
                        else { // Turning around
                            velocity = new Vector2(0, 0.1f);
                            if(grapple.X > 0) {
                                newVelocity = new Vector2(-grapple.Y, grapple.X);
                            } else {
                                newVelocity = new Vector2(grapple.Y, -grapple.X);
                            }
                        }

                        newVelocity.Normalize();
                        velocity =  newVelocity * velocity.Length();

                        float angleVel = velocity.Length() / grapple.Length();
                        if(velocity.X < 0 || velocity.X == 0 && grapple.X < 0) {
                            angleVel *= -1;
                        }

                        // Update position by rotating by the angular velocity
                        Vector2 grapplePoint = previousMidPoint + grapple;

                        double newAngle = Math.Acos(grapple.X / grapple.Length()) + angleVel; // 0: straight left; Pi / 2: straight down
                        
                        Midpoint = new Vector2(grapplePoint.X - (float) Math.Cos(newAngle) * grapple.Length(), grapplePoint.Y + (float) Math.Sin(newAngle) * grapple.Length());

                        grapple = grapplePoint - Midpoint; // Update grapple

                        // Adjust velocity magnitude
                        velocity.Normalize();

                        double expression = 2 * (energy - GRAVITY * (grapple.Length() + grapple.Y));
                        if(expression < 0) {
                            expression = 0;
                        }
                        velocity *= (float) Math.Sqrt(expression); 
                    }

                    collisions = CheckCollisions();                   

                    if(collisions.Contains(Direction.Up) || Input.IsUnpressed(Inputs.Ability) || form != Type.Sticky || grappledWall != null && !grappledWall.Intersects(Midpoint + grapple)) { // Let go of swinging or hit ceiling
                        state = PlayerState.Aerial;
                        acceleration.Y = GRAVITY; // Prevent jump height extension
                        acceleration.X = 0;
                        grappledWall = null;
                        if(form != Type.Sticky) {
                            abilityReset = true;
                        }
                    }
                    else if(collisions.Contains(Direction.Down)) { // Land on ground
                        state = PlayerState.Grounded;
                        velocity = Vector2.Zero;
                        acceleration.X = 0;
                        abilityReset = true;
                        grappledWall = null;
                    }
                    else if(collisions.Count > 0) { // Hit a wall
                        state = PlayerState.Hang;
                        velocity = Vector2.Zero;
                        acceleration.X = 0;
                        abilityReset = true;
                        grappledWall = null;

                        // Set last wall to which side the player hit
                        if(collisions.Contains(Direction.Left)) {
                            lastWall = Direction.Left;
                            facingRight = true;
                        }
                        else if(collisions.Contains(Direction.Right)) {
                            lastWall = Direction.Right;
                            facingRight = false;
                        }
                    }
                    break;
            }

            // manage grapple search
            if(grappleExtending) {
                ExtendGrapple(2 * GRAPPLE_SPEED);

                if(IsGrappleColliding()) { // Hit a wall with grapple
                        grappleExtending = false;

                        if(height == 25) {
                            Uncrouch();
                        }
                    
                        if(state == PlayerState.Aerial) { // Swing grapple
                            state = PlayerState.GrappleSwing; 
                            if(grapple.Y <= 0 && velocity.Y >= 0 && (grapple.X > 0 && velocity.X < 0 || grapple.X < 0 && velocity.X > 0)) {                       
                                // Correct velocity direction
                                velocity.X = -velocity.X;
                            }
                        } else { // Pull grapple
                            state = PlayerState.GrapplePull;
                            velocity = grapple;
                            velocity.Normalize();
                            velocity *= GRAPPLE_SPEED;
                        }

                        if(grapple.X > 0) {
                            facingRight = true;
                        }
                        else if(grapple.X < 0) {
                            facingRight = false;
                        }
                }
                else if(grapple.Length() >= MAX_GRAPPLE_LENGTH - 1f) { // Didn't hit a wall
                    grappleExtending = false;
                }
            }
            else if(state != PlayerState.GrapplePull && state != PlayerState.GrappleSwing && !grappleExtending && grapple.Length() > 0) {
                ExtendGrapple(-GRAPPLE_SPEED);
            }

            // Reduce cooldown/ buffer
            if(abilityFrames < 0) { 
                abilityFrames++;
            }

            if(collisions != null && collisions.Contains(Direction.None)) {
                // pinched between walls
                Die();
                lastWall = Direction.None; // don't shift around after going inside walls
            }
        }

        public override void Draw(SpriteBatch sb) {
            // Draw grapple
            sb.Draw(Game1.Pixel, new Rectangle((int)(Midpoint.X - Camera.GetOffset().X), (int)(Midpoint.Y - Camera.GetOffset().Y), (int)grapple.Length(), 10), null, Chemical.ColorOf(form), GetGrappleAngle(grapple), new Vector2(0, 0.5f), SpriteEffects.None, 0);
            if(state == PlayerState.GrapplePull || state == PlayerState.GrappleSwing) { // Draw grapple wall point
                int width = 18; // even number
                Vector2 point = Midpoint + grapple;
                sb.Draw(Game1.GrapplePoint, new Rectangle((int)(point.X - width / 2 - Camera.GetOffset().X), (int)(point.Y - width / 2 - Camera.GetOffset().Y), width, width), Chemical.ColorOf(form));
            }
            // Draw aim indicator
            else if(Game1.Game.GameState == GameState.Game && form == Type.Sticky && state != PlayerState.Dead && grapple.Length() == 0 && (!Input.IsGamepadConnected || Input.IsPressed(Inputs.Up) || Input.IsPressed(Inputs.Down) || Input.IsPressed(Inputs.Left) || Input.IsPressed(Inputs.Right))) {
                Vector2 indicatorPos = Input.GetAimAngle() * 150;
                Vector2 maxIndicator = Input.GetAimAngle() * MAX_GRAPPLE_LENGTH;
                const int indicatorRadius = 8;
                sb.Draw(Game1.Aimer, new Rectangle((int)(Midpoint.X + indicatorPos.X - indicatorRadius - Camera.GetOffset().X), (int)(Midpoint.Y + indicatorPos.Y - indicatorRadius - Camera.GetOffset().Y), indicatorRadius * 2, indicatorRadius * 2), Color.White);
                sb.Draw(Game1.Pixel, new Rectangle((int)(Midpoint.X + maxIndicator.X - indicatorRadius / 4 - Camera.GetOffset().X), (int)(Midpoint.Y + maxIndicator.Y - indicatorRadius / 4 - Camera.GetOffset().Y), indicatorRadius / 2, indicatorRadius / 2), Color.White);
            }

            // draw player
            SpriteEffects orientation = (facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            Texture2D sprite = null;
            if(specialAnimation != null) {
                sprite = specialAnimation.GetNext();
                if(sprite == null) {
                    specialAnimation = null;
                }
            }

            if(specialAnimation == null) {
                switch(state) {
                    case PlayerState.Dead:
                        sprite = deathAnimation.GetNext();
                        if(deathAnimation.Complete) {
                            abilityFrames--;
                            if(abilityFrames <= 0) {
                                Alive = false;
                            }
                            return;
                        }

                        Color color = Color.White;
                        if(deathAnimation.CurrentFrame < 8) {
                            color = Chemical.ColorOf(form);
                        }

                        switch(lastWall) {
                            case Direction.Right:
                                sb.Draw(sprite, new Rectangle(DrawBox.X, DrawBox.Y + DrawBox.Height, DrawBox.Width, DrawBox.Height), null, color, (float)Math.PI * 3 / 2, Vector2.Zero, SpriteEffects.None, 0f);
                                break;

                            case Direction.Left:
                                sb.Draw(sprite, new Rectangle(DrawBox.X + DrawBox.Width, DrawBox.Y, DrawBox.Width, DrawBox.Height), null, color, (float)Math.PI / 2, Vector2.Zero, SpriteEffects.None, 0f);
                                break;

                            case Direction.Up:
                                sb.Draw(sprite, DrawBox, null, color, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
                                break;

                            default:
                                sb.Draw(sprite, DrawBox, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                                break;
                        }
                        return;

                    case PlayerState.Grounded:
                        if(Speed >= 6f) {
                            sprite = Game1.PlayerWalk;
                        }
                        else if(Speed >= 2) {
                            sprite = Game1.PlayerWalkTransition;
                        }
                        else {
                            sprite = Game1.PlayerIdle;
                        }
                        break;

                    case PlayerState.WallJump:
                    case PlayerState.Aerial:
                        float vertSpeed = velocity.Y;
                        if(vertSpeed <= -4f) {
                            sprite = Game1.PlayerAirBottom;
                        }
                        else if(vertSpeed <= -2f) {
                            sprite = Game1.PlayerAirLower;
                        }
                        else if(vertSpeed >= 4f) {
                            sprite = Game1.PlayerAirTop;
                        }
                        else if(vertSpeed >= 2f) {
                            sprite = Game1.PlayerAirUpper;
                        }
                        else {
                            sprite = Game1.PlayerAirMiddle;
                        }

                        if(form == Type.Sticky && Input.IsPressed(Inputs.Jump) && velocity.Y > 0) {
                            sprite = glideAnimation.GetNext();
                            sb.Draw(sprite, new Rectangle(DrawBox.X - 15, DrawBox.Y, width + 30, height), null, Chemical.ColorOf(form), 0f, Vector2.Zero, orientation, 0f);
                            return;
                        }
                        else if(glideAnimation.CurrentFrame > -1) { 
                            glideAnimation.Restart();
                        }
                        break;

                    case PlayerState.Crouch:
                        sprite = Game1.PlayerCrouch;
                        if(Speed >= 2f) {
                            sprite = Game1.PlayerSlide;
                        }
                        break;

                    case PlayerState.Hang:
                        if(lastWall == Direction.Up) {
                            sprite = Game1.PlayerHang;
                        } else {
                            sprite = Game1.PlayerWall;
                        }
                        break;

                    case PlayerState.WallClimb:
                    case PlayerState.Walled:
                        if(velocity.Y >= 2f) {
                            sprite = Game1.PlayerWallDown;
                        }
                        else if(velocity.Y <= -2f) {
                            sprite = Game1.PlayerWallUp;
                        }
                        else {
                            sprite = Game1.PlayerWall;
                        }
                        break;

                    case PlayerState.Dash:
                        sprite = Game1.PlayerAirMiddle;
                        if(jumpReset) {
                            sprite = Game1.PlayerWalk;
                        }
                        if(velocity.Y > 0) {
                            sprite = Game1.PlayerPlummet;
                        }
                        break;

                    case PlayerState.GrapplePull:
                    case PlayerState.GrappleSwing:
                        sprite = Game1.PlayerNeutral;
                        break;

                    default:
                        sprite = Game1.PlayerIdle;
                        break;
                }
            }

            if(sprite != null) {
                sb.Draw(sprite, DrawBox, null, Chemical.ColorOf(form), 0f, Vector2.Zero, orientation, 0f);
            }
        }

        // plays the player's death animation. At the end, the level resets
        public void Die(Direction dissolveInto = Direction.None) {
            if(state != PlayerState.Dead) {
                deathAnimation = new Animation(Game1.PlayerNeutralDeath, AnimationType.End, 2); // default to aerial death animation

                // determine which direction to die
                switch(state) {
                    case PlayerState.Walled:
                    case PlayerState.WallClimb:
                        if(facingRight) {
                            lastWall = Direction.Left;
                        } else {
                            lastWall = Direction.Right;
                        }
                        break;
                    case PlayerState.Hang:
                        // lastwall stays the same
                        break;

                    case PlayerState.Crouch:
                    case PlayerState.Grounded:
                        dissolveInto = Direction.Down;
                        break;

                    default:
                        lastWall = Direction.None;
                        break;
                }

                if(dissolveInto != Direction.None || lastWall != Direction.None) {
                    if(dissolveInto != Direction.None) {
                        lastWall = dissolveInto;
                    }
                    deathAnimation = new Animation(Game1.PlayerDeath, AnimationType.End, 2); // switch to surface death animation
                }

                state = PlayerState.Dead;
                deathAnimation.Restart();
                velocity = Vector2.Zero;
                acceleration = Vector2.Zero;
                abilityFrames = 5; // frames after animation ends before restarting the level
                grappleExtending = false;
                specialAnimation = null;
                grappledWall = null;
                if(height == 25) {
                    Uncrouch();
                }
            }
        }

        public override void Restart() {
            base.Restart();
            Alive = true;
            acceleration = Vector2.Zero;
            velocity = Vector2.Zero;
            abilityFrames = 0;
            abilityReset = true;
            jumpReset = true;
            facingRight = true;
            grappleExtending = false;
            grapple = Vector2.Zero;
            state = PlayerState.Grounded;
            previousPos = position;
            Uncrouch();
        }

        public void SetForm(Type type) {
            form = type;

            if(abilityFrames < 0) { // Reset cooldown
                abilityFrames = 0;
            }
            if(state != PlayerState.GrappleSwing) {
                abilityReset = true;
            }
            jumpReset = true;
            lastWall = Direction.None;
        }

        // Gives the player an upward velocity and downward acceleration.
        private void Jump(bool extendable, float initialVel = JUMP_VEL) {
            velocity.Y = initialVel;

            if(extendable) {
                acceleration.Y = JUMP_ACCEL;
            }
            else {
                acceleration.Y = GRAVITY;
            }

            state = PlayerState.Aerial;
        }

        private void Walk(float acceleration, float friction) {
            if(velocity.X < MAX_WALK_SPEED && Input.IsPressed(Inputs.Right) && Input.IsUnpressed(Inputs.Left)) { // Move right
                AccerlerateTo(MAX_WALK_SPEED, acceleration);
                facingRight = true;
            }
            else if(velocity.X > -MAX_WALK_SPEED && Input.IsPressed(Inputs.Left) && Input.IsUnpressed(Inputs.Right)) { // Move left
                AccerlerateTo(-MAX_WALK_SPEED, acceleration);
                facingRight = false;
            }
            else { // Apply friction
                AccerlerateTo(0, friction);
            }
        }

        // Accelerates the x velocity toward the target speed, but does not pass it
        private void AccerlerateTo(float targetVel, float acceleration) {
            if(velocity.X > targetVel) {
                velocity.X -= acceleration;
                if(velocity.X < targetVel) { // Passed the target
                    velocity.X = targetVel; 
                }
            }
            else if(velocity.X < targetVel) {
                velocity.X += acceleration;
                if(velocity.X > targetVel) {
                    velocity.X = targetVel; 
                }
            }
        }

        // Uses the bouncy form's double jump ability
        private void AirJump() {
            Jump(true, SHORT_JUMP_VEL);
            jumpReset = false;
        }

        // Sets up the Dash state
        private void Dash(bool canJumpCancel = false, bool downward = false) {
            acceleration = Vector2.Zero;

            // Set dash velocity
            velocity.Y = 0;
            if(facingRight) {
                velocity.X = DASH_SPEED;
            }
            else {
                velocity.X = -DASH_SPEED;
            }

            if(downward) {
                velocity = new Vector2(0, DASH_SPEED);
            }

            // Set dash mechanics
            abilityFrames = DASH_DURATION;
            jumpReset = canJumpCancel;    

            state = PlayerState.Dash;
        }

        // Sets up the grapple search
        private void Grapple(Direction lastWall) {
            // Cancel the function if the player is already grappling (no longer uses state change)
            if(grappleExtending || grapple.Length() > 0) {
                return;
            }

            Vector2 aimAngle = Input.GetAimAngle();

            // set default angle way from surface if no aim
            if(aimAngle == Vector2.Zero) {
                if(state == PlayerState.Aerial) {
                    if(facingRight) {
                        aimAngle = new Vector2(1, 0);
                    } else {
                        aimAngle = new Vector2(-1, 0);
                    }
                }
                else if(lastWall == Direction.Left) {
                    aimAngle = new Vector2(1, 0);
                }
                else if(lastWall == Direction.Right) {
                    aimAngle = new Vector2(-1, 0);
                }
                else if(lastWall == Direction.Up) {
                    aimAngle = new Vector2(0, 1);
                }
                else {
                    aimAngle = new Vector2(0, -1);
                }
            }

            // correct angle if towards wall
            if(lastWall == Direction.Left && aimAngle.X < 0 || lastWall == Direction.Right && aimAngle.X > 0) {
                if(aimAngle.Y > 0) {
                    aimAngle = new Vector2(0, 1);
                } else {
                    aimAngle = new Vector2(0, -1);
                }
            }
            else if(lastWall == Direction.Up && aimAngle.Y < 0 || (lastWall == Direction.Down || state == PlayerState.Crouch) && aimAngle.Y > 0) {
                if(aimAngle.X > 0) {
                    aimAngle = new Vector2(1, 0);
                } else {
                    aimAngle = new Vector2(-1, 0);
                }
            }

            grapple = aimAngle * GRAPPLE_SPEED;
            grappleExtending = true;               
            this.lastWall = lastWall;
        }

        // Changes the length of the grapple by the input amount. A negative number shortens it
        private void ExtendGrapple(float length) {
            if(length < 0 && grapple.Length() < -length) {
                grapple = Vector2.Zero;
                return;
            }
            
            Vector2 lengthChanger = grapple;
            lengthChanger.Normalize();
            lengthChanger *= length;

            grapple += lengthChanger;

            if(grapple.Length() > MAX_GRAPPLE_LENGTH) {
                grapple.Normalize();
                grapple *= MAX_GRAPPLE_LENGTH;
            }
        }

        // Checks if the grapple intersects a wall
        private bool IsGrappleColliding() {
            Vector2 position = Midpoint + grapple; // The end point of the grapple
            Vector2 targetPoint;

            foreach(Wall wall in Game1.Game.CurrentLevel.Walls) {
                if(!wall.Collides) {
                    continue;
                }

                FloatRect rectangle = wall.Hitbox;

                // Avoid divide by zero error if vertical slope
                const float ERROR_AVOIDER = 3.5f; // place grapple point a little in the wall so it stays intersecting when it shifts a little
                if(grapple.X == 0) {
                    if(grapple.Y > 0 && position.Y >= rectangle.Y && rectangle.Y > Midpoint.Y && position.Y <= rectangle.Y + 2 * GRAPPLE_SPEED && position.X >= rectangle.X && position.X <= rectangle.X + rectangle.Width) { // Top
                        grapple.Y = rectangle.Y - Midpoint.Y + ERROR_AVOIDER;
                        grappledWall = wall;
                        return true;
                    } 
                    else if(grapple.Y < 0 && position.Y < rectangle.Y + rectangle.Height && Midpoint.Y > rectangle.Y + rectangle.Height && position.Y >= rectangle.Y - 2 * GRAPPLE_SPEED && position.X >= rectangle.X && position.X <= rectangle.X + rectangle.Width) { // Bottom
                        grapple.Y = (rectangle.Y + rectangle.Height) - Midpoint.Y - ERROR_AVOIDER;
                        grappledWall = wall;
                        return true;
                    }
                }

                float m = grapple.Y / grapple.X;
                float b = position.Y - m * position.X;
                float x;
                float y;

                if(grapple.Y > 0 && position.Y >= rectangle.Y && rectangle.Y > Midpoint.Y) { // Top
                    // Find the point where the grapple intersects with the side
                    y = rectangle.Y;
                    x = (y - b) / m;

                    if(x >= rectangle.X && x <= rectangle.X + rectangle.Width) {
                        targetPoint = new Vector2(x, y);
                        grapple = (targetPoint - Midpoint);
                        grapple.Y += ERROR_AVOIDER;
                        grappledWall = wall;
                        return true;
                    }
                }
                else if(grapple.Y < 0 && position.Y <= rectangle.Y + rectangle.Height && Midpoint.Y > rectangle.Y + rectangle.Height) { // Bottom
                    // Find the point where the grapple intersects with the side
                    y = rectangle.Y + rectangle.Height;
                    x = (y - b) / m;

                    if(x >= rectangle.X && x <= rectangle.X + rectangle.Width) {
                        targetPoint = new Vector2(x, y);
                        grapple = (targetPoint - Midpoint);
                        grapple.Y -= ERROR_AVOIDER;
                        grappledWall = wall;
                        return true;
                    }
                }

                if(grapple.X > 0 && position.X >= rectangle.X && rectangle.X > Midpoint.X) { // Right
                    // Find the point where the grapple intersects with the side
                    x = rectangle.X;
                    y = m * x + b;

                    if(y >= rectangle.Y && y <= rectangle.Y + rectangle.Height) {
                        targetPoint = new Vector2(x, y);
                        grapple = (targetPoint - Midpoint);
                        grapple.X += ERROR_AVOIDER;
                        grappledWall = wall;
                        return true;
                    }
                }
                else if(grapple.X < 0 && position.X <= rectangle.X + rectangle.Width && Midpoint.X > rectangle.X + rectangle.Width) { // Left
                    // Find the point where the grapple intersects with the side
                    x = rectangle.X + rectangle.Width;
                    y = m * x + b;

                    if(y >= rectangle.Y && y <= rectangle.Y + rectangle.Height) {
                        targetPoint = new Vector2(x, y);
                        grapple = (targetPoint - Midpoint);
                        grapple.X -= ERROR_AVOIDER;
                        grappledWall = wall;
                        return true;
                    }
                }
            }

            // Check platform collision
            foreach(Platform platform in Game1.Game.CurrentLevel.Platforms) {
                if(grapple.Y > 0 && position.Y >= platform.Y && platform.Y > this.Y + this.Height) { // Top
                    float x = grapple.X * (platform.Y - position.Y) / grapple.Y + position.X;
                    if(x >= platform.X && x <= platform.X + platform.Width) {
                        targetPoint = new Vector2(x, platform.Y);
                        grapple = (targetPoint - Midpoint);
                        return true;
                    }
                }
            }

            return false;
        }

        private void WallClimb() {
            state = PlayerState.WallClimb;
            abilityFrames = CLIMB_DURATION;

            if(facingRight) {
                lastWall = Direction.Left;                           
            } else {
                lastWall = Direction.Right;
            }
        }

        // Squashes the hitbox to enter the crouching state
        private void Crouch() {
            height = 25;
            position.Y += 25;
        }

        // Sets the hitbox back to normal after crouching
        private void Uncrouch() {
            height = 50;
            position.Y -= 25;
        }

        private bool IsOffCooldown() { 
            return (abilityFrames == 0 || abilityFrames < 0 && form == Type.Bouncy) && abilityReset;
        }

        private float GetGrappleAngle(Vector2 vector) {
            return (float) Math.Acos(vector.X / vector.Length()) * (vector.Y > 0 ? 1 : -1);
        }

        // Moves the entity to the outside of any wall they are colliding with. Returns which sides of the entity are intersecting
        protected List<Direction> CheckCollisions() {
            List<Direction> result = new List<Direction>();

            // Wall collisions
            foreach(Wall wall in Game1.Game.CurrentLevel.Walls) {
                // Check if colliding at all
                if(!this.Hitbox.Intersects(wall.Hitbox) || !wall.Collides) {
                    continue;
                }

                wall.PlayerColliding = true;

                // Determine which side the entity entered the wall
                Direction vertical = Direction.None;
                Direction horizontal = Direction.None;

                if(previousPos.X >= wall.Hitbox.X + wall.Hitbox.Width) {
                    horizontal = Direction.Right;
                }
                else if(previousPos.X + previousSize.X <= wall.Hitbox.X) {
                    horizontal = Direction.Left;
                }
                if(previousPos.Y >= wall.Hitbox.Y + wall.Hitbox.Height) { // Bottom
                    vertical = Direction.Down;
                }
                else if(previousPos.Y + previousSize.Y <= wall.Hitbox.Y) { // Top
                    vertical = Direction.Up;
                }

                // If clipped into a wall, go to the closest edge
                if(vertical == Direction.None && horizontal == Direction.None) {
                    // Determine distance player would travel in each direction
                    float up, down, left, right;
                    up = position.Y + height - wall.Hitbox.Y;
                    down = wall.Hitbox.Y + wall.Hitbox.Height - position.Y;
                    left = position.X + width - wall.Hitbox.X;
                    right = wall.Hitbox.X + wall.Hitbox.Width - position.X;

                    // Find the shortest distance and move there
                    if(up < right && up < left && up < down) { // Up
                        position.Y -= up;
                        result.Add(Direction.Down);
                    }
                    else if(right < left && right < down) { // Right
                        position.X += right;
                        result.Add(Direction.Left);
                    }
                    else if(left < down) { // Left
                        position.X -= left;
                        result.Add(Direction.Right);
                    }
                    else { // Down
                        position.Y += down;
                        result.Add(Direction.Up);
                    }

                    continue;
                }

                // Sets the entity to the correct position. When it is diagonal, up is preferered over sides and sides are preferred over bottom
                if(vertical == Direction.None || (vertical == Direction.Down && horizontal != Direction.None)) {
                    velocity.X = 0;
                    acceleration.X = 0;
                    if(horizontal == Direction.Right) { // Right
                        position.X = wall.Hitbox.X + wall.Hitbox.Width;
                        result.Add(Direction.Left);
                    }
                    else { // Left
                        position.X = wall.Hitbox.X - this.width;
                        result.Add(Direction.Right);
                    }
                }
                else {
                    velocity.Y = 0;
                    acceleration.Y = 0;
                    if(vertical == Direction.Down) { // Down
                        position.Y = wall.Hitbox.Y + wall.Hitbox.Height;
                        result.Add(Direction.Up);
                    }
                    else { // Up
                        position.Y = wall.Hitbox.Y - this.height;
                        //canDrop = false;
                        result.Add(Direction.Down);
                    }
                }
            }

            // Platforms (second because a platform below a wall could snag a player on the corner otherwise)
            if( ! (Input.IsPressed(Inputs.Down) && Input.IsPressed(Inputs.Jump))) { // pass through
                foreach(Platform platform in Game1.Game.CurrentLevel.Platforms) {
                    // Check if colliding and entering from above
                    if(platform.Y > this.Y && platform.Y < this.Y + this.height && this.X + this.width > platform.X && this.X < platform.X + platform.Width && previousPos.Y + previousSize.Y <= platform.Y) {
                        velocity.Y = 0;
                        acceleration.Y = 0;
                        position.Y = platform.Y - this.height;
                        result.Add(Direction.Down);
                    }
                }
            }

            // If offscreen, put back on screen
            if(position.X < 0) { // Left
                Vector2 grapplePoint = Midpoint + grapple;
                position.X = 0;

                if(state == PlayerState.GrappleSwing) {
                    // keep grapple point the same
                    grapple = grapplePoint - Midpoint;
                }
            }

            // If off the bottom of the screen, die
            if(position.Y > Game1.Game.CurrentLevel.Height) {
                result.Add(Direction.None); // die
            }

            previousPos = position;
            previousSize = new Vector2(width, height);

            if(result.Contains(Direction.Right) && result.Contains(Direction.Left) || result.Contains(Direction.Up) && result.Contains(Direction.Down)) {
                // pinched between two walls
                result.Add(Direction.None); // die
                lastWall = Direction.None; // fixes death visual bug
            }

            return result;
        }
    }
}