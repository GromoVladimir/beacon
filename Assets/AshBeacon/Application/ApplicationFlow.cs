using System;
using System.Threading.Tasks;
using AshBeacon.Domain;

namespace AshBeacon.Application
{
    public enum ApplicationScreen { Boot, MainMenu, Gameplay }

    public interface ISceneLoader
    {
        Task LoadAsync(ApplicationScreen screen);
    }

    // Commands are issued on the application's main thread. No global instance.
    public sealed class ApplicationFlow
    {
        private readonly ISceneLoader scenes;
        private long lastRunNumber;

        public ApplicationFlow(ISceneLoader scenes)
        {
            this.scenes = scenes ?? throw new ArgumentNullException(nameof(scenes));
        }

        public ApplicationScreen Screen { get; private set; } = ApplicationScreen.Boot;
        public GameSession Session { get; private set; }
        public bool IsLoading { get; private set; }

        public Task<bool> BootAsync() => ChangeAsync(ApplicationScreen.Boot, ApplicationScreen.MainMenu);
        public Task<bool> StartPrototypeAsync() => ChangeAsync(ApplicationScreen.MainMenu, ApplicationScreen.Gameplay);
        public Task<bool> ReturnToMenuAsync() => ChangeAsync(ApplicationScreen.Gameplay, ApplicationScreen.MainMenu);

        private async Task<bool> ChangeAsync(ApplicationScreen expected, ApplicationScreen target)
        {
            if (IsLoading || Screen != expected)
                return false;

            // Prepare before loading so even an overflow cannot leave a half-committed transition.
            var nextSession = target == ApplicationScreen.Gameplay
                ? new GameSession(checked(lastRunNumber + 1)) : null;
            IsLoading = true;
            try
            {
                await scenes.LoadAsync(target);
                Session = nextSession;
                if (nextSession != null)
                    lastRunNumber = nextSession.RunNumber;
                Screen = target;
                return true;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
