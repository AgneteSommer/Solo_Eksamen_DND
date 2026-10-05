using System.Collections;
using UnityEngine;
using DNDBeyond.Core;
using DNDBeyond.Data;

namespace DNDBeyond.Core
{
    public class PaperDollView : MonoBehaviour
    {
        [Header("Layer Sprite Renderers (0 to 7)")]
        [SerializeField] private SpriteRenderer pedestalRenderer;
        [SerializeField] private SpriteRenderer pedestalAuraRenderer;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer raceFeaturesRenderer;
        [SerializeField] private SpriteRenderer clothesRenderer;
        [SerializeField] private SpriteRenderer armorRenderer;
        [SerializeField] private SpriteRenderer hairRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;

        [Header("Default Base Sprites")]
        [SerializeField] private Sprite defaultBodySprite;
        [SerializeField] private Sprite defaultClothesSprite;
        [SerializeField] private Sprite defaultPedestalSprite;
        [SerializeField] private Sprite defaultHairSprite;

        [Header("Race Palette (Skin Tints)")]
        [SerializeField] private Color elfSkinTint = new Color(0.96f, 0.88f, 0.82f);
        [SerializeField] private Color tieflingSkinTint = new Color(0.85f, 0.35f, 0.38f);
        [SerializeField] private Color neutralSkinTint = new Color(0.92f, 0.85f, 0.80f);

        [Header("Class Aura Colors")]
        [SerializeField] private Color barbarianAura = new Color(0.95f, 0.3f, 0.15f, 0.6f);
        [SerializeField] private Color wizardAura = new Color(0.2f, 0.6f, 1.0f, 0.6f);
        [SerializeField] private Color neutralAura = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        private Coroutine punchCoroutine;
        private Vector3 baseScale = new Vector3(1.42f, 1.42f, 1.42f);

        private void Awake()
        {
            if (transform.localScale != Vector3.one)
            {
                baseScale = transform.localScale;
            }
        }

        public void SetBaseScale(Vector3 scale)
        {
            baseScale = scale;
            transform.localScale = scale;
        }

        private void Start()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated += UpdateVisuals;
                UpdateVisuals(CharacterCustomizerManager.Instance.CurrentBuild);
            }
        }

        private void OnDestroy()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated -= UpdateVisuals;
            }
        }

        public void UpdateVisuals(CharacterBuild build)
        {
            if (build == null) return;

            // 1. Race Features & Skin Tint
            if (build.currentRace != null)
            {
                if (build.RaceType == CharacterRace.Elf)
                {
                    if (bodyRenderer != null) bodyRenderer.color = elfSkinTint;
                    if (raceFeaturesRenderer != null)
                    {
                        raceFeaturesRenderer.sprite = build.currentRace.mannequinSprite;
                        raceFeaturesRenderer.color = build.currentRace.primaryColor;
                        raceFeaturesRenderer.enabled = raceFeaturesRenderer.sprite != null;
                    }
                }
                else if (build.RaceType == CharacterRace.Tiefling)
                {
                    if (bodyRenderer != null) bodyRenderer.color = tieflingSkinTint;
                    if (raceFeaturesRenderer != null)
                    {
                        raceFeaturesRenderer.sprite = build.currentRace.mannequinSprite;
                        raceFeaturesRenderer.color = build.currentRace.primaryColor;
                        raceFeaturesRenderer.enabled = raceFeaturesRenderer.sprite != null;
                    }
                }
            }
            else
            {
                if (bodyRenderer != null) bodyRenderer.color = neutralSkinTint;
                if (raceFeaturesRenderer != null) raceFeaturesRenderer.enabled = false;
            }

            // 2. Class Aura & Pedestal
            if (build.currentClass != null)
            {
                if (pedestalAuraRenderer != null)
                {
                    pedestalAuraRenderer.enabled = true;
                    Color c = build.currentClass.primaryColor;
                    pedestalAuraRenderer.color = new Color(c.r, c.g, c.b, 0.65f);
                    if (build.currentClass.mannequinSprite != null)
                        pedestalAuraRenderer.sprite = build.currentClass.mannequinSprite;
                }
            }
            else
            {
                if (pedestalAuraRenderer != null)
                {
                    pedestalAuraRenderer.enabled = false;
                }
            }

            // 3. Armor Layer
            if (build.currentArmor != null && build.currentArmor.mannequinSprite != null)
            {
                if (armorRenderer != null)
                {
                    armorRenderer.enabled = true;
                    armorRenderer.sprite = build.currentArmor.mannequinSprite;
                    armorRenderer.color = build.currentArmor.primaryColor;
                }
            }
            else
            {
                if (armorRenderer != null)
                {
                    armorRenderer.enabled = false;
                }
            }

            // 4. Weapon Layer
            if (build.currentWeapon != null && build.currentWeapon.mannequinSprite != null)
            {
                if (weaponRenderer != null)
                {
                    weaponRenderer.enabled = true;
                    weaponRenderer.sprite = build.currentWeapon.mannequinSprite;
                    weaponRenderer.color = build.currentWeapon.primaryColor;
                }
            }
            else
            {
                if (weaponRenderer != null)
                {
                    weaponRenderer.enabled = false;
                }
            }

            // Snappy feedback
            TriggerEquipPunch();
        }

        public void TriggerEquipPunch()
        {
            if (punchCoroutine != null) StopCoroutine(punchCoroutine);
            punchCoroutine = StartCoroutine(DoPunchScale());
        }

        private IEnumerator DoPunchScale()
        {
            Vector3 originalScale = baseScale;
            Vector3 targetScale = baseScale * 1.05f;

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
            SpriteRenderer pedestal,
            SpriteRenderer pedestalAura,
            SpriteRenderer body,
            SpriteRenderer raceFeatures,
            SpriteRenderer clothes,
            SpriteRenderer armor,
            SpriteRenderer hair,
            SpriteRenderer weapon)
        {
            pedestalRenderer = pedestal;
            pedestalAuraRenderer = pedestalAura;
            bodyRenderer = body;
            raceFeaturesRenderer = raceFeatures;
            clothesRenderer = clothes;
            armorRenderer = armor;
            hairRenderer = hair;
            weaponRenderer = weapon;
        }
    }
}
