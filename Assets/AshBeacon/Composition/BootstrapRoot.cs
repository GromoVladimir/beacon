using System;
using System.Collections;
using System.Threading.Tasks;
using AshBeacon.Application;
using AshBeacon.UnityAdapters;
using UnityEngine;

namespace AshBeacon.Composition
{
    public sealed class BootstrapRoot : MonoBehaviour
    {
        public ApplicationFlow Flow { get; private set; }
        private bool restoringEntry;

        private void Awake()
        {
            // A repeated Bootstrap scene cannot replace the existing lifetime owner.
            foreach (var root in FindObjectsByType<BootstrapRoot>(FindObjectsInactive.Include))
            {
                if (root != this && root.Flow != null)
                {
                    if (!root.restoringEntry) root.StartCoroutine(root.RestoreEntry());
                    Destroy(gameObject);
                    return;
                }
            }
            Flow = new ApplicationFlow(new UnitySceneLoader());
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator RestoreEntry()
        {
            restoringEntry = true;
            yield return null;
            while (Flow.IsLoading) yield return null;
            RestoreMenu();
        }

        private async void RestoreMenu()
        {
            try
            {
                if (Flow.Screen == ApplicationScreen.Gameplay)
                    await NavigateAsync(Flow.ReturnToMenuAsync);
                else if (Flow.Screen == ApplicationScreen.MainMenu)
                {
                    await new UnitySceneLoader().LoadAsync(ApplicationScreen.MainMenu);
                    BindView();
                }
            }
            catch (Exception error) { Debug.LogException(error, this); }
            finally { restoringEntry = false; }
        }

        private async void Start()
        {
            if (Flow == null) return;
            try { await NavigateAsync(Flow.BootAsync); }
            catch (Exception error) { Debug.LogException(error, this); }
        }

        private async Task<bool> NavigateAsync(Func<Task<bool>> command)
        {
            if (!await command()) return false;
            BindView();
            return true;
        }

        private void BindView()
        {
            var view = FindAnyObjectByType<NavigationView>();
            if (view == null) throw new InvalidOperationException("Loaded scene has no NavigationView.");
            view.Initialize(Flow.Screen == ApplicationScreen.MainMenu
                ? () => NavigateAsync(Flow.StartPrototypeAsync)
                : () => NavigateAsync(Flow.ReturnToMenuAsync));
        }
    }
}
