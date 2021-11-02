using RedUtils;
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
            // search for the first avaliable shot using DefaultShotCheck
            Shot shot = FindShot(DefaultShotCheck, new Target(TheirGoal));

            // if a shot is found, go for the shot. Otherwise, if there is an Action to execute, execute it. If none of the others apply, drive back to goal.
            Action = shot ?? Action ?? new Drive(Me, OurGoal.Location);
        }

    }
}
