using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace AshBeacon.UnityAdapters
{
    public sealed class NavigationView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button actionButton;
        [SerializeField] private TMP_Text statusText;
        private Func<Task<bool>> navigate;
        private bool busy;

        public UnityEngine.UI.Button ActionButton => actionButton;

        private void Awake()
        {
            actionButton.interactable = false;
            actionButton.onClick.AddListener(OnClicked);
        }

        public void Initialize(Func<Task<bool>> action)
        {
            navigate = action ?? throw new ArgumentNullException(nameof(action));
            actionButton.interactable = true;
        }

        private async void OnClicked()
        {
            if (busy || navigate == null) return;
            busy = true;
            actionButton.interactable = false;
            try { await navigate(); }
            catch (Exception)
            {
                if (this != null) statusText.text = "Не удалось открыть экран. Попробуйте ещё раз.";
            }
            finally
            {
                if (this != null)
                {
                    busy = false;
                    actionButton.interactable = true;
                }
            }
        }

        private void OnDestroy()
        {
            if (actionButton != null) actionButton.onClick.RemoveListener(OnClicked);
            navigate = null;
        }
    }
}
