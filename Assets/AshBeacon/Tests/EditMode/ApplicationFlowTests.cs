using System;
using System.Threading.Tasks;
using AshBeacon.Application;
using NUnit.Framework;

namespace AshBeacon.Tests
{
    public sealed class ApplicationFlowTests
    {
        private sealed class Loader : ISceneLoader
        {
            public int Calls;
            public Func<Task> Load = () => Task.CompletedTask;
            public Task LoadAsync(ApplicationScreen screen) { Calls++; return Load(); }
        }

        [Test]
        public async Task FullCycleReplacesSessionWithoutRebootingServices()
        {
            var loader = new Loader();
            var flow = new ApplicationFlow(loader);
            Assert.That(await flow.BootAsync(), Is.True);
            Assert.That(flow.Screen, Is.EqualTo(ApplicationScreen.MainMenu));
            Assert.That(await flow.StartPrototypeAsync(), Is.True);
            var first = flow.Session;
            Assert.That(flow.Screen, Is.EqualTo(ApplicationScreen.Gameplay));
            Assert.That(first.RunNumber, Is.EqualTo(1));
            Assert.That(await flow.ReturnToMenuAsync(), Is.True);
            Assert.That(flow.Session, Is.Null);
            Assert.That(await flow.StartPrototypeAsync(), Is.True);
            Assert.That(flow.Session, Is.Not.SameAs(first));
            Assert.That(flow.Session.RunNumber, Is.EqualTo(2));
            Assert.That(loader.Calls, Is.EqualTo(4));
        }

        [Test]
        public async Task LoadingRejectsRepeatedAndOutOfOrderCommands()
        {
            var pending = new TaskCompletionSource<bool>();
            var loader = new Loader { Load = () => pending.Task };
            var flow = new ApplicationFlow(loader);
            Assert.That(await flow.StartPrototypeAsync(), Is.False);
            Assert.That(await flow.ReturnToMenuAsync(), Is.False);
            var boot = flow.BootAsync();
            Assert.That(flow.IsLoading, Is.True);
            Assert.That(await flow.BootAsync(), Is.False);
            Assert.That(await flow.StartPrototypeAsync(), Is.False);
            Assert.That(flow.Screen, Is.EqualTo(ApplicationScreen.Boot));
            pending.SetResult(true);
            Assert.That(await boot, Is.True);
            Assert.That(flow.IsLoading, Is.False);
            Assert.That(await flow.BootAsync(), Is.False);
            Assert.That(loader.Calls, Is.EqualTo(1));
        }

        [Test]
        public async Task FailedStartCanBeRetriedWithoutConsumingSessionNumber()
        {
            var loader = new Loader();
            var flow = new ApplicationFlow(loader);
            await flow.BootAsync();
            loader.Load = () => Task.FromException(new InvalidOperationException("load failed"));
            Assert.ThrowsAsync<InvalidOperationException>(async () => { await flow.StartPrototypeAsync(); });
            Assert.That(flow.Screen, Is.EqualTo(ApplicationScreen.MainMenu));
            Assert.That(flow.Session, Is.Null);
            Assert.That(flow.IsLoading, Is.False);
            loader.Load = () => Task.CompletedTask;
            Assert.That(await flow.StartPrototypeAsync(), Is.True);
            Assert.That(flow.Session.RunNumber, Is.EqualTo(1));
        }

        [Test]
        public async Task FailedReturnKeepsExistingSession()
        {
            var loader = new Loader();
            var flow = new ApplicationFlow(loader);
            await flow.BootAsync();
            await flow.StartPrototypeAsync();
            var session = flow.Session;
            loader.Load = () => throw new InvalidOperationException("synchronous load failure");
            Assert.ThrowsAsync<InvalidOperationException>(async () => { await flow.ReturnToMenuAsync(); });
            Assert.That(flow.Screen, Is.EqualTo(ApplicationScreen.Gameplay));
            Assert.That(flow.Session, Is.SameAs(session));
            Assert.That(flow.IsLoading, Is.False);
            loader.Load = () => Task.CompletedTask;
            Assert.That(await flow.ReturnToMenuAsync(), Is.True);
            Assert.That(flow.Session, Is.Null);
        }

        [Test]
        public void CoreAssembliesHaveNoUnityDependencies()
        {
            foreach (var assembly in new[] { typeof(ApplicationFlow).Assembly, typeof(AshBeacon.Domain.GameSession).Assembly })
                foreach (var reference in assembly.GetReferencedAssemblies())
                    Assert.That(reference.Name.StartsWith("Unity"), Is.False, assembly.GetName().Name + " -> " + reference.Name);
        }

        [Test]
        public void MissingLoaderIsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new ApplicationFlow(null));
        }
    }
}
