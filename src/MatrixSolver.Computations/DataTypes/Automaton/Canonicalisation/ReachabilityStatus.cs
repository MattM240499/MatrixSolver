namespace MatrixSolver.Computations.DataTypes.Automata.Canonicalisation
{
    /// <summary>
    /// Defines in what capacity a given path is reachable. Odd if it can be reached with an odd number of X's, and even if it can be reached with an even number.
    /// </summary>
    public class ReachabilityStatus
    {
        public bool EvenReachable { get; set; }
        public bool OddReachable { get; set; }
        public ReachabilityStatus()
        { }

        public static ReachabilityStatus Even()
        {
            return new ReachabilityStatus() { EvenReachable = true };
        }

        public ReachabilityStatus Times(ReachabilityStatus status)
        {
            var newRs = new ReachabilityStatus();
            newRs.EvenReachable = (status.EvenReachable && this.EvenReachable) || (status.OddReachable && this.OddReachable);
            newRs.OddReachable = (status.EvenReachable && this.OddReachable) || (status.OddReachable && this.EvenReachable);
            return newRs;
        }
    }
}