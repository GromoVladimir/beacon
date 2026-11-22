namespace AshBeacon.Domain
{
    // Identity of one in-memory prototype run; contains no persistent progress.
    public sealed class GameSession
    {
        public GameSession(long runNumber)
        {
            if (runNumber <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(runNumber));
            RunNumber = runNumber;
        }

        public long RunNumber { get; }
    }
}
