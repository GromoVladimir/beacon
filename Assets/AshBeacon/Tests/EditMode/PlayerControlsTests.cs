using System;
using AshBeacon.Application;
using AshBeacon.Domain;
using NUnit.Framework;

namespace AshBeacon.Tests
{
    public sealed class PlayerControlsTests
    {
        [Test]
        public void AxisAndDiagonalHaveEqualSpeedButAnalogMagnitudeIsPreserved()
        {
            var movement = new PlayerMovement();
            var axis = movement.Read(1, 0);
            var diagonal = movement.Read(1, 1);
            Assert.That(Math.Sqrt(diagonal.X * diagonal.X + diagonal.Y * diagonal.Y), Is.EqualTo(axis.X).Within(1e-6));
            Assert.That(movement.Read(.25f, 0).X, Is.EqualTo(.25f));
            Assert.That(movement.Read(float.NaN, 1).Y, Is.Zero);
        }

        [Test]
        public void FacingKeepsAxisAcrossDiagonalNoiseAndChangesBeyondFifteenDegrees()
        {
            var movement = new PlayerMovement();
            movement.Read(1, 0);
            movement.Read(1, 1.7f);
            Assert.That(movement.Facing, Is.EqualTo(Facing.Right));
            movement.Read(1, 1.8f);
            Assert.That(movement.Facing, Is.EqualTo(Facing.Up));
            movement.Read(1.7f, 1);
            Assert.That(movement.Facing, Is.EqualTo(Facing.Up));
            movement.Read(1.8f, 1);
            Assert.That(movement.Facing, Is.EqualTo(Facing.Right));
            movement.Read(-1, 0);
            Assert.That(movement.Facing, Is.EqualTo(Facing.Left));
            movement.Read(0, -1);
            movement.Read(0, 0);
            Assert.That(movement.Facing, Is.EqualTo(Facing.Down));
        }

        [Test]
        public void BufferedPressExpiresAndCanOnlyBeConsumedOnce()
        {
            var input = new PlayerControls();
            input.Press(PlayerCommand.Item);
            input.Advance(.12);
            Assert.That(input.Consume(PlayerCommand.Item), Is.True);
            Assert.That(input.Consume(PlayerCommand.Item), Is.False);
            input.Press(PlayerCommand.Item);
            input.Advance(.121);
            Assert.That(input.Consume(PlayerCommand.Item), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => input.Advance(-1));
        }

        [Test]
        public void HoldingSwordDoesNotInventRepeatedPressesAndReleaseKeepsTapBuffered()
        {
            var input = new PlayerControls();
            input.SetSword(true);
            Assert.That(input.Consume(PlayerCommand.Sword), Is.True);
            input.SetSword(true);
            Assert.That(input.SwordHeld, Is.True);
            Assert.That(input.Consume(PlayerCommand.Sword), Is.False);
            input.SetSword(false);
            input.SetSword(true);
            input.SetSword(false);
            Assert.That(input.Consume(PlayerCommand.Sword), Is.True);
        }

        [Test]
        public void IndependentPauseOwnersClearAllInputWithoutResettingFacing()
        {
            var input = new PlayerControls();
            input.SetMove(-1, 0);
            input.SetSword(true);
            input.Press(PlayerCommand.Interact);
            input.SetPause(PauseReason.Menu, true);
            input.SetPause(PauseReason.Focus, true);
            input.SetPause(PauseReason.Background, true);
            input.SetPause(PauseReason.ItemWheel, true);
            input.SetPause(PauseReason.Menu, false);
            input.SetPause(PauseReason.Focus, false);
            Assert.That(input.IsPaused, Is.True);
            input.SetMove(1, 0);
            input.SetSword(true);
            input.Advance(100);
            Assert.That(input.Move.X, Is.Zero);
            Assert.That(input.SwordHeld, Is.False);
            input.SetPause(PauseReason.Background, false);
            input.SetPause(PauseReason.ItemWheel, false);
            Assert.That(input.IsPaused, Is.False);
            Assert.That(input.Consume(PlayerCommand.Sword), Is.False);
            Assert.That(input.Consume(PlayerCommand.Interact), Is.False);
            Assert.That(input.Movement.Facing, Is.EqualTo(Facing.Left));
        }
    }
}
