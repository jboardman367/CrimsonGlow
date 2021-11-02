using RedUtils;
using RedUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public partial class RedBot
    {
        public void DefaultLogic()
        {
			// Don't override a shot
			if (Action is Shot)
				return;

			bool beingScoredOn = Ball.Prediction.FindGoal((Team + 1) % 2) is not null;
			// Only intervene on rotation if threatened (basic)
			if (Action is RotateGrabBoost && !beingScoredOn)
				return;

			// Rotate if on low
			if (Me.Boost < 5 && !beingScoredOn)
            {
				Action = new RotateGrabBoost(Me);
				return;
            }

            // search for the first avaliable shot using DefaultShotCheck
            Shot shot = FindShot(DefaultShotCheck, new Target(TheirGoal));

            // if a shot is found, go for the shot. Otherwise, if there is an Action to execute, execute it. If none of the others apply, drive back to goal.
            Action = shot ?? Action ?? new Drive(Me, OurGoal.Location);
        }

		public enum ShotTypes
        {
			aerialShot,
			groundShot,

        }

		public Shot LowShotCheck(BallSlice slice, Target target, string[] allowedTypes)
		{
			if (slice != null) // Check if the slice even exists
			{
				float timeRemaining = slice.Time - Game.Time;

				// Check first if the slice is in the future and if it's even possible to shoot at our target
				if (timeRemaining > 0 && target.Fits(slice.Location))
				{
					Ball ballAfterHit = slice.ToBall();
					Vec3 carFinVel = ((slice.Location - Me.Location) / timeRemaining).Cap(0, Car.MaxSpeed);
					ballAfterHit.velocity = carFinVel + slice.Velocity.Flatten(carFinVel.Normalize()) * 0.8f;
					Vec3 shotTarget = target.Clamp(ballAfterHit);

					// Let's try a ground shot
					GroundShot groundShot = new GroundShot(Me, slice, shotTarget);
					if (groundShot.IsValid(Me))
					{
						return groundShot;
					}

					// Otherwise, we'll try a jump shot
					JumpShot jumpShot = new JumpShot(Me, slice, shotTarget);
					if (jumpShot.IsValid(Me))
					{
						return jumpShot;
					}
				}
			}

			return null; // if none of those work, we'll just return null (meaning no shot was found)
		}


		/*
		public Line3 BallLikelyBoom()
        {
			Ball ballInOne = Ball.Prediction.Slices[60].ToBall();
        }
		*/
	}
}
