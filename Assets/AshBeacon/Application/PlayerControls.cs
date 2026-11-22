using System;
using System.Collections.Generic;
using AshBeacon.Domain;

namespace AshBeacon.Application
{
    public enum PlayerCommand { Sword, Item, Interact }
    public enum PauseReason { Menu, ItemWheel, Focus, Background }

    public sealed class PlayerControls
    {
        private readonly HashSet<PauseReason> pauses = new HashSet<PauseReason>();
        private readonly Dictionary<PlayerCommand, double> presses = new Dictionary<PlayerCommand, double>();
        private double simulationTime;
        public const double BufferSeconds = 0.12;
        public bool IsPaused => pauses.Count != 0;
        public bool SwordHeld { get; private set; }
        public MoveVector Move { get; private set; }
        public PlayerMovement Movement { get; } = new PlayerMovement();

        public bool HasPause(PauseReason reason) => pauses.Contains(reason);
        public void SetPause(PauseReason reason, bool value)
        {
            if (value) { pauses.Add(reason); ClearInput(); }
            else pauses.Remove(reason);
        }
        public void SetMove(float x, float y)
        {
            if (!IsPaused) Move = Movement.Read(x, y);
        }
        public void Press(PlayerCommand command)
        {
            if (!IsPaused) presses[command] = simulationTime;
        }
        public void SetSword(bool held)
        {
            if (IsPaused) return;
            if (held && !SwordHeld) Press(PlayerCommand.Sword);
            SwordHeld = held;
        }
        public bool Consume(PlayerCommand command)
        {
            if (IsPaused || !presses.TryGetValue(command, out double pressedAt)) return false;
            presses.Remove(command);
            return simulationTime - pressedAt <= BufferSeconds + 1e-9;
        }
        public void Advance(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!IsPaused) simulationTime += seconds;
        }
        public void ClearInput()
        {
            Move = new MoveVector(0, 0);
            SwordHeld = false;
            presses.Clear();
        }
    }
}
