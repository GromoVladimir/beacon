using System;
using System.Threading.Tasks;
using AshBeacon.Application;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AshBeacon.UnityAdapters
{
    public sealed class UnitySceneLoader : ISceneLoader
    {
        public const string BootstrapPath = "Assets/Scenes/Bootstrap.unity";
        public const string MenuPath = "Assets/Scenes/MainMenu.unity";
        public const string GameplayPath = "Assets/Scenes/Gameplay.unity";

        public async Task LoadAsync(ApplicationScreen screen)
        {
            string path;
            switch (screen)
            {
                case ApplicationScreen.MainMenu: path = MenuPath; break;
                case ApplicationScreen.Gameplay: path = GameplayPath; break;
                default: throw new ArgumentOutOfRangeException(nameof(screen));
            }
            var operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            if (operation == null) throw new InvalidOperationException("Scene load could not start: " + path);
            await operation;
        }
    }
}
