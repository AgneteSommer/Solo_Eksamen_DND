using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DNDBeyond.UI
{
    public class ReturnToMenuButton : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [SerializeField] private string menuSceneName = "MainMenu";
        [SerializeField] private int menuSceneIndex = 0;

        [Header("Audio")]
        [SerializeField] private AudioClip clickSound;
        private AudioSource audioSource;
        private bool isTransitioning = false;

        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnClick);
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        public void Setup(AudioClip clip, string sceneName = "MainMenu", int sceneIndex = 0)
        {
            clickSound = clip;
            menuSceneName = sceneName;
            menuSceneIndex = sceneIndex;
        }

        public void OnClick()
        {
            if (isTransitioning) return;
            StartCoroutine(DoReturn());
        }

        private IEnumerator DoReturn()
        {
            isTransitioning = true;
            if (clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            yield return new WaitForSecondsRealtime(0.08f);

            if (!string.IsNullOrEmpty(menuSceneName) && Application.CanStreamedLevelBeLoaded(menuSceneName))
            {
                SceneManager.LoadScene(menuSceneName);
            }
            else
            {
                SceneManager.LoadScene(menuSceneIndex);
            }
        }
    }
}
