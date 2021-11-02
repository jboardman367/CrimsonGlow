using RedUtils;
using RedUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    class RotateGrabBoost : IAction
    {
		/// <summary>Whether or not we have arrived at our destination</summary>
		public bool Finished { get; private set; }
		/// <summary>Whether or not this action can currently be interrupted</summary>
		public bool Interruptible { get; private set; }

		public Arrive arrive;

		public RotateGrabBoost(Car car)
		{
			Finished = false;
			Interruptible = true;
			arrive = new Arrive(car, Vec3.Zero, Vec3.Zero);
			arrive.Drive.WasteBoost = true;
		}
		public RotateGrabBoost(Car car, bool interruptible)
		{
			Finished = false;
			Interruptible = interruptible;
			arrive = new Arrive(car, Vec3.Zero, Vec3.Zero);
			arrive.Drive.WasteBoost = true;
		}

		public void Run(RUBot bot)
		{
			ChooseBoost(bot);
			arrive.Run(bot);
			Finished = arrive.Finished;
		}
        private void ChooseBoost(RUBot bot)
        {
            int side = 1 - 2 * bot.Team;
			Ball ballInOne = Ball.MainBall;
			try
			{
				ballInOne = Ball.Prediction[60].ToBall();
			}
			catch (IndexOutOfRangeException) { }
			if (ballInOne.location.y * side > 3000)
			{
				// Consider mid boosts
				Vec3 left_mid = new Vec3(3584f, 0f, 73f);
				Vec3 right_mid = new Vec3(-3584f, 0f, 73f);
				bool mid_left_available = Field.Boosts[18].IsActive || Field.Boosts[18].TimeUntilActive < Drive.GetEta(bot.Me, left_mid) + 0.2;
				bool mid_right_available = Field.Boosts[15].IsActive || Field.Boosts[15].TimeUntilActive < Drive.GetEta(bot.Me, right_mid) + 0.2;

				if (mid_left_available && mid_right_available)
				{
					// Both are available, choose the best one
					bool left_behind = bot.Me.Forward.Dot(left_mid - bot.Me.Location) < 0;
					bool right_behind = bot.Me.Forward.Dot(right_mid - bot.Me.Location) < 0;

					if (right_behind && left_behind || !right_behind && !left_behind)
					{
						// We need another metric for the tiebreaker
						// So we will just prefer the one that is on the correct rotational side
						if (bot.Me.Location.y * side < 0)
                        {
							// Rotating in
							if (Ball.Location.x > 0)
                            {
								// Go for the left boost
								arrive.Target = left_mid;
								arrive.Direction = bot.TheirGoal.LeftPost - left_mid;
								return;
							}
                            else
                            {
								// Go for the right boost
								arrive.Target = right_mid;
								arrive.Direction = bot.TheirGoal.RightPost - right_mid;
								return;
							}
                        }
                        else
                        {
							// Rotating out
							if (Ball.Location.x > 0)
							{
								// Go for the right boost
								arrive.Target = right_mid;
								arrive.Direction = new Vec3(2, 0, -3*side);
								return;
							}
							else
							{
								// Go for the left boost
								arrive.Target = left_mid;
								arrive.Direction = new Vec3(-2, 0, -3 * side);
								return;
							}
						}
					}

					if (right_behind)
                    {
						//Go for left boost, rotating in
						arrive.Target = left_mid;
						arrive.Direction = bot.TheirGoal.LeftPost - left_mid;
						return;
					}

					// Must be left behind if not returned
					//Go for left boost, rotating in
					arrive.Target = left_mid;
					arrive.Direction = bot.TheirGoal.LeftPost - left_mid;
					return;
				}

				if (mid_left_available)
                {
					if (bot.Me.Location.x > 0 && bot.Me.Location.y * side > 0)
                    {
						// In the attacking left quadrant, so rotate out
						arrive.Target = left_mid;
						arrive.Direction = new Vec3(-2, 0, -3 * side);
						return;
					}
                    else
                    {
						arrive.Target = left_mid;
						arrive.Direction = bot.TheirGoal.LeftPost - left_mid;
						return;
					}
                }

				if (mid_right_available)
				{
					if (bot.Me.Location.x < 0 && bot.Me.Location.y * side > 0)
					{
						// In the attacking right quadrant, so rotate out
						arrive.Target = right_mid;
						arrive.Direction = new Vec3(2, 0, -3 * side);
						return;
					}
					else
					{
						arrive.Target = right_mid;
						arrive.Direction = bot.TheirGoal.RightPost - right_mid;
						return;
					}
				}

				// If it gets here without returning, drop out into the back boost section
			}

			// Now, we consider back boosts
			bool back_left_available;
			bool back_right_available;
			Vec3 left_back;
			Vec3 right_back;

			if (side == 1)
			{
				left_back = new Vec3(3072f, -4096f, 73f);
				right_back = new Vec3(-3072f, -4096f, 73f);
				back_left_available = Field.Boosts[4].IsActive || Field.Boosts[4].TimeUntilActive < Drive.GetEta(bot.Me, left_back) + 0.2;
				back_right_available = Field.Boosts[3].IsActive || Field.Boosts[3].TimeUntilActive < Drive.GetEta(bot.Me, right_back) + 0.2;
			}
            else
			{
				left_back = new Vec3(3072f, 4096f, 73f);
				right_back = new Vec3(-3072f, 4096f, 73f);
				back_left_available = Field.Boosts[30].IsActive || Field.Boosts[30].TimeUntilActive < Drive.GetEta(bot.Me, left_back) + 0.2;
				back_right_available = Field.Boosts[29].IsActive || Field.Boosts[29].TimeUntilActive < Drive.GetEta(bot.Me, right_back) + 0.2;
			}

			if (back_left_available && back_right_available)
            {
				// Need to choose which one
				if (ballInOne.location.x < 0)
                {
					// Get back left
					arrive.Target = left_back;
					arrive.Direction = bot.OurGoal.LeftPost - left_back;
					return;
                }
                else
                {
					// Get back right
					arrive.Target = right_back;
					arrive.Direction = bot.OurGoal.RightPost - right_back;
					return;
				}
            }

			if (back_left_available)
            {
				// Get back left
				arrive.Target = left_back;
				arrive.Direction = bot.OurGoal.LeftPost - left_back;
				return;
			}

			if (back_right_available)
            {
				// Get back right
				arrive.Target = right_back;
				arrive.Direction = bot.OurGoal.RightPost - right_back;
				return;
			}

			// If it gets here, there isn't much to do. Just rotate backpost.

			if (ballInOne.location.x < 0)
            {
				arrive.Target = new Vec3(1800, -4900 * side, 0);
				arrive.Direction = new Vec3(-1, 0, 0);
            }
            else
            {
				arrive.Target = new Vec3(-1800, -4900 * side, 0);
				arrive.Direction = new Vec3(1, 0, 0);
			}
		}
	}
}
