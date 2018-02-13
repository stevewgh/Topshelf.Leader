using System;

namespace Topshelf.Leader
{
    public class StubLeaseLengthCalculator : ILeaseLengthCalculator
    {
        private readonly TimeSpan fixedTime;

        public StubLeaseLengthCalculator(TimeSpan fixedTime)
        {
            this.fixedTime = fixedTime;
        }

        public TimeSpan Calculate()
        {
            return fixedTime;
        }
    }
}