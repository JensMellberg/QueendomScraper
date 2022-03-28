using System;
using System.Collections.Generic;
using System.Text;

namespace QueendomScraper
{
    public class Counter
    {
        int counter = 0;
        int finishedCalls = 0;
        bool waitFlag = false;

        public void StartCall()
        {
            counter++;
            if (counter % 250 == 0) // Dont overload their server :)
            {
                waitFlag = true;
            } 
        }

        public bool ShouldWaitBeforeNextCall()
        {
            if (waitFlag)
            {
                waitFlag = false;
                return true;
            }

            return false;
        }


        public void FinishCall()
        {
            finishedCalls++;
        }

        public bool IsFinished() => finishedCalls == counter;
    }
}
