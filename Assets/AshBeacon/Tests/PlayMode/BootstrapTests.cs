using System.Collections;
using AshBeacon.Application;
using AshBeacon.Composition;
using AshBeacon.UnityAdapters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AshBeacon.Tests
{
    public sealed class BootstrapTests
    {
        private BootstrapRoot root;

        private static IEnumerator WaitForScreen(ApplicationScreen screen)
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while (Time.realtimeSinceStartup < deadline)
            {
                var current = Object.FindAnyObjectByType<BootstrapRoot>();
                var view = Object.FindAnyObjectByType<NavigationView>();
                if (current != null && current.Flow != null && current.Flow.Screen == screen &&
                    !current.Flow.IsLoading && view != null && view.ActionButton.interactable)
                    yield break;
                yield return null;
            }
            Assert.Fail("Timed out waiting for screen: " + screen);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach (var previous in Object.FindObjectsByType<BootstrapRoot>())
            {
                while (previous.Flow != null && previous.Flow.IsLoading) yield return null;
                Object.Destroy(previous.gameObject);
            }
            yield return null;
            yield return SceneManager.LoadSceneAsync(UnitySceneLoader.BootstrapPath);
            yield return WaitForScreen(ApplicationScreen.MainMenu);
            root = Object.FindAnyObjectByType<BootstrapRoot>();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var owner in Object.FindObjectsByType<BootstrapRoot>())
                Object.Destroy(owner.gameObject);
            yield return null;
            var previousScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("T02TestCleanup"));
            yield return SceneManager.UnloadSceneAsync(previousScene);
        }

        [UnityTest]
        public IEnumerator MenuButtonLoadsEmptyRoomAndRejectsDoubleClick()
        {
            Assert.That(root.Flow.Session, Is.Null);
            var button = Object.FindAnyObjectByType<NavigationView>().ActionButton;
            button.onClick.Invoke();
            button.onClick.Invoke();
            yield return WaitForScreen(ApplicationScreen.Gameplay);
            yield return null;
            Assert.That(Object.FindAnyObjectByType<BootstrapRoot>(), Is.SameAs(root));
            Assert.That(root.Flow.Session.RunNumber, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<BootstrapRoot>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>().Length, Is.EqualTo(1));
            var room = GameObject.Find("TestRoom");
            Assert.That(room, Is.Not.Null);
            Assert.That(room.GetComponentsInChildren<BoxCollider2D>().Length, Is.EqualTo(4));
            Assert.That(room.transform.Find("Floor").localScale, Is.EqualTo(new Vector3(20,12,1)));
            Assert.That(Camera.main.orthographic, Is.True);
            Assert.That(Camera.main.orthographicSize, Is.GreaterThanOrEqualTo(6));
            Assert.That(Camera.main.orthographicSize * Camera.main.aspect, Is.GreaterThanOrEqualTo(10));
        }

        [UnityTest]
        public IEnumerator ReturnDisposesSessionAndNewStartCreatesAnother()
        {
            Object.FindAnyObjectByType<NavigationView>().ActionButton.onClick.Invoke();
            yield return WaitForScreen(ApplicationScreen.Gameplay);
            var first = root.Flow.Session;
            Object.FindAnyObjectByType<NavigationView>().ActionButton.onClick.Invoke();
            yield return WaitForScreen(ApplicationScreen.MainMenu);
            Assert.That(root.Flow.Session, Is.Null);
            Assert.That(GameObject.Find("TestRoom"), Is.Null);
            Object.FindAnyObjectByType<NavigationView>().ActionButton.onClick.Invoke();
            yield return WaitForScreen(ApplicationScreen.Gameplay);
            Assert.That(root.Flow.Session, Is.Not.SameAs(first));
            Assert.That(root.Flow.Session.RunNumber, Is.EqualTo(2));
            Assert.That(Object.FindAnyObjectByType<BootstrapRoot>(), Is.SameAs(root));
        }

        [UnityTest]
        public IEnumerator ReenterBootstrapFromGameplayAndMenuKeepsOneOwner()
        {
            Object.FindAnyObjectByType<NavigationView>().ActionButton.onClick.Invoke();
            yield return WaitForScreen(ApplicationScreen.Gameplay);
            yield return SceneManager.LoadSceneAsync(UnitySceneLoader.BootstrapPath);
            yield return WaitForScreen(ApplicationScreen.MainMenu);
            Assert.That(root.Flow.Session, Is.Null);
            yield return SceneManager.LoadSceneAsync(UnitySceneLoader.BootstrapPath);
            yield return WaitForScreen(ApplicationScreen.MainMenu);
            Assert.That(Object.FindObjectsByType<BootstrapRoot>().Length, Is.EqualTo(1));
            Assert.That(Object.FindAnyObjectByType<BootstrapRoot>(), Is.SameAs(root));
            Assert.That(Object.FindObjectsByType<EventSystem>().Length, Is.EqualTo(1));
        }
    }
}
