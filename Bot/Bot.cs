using System;
using System.Threading;
using System.Drawing;
using RedUtils;
using RedUtils.Math;
/* 
 * This is the main file. It contains your bot class. Feel free to change the name!
 * An instance of this class will be created for each instance of your bot in the game.
 * Your bot derives from the "RedUtilsBot" class, contained in the Bot file inside the RedUtils project.
 * The run function listed below runs every tick, and should contain the custom strategy code (made by you!)
 * Right now though, it has a default ball chase strategy. Feel free to read up and use anything you like for your own strategy.
*/
namespace Bot
{
    // Your bot class! :D
    public partial class RedBot : RUBot
    {
        // We want the constructor for our Bot to extend from RUBot, but feel free to add some other initialization in here as well.
        public RedBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex) { }

        // Runs every tick. Should be used to find an Action to execute
        public override void Run()
        {
            // Prints out the current action to the screen, so we know what our bot is doing
            DrawText2D(Action != null ? Action.ToString() : "", Color.FromArgb(255, 255, 255), new Vec3(10, 10 + 60 * Index), 5, 5);

            if (IsKickoff && Action == null)
            {
                bool goingForKickoff = true; // by default, go for kickoff
                foreach (Car teammate in Teammates)
                {
                    // if any teammates are closer to the ball, then don't go for kickoff
                    goingForKickoff = goingForKickoff && Me.Location.Dist(Ball.Location) <= teammate.Location.Dist(Ball.Location);
                }

                Action = goingForKickoff ? new Kickoff() : new GetBoost(Me, false); // if we aren't going for the kickoff, get boost
            }
            else if (Action == null || (Action.Interruptible))
            {
               switch (Teammates.Count)
                {
                    case 0:
                        OnesLogic();
                        break;
                    case 1:
                        TwosLogic();
                        break;
                    case 2:
                        ThreesLogic();
                        break;
                    default:
                        HordeLogic();
                        break;

                }
			}
        }

        
    }
    public struct Line3
    {
        public Vec3 Slope;

        public Vec3 Point;

        public Vec3 PointAtY(float y_value)
        {
            float y_diff = y_value - Point.y;
            return Point + y_diff * Slope;
        }

        public Vec3 PointAtX(float x_value)
        {
            float x_diff = x_value - Point.y;
            return Point + x_diff * Slope;
        }

        public Vec3 PerpToPoint(Vec3 point)
        {
            return (point - Point) - (point - Point).Dot(Slope) * Slope;
        }

        public Line3(Vec3 point, Vec3 slope)
        {
            Slope = slope.Normalize();
            Point = point;
        }

        public Line3(Ball ball)
        {
            Slope = ball.velocity;
            Point = ball.location;
        }

        public Line3(Car car)
        {
            Slope = car.Velocity;
            Point = car.Location;
        }
    }
}
