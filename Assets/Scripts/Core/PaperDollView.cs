using System.Collections;
using UnityEngine;
using DNDBeyond.Core;
using DNDBeyond.Data;

namespace DNDBeyond.Core
{
    public class PaperDollView : MonoBehaviour
    {
        [Header("Layer Sprite Renderers")]
        [SerializeField] private SpriteRenderer bodyRenderer;           // Order 10 (Elf / Tiefling)
        [SerializeField] private SpriteRenderer armorRenderer;          // Order 20 (No Armor / Light / Medium / Heavy)
        [SerializeField] private SpriteRenderer hairRenderer;           // Order 30 (Hair 1 / Hair 2)
        [SerializeField] private SpriteRenderer hornsRenderer;          // Order 40 (Horn 1 - Renders on top of Hair!)
        [SerializeField] private SpriteRenderer weaponRenderer;         // Order 50 (Daggers / Great Sword / Long Bow / Staff)

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clothesApplySound;
        [SerializeField] private AudioClip metalArmorSound;
        [SerializeField] private AudioClip weaponEquipSound;

        private Coroutine punchCoroutine;
        private Vector3 baseScale = new Vector3(0.43f, 0.43f, 1.0f);
        private EquipmentSO lastArmor;
        private EquipmentSO lastWeapon;
        private bool isInitialUpdate = true;

        private void Awake()
        {
            if (transform.localScale != Vector3.one)
            {
                baseScale = transform.localScale;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f; // 2D sound
                }
            }
        }

        public void SetBaseScale(Vector3 scale)
        {
            baseScale = scale;
            transform.localScale = scale;
        }

        private void OnEnable()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated -= UpdateVisuals;
                CharacterCustomizerManager.Instance.OnCharacterUpdated += UpdateVisuals;
                UpdateVisuals(CharacterCustomizerManager.Instance.CurrentBuild);
            }
        }

        private void OnDisable()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated -= UpdateVisuals;
            }
        }

        public void UpdateVisuals(CharacterBuild build)
        {
            if (build == null) return;

            // Audio equip feedback triggers
            if (!isInitialUpdate)
            {
                if (build.currentArmor != lastArmor && build.currentArmor != null)
                {
                    if (build.currentArmor.armorType == ArmorType.None || build.currentArmor.armorType == ArmorType.Light)
                    {
                        PlaySound(clothesApplySound);
                    }
                    else if (build.currentArmor.armorType == ArmorType.Medium || build.currentArmor.armorType == ArmorType.Heavy)
                    {
                        PlaySound(metalArmorSound);
                    }
                }

                if (build.currentWeapon != lastWeapon && build.currentWeapon != null)
                {
                    PlaySound(weaponEquipSound);
                }
            }

            isInitialUpdate = false;
            lastArmor = build.currentArmor;
            lastWeapon = build.currentWeapon;

            // 10. Body Layer (Order 10: Elf / Tiefling)
            if (bodyRenderer != null)
            {
                if (build.currentRace != null && build.currentRace.mannequinSprite != null)
                {
                    bodyRenderer.enabled = true;
                    bodyRenderer.sprite = build.currentRace.mannequinSprite;
                    bodyRenderer.color = Color.white;
                }
                else
                {
                    bodyRenderer.enabled = false;
                }
            }

            // 20. Armor Layer (Order 20: No Armor / Light / Medium / Heavy)
            if (armorRenderer != null)
            {
                if (build.currentArmor != null && build.currentArmor.mannequinSprite != null)
                {
                    armorRenderer.enabled = true;
                    armorRenderer.sprite = build.currentArmor.mannequinSprite;
                    armorRenderer.color = Color.white;
                }
                else
                {
                    armorRenderer.enabled = false;
                }
            }

            // 30. Hair Layer (Order 30: Hair 1 / Hair 2)
            if (hairRenderer != null)
            {
                if (build.currentHair != null && build.currentHair.mannequinSprite != null)
                {
                    hairRenderer.enabled = true;
                    hairRenderer.sprite = build.currentHair.mannequinSprite;
                    hairRenderer.color = Color.white;
                }
                else
                {
                    hairRenderer.enabled = false;
                }
            }

            // 40. Horns Layer (Order 40: Horn 1 - Renders ON TOP of hair!)
            if (hornsRenderer != null)
            {
                if (build.currentHorns != null && build.currentHorns.mannequinSprite != null)
                {
                    hornsRenderer.enabled = true;
                    hornsRenderer.sprite = build.currentHorns.mannequinSprite;
                    hornsRenderer.color = Color.white;
                }
                else
                {
                    hornsRenderer.enabled = false;
                }
            }

            // 50. Weapon Layer (Order 50: Daggers / Great Sword / Long Bow / Staff)
            if (weaponRenderer != null)
            {
                if (build.currentWeapon != null && build.currentWeapon.mannequinSprite != null)
                {
                    weaponRenderer.enabled = true;
                    weaponRenderer.sprite = build.currentWeapon.mannequinSprite;
                    weaponRenderer.color = Color.white;
                }
                else
                {
                    weaponRenderer.enabled = false;
                }
            }

            // Snappy equip punch animation
            TriggerEquipPunch();
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public void TriggerEquipPunch()
        {
            if (punchCoroutine != null) StopCoroutine(punchCoroutine);
            punchCoroutine = StartCoroutine(DoPunchScale());
        }

        private IEnumerator DoPunchScale()
        {
            Vector3 originalScale = baseScale;
            Vector3 targetScale = baseScale * 1.04f;

            float elapsed = 0f;
            float duration = 0.08f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            elapsed = 0f;
            duration = 0.12f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }

        public void AssignRenderers(
            SpriteRenderer body,
            SpriteRenderer armor,
            SpriteRenderer hair,
            SpriteRenderer horns,
            SpriteRenderer weapon)
        {
            bodyRenderer = body;
            armorRenderer = armor;
            hairRenderer = hair;
            hornsRenderer = horns;
            weaponRenderer = weapon;
        }

        public void AssignAudio(AudioSource source, AudioClip clothesClip, AudioClip metalClip, AudioClip weaponClip)
        {
            audioSource = source;
            clothesApplySound = clothesClip;
            metalArmorSound = metalClip;
            weaponEquipSound = weaponClip;
        }
    }
}
