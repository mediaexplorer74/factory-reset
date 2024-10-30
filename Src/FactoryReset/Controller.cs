// Controller

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameManager
{
    public class Controller
    {
        public static TouchCollection TS_Is;
        public static TouchCollection TS_Was;
        public static float X_Was = 0;
        public static float Y_Was = 0;

        public static bool TouchMoveLeft = false;
        public static bool TouchMoveRight = false;
        public static bool TouchMoveUp = false;
        public static bool TouchMoveDown = false;

        public static bool DoubleTouchMoveLeft = false;
        public static bool DoubleTouchMoveRight = false;
        public static bool DoubleTouchMoveUp = false;
        public static bool DoubleTouchMoveDown = false;

        public static float VibrationMultiplier = 0.5F;

        readonly int gamepadIndex;

        public State Is, Was;


        public struct State
        {
            public readonly bool MoveLeft, MoveRight, MoveUp, MoveDown,
                Jump, Climb, Hide, Crouch, Pause, Call, Interact, Advance, Back;

            public State(KeyboardState key, GamePadState pad, TouchCollection tp)
            {
                // Single "swipe"
                if (TS_Is.Count == 1)
                {
                    if (tp[0].Position.X > X_Was)
                        TouchMoveRight = true;
                    else
                        TouchMoveRight = false;

                    if (tp[0].Position.X < X_Was)
                        TouchMoveLeft = true;
                    else
                        TouchMoveLeft = false;

                    if (tp[0].Position.Y > Y_Was)
                        TouchMoveDown = true;
                    else
                        TouchMoveDown = false;

                    if (tp[0].Position.Y < Y_Was)
                        TouchMoveUp = true;
                    else
                        TouchMoveUp = false;
                }

                // Double "swipe"
                if (TS_Is.Count == 2)
                {
                    if (tp[0].Position.X > X_Was)
                        DoubleTouchMoveRight = true;
                    else
                        DoubleTouchMoveRight = false;

                    if (tp[0].Position.X < X_Was)
                        DoubleTouchMoveLeft = true;
                    else
                        DoubleTouchMoveLeft = false;

                    if (tp[0].Position.Y > Y_Was)
                        DoubleTouchMoveDown = true;
                    else
                        DoubleTouchMoveDown = false;

                    if (tp[0].Position.Y < Y_Was)
                        DoubleTouchMoveUp = true;
                    else
                        DoubleTouchMoveUp = false;
                }

                MoveLeft = TouchMoveLeft
                    || key.IsKeyDown(Keys.A)
                    || key.IsKeyDown(Keys.Left)
                    || pad.DPad.Left == ButtonState.Pressed
                    || pad.ThumbSticks.Left.X < -0.25;

                MoveRight = TouchMoveRight
                    || key.IsKeyDown(Keys.D)
                    || key.IsKeyDown(Keys.Right)
                    || pad.DPad.Right == ButtonState.Pressed
                    || pad.ThumbSticks.Left.X > +0.25;

                MoveUp = TouchMoveUp
                    || key.IsKeyDown(Keys.W)
                    || key.IsKeyDown(Keys.Up)
                    || pad.DPad.Up == ButtonState.Pressed
                    || pad.ThumbSticks.Left.Y > +0.25;

                MoveDown = TouchMoveDown
                    || key.IsKeyDown(Keys.S)
                    || key.IsKeyDown(Keys.Down)
                    || pad.DPad.Down == ButtonState.Pressed
                    || pad.ThumbSticks.Left.Y < -0.25;

                Jump = tp.Count == 1
                    || key.IsKeyDown(Keys.Space)
                    || pad.Buttons.A == ButtonState.Pressed
                    || pad.Buttons.B == ButtonState.Pressed;

                Climb = key.IsKeyDown(Keys.LeftShift)
                    || pad.Buttons.LeftShoulder == ButtonState.Pressed
                    || pad.Buttons.RightShoulder == ButtonState.Pressed
                    || pad.Triggers.Left >= 0.5
                    || pad.Triggers.Right >= 0.5;

                Hide = DoubleTouchMoveDown
                    || key.IsKeyDown(Keys.F)
                    || pad.Buttons.X == ButtonState.Pressed
                    || pad.Buttons.Y == ButtonState.Pressed;

                Crouch = key.IsKeyDown(Keys.LeftControl)
                    || pad.Buttons.LeftStick == ButtonState.Pressed;

                Pause = tp.Count == 3
                    || key.IsKeyDown(Keys.Escape)
                    || pad.Buttons.Start == ButtonState.Pressed;

                Call = DoubleTouchMoveUp // optional =)
                    || key.IsKeyDown(Keys.C)
                    // Not sure if this is actually SELECT
                    || pad.Buttons.Back == ButtonState.Pressed;

                Interact = DoubleTouchMoveRight
                    || key.IsKeyDown(Keys.E)
                    || pad.Buttons.X == ButtonState.Pressed
                    || pad.Buttons.Y == ButtonState.Pressed;

                Advance = tp.Count == 1
                    || key.IsKeyDown(Keys.Space)
                    || pad.Buttons.A == ButtonState.Pressed
                    || pad.Buttons.B == ButtonState.Pressed;

                Back = DoubleTouchMoveLeft
                    || key.IsKeyDown(Keys.Back)
                    || pad.Buttons.B == ButtonState.Pressed
                    || pad.Buttons.Back == ButtonState.Pressed;
            }
        };


        private float VibrationTimer = 0;

        public Controller(int gamepadIndex = 0)
        {
            this.gamepadIndex = gamepadIndex;
        }

        public void Vibrate(float left, float right, float duration)
        {
            GamePad.SetVibration(gamepadIndex, left * VibrationMultiplier, right * VibrationMultiplier);

            VibrationTimer = duration;
        }

        public bool MoveLeft => Is.MoveLeft;

        public bool MoveRight => Is.MoveRight;

        public bool MoveUp => Is.MoveUp;

        public bool MoveDown => Is.MoveDown;

        public bool Jump => Is.Jump;
        public bool Climb => Is.Climb;
        public bool Hide => Is.Hide;
        public bool Crouch => Is.Crouch;

        public bool Pause => Is.Pause;
        public bool Call => Is.Call;
        public bool Interact => Is.Interact;
        public bool Advance => Is.Advance;

        public bool Back => Is.Back;

        public void Update()
        {
            Was = Is;
            TS_Was = TS_Is;

            if (TS_Was.Count > 0)
            {
                X_Was = TS_Was[0].Position.X;
                Y_Was = TS_Was[0].Position.Y;
            }

            TS_Is = TouchPanel.GetState();
            Is = new State(Keyboard.GetState(), GamePad.GetState(gamepadIndex), TS_Is);

            if (VibrationTimer > 0)
            {
                VibrationTimer -= Game1.DeltaT;
                if (VibrationTimer <= 0)
                    GamePad.SetVibration(gamepadIndex, 0, 0);
            }
        }
    }
}
