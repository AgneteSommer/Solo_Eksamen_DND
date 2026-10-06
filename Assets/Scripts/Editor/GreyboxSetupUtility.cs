using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;
using DNDBeyond.Data;
using DNDBeyond.Core;
using DNDBeyond.UI;

namespace DNDBeyond.Editor
{
    public static class GreyboxSetupUtility
    {
        private const string SPRITES_DIR = "Assets/Sprites";
        private const string DATA_DIR = "Assets/Data";
        private const string RULES_DIR = "Assets/Data/Rules";
        private const string PREFABS_DIR = "Assets/Prefabs";

        [MenuItem("DND Beyond/0. Setup All Scenes & Build Settings")]
        public static void SetupAllScenesAndBuildSettings()
        {
            GenerateAllAssets();

            // 1. Build MainMenu Scene
            BuildMainMenuScene();

            // 2. Build SampleScene
            var sampleScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(sampleScene);
            BuildCompleteScene();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sampleScene, "Assets/Scenes/SampleScene.unity");

            // 3. Register Build Settings
            RegisterBuildSettings();

            // 4. Return to MainMenu Scene for testing
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);

            Debug.Log("<color=#44FF88><b>D&D Beyond:</b> All Scenes & Build Settings Built & Configured Successfully!</color>");
        }

        [MenuItem("DND Beyond/1. Generate Greybox Sprites & Assets")]
        public static void GenerateAllAssets()
        {
            EnsureDirectories();
            GenerateAllSprites();
            AssetDatabase.Refresh();

            var options = CreateScriptableObjects();
            var rules = CreateHarmonyRules();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#2EA3FF><b>D&D Beyond:</b> Generated {options.Count} Options (including all 12 classes) and {rules.Count} Synergy/Quirk Rules!</color>");
        }

        [MenuItem("DND Beyond/2. Build Complete Greybox Scene")]
        public static void BuildCompleteScene()
        {
            GenerateAllAssets();

            // Setup Camera
            SetupCamera();

            // Setup Background
            SetupSceneBackground();

            // Setup Mannequin grounded with platform directly under soles of feet
            var mannequinObj = SetupMannequin();

            // Setup Event System
            SetupEventSystem();

            // Setup Canvas with Bottom-Left Mini Sheet and Right-Side Thick Drill-Down Menu
            SetupCanvas(mannequinObj);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#44FF88><b>D&D Beyond:</b> Complete Refactored Scene Successfully Built and Saved!</color>");
        }

        [MenuItem("DND Beyond/3. Build Main Menu Scene")]
        public static void BuildMainMenuScene()
        {
            EnsureDirectories();
            string scenePath = "Assets/Scenes/MainMenu.unity";
            UnityEngine.SceneManagement.Scene menuScene;
            if (File.Exists(scenePath))
            {
                menuScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            }
            else
            {
                menuScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                    UnityEditor.SceneManagement.NewSceneMode.Single);
            }

            // Clean existing objects
            var rootObjects = menuScene.GetRootGameObjects();
            for (int i = rootObjects.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(rootObjects[i]);
            }

            // 1. Camera
            var camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camObj.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.05f, 1f);
            cam.transform.position = new Vector3(0, 0, -10f);

            // 2. Event System
            var esGO = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));

            // 3. Canvas
            var canvasGO = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 10f;

            // 4. Aspect Ratio Container (aspect ratio 2880 / 1526 = 1.887287f)
            GameObject containerGO = new GameObject("MenuContainer", typeof(RectTransform), typeof(AspectRatioFitter));
            containerGO.transform.SetParent(canvasGO.transform, false);
            var cRect = containerGO.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.anchoredPosition = Vector2.zero;
            cRect.sizeDelta = new Vector2(1920, 1080);

            var fitter = containerGO.GetComponent<AspectRatioFitter>();
            fitter.aspectRatio = 2880f / 1526f;
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            // 5. Main Menu Background Image (Full screen base layer)
            GameObject bgImgGO = new GameObject("BackgroundImage", typeof(RectTransform), typeof(Image));
            bgImgGO.transform.SetParent(containerGO.transform, false);
            var bgRect = bgImgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgImgGO.GetComponent<Image>();
            bgImg.sprite = LoadSubSprite("Assets/Sprites/Main Menu/MainMenu_Background.png");
            bgImg.preserveAspect = false;

            // 6. Main Menu Button Overlay Image (Full screen 1:1 overlay, non-interactive)
            GameObject overlayImgGO = new GameObject("ButtonOverlayImage", typeof(RectTransform), typeof(Image));
            overlayImgGO.transform.SetParent(containerGO.transform, false);
            var overlayRect = overlayImgGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            var overlayImg = overlayImgGO.GetComponent<Image>();
            overlayImg.sprite = LoadSubSprite("Assets/Sprites/Main Menu/MainMenu_Button.png");
            overlayImg.preserveAspect = false;
            overlayImg.raycastTarget = false;

            // 7. Interactive Button Hit Target (strictly over Visual Designer card bounds: X[417..2430], Y[28..455])
            // anchorMin = (417/2880, 28/1526) ≈ (0.1448, 0.0183)
            // anchorMax = (2430/2880, 455/1526) ≈ (0.8438, 0.2982)
            GameObject btnGO = new GameObject("VisualDesignerButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MainMenuController));
            btnGO.transform.SetParent(containerGO.transform, false);
            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(417f / 2880f, 28f / 1526f);
            btnRect.anchorMax = new Vector2(2430f / 2880f, 455f / 1526f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnImg = btnGO.GetComponent<Image>();
            btnImg.color = new Color(0f, 0f, 0f, 0f); // Transparent raycast target
            btnImg.raycastTarget = true;

            var btn = btnGO.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnColors = btn.colors;
            btnColors.normalColor = Color.clear;
            btnColors.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
            btnColors.pressedColor = new Color(0f, 0f, 0f, 0.12f);
            btn.colors = btnColors;

            var controller = btnGO.GetComponent<MainMenuController>();
            var clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/UI_Click.wav");
            controller.Setup(clickClip, "SampleScene", 1, overlayImg);

            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(menuScene, scenePath);
            Debug.Log("<color=#44FF88><b>D&D Beyond:</b> MainMenu scene successfully built and saved!</color>");
        }

        [MenuItem("DND Beyond/4. Register Build Settings")]
        public static void RegisterBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("<color=#44FF88><b>D&D Beyond:</b> Build Settings registered (0: MainMenu, 1: SampleScene).</color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(SPRITES_DIR)) Directory.CreateDirectory(SPRITES_DIR);
            if (!Directory.Exists(DATA_DIR)) Directory.CreateDirectory(DATA_DIR);
            if (!Directory.Exists(RULES_DIR)) Directory.CreateDirectory(RULES_DIR);
            if (!Directory.Exists(PREFABS_DIR)) Directory.CreateDirectory(PREFABS_DIR);
        }

        #region Sprite Generation (High-Res 512x512)

        private static void GenerateAllSprites()
        {
            CreateAndSaveSprite("spr_pedestal", 512, 256, DrawPedestal);
            CreateAndSaveSprite("spr_pedestal_aura", 512, 256, DrawPedestalAura);
            CreateAndSaveSprite("spr_body", 512, 512, DrawBodySilhouette);
            CreateAndSaveSprite("spr_hair", 512, 512, DrawHair);
            CreateAndSaveSprite("spr_clothes", 512, 512, DrawClothes);
            CreateAndSaveSprite("spr_elf_ears", 512, 512, DrawElfEars);
            CreateAndSaveSprite("spr_tiefling_horns", 512, 512, DrawTieflingHornsAndTail);
            CreateAndSaveSprite("spr_armor_robes", 512, 512, DrawRobesArmor);
            CreateAndSaveSprite("spr_armor_light", 512, 512, DrawLightLeatherArmor);
            CreateAndSaveSprite("spr_armor_medium", 512, 512, DrawMediumScaleArmor);
            CreateAndSaveSprite("spr_armor_heavy", 512, 512, DrawHeavyPlateArmor);
            CreateAndSaveSprite("spr_weapon_greataxe", 512, 512, DrawGreataxe);
            CreateAndSaveSprite("spr_weapon_staff", 512, 512, DrawArcaneStaff);
            CreateAndSaveSprite("spr_weapon_dagger", 512, 512, DrawDagger);

            CreateAndSaveSprite("spr_d20", 256, 256, DrawD20);
            CreateAndSaveSprite("spr_card_frame", 128, 128, DrawCardFrame, border: new Vector4(12, 12, 12, 12));
            CreateAndSaveSprite("spr_box_white", 64, 64, DrawSolidBox, border: new Vector4(8, 8, 8, 8));
        }

        private static void CreateAndSaveSprite(string name, int width, int height, System.Action<Texture2D> drawAction, Vector4? border = null)
        {
            string path = $"{SPRITES_DIR}/{name}.png";
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color[] clear = new Color[width * height];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            tex.SetPixels(clear);

            drawAction(tex);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                if (border.HasValue)
                {
                    importer.spriteBorder = border.Value;
                }
                importer.SaveAndReimport();
            }
        }

        private static void DrawSolidBox(Texture2D tex)
        {
            Color borderCol = new Color(0.88f, 0.88f, 0.88f, 1f); // #E0E0E0
            Color fillCol = Color.white;

            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    bool isBorder = x <= 2 || x >= tex.width - 3 || y <= 2 || y >= tex.height - 3;
                    tex.SetPixel(x, y, isBorder ? borderCol : fillCol);
                }
            }
        }

        private static void DrawPedestal(Texture2D tex)
        {
            int cx = tex.width / 2;
            int cy = 90;
            int rx = 210;
            int ry = 70;

            for (int y = 30; y < 110; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    if (dx * dx <= 1f)
                    {
                        float shade = 0.30f + 0.15f * (1f - dx * dx);
                        tex.SetPixel(x, y, new Color(shade, shade + 0.02f, shade + 0.05f, 1f));
                    }
                }
            }

            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    float distSq = dx * dx + dy * dy;
                    if (distSq <= 1f)
                    {
                        float rim = 0.55f + 0.15f * dy;
                        tex.SetPixel(x, y, new Color(rim, rim + 0.03f, rim + 0.08f, 1f));
                    }
                }
            }
        }

        private static void DrawPedestalAura(Texture2D tex)
        {
            int cx = tex.width / 2;
            int cy = 90;
            int rx = 200;
            int ry = 64;

            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    float distSq = dx * dx + dy * dy;
                    if (distSq <= 1f)
                    {
                        float ring = Mathf.Abs(Mathf.Sqrt(distSq) - 0.78f);
                        if (ring < 0.16f)
                        {
                            float alpha = (1f - ring / 0.16f) * 0.9f;
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                        }
                    }
                }
            }
        }

        private static void DrawBodySilhouette(Texture2D tex)
        {
            int cx = tex.width / 2;

            // Head
            DrawFilledCircle(tex, cx, 410, 44, Color.white);
            // Neck
            DrawFilledRect(tex, cx - 12, 355, 24, 20, Color.white);
            // Torso
            DrawFilledRect(tex, cx - 44, 210, 88, 150, Color.white);
            // Arms
            DrawFilledRect(tex, cx - 72, 220, 24, 130, Color.white);
            DrawFilledRect(tex, cx + 48, 220, 24, 130, Color.white);
            // Legs
            DrawFilledRect(tex, cx - 36, 60, 28, 150, Color.white);
            DrawFilledRect(tex, cx + 8, 60, 28, 150, Color.white);
            // Feet: soles are at Y = 44
            DrawFilledRect(tex, cx - 44, 44, 36, 20, Color.white);
            DrawFilledRect(tex, cx + 8, 44, 36, 20, Color.white);
        }

        private static void DrawHair(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color hairColor = new Color(0.2f, 0.15f, 0.12f, 1f);
            DrawFilledCircle(tex, cx, 436, 44, hairColor);
            DrawFilledRect(tex, cx - 48, 390, 16, 44, hairColor);
            DrawFilledRect(tex, cx + 32, 390, 16, 44, hairColor);
        }

        private static void DrawClothes(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color underColor = new Color(0.48f, 0.44f, 0.40f, 1f);
            DrawFilledRect(tex, cx - 40, 210, 80, 120, underColor);
            DrawFilledRect(tex, cx - 34, 90, 24, 120, underColor * 0.9f);
            DrawFilledRect(tex, cx + 10, 90, 24, 120, underColor * 0.9f);
        }

        private static void DrawElfEars(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color earColor = new Color(0.96f, 0.88f, 0.82f, 1f);
            for (int i = 0; i < 40; i++)
            {
                int x = cx - 44 - i;
                int y = 410 + (i / 2);
                DrawFilledCircle(tex, x, y, 7, earColor);
            }
            for (int i = 0; i < 40; i++)
            {
                int x = cx + 44 + i;
                int y = 410 + (i / 2);
                DrawFilledCircle(tex, x, y, 7, earColor);
            }
        }

        private static void DrawTieflingHornsAndTail(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color hornColor = new Color(0.25f, 0.12f, 0.15f, 1f);

            for (int t = 0; t <= 60; t++)
            {
                float angle = Mathf.PI * 0.65f + (t / 60f) * 0.7f;
                int hx = cx - 24 - (int)(Mathf.Cos(angle) * (40 + t * 1.8f));
                int hy = 430 + (int)(Mathf.Sin(angle) * (60 + t * 1.2f));
                DrawFilledCircle(tex, hx, hy, 10 - (t / 8), hornColor);
            }

            for (int t = 0; t <= 60; t++)
            {
                float angle = Mathf.PI * 0.35f - (t / 60f) * 0.7f;
                int hx = cx + 24 + (int)(Mathf.Cos(angle) * (40 + t * 1.8f));
                int hy = 430 + (int)(Mathf.Sin(angle) * (60 + t * 1.2f));
                DrawFilledCircle(tex, hx, hy, 10 - (t / 8), hornColor);
            }

            Color tailColor = new Color(0.85f, 0.35f, 0.38f, 1f);
            for (int t = 0; t <= 80; t++)
            {
                float rad = (t / 80f) * Mathf.PI;
                int tx = cx - 36 - (int)(Mathf.Sin(rad) * 70);
                int ty = 190 - t * 2 + (int)(Mathf.Cos(rad) * 30);
                DrawFilledCircle(tex, tx, ty, 8 - (t / 14), tailColor);
            }
        }

        private static void DrawRobesArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color robeColor = new Color(0.35f, 0.28f, 0.55f, 1f);
            Color trimColor = new Color(0.92f, 0.78f, 0.3f, 1f);

            DrawFilledRect(tex, cx - 48, 120, 96, 230, robeColor);
            DrawFilledRect(tex, cx - 76, 240, 32, 100, robeColor);
            DrawFilledRect(tex, cx + 44, 240, 32, 100, robeColor);
            DrawFilledRect(tex, cx - 8, 120, 16, 230, trimColor);
            DrawFilledRect(tex, cx - 44, 230, 88, 16, trimColor);
        }

        private static void DrawLightLeatherArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color leather = new Color(0.55f, 0.35f, 0.20f, 1f);
            Color darkLeather = new Color(0.38f, 0.22f, 0.12f, 1f);

            DrawFilledRect(tex, cx - 46, 210, 92, 150, leather);
            DrawFilledRect(tex, cx - 52, 320, 20, 40, darkLeather);
            DrawFilledRect(tex, cx + 32, 320, 20, 40, darkLeather);
            for (int i = 0; i < 80; i++)
            {
                DrawFilledCircle(tex, cx - 40 + i, 340 - i, 6, darkLeather);
            }
        }

        private static void DrawMediumScaleArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color bronze = new Color(0.70f, 0.52f, 0.28f, 1f);
            Color iron = new Color(0.45f, 0.48f, 0.52f, 1f);

            DrawFilledRect(tex, cx - 50, 210, 100, 150, iron);
            for (int row = 0; row < 5; row++)
            {
                int y = 230 + row * 24;
                for (int col = -2; col <= 2; col++)
                {
                    DrawFilledCircle(tex, cx + col * 18, y, 10, bronze);
                }
            }
            DrawFilledCircle(tex, cx - 56, 340, 20, bronze);
            DrawFilledCircle(tex, cx + 56, 340, 20, bronze);
        }

        private static void DrawHeavyPlateArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color steel = new Color(0.82f, 0.85f, 0.90f, 1f);
            Color steelDark = new Color(0.52f, 0.56f, 0.62f, 1f);
            Color goldTrim = new Color(0.92f, 0.75f, 0.25f, 1f);

            DrawFilledRect(tex, cx - 52, 210, 104, 150, steel);
            DrawFilledRect(tex, cx - 6, 220, 12, 140, steelDark);
            DrawFilledRect(tex, cx - 84, 310, 36, 52, steel);
            DrawFilledRect(tex, cx + 48, 310, 36, 52, steel);
            DrawFilledRect(tex, cx - 84, 356, 36, 8, goldTrim);
            DrawFilledRect(tex, cx + 48, 356, 36, 8, goldTrim);
            DrawFilledRect(tex, cx - 48, 170, 96, 44, steelDark);
            DrawFilledRect(tex, cx - 38, 64, 32, 100, steel);
            DrawFilledRect(tex, cx + 6, 64, 32, 100, steel);
        }

        private static void DrawGreataxe(Texture2D tex)
        {
            int cx = tex.width / 2 + 110;
            Color wood = new Color(0.45f, 0.28f, 0.15f, 1f);
            Color steel = new Color(0.85f, 0.88f, 0.92f, 1f);
            Color steelEdge = new Color(0.96f, 0.98f, 1f, 1f);

            DrawFilledRect(tex, cx - 8, 60, 16, 360, wood);

            int hy = 350;
            for (int r = 0; r <= 64; r++)
            {
                int x = cx - r;
                int h = 30 + (int)(r * 0.9f);
                DrawFilledRect(tex, x - 4, hy - h / 2, 8, h, r > 52 ? steelEdge : steel);
            }
            for (int r = 0; r <= 64; r++)
            {
                int x = cx + r;
                int h = 30 + (int)(r * 0.9f);
                DrawFilledRect(tex, x - 4, hy - h / 2, 8, h, r > 52 ? steelEdge : steel);
            }
            DrawFilledCircle(tex, cx, hy, 20, new Color(0.3f, 0.3f, 0.35f, 1f));
        }

        private static void DrawArcaneStaff(Texture2D tex)
        {
            int cx = tex.width / 2 + 110;
            Color wood = new Color(0.52f, 0.35f, 0.22f, 1f);
            Color crystal = new Color(0.35f, 0.75f, 1.0f, 1f);
            Color glow = new Color(0.6f, 0.9f, 1.0f, 0.75f);

            DrawFilledRect(tex, cx - 8, 50, 16, 370, wood);
            DrawFilledRect(tex, cx - 24, 390, 12, 50, wood);
            DrawFilledRect(tex, cx + 12, 390, 12, 50, wood);

            DrawFilledCircle(tex, cx, 420, 32, glow);
            DrawFilledCircle(tex, cx, 420, 22, crystal);
            DrawFilledCircle(tex, cx - 6, 426, 8, Color.white);
        }

        private static void DrawDagger(Texture2D tex)
        {
            int cx = tex.width / 2 + 90;
            int cy = 220;
            Color steel = new Color(0.85f, 0.88f, 0.95f, 1f);
            Color gold = new Color(0.92f, 0.75f, 0.25f, 1f);
            Color grip = new Color(0.2f, 0.2f, 0.2f, 1f);

            DrawFilledRect(tex, cx - 6, cy - 70, 12, 60, grip);
            DrawFilledCircle(tex, cx, cy - 72, 12, gold);
            DrawFilledRect(tex, cx - 32, cy - 10, 64, 12, gold);

            for (int y = 0; y < 110; y++)
            {
                int halfW = (int)(16f * (1f - (float)y / 110f));
                for (int x = cx - halfW; x <= cx + halfW; x++)
                {
                    tex.SetPixel(x, cy + y, steel);
                }
            }
        }

        private static void DrawD20(Texture2D tex)
        {
            int cx = tex.width / 2;
            int cy = tex.height / 2;
            int r = 105;

            Color gold = new Color(0.95f, 0.80f, 0.25f, 1f);
            Color goldDark = new Color(0.65f, 0.48f, 0.1f, 1f);

            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    if (dist <= r)
                    {
                        float innerRatio = dist / r;
                        tex.SetPixel(x, y, Color.Lerp(gold, goldDark, innerRatio));
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                int px = cx + (int)(Mathf.Cos(a) * r);
                int py = cy + (int)(Mathf.Sin(a) * r);
                DrawLine(tex, cx, cy, px, py, Color.white);
            }

            DrawFilledCircle(tex, cx, cy, 32, new Color(0.18f, 0.14f, 0.05f, 0.95f));
        }

        private static void DrawCardFrame(Texture2D tex)
        {
            Color border = new Color(0.88f, 0.88f, 0.88f, 1f); // #E0E0E0
            Color fill = Color.white;

            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    bool isBorder = x <= 2 || x >= tex.width - 3 || y <= 2 || y >= tex.height - 3;
                    tex.SetPixel(x, y, isBorder ? border : fill);
                }
            }
        }

        private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int r, Color col)
        {
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        int dx = x - cx;
                        int dy = y - cy;
                        if (dx * dx + dy * dy <= r * r)
                        {
                            tex.SetPixel(x, y, col);
                        }
                    }
                }
            }
        }

        private static void DrawFilledRect(Texture2D tex, int x, int y, int w, int h, Color col)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    {
                        tex.SetPixel(px, py, col);
                    }
                }
            }
        }

        private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height)
                {
                    tex.SetPixel(x0, y0, col);
                }
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        #endregion

        #region ScriptableObjects Creation (All 12 Classes in Alphabetical Order)

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITES_DIR}/{name}.png");
        }

        private static Sprite LoadClassLogoSprite(string className)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITES_DIR}/Classes/{className.ToLower()}.svg");
        }

        private static Sprite LoadFinalSprite(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Final/{relativePath}.png");
        }

        private static Sprite LoadSubSprite(string assetPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static List<CharacterOptionSO> CreateScriptableObjects()
        {
            List<CharacterOptionSO> list = new List<CharacterOptionSO>();

            // Races
            list.Add(CreateOrUpdateOption<CharacterOptionSO>("race_elf", "Elf", OptionCategory.Race, opt =>
            {
                opt.raceType = CharacterRace.Elf;
                opt.icon = LoadFinalSprite("Races/Elf");
                opt.mannequinSprite = LoadFinalSprite("Races/Elf");
                opt.primaryColor = Color.white;
                opt.flavorTagline = "Fey Ancestry & Keen Senses";
                opt.description = "Graceful humanoid with pointed ears and natural affinity for arcane dexterity.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("race_tiefling", "Tiefling", OptionCategory.Race, opt =>
            {
                opt.raceType = CharacterRace.Tiefling;
                opt.icon = LoadFinalSprite("Races/Tiefling");
                opt.mannequinSprite = LoadFinalSprite("Races/Tiefling");
                opt.primaryColor = Color.white;
                opt.flavorTagline = "Hellish Resistance & Darkvision";
                opt.description = "Horned humanoid bearing the infernal heritage of the Lower Planes.";
            }));

            // ALL 12 CORE CLASSES IN STRICT ALPHABETICAL ORDER (FEELING-FOCUSED 1-SENTENCE HOOKS & D&D BEYOND LOGOS)
            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_barbarian", "Barbarian", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Barbarian;
                opt.icon = LoadClassLogoSprite("barbarian");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.90f, 0.29f, 0.10f); // Crimson/Orange
                opt.flavorTagline = "A fierce warrior driven by primal fury who charges headfirst into the heat of battle.";
                opt.description = "A fierce warrior driven by primal fury who charges headfirst into the heat of battle.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_bard", "Bard", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Bard;
                opt.icon = LoadClassLogoSprite("bard");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.85f, 0.11f, 0.38f); // Magenta
                opt.flavorTagline = "An inspiring performer and charismatic storyteller whose music weaves enchantment and wonder.";
                opt.description = "An inspiring performer and charismatic storyteller whose music weaves enchantment and wonder.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_cleric", "Cleric", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Cleric;
                opt.icon = LoadClassLogoSprite("cleric");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.99f, 0.85f, 0.21f); // Radiant Gold
                opt.flavorTagline = "A devout champion of the gods who channels divine light, miracles, and protective magic.";
                opt.description = "A devout champion of the gods who channels divine light, miracles, and protective magic.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_druid", "Druid", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Druid;
                opt.icon = LoadClassLogoSprite("druid");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.26f, 0.63f, 0.28f); // Forest Green
                opt.flavorTagline = "A guardian of the wilderness who commands the forces of nature and transforms into mighty beasts.";
                opt.description = "A guardian of the wilderness who commands the forces of nature and transforms into mighty beasts.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_fighter", "Fighter", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Fighter;
                opt.icon = LoadClassLogoSprite("fighter");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.33f, 0.43f, 0.48f); // Steel Blue
                opt.flavorTagline = "A master of weapons and battlefield tactics who conquers danger with pure combat skill.";
                opt.description = "A master of weapons and battlefield tactics who conquers danger with pure combat skill.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_monk", "Monk", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Monk;
                opt.icon = LoadClassLogoSprite("monk");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.00f, 0.67f, 0.76f); // Cyan
                opt.flavorTagline = "A disciplined martial artist who channels spiritual inner ki into lightning-fast unarmed strikes.";
                opt.description = "A disciplined martial artist who channels spiritual inner ki into lightning-fast unarmed strikes.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_paladin", "Paladin", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Paladin;
                opt.icon = LoadClassLogoSprite("paladin");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(1.00f, 0.63f, 0.00f); // Amber Gold
                opt.flavorTagline = "A noble warrior bound by a sacred oath to smite evil and stand as an unyielding beacon of hope.";
                opt.description = "A noble warrior bound by a sacred oath to smite evil and stand as an unyielding beacon of hope.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_ranger", "Ranger", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Ranger;
                opt.icon = LoadClassLogoSprite("ranger");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.18f, 0.49f, 0.20f); // Hunter Emerald
                opt.flavorTagline = "A master tracker and scout who walks the untamed frontiers with deadly precision and wilderness magic.";
                opt.description = "A master tracker and scout who walks the untamed frontiers with deadly precision and wilderness magic.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_rogue", "Rogue", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Rogue;
                opt.icon = LoadClassLogoSprite("rogue");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.37f, 0.21f, 0.69f); // Shadow Violet
                opt.flavorTagline = "A cunning trickster who excels in stealth, agility, and striking lethal blows from the shadows.";
                opt.description = "A cunning trickster who excels in stealth, agility, and striking lethal blows from the shadows.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_sorcerer", "Sorcerer", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Sorcerer;
                opt.icon = LoadClassLogoSprite("sorcerer");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.76f, 0.09f, 0.36f); // Arcane Crimson
                opt.flavorTagline = "A passionate magic wielder born with wild, raw arcane power coursing through their veins.";
                opt.description = "A passionate magic wielder born with wild, raw arcane power coursing through their veins.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_warlock", "Warlock", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Warlock;
                opt.icon = LoadClassLogoSprite("warlock");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.42f, 0.11f, 0.60f); // Eldritch Purple
                opt.flavorTagline = "A seeker of occult secrets who draws eerie eldritch power from a pact with an otherworldly patron.";
                opt.description = "A seeker of occult secrets who draws eerie eldritch power from a pact with an otherworldly patron.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("class_wizard", "Wizard", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Wizard;
                opt.icon = LoadClassLogoSprite("wizard");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.12f, 0.53f, 0.90f); // Arcane Sapphire
                opt.flavorTagline = "A scholarly master of the arcane who bends reality to their will through intellect and spellbooks.";
                opt.description = "A scholarly master of the arcane who bends reality to their will through intellect and spellbooks.";
            }));

            // Armors
            list.Add(CreateOrUpdateOption<EquipmentSO>("armor_robes", "No Armor (Clothes)", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.None;
                eq.baseAC = 10;
                eq.stealthDisadvantage = false;
                eq.icon = LoadFinalSprite("Armor/No Armor");
                eq.mannequinSprite = LoadFinalSprite("Armor/No Armor");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "Unarmored Attire (No Restriction)";
                eq.description = "Comfortable, unrestrictive traveler robes allowing complete somatic freedom.";
            }));

            list.Add(CreateOrUpdateOption<EquipmentSO>("armor_light", "Light Armor", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.Light;
                eq.baseAC = 11;
                eq.stealthDisadvantage = false;
                eq.icon = LoadFinalSprite("Armor/Light Armor");
                eq.mannequinSprite = LoadFinalSprite("Armor/Light Armor");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "Light Armor (AC 11 + Full DEX)";
                eq.description = "Supple molded leather offering protection without impeding agility or stealth.";
            }));

            list.Add(CreateOrUpdateOption<EquipmentSO>("armor_medium", "Medium Armor", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.Medium;
                eq.baseAC = 14;
                eq.stealthDisadvantage = true;
                eq.icon = LoadFinalSprite("Armor/Medium Armor");
                eq.mannequinSprite = LoadFinalSprite("Armor/Medium Armor");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "Medium Armor (AC 14 + DEX max 2)";
                eq.description = "Overlapping bronze and iron scales. Sturdy protection, but causes disadvantage on stealth.";
            }));

            list.Add(CreateOrUpdateOption<EquipmentSO>("armor_heavy", "Plate Armor", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.Heavy;
                eq.baseAC = 18;
                eq.stealthDisadvantage = true;
                eq.icon = LoadFinalSprite("Armor/Heavy Armor");
                eq.mannequinSprite = LoadFinalSprite("Armor/Heavy Armor");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "Heavy Armor (AC 18 Flat, Disadv Stealth)";
                eq.description = "Interlocking steel plates covering the entire body. Maximum AC, but requires heavy armor proficiency.";
            }));

            // Weapons
            list.Add(CreateOrUpdateOption<EquipmentSO>("weapon_dagger", "Daggers", OptionCategory.Weapon, eq =>
            {
                eq.weaponType = WeaponType.Dagger;
                eq.damage = "1d4";
                eq.damageType = "Piercing";
                eq.weaponProperties = "Finesse, Light";
                eq.isArcaneFocus = false;
                eq.icon = LoadFinalSprite("Weapons/Daggers");
                eq.mannequinSprite = LoadFinalSprite("Weapons/Daggers");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "1d4 Piercing (Finesse, Light)";
                eq.description = "Swift and easily concealed twin blades suitable for precision sneak attacks.";
            }));

            list.Add(CreateOrUpdateOption<EquipmentSO>("weapon_greataxe", "Great Sword", OptionCategory.Weapon, eq =>
            {
                eq.weaponType = WeaponType.GreatSword;
                eq.damage = "2d6";
                eq.damageType = "Slashing";
                eq.weaponProperties = "Heavy, Two-Handed";
                eq.isArcaneFocus = false;
                eq.icon = LoadFinalSprite("Weapons/Great Sword");
                eq.mannequinSprite = LoadFinalSprite("Weapons/Great Sword");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "2d6 Slashing (Martial Heavy)";
                eq.description = "Massive two-handed greatsword capable of cleaving multiple foes in sweeping arcs.";
            }));

            list.Add(CreateOrUpdateOption<EquipmentSO>("weapon_longbow", "Long Bow", OptionCategory.Weapon, eq =>
            {
                eq.weaponType = WeaponType.LongBow;
                eq.damage = "1d8";
                eq.damageType = "Piercing";
                eq.weaponProperties = "Heavy, Two-Handed, Ranged";
                eq.isArcaneFocus = false;
                eq.icon = LoadFinalSprite("Weapons/Long Bow");
                eq.mannequinSprite = LoadFinalSprite("Weapons/Long Bow");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "1d8 Piercing (Ranged Martial)";
                eq.description = "Curved yew longbow delivering lethal arrows from extraordinary distances.";
            }));

            list.Add(CreateOrUpdateOption<EquipmentSO>("weapon_staff", "Arcane Staff", OptionCategory.Weapon, eq =>
            {
                eq.weaponType = WeaponType.Staff;
                eq.damage = "1d6";
                eq.damageType = "Bludgeoning";
                eq.weaponProperties = "Versatile (Arcane Focus)";
                eq.isArcaneFocus = true;
                eq.icon = LoadFinalSprite("Weapons/Staff");
                eq.mannequinSprite = LoadFinalSprite("Weapons/Staff");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "1d6 Bludgeoning (Arcane Focus)";
                eq.description = "Quarterstaff crowned with an attuned crystal orb, channeling magical spells.";
            }));

            // Appearance: Hair & Horns
            list.Add(CreateOrUpdateOption<CharacterOptionSO>("app_hair_1", "Braided Hair", OptionCategory.Appearance, opt =>
            {
                opt.appearanceSlot = AppearanceSlot.Hair;
                opt.icon = LoadFinalSprite("Apperence/Hair1");
                opt.mannequinSprite = LoadFinalSprite("Apperence/Hair1");
                opt.primaryColor = Color.white;
                opt.flavorTagline = "Neat adventurer braids";
                opt.description = "Neatly bound braided hair suited for rugged wilderness exploration.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("app_hair_2", "Wild Locks", OptionCategory.Appearance, opt =>
            {
                opt.appearanceSlot = AppearanceSlot.Hair;
                opt.icon = LoadFinalSprite("Apperence/Hair2");
                opt.mannequinSprite = LoadFinalSprite("Apperence/Hair2");
                opt.primaryColor = Color.white;
                opt.flavorTagline = "Untamed flowing hair";
                opt.description = "Wind-swept wild locks flowing freely down the shoulders.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("app_hair_none", "No Hair (Bald)", OptionCategory.Appearance, opt =>
            {
                opt.appearanceSlot = AppearanceSlot.Hair;
                opt.icon = null;
                opt.mannequinSprite = null;
                opt.primaryColor = Color.white;
                opt.flavorTagline = "Clean shaved look";
                opt.description = "Shaved head providing an austere, monastic appearance.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("app_horns_1", "Infernal Horns", OptionCategory.Appearance, opt =>
            {
                opt.appearanceSlot = AppearanceSlot.Horns;
                opt.icon = LoadFinalSprite("Apperence/Horn1");
                opt.mannequinSprite = LoadFinalSprite("Apperence/Horn1");
                opt.primaryColor = Color.white;
                opt.flavorTagline = "Swept-back demonic horns";
                opt.description = "Curved infernal horns crowning the brow in classic fiendish tradition.";
            }));

            list.Add(CreateOrUpdateOption<CharacterOptionSO>("app_horns_none", "No Horns", OptionCategory.Appearance, opt =>
            {
                opt.appearanceSlot = AppearanceSlot.Horns;
                opt.icon = null;
                opt.mannequinSprite = null;
                opt.primaryColor = Color.white;
                opt.flavorTagline = "Smooth brow without horns";
                opt.description = "A smooth mortal brow free of horns or infernal crests.";
            }));

            return list;
        }

        private static T CreateOrUpdateOption<T>(string id, string displayName, OptionCategory cat, System.Action<T> configure) where T : CharacterOptionSO
        {
            string path = $"{DATA_DIR}/{id}.asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.id = id;
            asset.displayName = displayName;
            asset.category = cat;
            configure(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static List<HarmonyRuleSO> CreateHarmonyRules()
        {
            List<HarmonyRuleSO> rules = new List<HarmonyRuleSO>();

            // --- 1. ARMOR QUIRKS (Priority 100 - 90) ---

            // Barbarian in Heavy Armor (Rage disabled)
            rules.Add(CreateOrUpdateRule("rule_barbarian_heavy_armor", "Rule Quirk: Barbarians & Heavy Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Barbarian;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Rage Disabled in Heavy Armor";
                r.insightNoteText = "Barbarians can equip heavy armor, but their signature feature — Rage — does not grant damage resistance or bonus damage while wearing it!";
            }));

            // Monk in Armor (Disables Martial Arts & Unarmored Defense)
            rules.Add(CreateOrUpdateRule("rule_monk_heavy_armor", "Rule Quirk: Monks & Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Monk;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Martial Arts & Movement Disabled";
                r.insightNoteText = "Monks lose their Martial Arts, Unarmored Movement, and Unarmored Defense benefits when wearing heavy armor!";
            }));

            rules.Add(CreateOrUpdateRule("rule_monk_medium_armor", "Rule Quirk: Monks & Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Monk;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Martial Arts & Movement Disabled";
                r.insightNoteText = "Monks lose their Martial Arts, Unarmored Movement, and Unarmored Defense benefits when wearing armor!";
            }));

            rules.Add(CreateOrUpdateRule("rule_monk_light_armor", "Rule Quirk: Monks & Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 90;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Monk;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Light;
                r.shortStatus = "Martial Arts & Movement Disabled";
                r.insightNoteText = "Even light armor negates a Monk's Martial Arts and Unarmored Defense features!";
            }));

            // Wizard in Armor (Blocks Spellcasting)
            rules.Add(CreateOrUpdateRule("rule_wizard_heavy_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Spellcasting Completely Blocked";
                r.insightNoteText = "In D&D 5e, wearing armor you lack proficiency with prevents you from casting any spells! Wizards lack heavy armor proficiency.";
            }));

            rules.Add(CreateOrUpdateRule("rule_wizard_medium_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Spellcasting Blocked";
                r.insightNoteText = "Wizards lack Medium Armor proficiency. In D&D 5e, wearing armor without proficiency completely blocks all spellcasting!";
            }));

            rules.Add(CreateOrUpdateRule("rule_wizard_light_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 90;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Light;
                r.shortStatus = "Spellcasting Blocked";
                r.insightNoteText = "Standard Wizards lack Light Armor proficiency. You cannot cast spells while wearing armor you are not proficient with!";
            }));

            // Sorcerer in Armor (Blocks Spellcasting)
            rules.Add(CreateOrUpdateRule("rule_sorcerer_heavy_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Sorcerer;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Innate Magic Blocked";
                r.insightNoteText = "Sorcerers possess no armor proficiencies. Non-proficient heavy armor restricts somatic movements and halts all spellcasting!";
            }));

            rules.Add(CreateOrUpdateRule("rule_sorcerer_medium_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Sorcerer;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Innate Magic Blocked";
                r.insightNoteText = "Sorcerers possess no armor proficiencies. Wearing medium armor halts all spellcasting gestures!";
            }));

            rules.Add(CreateOrUpdateRule("rule_sorcerer_light_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 90;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Sorcerer;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Light;
                r.shortStatus = "Innate Magic Blocked";
                r.insightNoteText = "Sorcerers possess no armor proficiencies. Even light armor blocks all spellcasting!";
            }));

            // Bard in Med/Heavy Armor (Blocks Spellcasting)
            rules.Add(CreateOrUpdateRule("rule_bard_heavy_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Bard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Spellcasting Blocked";
                r.insightNoteText = "Bards lack Heavy Armor proficiency. In D&D 5e, wearing armor you lack proficiency with prevents you from casting any spells!";
            }));

            rules.Add(CreateOrUpdateRule("rule_bard_medium_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Bard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Spellcasting Blocked";
                r.insightNoteText = "Bards lack Medium Armor proficiency. In D&D 5e, wearing armor without proficiency prevents you from casting spells!";
            }));

            // Rogue in Med/Heavy Armor (Disadvantage on Stealth & Attacks)
            rules.Add(CreateOrUpdateRule("rule_rogue_heavy_armor", "Rule Quirk: Stealth Disadvantage", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Rogue;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Stealth Disadvantage & Attack Penalties";
                r.insightNoteText = "Rogues lack heavy armor proficiency. It imposes disadvantage on Dexterity ability checks (including Stealth) and attack rolls!";
            }));

            rules.Add(CreateOrUpdateRule("rule_rogue_medium_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 90;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Rogue;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Stealth Disadvantage";
                r.insightNoteText = "Rogues lack medium armor proficiency. Non-proficient armor imposes disadvantage on Dexterity checks, including Stealth!";
            }));

            // Warlock in Med/Heavy Armor (Blocks Spellcasting)
            rules.Add(CreateOrUpdateRule("rule_warlock_heavy_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Warlock;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Pact Magic Blocked";
                r.insightNoteText = "Standard Warlocks lack heavy armor proficiency. Wearing non-proficient armor prevents you from casting pact spells!";
            }));

            rules.Add(CreateOrUpdateRule("rule_warlock_medium_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Warlock;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Pact Magic Blocked";
                r.insightNoteText = "Standard Warlocks lack medium armor proficiency. Non-proficient armor blocks all spellcasting!";
            }));

            // Druid in Metal Armor (Druidic Taboo)
            rules.Add(CreateOrUpdateRule("rule_druid_heavy_armor", "Rule Quirk: Druidic Taboo", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Druid;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Metal Armor Taboo & Non-Proficient";
                r.insightNoteText = "Druids lack heavy armor proficiency and hold an ancient taboo against wearing worked metal armor, severing their connection to nature.";
            }));

            rules.Add(CreateOrUpdateRule("rule_druid_medium_armor", "Rule Quirk: Druidic Taboo", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 90;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Druid;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Metal Armor Taboo";
                r.insightNoteText = "Druids will not wear armor made of metal! Metal disrupts their connection to primal nature.";
            }));

            // Cleric in Heavy Armor
            rules.Add(CreateOrUpdateRule("rule_cleric_heavy_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Cleric;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Non-Proficient Armor";
                r.insightNoteText = "Standard Clerics lack Heavy Armor proficiency (reserved for specific divine domains like Life or War). Wearing non-proficient armor blocks spellcasting!";
            }));

            // Ranger in Heavy Armor
            rules.Add(CreateOrUpdateRule("rule_ranger_heavy_armor", "Rule Quirk: Non-Proficient Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Ranger;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Non-Proficient Armor & Stealth Disadvantage";
                r.insightNoteText = "Rangers lack heavy armor proficiency. It impairs stealth and imposes disadvantage on physical checks!";
            }));

            // --- 2. ARMOR SYNERGIES (Priority 70 - 60) ---

            // Fighter in Heavy Armor
            rules.Add(CreateOrUpdateRule("rule_fighter_heavy_armor", "Harmonious Synergy: Frontline Master", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 70;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Fighter;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Full Heavy Armor Mastery (AC 18)";
                r.insightNoteText = "Full Heavy Armor proficiency grants maximum protection (AC 18), allowing you to hold the frontline with unmatched resilience!";
            }));

            // Paladin in Heavy Armor
            rules.Add(CreateOrUpdateRule("rule_paladin_heavy_armor", "Harmonious Synergy: Holy Knight", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 70;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Paladin;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Crusader Resilience (AC 18)";
                r.insightNoteText = "Paladins are trained to fight in heavy plate armor. Maximizes your survivability while delivering divine smites in melee!";
            }));

            // Barbarian Unarmored
            rules.Add(CreateOrUpdateRule("rule_barbarian_unarmored", "Harmonious Synergy: Primal Toughness", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 65;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Barbarian;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.None;
                r.shortStatus = "Unarmored Defense Active (AC 15)";
                r.insightNoteText = "While unarmored, your Armor Class equals 10 + Dexterity modifier + Constitution modifier (Base AC 15), giving you primal resilience without steel!";
            }));

            // Monk Unarmored
            rules.Add(CreateOrUpdateRule("rule_monk_unarmored", "Harmonious Synergy: Martial Arts", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 65;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Monk;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.None;
                r.shortStatus = "Unarmored Defense Active (AC 15)";
                r.insightNoteText = "While unarmored, your Armor Class equals 10 + Dexterity modifier + Wisdom modifier (Base AC 15) and your Martial Arts mobility is fully active!";
            }));

            // Wizard Unarmored
            rules.Add(CreateOrUpdateRule("rule_wizard_unarmored", "Harmonious Synergy: Natural Arcane Robes", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 60;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.None;
                r.shortStatus = "Natural Arcane Robes";
                r.insightNoteText = "Scholarly robes allow free somatic hand gestures for intricate spellcasting formulas without encumbrance.";
            }));

            // --- 3. WEAPON QUIRKS (Priority 95 - 80) ---

            // Rogue with Great Sword (Sneak Attack requires Finesse)
            rules.Add(CreateOrUpdateRule("rule_rogue_greatsword", "Rule Quirk: Sneak Attack Incompatible", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Rogue;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.GreatSword;
                r.shortStatus = "Sneak Attack Incompatible";
                r.insightNoteText = "The Great Sword is a heavy two-handed weapon, not a finesse weapon. Sneak Attack strictly requires a Finesse or Ranged weapon!";
            }));

            // Monk with Great Sword (Disables Martial Arts)
            rules.Add(CreateOrUpdateRule("rule_monk_greatsword", "Rule Quirk: Martial Arts Disabled", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 95;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Monk;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.GreatSword;
                r.shortStatus = "Martial Arts Disabled";
                r.insightNoteText = "Heavy and two-handed weapons are not monk weapons. Wielding one disables your Martial Arts benefits and bonus unarmed strikes!";
            }));

            // Monk with Longbow (Heavy weapon disables Martial Arts)
            rules.Add(CreateOrUpdateRule("rule_monk_longbow", "Rule Quirk: Heavy Weapon Impairment", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 90;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Monk;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.LongBow;
                r.shortStatus = "Martial Arts Disabled";
                r.insightNoteText = "The Longbow has the Heavy property and is not a monk weapon. Wielding it disables your Martial Arts features!";
            }));

            // Casters with Great Sword (Non-proficient)
            CharacterClass[] nonMartialCasters = new[] {
                CharacterClass.Wizard, CharacterClass.Sorcerer, CharacterClass.Bard,
                CharacterClass.Cleric, CharacterClass.Druid, CharacterClass.Warlock
            };
            foreach (var casterClass in nonMartialCasters)
            {
                string id = $"rule_{casterClass.ToString().ToLower()}_greatsword";
                rules.Add(CreateOrUpdateRule(id, "Rule Quirk: Non-Proficient Weapon", r =>
                {
                    r.harmonyState = HarmonyState.DiscoveryQuirk;
                    r.priority = 85;
                    r.checkClass = true;
                    r.requiredClass = casterClass;
                    r.checkWeapon = true;
                    r.requiredWeapon = WeaponType.GreatSword;
                    r.shortStatus = "Non-Proficient Weapon";
                    r.insightNoteText = $"{casterClass}s lack martial weapon proficiency. You cannot add your proficiency bonus to attack rolls with this heavy blade!";
                }));
            }

            // Longbow on Non-Martials (Non-proficient, unless Elf!)
            CharacterClass[] nonMartialShooters = new[] {
                CharacterClass.Wizard, CharacterClass.Sorcerer, CharacterClass.Bard,
                CharacterClass.Cleric, CharacterClass.Druid, CharacterClass.Rogue, CharacterClass.Warlock
            };
            foreach (var shooterClass in nonMartialShooters)
            {
                string id = $"rule_{shooterClass.ToString().ToLower()}_longbow";
                rules.Add(CreateOrUpdateRule(id, "Rule Quirk: Non-Proficient Weapon", r =>
                {
                    r.harmonyState = HarmonyState.DiscoveryQuirk;
                    r.priority = 80;
                    r.checkClass = true;
                    r.requiredClass = shooterClass;
                    r.checkWeapon = true;
                    r.requiredWeapon = WeaponType.LongBow;
                    r.ignoreIfRace = true;
                    r.ignoredRace = CharacterRace.Elf;
                    r.shortStatus = "Non-Proficient (Unless Elf)";
                    r.insightNoteText = $"{shooterClass}s lack Longbow proficiency. You cannot add your proficiency bonus to attack rolls (unless your race is Elf, granting Elf Weapon Training)!";
                }));
            }

            // --- 4. WEAPON & RACIAL SYNERGIES (Priority 85 - 50) ---

            // Elf + Longbow (Special Racial Synergy - overrides class quirks!)
            rules.Add(CreateOrUpdateRule("rule_elf_longbow", "Harmonious Synergy: Elf Weapon Training", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 85;
                r.checkRace = true;
                r.requiredRace = CharacterRace.Elf;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.LongBow;
                r.shortStatus = "Elf Weapon Training (Proficient)";
                r.insightNoteText = "Your Elven heritage grants natural racial proficiency with the Longbow regardless of your class!";
            }));

            // Barbarian + Great Sword
            rules.Add(CreateOrUpdateRule("rule_barbarian_peak_fury", "Harmonious Synergy: Primal Fury", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 65;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Barbarian;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.GreatSword;
                r.shortStatus = "Peak Martial Synergy";
                r.insightNoteText = "Peak Synergy: Full Rage benefits active! Wielding a massive two-handed Great Sword empowers Reckless Attack for devastating 2d6 slashing damage.";
            }));

            // Fighter + Great Sword
            rules.Add(CreateOrUpdateRule("rule_fighter_greatsword", "Harmonious Synergy: Peak Martial Mastery", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 65;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Fighter;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.GreatSword;
                r.shortStatus = "Peak Martial Mastery";
                r.insightNoteText = "Two-handed heavy weapon mastery synergizes with Great Weapon Fighting style and Action Surge for overwhelming melee offense!";
            }));

            // Paladin + Great Sword
            rules.Add(CreateOrUpdateRule("rule_paladin_greatsword", "Harmonious Synergy: Divine Smite Juggernaut", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 65;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Paladin;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.GreatSword;
                r.shortStatus = "Divine Smite Juggernaut";
                r.insightNoteText = "Delivers massive two-handed weapon damage to maximize the impact of your divine smites in close-quarters combat!";
            }));

            // Ranger + Long Bow
            rules.Add(CreateOrUpdateRule("rule_ranger_longbow", "Harmonious Synergy: Master Archer", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 60;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Ranger;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.LongBow;
                r.shortStatus = "Deadly Ranged Accuracy";
                r.insightNoteText = "The Long Bow grants unmatched range and 1d8 piercing damage, synergizing with your Archery fighting style and hunter's mark!";
            }));

            // Fighter + Long Bow
            rules.Add(CreateOrUpdateRule("rule_fighter_longbow", "Harmonious Synergy: Master Sharpshooter", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 60;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Fighter;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.LongBow;
                r.shortStatus = "Master Sharpshooter";
                r.insightNoteText = "Full martial weapon proficiency enables long-range bombardment with multiple attacks per turn!";
            }));

            // Rogue + Dagger
            rules.Add(CreateOrUpdateRule("rule_rogue_dagger", "Harmonious Synergy: Sneak Attack", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 60;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Rogue;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.Dagger;
                r.shortStatus = "Finesse Sneak Attack Ready";
                r.insightNoteText = "Daggers possess the Finesse property, qualifying for your deadly Sneak Attack extra damage!";
            }));

            // Staff Synergies for Casters & Monk
            rules.Add(CreateOrUpdateRule("rule_wizard_staff", "Harmonious Synergy: Arcane Focus", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 55;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.Staff;
                r.shortStatus = "Spellcasting Focus Attuned";
                r.insightNoteText = "Your quarterstaff doubles as an Arcane Focus, channeling spells cleanly without needing a material component pouch.";
            }));

            rules.Add(CreateOrUpdateRule("rule_sorcerer_staff", "Harmonious Synergy: Arcane Focus", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 55;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Sorcerer;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.Staff;
                r.shortStatus = "Arcane Focus Attuned";
                r.insightNoteText = "Your quarterstaff serves as an arcane focus, channeling wild innate magic into focused spells.";
            }));

            rules.Add(CreateOrUpdateRule("rule_warlock_staff", "Harmonious Synergy: Pact Focus", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 55;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Warlock;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.Staff;
                r.shortStatus = "Pact Focus Attuned";
                r.insightNoteText = "Your staff channels eldritch energy directly from your otherworldly patron.";
            }));

            rules.Add(CreateOrUpdateRule("rule_druid_staff", "Harmonious Synergy: Nature Focus", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 55;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Druid;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.Staff;
                r.shortStatus = "Druidic Focus & Shillelagh";
                r.insightNoteText = "A wooden staff serves as a druidic focus and can be empowered with the Shillelagh cantrip for magical melee strikes.";
            }));

            rules.Add(CreateOrUpdateRule("rule_monk_staff", "Harmonious Synergy: Monk Weapon", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 55;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Monk;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.Staff;
                r.shortStatus = "Dedicated Monk Weapon";
                r.insightNoteText = "The quarterstaff is a versatile monk weapon, scaling with your Martial Arts die and allowing fluid two-handed strikes!";
            }));

            return rules;
        }

        private static HarmonyRuleSO CreateOrUpdateRule(string id, string title, System.Action<HarmonyRuleSO> configure)
        {
            string path = $"{RULES_DIR}/{id}.asset";
            HarmonyRuleSO rule = AssetDatabase.LoadAssetAtPath<HarmonyRuleSO>(path);
            if (rule == null)
            {
                rule = ScriptableObject.CreateInstance<HarmonyRuleSO>();
                AssetDatabase.CreateAsset(rule, path);
            }
            rule.ruleId = id;
            rule.ruleTitle = title;
            configure(rule);
            EditorUtility.SetDirty(rule);
            return rule;
        }

        #endregion

        #region Scene Assembly

        private static void SetupCamera()
        {
            var camObj = Camera.main != null ? Camera.main.gameObject : GameObject.Find("Main Camera");
            if (camObj == null)
            {
                camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            }

            var cam = camObj.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.96f, 0.96f, 0.95f, 1f); // D&D Beyond Parchment #F5F5F3
            cam.transform.position = new Vector3(0, 0, -10f);
        }

        private static void SetupSceneBackground()
        {
            GameObject bgGO = GameObject.Find("SceneBackground");
            if (bgGO == null)
            {
                bgGO = new GameObject("SceneBackground");
            }
            bgGO.transform.position = new Vector3(0f, 0f, 5f);

            // Camera orthographic size is 5f -> camera world height is 10 units.
            // Background sprite is 2880 x 1526 with pixelsPerUnit = 100 -> height = 15.26 units.
            // Scale = 10f / 15.26f = 0.655308f
            float scale = 10f / 15.26f;
            bgGO.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = bgGO.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bgGO.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSubSprite("Assets/Sprites/CharacterCreater_Background.png");
            sr.sortingOrder = -100; // Behind mannequin (orders 0-7)
        }

        private static GameObject SetupMannequin()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "ModularMannequin")
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
            GameObject mannequin = new GameObject("ModularMannequin");

            // Position mannequin: X = -1.2 (centered between left and right UI), Y = -0.7 (comfortably below top navbar)
            // With 1000x1600 sprites and scale 0.43: feet rest naturally on forest path, head/horns well below navbar (zero overlap)
            mannequin.transform.position = new Vector3(-1.2f, -0.7f, 0f);
            mannequin.transform.localScale = new Vector3(0.43f, 0.43f, 1.0f);

            var paperDoll = mannequin.GetComponent<PaperDollView>();
            if (paperDoll == null) paperDoll = mannequin.AddComponent<PaperDollView>();
            paperDoll.SetBaseScale(new Vector3(0.43f, 0.43f, 1.0f));

            // Hand-drawn sprites: all SpriteRenderers at local position (0, 0, 0) with center pivot for 100% pixel-perfect alignment
            var body = GetOrCreateLayerChild(mannequin, "10_Body", 10, LoadFinalSprite("Races/Elf"), Color.white, Vector3.zero, Vector3.one);
            var armor = GetOrCreateLayerChild(mannequin, "20_Armor", 20, LoadFinalSprite("Armor/No Armor"), Color.white, Vector3.zero, Vector3.one);
            var hair = GetOrCreateLayerChild(mannequin, "30_Hair", 30, LoadFinalSprite("Apperence/Hair1"), Color.white, Vector3.zero, Vector3.one);
            var horns = GetOrCreateLayerChild(mannequin, "40_Horns", 40, LoadFinalSprite("Apperence/Horn1"), Color.white, Vector3.zero, Vector3.one); // Renders ON TOP of hair!
            var weapon = GetOrCreateLayerChild(mannequin, "50_Weapons", 50, LoadFinalSprite("Weapons/Great Sword"), Color.white, Vector3.zero, Vector3.one);

            body.enabled = false;
            armor.enabled = false;
            hair.enabled = false;
            horns.enabled = false;
            weapon.enabled = false;

            paperDoll.AssignRenderers(body, armor, hair, horns, weapon);

            var audioSource = mannequin.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = mannequin.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            var clothesClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Clothes Apply.wav");
            var metalClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Metal Armor.mp3");
            var weaponClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Weapon Equip.mp3");
            paperDoll.AssignAudio(audioSource, clothesClip, metalClip, weaponClip);

            // Empty Center Stage Startup: hidden/inactive at launch until first choice is made
            mannequin.SetActive(false);

            return mannequin;
        }

        private static SpriteRenderer GetOrCreateLayerChild(GameObject parent, string name, int order, Sprite sprite, Color color, Vector3 localPos, Vector3 localScale)
        {
            Transform childTr = parent.transform.Find(name);
            GameObject go = childTr != null ? childTr.gameObject : new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            sr.sprite = sprite;
            sr.color = color;
            return sr;
        }

        private static void SetupEventSystem()
        {
            var es = UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGO = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
            }
            else
            {
                var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (oldModule != null) UnityEngine.Object.DestroyImmediate(oldModule);
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
        }

        private static void SetupCanvas(GameObject mannequinObj)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "MainCanvas")
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            GameObject canvasGO = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 10f;

            var manager = canvasGO.AddComponent<CharacterCustomizerManager>();
            var evaluator = canvasGO.AddComponent<RuleHarmonyEvaluator>();

            var allOptionGuids = AssetDatabase.FindAssets("t:CharacterOptionSO", new[] { DATA_DIR });
            List<CharacterOptionSO> optionList = new List<CharacterOptionSO>();
            foreach (var guid in allOptionGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                var opt = AssetDatabase.LoadAssetAtPath<CharacterOptionSO>(p);
                if (opt != null) optionList.Add(opt);
            }
            manager.SetAvailableOptions(optionList);

            var allRuleGuids = AssetDatabase.FindAssets("t:HarmonyRuleSO", new[] { RULES_DIR });
            List<HarmonyRuleSO> ruleList = new List<HarmonyRuleSO>();
            foreach (var guid in allRuleGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                var rule = AssetDatabase.LoadAssetAtPath<HarmonyRuleSO>(p);
                if (rule != null) ruleList.Add(rule);
            }
            evaluator.SetRules(ruleList);

            // Clean-Slate Startup: Player begins with empty canvas (0/4 Choices Made)
            manager.SetMannequinRoot(mannequinObj);
            manager.CurrentBuild.currentRace = null;
            manager.CurrentBuild.currentClass = null;
            manager.CurrentBuild.currentArmor = null;
            manager.CurrentBuild.currentWeapon = null;
            manager.CurrentBuild.currentHair = null;
            manager.CurrentBuild.currentHorns = null;

            Sprite boxSprite = LoadSprite("spr_box_white");
            Sprite d20Sprite = LoadSprite("spr_d20");
            Sprite cardSprite = LoadSprite("spr_card_frame");

            // 1. Back to Methods Button (Independent pill button, NO redundant header panel)
            CreateBackToMethodsButton(canvasGO.transform, cardSprite);

            // 2. Top-Left: D20 + Rule Quirk Card (safely below Y = 970 navbar)
            CreateTopLeftD20AndNote(canvasGO.transform, boxSprite, d20Sprite, cardSprite);

            // 3. Bottom-Left: Mini Character Sheet (VerticalLayoutGroup, no overlap)
            CreateBottomLeftMiniSheet(canvasGO.transform, boxSprite, cardSprite);

            // 4. Center Drop Zone
            CreateCenterDropZone(canvasGO.transform, boxSprite);

            // 5. Drawer Item Prefab (horizontal row for 490px menu)
            GameObject drawerItemPrefab = CreateDrawerItemPrefab(cardSprite, boxSprite);

            // 6. Right-Side Thick Menu (Width: 490px, inset 40px from right: X = 1370 to 1860, top below 970)
            CreateRightSideMenu(canvasGO.transform, boxSprite, cardSprite, drawerItemPrefab);

            // 7. Export Modal Overlay
            CreateExportModal(canvasGO.transform, boxSprite, cardSprite);
        }

        private static void CreateBackToMethodsButton(Transform canvasTr, Sprite cardSprite)
        {
            GameObject backBtnGO = new GameObject("BackToMethodsButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(ReturnToMenuButton));
            backBtnGO.transform.SetParent(canvasTr, false);
            var bRect = backBtnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 1f);
            bRect.anchorMax = new Vector2(0f, 1f);
            bRect.pivot = new Vector2(0f, 1f);
            bRect.anchoredPosition = new Vector2(40f, -115f); // Top sits at Y = 965, safely below Y = 970 navbar!
            bRect.sizeDelta = new Vector2(175f, 36f);

            var bImg = backBtnGO.GetComponent<Image>();
            bImg.sprite = cardSprite;
            bImg.type = Image.Type.Sliced;
            bImg.color = Color.white; // Crisp white rounded card
            var btn = backBtnGO.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.94f, 0.95f, 0.97f, 1f);
            colors.pressedColor = new Color(0.88f, 0.90f, 0.93f, 1f);
            btn.colors = colors;

            GameObject backLabelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            backLabelGO.transform.SetParent(backBtnGO.transform, false);
            var blRect = backLabelGO.GetComponent<RectTransform>();
            blRect.anchorMin = Vector2.zero;
            blRect.anchorMax = Vector2.one;
            blRect.sizeDelta = Vector2.zero;

            var blTmp = backLabelGO.GetComponent<TextMeshProUGUI>();
            blTmp.text = "<b>< Back to Methods</b>";
            blTmp.fontSize = 14;
            blTmp.fontStyle = FontStyles.Bold;
            blTmp.alignment = TextAlignmentOptions.Center;
            blTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f); // #242527
            blTmp.raycastTarget = false;

            var returnComp = backBtnGO.GetComponent<ReturnToMenuButton>();
            var clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/UI_Click.wav");
            returnComp.Setup(clickClip, "MainMenu", 0);
        }

        private static void CreateTopLeftD20AndNote(Transform canvasTr, Sprite boxSprite, Sprite d20Sprite, Sprite cardSprite)
        {
            GameObject d20Container = new GameObject("D20HarmonyPanel", typeof(RectTransform));
            d20Container.transform.SetParent(canvasTr, false);
            var rect = d20Container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(40f, -160f); // Sits at Y = 920, safely below Y = 970 navbar!
            rect.sizeDelta = new Vector2(400, 480);

            var harmonyUI = d20Container.AddComponent<D20HarmonyUI>();

            // 1. Permanent HUD Box (Cannot Minimize): SYNERGY
            GameObject btnGO = new GameObject("D20Button", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(d20Container.transform, false);
            var bRect = btnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 1f);
            bRect.anchorMax = new Vector2(0f, 1f);
            bRect.pivot = new Vector2(0f, 1f);
            bRect.anchoredPosition = Vector2.zero;
            bRect.sizeDelta = new Vector2(340, 80);

            var btnImg = btnGO.GetComponent<Image>();
            btnImg.sprite = cardSprite;
            btnImg.type = Image.Type.Sliced;
            btnImg.color = Color.white; // Crisp white card
            var btn = btnGO.GetComponent<Button>();

            // D20 Glow
            GameObject glowGO = new GameObject("D20Glow", typeof(RectTransform), typeof(Image));
            glowGO.transform.SetParent(btnGO.transform, false);
            var gRect = glowGO.GetComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0.5f);
            gRect.anchorMax = new Vector2(0f, 0.5f);
            gRect.pivot = new Vector2(0.5f, 0.5f);
            gRect.anchoredPosition = new Vector2(45, 0);
            gRect.sizeDelta = new Vector2(68, 68);
            var glowImg = glowGO.GetComponent<Image>();
            glowImg.sprite = d20Sprite;
            glowImg.color = new Color(0.20f, 0.65f, 0.30f, 0.85f);

            // D20 Icon
            GameObject iconGO = new GameObject("D20Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(btnGO.transform, false);
            var iRect = iconGO.GetComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0f, 0.5f);
            iRect.anchorMax = new Vector2(0f, 0.5f);
            iRect.pivot = new Vector2(0.5f, 0.5f);
            iRect.anchoredPosition = new Vector2(45, 0);
            iRect.sizeDelta = new Vector2(54, 54);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.sprite = d20Sprite;

            // Status label (Synergy branding)
            GameObject statusGO = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusGO.transform.SetParent(btnGO.transform, false);
            var sRect = statusGO.GetComponent<TextMeshProUGUI>().rectTransform;
            sRect.anchorMin = new Vector2(0f, 0.5f);
            sRect.anchorMax = new Vector2(1f, 0.5f);
            sRect.pivot = new Vector2(0f, 0.5f);
            sRect.anchoredPosition = new Vector2(90, 0);
            sRect.sizeDelta = new Vector2(-95, 60);
            var sTmp = statusGO.GetComponent<TextMeshProUGUI>();
            sTmp.text = "<b><color=#181818>SYNERGY</color></b>\n<size=12><color=#2E7D32>Harmonious</color></size>";
            sTmp.fontSize = 20;
            sTmp.lineSpacing = 3;
            sTmp.alignment = TextAlignmentOptions.MidlineLeft;
            sTmp.color = Color.white;

            // Quirk Badge
            GameObject badgeGO = new GameObject("QuirkBadge", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(btnGO.transform, false);
            var badgeRect = badgeGO.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(1f, 1f);
            badgeRect.anchoredPosition = new Vector2(8, 8);
            badgeRect.sizeDelta = new Vector2(26, 26);
            var badgeImg = badgeGO.GetComponent<Image>();
            badgeImg.sprite = boxSprite;
            badgeImg.color = new Color(0.95f, 0.25f, 0.2f, 1f);

            // 2. Pop-up Insight Card (CAN Minimize / Close): RULE QUIRK
            GameObject noteCard = new GameObject("NoteCardPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            noteCard.transform.SetParent(d20Container.transform, false);
            var nRect = noteCard.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0f, 1f);
            nRect.anchorMax = new Vector2(0f, 1f);
            nRect.pivot = new Vector2(0f, 1f);
            nRect.anchoredPosition = new Vector2(-430, -95);
            nRect.sizeDelta = new Vector2(390, 240);

            var nImg = noteCard.GetComponent<Image>();
            nImg.sprite = cardSprite;
            nImg.type = Image.Type.Sliced;
            nImg.color = Color.white; // Crisp white card
            var cg = noteCard.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            noteCard.SetActive(false);

            // Rule Quirk Tag (15pt bold red)
            GameObject tagGO = new GameObject("TagText", typeof(RectTransform), typeof(TextMeshProUGUI));
            tagGO.transform.SetParent(noteCard.transform, false);
            var tagRect = tagGO.GetComponent<RectTransform>();
            tagRect.anchorMin = new Vector2(0f, 1f);
            tagRect.anchorMax = new Vector2(1f, 1f);
            tagRect.pivot = new Vector2(0f, 1f);
            tagRect.anchoredPosition = new Vector2(16, -14);
            tagRect.sizeDelta = new Vector2(-60, 24);
            var tagTmp = tagGO.GetComponent<TextMeshProUGUI>();
            tagTmp.text = "[ RULE QUIRK ]";
            tagTmp.fontSize = 15;
            tagTmp.fontStyle = FontStyles.Bold;
            tagTmp.color = new Color(0.77f, 0.12f, 0.12f); // D&D red

            // Note Title (20pt bold charcoal)
            GameObject noteTitleGO = new GameObject("NoteTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            noteTitleGO.transform.SetParent(noteCard.transform, false);
            var ntRect = noteTitleGO.GetComponent<RectTransform>();
            ntRect.anchorMin = new Vector2(0f, 1f);
            ntRect.anchorMax = new Vector2(1f, 1f);
            ntRect.pivot = new Vector2(0f, 1f);
            ntRect.anchoredPosition = new Vector2(16, -40);
            ntRect.sizeDelta = new Vector2(-60, 30);
            var ntTmp = noteTitleGO.GetComponent<TextMeshProUGUI>();
            ntTmp.text = "Rule Quirk: Barbarians & Heavy Armor";
            ntTmp.fontSize = 19;
            ntTmp.fontStyle = FontStyles.Bold;
            ntTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f); // #242527

            // Note Body (16pt soft charcoal)
            GameObject noteBodyGO = new GameObject("NoteBody", typeof(RectTransform), typeof(TextMeshProUGUI));
            noteBodyGO.transform.SetParent(noteCard.transform, false);
            var nbRect = noteBodyGO.GetComponent<RectTransform>();
            nbRect.anchorMin = new Vector2(0f, 0f);
            nbRect.anchorMax = new Vector2(1f, 1f);
            nbRect.pivot = new Vector2(0.5f, 0.5f);
            nbRect.anchoredPosition = new Vector2(0, -28);
            nbRect.sizeDelta = new Vector2(-32, -85);
            var nbTmp = noteBodyGO.GetComponent<TextMeshProUGUI>();
            nbTmp.text = "Barbarians can equip heavy armor, but their signature feature — Rage — does not grant damage resistance while wearing it!";
            nbTmp.fontSize = 16;
            nbTmp.lineSpacing = 5;
            nbTmp.color = new Color(0.35f, 0.38f, 0.42f, 1f); // #555555

            // Dismiss [X] Button
            GameObject closeBtnGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGO.transform.SetParent(noteCard.transform, false);
            var cRect = closeBtnGO.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(1f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(1f, 1f);
            cRect.anchoredPosition = new Vector2(-12, -12);
            cRect.sizeDelta = new Vector2(30, 30);
            var cImg = closeBtnGO.GetComponent<Image>();
            cImg.sprite = boxSprite;
            cImg.color = new Color(0.92f, 0.92f, 0.94f, 1f); // Soft grey pill
            var cBtn = closeBtnGO.GetComponent<Button>();

            GameObject closeTxt = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeTxt.transform.SetParent(closeBtnGO.transform, false);
            var ct = closeTxt.GetComponent<TextMeshProUGUI>();
            ct.text = "✕";
            ct.fontSize = 16;
            ct.alignment = TextAlignmentOptions.Center;
            ct.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            SetPrivateField(harmonyUI, "d20Button", btn);
            SetPrivateField(harmonyUI, "d20GlowImage", glowImg);
            SetPrivateField(harmonyUI, "d20IconImage", iconImg);
            SetPrivateField(harmonyUI, "d20StatusText", sTmp);
            SetPrivateField(harmonyUI, "quirkBadgeImage", badgeImg);
            SetPrivateField(harmonyUI, "noteCardPanel", nRect);
            SetPrivateField(harmonyUI, "noteCardCanvasGroup", cg);
            SetPrivateField(harmonyUI, "noteTitleText", ntTmp);
            SetPrivateField(harmonyUI, "noteBodyText", nbTmp);
            SetPrivateField(harmonyUI, "noteTagText", tagTmp);
            SetPrivateField(harmonyUI, "noteCloseButton", cBtn);
        }

        private static void CreateBottomLeftMiniSheet(Transform canvasTr, Sprite boxSprite, Sprite cardSprite)
        {
            // Bottom-Left corner panel
            GameObject sheetPanel = new GameObject("MiniSheetPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            sheetPanel.transform.SetParent(canvasTr, false);
            var rect = sheetPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(40f, 30f);
            rect.sizeDelta = new Vector2(390, 345);

            var img = sheetPanel.GetComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white; // Crisp white card

            var vlg = sheetPanel.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(14, 14, 14, 14);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var miniUI = sheetPanel.AddComponent<MiniSheetUI>();

            // Title row
            GameObject headerGO = new GameObject("HeaderLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            headerGO.transform.SetParent(sheetPanel.transform, false);
            var hRect = headerGO.GetComponent<RectTransform>();
            hRect.sizeDelta = new Vector2(0, 26);
            var hTmp = headerGO.GetComponent<TextMeshProUGUI>();
            hTmp.text = "<b>MINI CHARACTER SHEET</b>";
            hTmp.fontSize = 18;
            hTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f); // #242527

            // Clean vertically-stacked rows (Clean-slate startup: 0/4 choices made)
            var speciesChip = CreateSheetRow(sheetPanel.transform, boxSprite, "[ ] Species: (None chosen)");
            var classChip = CreateSheetRow(sheetPanel.transform, boxSprite, "[ ] Class: (None chosen)");
            var armorChip = CreateSheetRow(sheetPanel.transform, boxSprite, "[ ] Armor: (None chosen)");
            var weaponChip = CreateSheetRow(sheetPanel.transform, boxSprite, "[ ] Weapon: (None chosen)");

            // Armor Class row (Base AC 10 badge)
            GameObject acGO = new GameObject("TotalACRow", typeof(RectTransform), typeof(Image));
            acGO.transform.SetParent(sheetPanel.transform, false);
            var acRect = acGO.GetComponent<RectTransform>();
            acRect.sizeDelta = new Vector2(0, 42);
            var acImg = acGO.GetComponent<Image>();
            acImg.sprite = boxSprite;
            acImg.type = Image.Type.Sliced;
            acImg.color = new Color(0.98f, 0.97f, 0.92f, 1f); // Soft light gold tint

            GameObject acTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            acTextGO.transform.SetParent(acGO.transform, false);
            var atRect = acTextGO.GetComponent<RectTransform>();
            atRect.anchorMin = Vector2.zero;
            atRect.anchorMax = Vector2.one;
            atRect.sizeDelta = Vector2.zero;
            var acTmp = acTextGO.GetComponent<TextMeshProUGUI>();
            acTmp.text = "<color=#966A00><b>ARMOR CLASS: 10 (Base)</b></color>";
            acTmp.fontSize = 18;
            acTmp.alignment = TextAlignmentOptions.Center;

            // 4-step checklist milestone row: Adventure Ready!
            GameObject readyGO = new GameObject("AdventureReadyRow", typeof(RectTransform), typeof(Image));
            readyGO.transform.SetParent(sheetPanel.transform, false);
            var readyRect = readyGO.GetComponent<RectTransform>();
            readyRect.sizeDelta = new Vector2(0, 40);
            var readyImg = readyGO.GetComponent<Image>();
            readyImg.sprite = boxSprite;
            readyImg.type = Image.Type.Sliced;
            readyImg.color = new Color(0.95f, 0.95f, 0.96f, 1f); // Soft grey when incomplete

            GameObject readyTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            readyTextGO.transform.SetParent(readyGO.transform, false);
            var rtRect = readyTextGO.GetComponent<RectTransform>();
            rtRect.anchorMin = Vector2.zero;
            rtRect.anchorMax = Vector2.one;
            rtRect.sizeDelta = Vector2.zero;
            var readyTmp = readyTextGO.GetComponent<TextMeshProUGUI>();
            readyTmp.text = "<color=#888888>⭐ Adventure Ready (0/4 Choices)</color>";
            readyTmp.fontSize = 16;
            readyTmp.alignment = TextAlignmentOptions.Center;

            SetPrivateField(miniUI, "speciesChipText", speciesChip);
            SetPrivateField(miniUI, "classChipText", classChip);
            SetPrivateField(miniUI, "armorChipText", armorChip);
            SetPrivateField(miniUI, "weaponChipText", weaponChip);
            SetPrivateField(miniUI, "totalAcText", acTmp);
            SetPrivateField(miniUI, "adventureReadyText", readyTmp);

            var miniAudio = sheetPanel.AddComponent<AudioSource>();
            miniAudio.playOnAwake = false;
            var questCompleteClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Quest_Complete.wav");
            SetPrivateField(miniUI, "audioSource", miniAudio);
            SetPrivateField(miniUI, "fanfareClip", questCompleteClip);
        }

        private static TextMeshProUGUI CreateSheetRow(Transform parent, Sprite boxSprite, string text)
        {
            GameObject rowGO = new GameObject("Row", typeof(RectTransform), typeof(Image));
            rowGO.transform.SetParent(parent, false);
            var rect = rowGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 38);

            var img = rowGO.GetComponent<Image>();
            img.sprite = boxSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.96f, 0.96f, 0.97f, 1f); // Soft neutral chip

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(rowGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.18f, 0.20f, 0.24f, 1f); // Dark charcoal text
            return tmp;
        }

        private static void CreateCenterDropZone(Transform canvasTr, Sprite boxSprite)
        {
            GameObject dropZoneGO = new GameObject("CharacterDropZone", typeof(RectTransform), typeof(Image), typeof(CharacterDropZone));
            dropZoneGO.transform.SetParent(canvasTr, false);
            var rect = dropZoneGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            // Mannequin stands at X = -1.2 world units (~ -130px canvas)
            rect.anchoredPosition = new Vector2(-130, -40);
            rect.sizeDelta = new Vector2(460, 800);

            var dropZone = dropZoneGO.GetComponent<CharacterDropZone>();
            var img = dropZoneGO.GetComponent<Image>();
            img.sprite = boxSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(1f, 1f, 1f, 0.03f);

            SetPrivateField(dropZone, "highlightBorder", img);
        }

        private static GameObject CreateDrawerItemPrefab(Sprite cardSprite, Sprite boxSprite)
        {
            string path = $"{PREFABS_DIR}/DrawerItemView.prefab";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            GameObject go = new GameObject("DrawerItemView", typeof(RectTransform), typeof(Image), typeof(Button), typeof(ItemDragHandler), typeof(DrawerItemView));
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(448, 86);

            var img = go.GetComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white; // Crisp white rounded card

            var btn = go.GetComponent<Button>();
            var dragHandler = go.GetComponent<ItemDragHandler>();
            var view = go.GetComponent<DrawerItemView>();

            // Selection Border (D&D Beyond Blue outline)
            GameObject borderGO = new GameObject("SelectionBorder", typeof(RectTransform), typeof(Image));
            borderGO.transform.SetParent(go.transform, false);
            var bRect = borderGO.GetComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.sizeDelta = new Vector2(4, 4);
            var bImg = borderGO.GetComponent<Image>();
            bImg.sprite = cardSprite;
            bImg.type = Image.Type.Sliced;
            bImg.color = new Color(0.15f, 0.46f, 0.70f, 1f);
            borderGO.transform.SetAsFirstSibling();

            // Icon Box (Badge frame with class theme background)
            GameObject iconBox = new GameObject("IconBox", typeof(RectTransform), typeof(Image));
            iconBox.transform.SetParent(go.transform, false);
            var ibRect = iconBox.GetComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0f, 0.5f);
            ibRect.anchorMax = new Vector2(0f, 0.5f);
            ibRect.pivot = new Vector2(0.5f, 0.5f);
            ibRect.anchoredPosition = new Vector2(42, 0);
            ibRect.sizeDelta = new Vector2(58, 58);
            var ibImg = iconBox.GetComponent<Image>();
            ibImg.sprite = boxSprite;
            ibImg.type = Image.Type.Sliced;
            ibImg.color = Color.white; // Color set by DrawerItemView to match class theme

            GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(iconBox.transform, false);
            var iRect = iconGO.GetComponent<RectTransform>();
            iRect.anchorMin = Vector2.zero;
            iRect.anchorMax = Vector2.one;
            iRect.sizeDelta = new Vector2(-12, -12);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.preserveAspect = true;

            // Title (Bold uppercase, dark charcoal #242527)
            GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(go.transform, false);
            var tRect = titleGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.52f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.pivot = new Vector2(0f, 1f);
            tRect.anchoredPosition = new Vector2(82, -8);
            tRect.sizeDelta = new Vector2(-125, 0);
            var tTmp = titleGO.GetComponent<TextMeshProUGUI>();
            tTmp.text = "ITEM NAME";
            tTmp.fontSize = 16;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.TopLeft;
            tTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f); // #242527

            // Subtitle / 1-Sentence Fantasy Hook (Soft grey #666666)
            GameObject subGO = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGO.transform.SetParent(go.transform, false);
            var sRect = subGO.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0f);
            sRect.anchorMax = new Vector2(1f, 0.54f);
            sRect.pivot = new Vector2(0f, 0f);
            sRect.anchoredPosition = new Vector2(82, 6);
            sRect.sizeDelta = new Vector2(-125, 0);
            var sTmp = subGO.GetComponent<TextMeshProUGUI>();
            sTmp.text = "Details / Stats";
            sTmp.fontSize = 12f;
            sTmp.textWrappingMode = TextWrappingModes.Normal;
            sTmp.lineSpacing = -2f;
            sTmp.alignment = TextAlignmentOptions.TopLeft;
            sTmp.color = new Color(0.40f, 0.40f, 0.40f, 1f); // #666666

            // Right-Arrow Chevron > (Subtle grey-blue #8091A5)
            GameObject chevronGO = new GameObject("Chevron", typeof(RectTransform), typeof(TextMeshProUGUI));
            chevronGO.transform.SetParent(go.transform, false);
            var chRect = chevronGO.GetComponent<RectTransform>();
            chRect.anchorMin = new Vector2(1f, 0.5f);
            chRect.anchorMax = new Vector2(1f, 0.5f);
            chRect.pivot = new Vector2(1f, 0.5f);
            chRect.anchoredPosition = new Vector2(-16, 0);
            chRect.sizeDelta = new Vector2(24, 24);
            var chTmp = chevronGO.GetComponent<TextMeshProUGUI>();
            chTmp.text = ">";
            chTmp.fontSize = 20;
            chTmp.fontStyle = FontStyles.Bold;
            chTmp.alignment = TextAlignmentOptions.Center;
            chTmp.color = new Color(0.50f, 0.57f, 0.65f, 1f);

            SetPrivateField(dragHandler, "cardTransform", rect);
            SetPrivateField(dragHandler, "iconImage", iconImg);

            SetPrivateField(view, "iconImage", iconImg);
            SetPrivateField(view, "badgeBackgroundImage", ibImg);
            SetPrivateField(view, "titleText", tTmp);
            SetPrivateField(view, "subtitleText", sTmp);
            SetPrivateField(view, "chevronText", chTmp);
            SetPrivateField(view, "selectionBorder", bImg);
            SetPrivateField(view, "dragHandler", dragHandler);
            SetPrivateField(view, "clickButton", btn);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateRightSideMenu(Transform canvasTr, Sprite boxSprite, Sprite cardSprite, GameObject drawerItemPrefab)
        {
            // Category Menu Column (Width: 490px, inset 40px from right browser scrollbar: X = 1370 to 1860, top below Y = 970)
            GameObject rightColumn = new GameObject("RightSidePanel", typeof(RectTransform), typeof(Image), typeof(CategoryDrawerUI));
            rightColumn.transform.SetParent(canvasTr, false);
            var rcRect = rightColumn.GetComponent<RectTransform>();
            rcRect.anchorMin = new Vector2(1f, 0f);
            rcRect.anchorMax = new Vector2(1f, 1f);
            rcRect.pivot = new Vector2(1f, 1f);
            rcRect.anchoredPosition = new Vector2(-60f, -110f); // Inset 60px from right (1920 - 60 = 1860), top below 970 (1080 - 110 = 970)
            rcRect.sizeDelta = new Vector2(490f, -135f); // Width 490px (1860 - 490 = 1370), bottom padding 25px

            var rcImg = rightColumn.GetComponent<Image>();
            rcImg.sprite = boxSprite;
            rcImg.type = Image.Type.Sliced;
            rcImg.color = new Color(0.97f, 0.97f, 0.96f, 0.98f); // D&D Beyond parchment

            var drawerUI = rightColumn.GetComponent<CategoryDrawerUI>();

            // -------------------------------------------------------------
            // LEVEL 1: Main Categories View
            // -------------------------------------------------------------
            GameObject mainCategoriesPanel = new GameObject("MainCategoriesView", typeof(RectTransform));
            mainCategoriesPanel.transform.SetParent(rightColumn.transform, false);
            var mcRect = mainCategoriesPanel.GetComponent<RectTransform>();
            mcRect.anchorMin = new Vector2(0f, 0f);
            mcRect.anchorMax = new Vector2(1f, 1f);
            mcRect.offsetMin = new Vector2(16, 75);
            mcRect.offsetMax = new Vector2(-16, -16);

            // Title
            GameObject titleGO = new GameObject("MainTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(mainCategoriesPanel.transform, false);
            var mtRect = titleGO.GetComponent<RectTransform>();
            mtRect.anchorMin = new Vector2(0f, 1f);
            mtRect.anchorMax = new Vector2(1f, 1f);
            mtRect.pivot = new Vector2(0.5f, 1f);
            mtRect.anchoredPosition = new Vector2(0, 0);
            mtRect.sizeDelta = new Vector2(0, 50);
            var mtTmp = titleGO.GetComponent<TextMeshProUGUI>();
            mtTmp.text = "<b><color=#181818>CUSTOMIZE CHARACTER</color></b>\n<size=14><color=#666666>Select a category to customize:</color></size>";
            mtTmp.fontSize = 20;
            mtTmp.alignment = TextAlignmentOptions.TopLeft;

            // Container for 4 chunky buttons
            GameObject buttonsContainer = new GameObject("ButtonsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            buttonsContainer.transform.SetParent(mainCategoriesPanel.transform, false);
            var bcRect = buttonsContainer.GetComponent<RectTransform>();
            bcRect.anchorMin = new Vector2(0f, 0f);
            bcRect.anchorMax = new Vector2(1f, 1f);
            bcRect.offsetMin = new Vector2(0, 0);
            bcRect.offsetMax = new Vector2(0, -60);

            var bvlg = buttonsContainer.GetComponent<VerticalLayoutGroup>();
            bvlg.spacing = 10;
            bvlg.childControlWidth = true;
            bvlg.childControlHeight = false;
            bvlg.childForceExpandWidth = true;
            bvlg.childForceExpandHeight = false;

            var (raceBtn, raceLabel) = CreateCategoryButton(buttonsContainer.transform, boxSprite, cardSprite, "1. Species / Race", "Current: (None chosen)");
            var (classBtn, classLabel) = CreateCategoryButton(buttonsContainer.transform, boxSprite, cardSprite, "2. Class", "Current: (None chosen)");
            var (armorBtn, armorLabel) = CreateCategoryButton(buttonsContainer.transform, boxSprite, cardSprite, "3. Armor & Attire", "Current: (None chosen)");
            var (weaponBtn, weaponLabel) = CreateCategoryButton(buttonsContainer.transform, boxSprite, cardSprite, "4. Weapons", "Current: (None chosen)");
            var (appearanceBtn, appearanceLabel) = CreateCategoryButton(buttonsContainer.transform, boxSprite, cardSprite, "5. Appearance", "Current: (Default)");

            // -------------------------------------------------------------
            // LEVEL 2: Subcategory View (Drill-Down)
            // -------------------------------------------------------------
            GameObject subcategoryPanel = new GameObject("SubcategoryView", typeof(RectTransform));
            subcategoryPanel.transform.SetParent(rightColumn.transform, false);
            var scRect = subcategoryPanel.GetComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0f, 0f);
            scRect.anchorMax = new Vector2(1f, 1f);
            scRect.offsetMin = new Vector2(16, 75);
            scRect.offsetMax = new Vector2(-16, -16);

            // Top Bar: Back button + Title
            GameObject topBar = new GameObject("TopBar", typeof(RectTransform));
            topBar.transform.SetParent(subcategoryPanel.transform, false);
            var tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.anchoredPosition = Vector2.zero;
            tbRect.sizeDelta = new Vector2(0, 52);

            // [< Back to Categories] Button
            GameObject backBtnGO = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnGO.transform.SetParent(topBar.transform, false);
            var bbRect = backBtnGO.GetComponent<RectTransform>();
            bbRect.anchorMin = new Vector2(0f, 0.5f);
            bbRect.anchorMax = new Vector2(0f, 0.5f);
            bbRect.pivot = new Vector2(0f, 0.5f);
            bbRect.anchoredPosition = Vector2.zero;
            bbRect.sizeDelta = new Vector2(195, 42);

            var bbImg = backBtnGO.GetComponent<Image>();
            bbImg.sprite = boxSprite;
            bbImg.type = Image.Type.Sliced;
            bbImg.color = Color.white; // Crisp white pill
            var backBtn = backBtnGO.GetComponent<Button>();

            GameObject bbText = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            bbText.transform.SetParent(backBtnGO.transform, false);
            var bbtRect = bbText.GetComponent<RectTransform>();
            bbtRect.anchorMin = Vector2.zero;
            bbtRect.anchorMax = Vector2.one;
            bbtRect.sizeDelta = Vector2.zero;
            var bbtTmp = bbText.GetComponent<TextMeshProUGUI>();
            bbtTmp.text = "<b>< Back to Categories</b>";
            bbtTmp.fontSize = 14;
            bbtTmp.alignment = TextAlignmentOptions.Center;
            bbtTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f); // #242527

            // Subcategory Title
            GameObject subTitleGO = new GameObject("SubcategoryTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subTitleGO.transform.SetParent(topBar.transform, false);
            var stRect = subTitleGO.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0f, 0.5f);
            stRect.anchorMax = new Vector2(1f, 0.5f);
            stRect.pivot = new Vector2(0f, 0.5f);
            stRect.anchoredPosition = new Vector2(208, 0);
            stRect.sizeDelta = new Vector2(-208, 40);
            var stTmp = subTitleGO.GetComponent<TextMeshProUGUI>();
            stTmp.text = "<b>SELECT CLASS</b>";
            stTmp.fontSize = 17;
            stTmp.fontStyle = FontStyles.Bold;
            stTmp.alignment = TextAlignmentOptions.MidlineLeft;
            stTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f); // #242527

            // Scroll View for subcategory items
            GameObject scrollViewGO = new GameObject("ItemsScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollViewGO.transform.SetParent(subcategoryPanel.transform, false);
            var svRect = scrollViewGO.GetComponent<RectTransform>();
            svRect.anchorMin = new Vector2(0f, 0f);
            svRect.anchorMax = new Vector2(1f, 1f);
            svRect.offsetMin = new Vector2(0, 0);
            svRect.offsetMax = new Vector2(0, -56);

            var svImg = scrollViewGO.GetComponent<Image>();
            svImg.sprite = boxSprite;
            svImg.type = Image.Type.Sliced;
            svImg.color = new Color(0.94f, 0.94f, 0.93f, 0.5f);

            var scrollRect = scrollViewGO.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGO.transform.SetParent(scrollViewGO.transform, false);
            var vpRect = viewportGO.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.sizeDelta = Vector2.zero;

            GameObject contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var cRect = contentGO.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(0.5f, 1f);
            cRect.sizeDelta = new Vector2(0, 0);

            var cvlg = contentGO.GetComponent<VerticalLayoutGroup>();
            cvlg.spacing = 8;
            cvlg.padding = new RectOffset(6, 6, 8, 8);
            cvlg.childControlWidth = true;
            cvlg.childControlHeight = false;
            cvlg.childForceExpandWidth = true;
            cvlg.childForceExpandHeight = false;

            var csf = contentGO.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRect;
            scrollRect.content = cRect;

            drawerUI.SetupDrillDownReferences(
                mainCategoriesPanel,
                raceBtn, raceLabel,
                classBtn, classLabel,
                armorBtn, armorLabel,
                weaponBtn, weaponLabel,
                appearanceBtn, appearanceLabel,
                subcategoryPanel,
                backBtn,
                stTmp,
                contentGO.transform,
                drawerItemPrefab);

            // Bottom Export Button
            CreateExportButton(rightColumn.transform, boxSprite);
        }

        private static (Button, TextMeshProUGUI) CreateCategoryButton(Transform parent, Sprite boxSprite, Sprite cardSprite, string title, string defaultEquipped)
        {
            GameObject btnGO = new GameObject("Category_" + title, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 78);

            var img = btnGO.GetComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white; // Crisp white card
            var btn = btnGO.GetComponent<Button>();

            // Title (18pt bold, dark charcoal)
            GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(btnGO.transform, false);
            var tRect = titleGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.pivot = new Vector2(0f, 1f);
            tRect.anchoredPosition = new Vector2(18, -8);
            tRect.sizeDelta = new Vector2(-60, 0);
            var tTmp = titleGO.GetComponent<TextMeshProUGUI>();
            tTmp.text = $"<b>{title}</b>";
            tTmp.fontSize = 18;
            tTmp.alignment = TextAlignmentOptions.TopLeft;
            tTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f); // #242527

            // Equipped Label (14pt blue)
            GameObject eqGO = new GameObject("EquippedLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            eqGO.transform.SetParent(btnGO.transform, false);
            var eqRect = eqGO.GetComponent<RectTransform>();
            eqRect.anchorMin = new Vector2(0f, 0f);
            eqRect.anchorMax = new Vector2(1f, 0.5f);
            eqRect.pivot = new Vector2(0f, 0f);
            eqRect.anchoredPosition = new Vector2(18, 10);
            eqRect.sizeDelta = new Vector2(-60, 0);
            var eqTmp = eqGO.GetComponent<TextMeshProUGUI>();
            eqTmp.text = defaultEquipped;
            eqTmp.fontSize = 14;
            eqTmp.alignment = TextAlignmentOptions.BottomLeft;
            eqTmp.color = new Color(0.15f, 0.46f, 0.70f, 1f); // #2576B3 D&D blue

            // Right arrow
            GameObject arrowGO = new GameObject("Arrow", typeof(RectTransform), typeof(TextMeshProUGUI));
            arrowGO.transform.SetParent(btnGO.transform, false);
            var aRect = arrowGO.GetComponent<RectTransform>();
            aRect.anchorMin = new Vector2(1f, 0.5f);
            aRect.anchorMax = new Vector2(1f, 0.5f);
            aRect.pivot = new Vector2(1f, 0.5f);
            aRect.anchoredPosition = new Vector2(-16, 0);
            aRect.sizeDelta = new Vector2(30, 30);
            var aTmp = arrowGO.GetComponent<TextMeshProUGUI>();
            aTmp.text = "<b>></b>";
            aTmp.fontSize = 22;
            aTmp.alignment = TextAlignmentOptions.Center;
            aTmp.color = new Color(0.50f, 0.57f, 0.65f, 1f); // #8091A5

            return (btn, eqTmp);
        }

        private static void CreateExportButton(Transform parent, Sprite boxSprite)
        {
            GameObject exportBtnGO = new GameObject("ExportButton", typeof(RectTransform), typeof(Image), typeof(Button));
            exportBtnGO.transform.SetParent(parent, false);
            var bRect = exportBtnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 0f);
            bRect.anchorMax = new Vector2(1f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.anchoredPosition = new Vector2(0, 16);
            bRect.sizeDelta = new Vector2(-32, 52);

            var bImg = exportBtnGO.GetComponent<Image>();
            bImg.sprite = boxSprite;
            bImg.type = Image.Type.Sliced;
            bImg.color = new Color(0.72f, 0.11f, 0.11f, 1f); // #B71C1C D&D Beyond Red
            var exportBtn = exportBtnGO.GetComponent<Button>();

            GameObject btnTextGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextGO.transform.SetParent(exportBtnGO.transform, false);
            var btRect = btnTextGO.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.sizeDelta = Vector2.zero;
            var btTmp = btnTextGO.GetComponent<TextMeshProUGUI>();
            btTmp.text = "<b>Export to D&D Beyond -></b>";
            btTmp.fontSize = 18;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;
        }

        private static void CreateExportModal(Transform canvasTr, Sprite boxSprite, Sprite cardSprite)
        {
            GameObject overlayGO = new GameObject("ExportModalOverlay", typeof(RectTransform), typeof(Image));
            overlayGO.transform.SetParent(canvasTr, false);
            var oRect = overlayGO.GetComponent<RectTransform>();
            oRect.anchorMin = Vector2.zero;
            oRect.anchorMax = Vector2.one;
            oRect.sizeDelta = Vector2.zero;
            var oImg = overlayGO.GetComponent<Image>();
            oImg.sprite = boxSprite;
            oImg.color = new Color(0f, 0f, 0f, 0.70f);

            GameObject modalCardGO = new GameObject("ModalCard", typeof(RectTransform), typeof(Image));
            modalCardGO.transform.SetParent(overlayGO.transform, false);
            var mRect = modalCardGO.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f);
            mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.pivot = new Vector2(0.5f, 0.5f);
            mRect.sizeDelta = new Vector2(620, 520);

            var mImg = modalCardGO.GetComponent<Image>();
            mImg.sprite = cardSprite;
            mImg.type = Image.Type.Sliced;
            mImg.color = Color.white; // Crisp white card

            var exportBtn = GameObject.Find("ExportButton")?.GetComponent<Button>();
            var exportUI = exportBtn != null ? exportBtn.gameObject.AddComponent<ExportSummaryUI>() : modalCardGO.AddComponent<ExportSummaryUI>();

            // Modal Header
            GameObject mHeadGO = new GameObject("ModalHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
            mHeadGO.transform.SetParent(modalCardGO.transform, false);
            var mhRect = mHeadGO.GetComponent<RectTransform>();
            mhRect.anchorMin = new Vector2(0f, 1f);
            mhRect.anchorMax = new Vector2(1f, 1f);
            mhRect.pivot = new Vector2(0.5f, 1f);
            mhRect.anchoredPosition = new Vector2(0, -22);
            mhRect.sizeDelta = new Vector2(-40, 40);
            var mhTmp = mHeadGO.GetComponent<TextMeshProUGUI>();
            mhTmp.text = "<b>D&D BEYOND EXPORT SUMMARY</b>";
            mhTmp.fontSize = 26;
            mhTmp.fontStyle = FontStyles.Bold;
            mhTmp.alignment = TextAlignmentOptions.Center;
            mhTmp.color = new Color(0.14f, 0.15f, 0.17f, 1f);

            // Details Text
            GameObject detailsGO = new GameObject("DetailsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            detailsGO.transform.SetParent(modalCardGO.transform, false);
            var dRect = detailsGO.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 1f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.pivot = new Vector2(0f, 1f);
            dRect.anchoredPosition = new Vector2(32, -75);
            dRect.sizeDelta = new Vector2(-64, 150);
            var dTmp = detailsGO.GetComponent<TextMeshProUGUI>();
            dTmp.text = "Species: (None)\nClass: (None)\nArmor: (None)\nWeapon: (None)";
            dTmp.fontSize = 17;
            dTmp.lineSpacing = 10;
            dTmp.color = new Color(0.24f, 0.26f, 0.30f, 1f);

            // Rule Harmony Text
            GameObject ruleGO = new GameObject("RuleHarmonyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            ruleGO.transform.SetParent(modalCardGO.transform, false);
            var rRect = ruleGO.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0f, 0f);
            rRect.anchorMax = new Vector2(1f, 1f);
            rRect.pivot = new Vector2(0.5f, 0.5f);
            rRect.anchoredPosition = new Vector2(0, -35);
            rRect.sizeDelta = new Vector2(-64, -280);
            var rTmp = ruleGO.GetComponent<TextMeshProUGUI>();
            rTmp.text = "<color=#B71C1C><b>[Synergy Status]</b></color>\nReady to export build to D&D Beyond!";
            rTmp.fontSize = 16;
            rTmp.lineSpacing = 6;
            rTmp.color = new Color(0.35f, 0.38f, 0.42f, 1f);

            // Feedback Text
            GameObject feedGO = new GameObject("FeedbackText", typeof(RectTransform), typeof(TextMeshProUGUI));
            feedGO.transform.SetParent(modalCardGO.transform, false);
            var fRect = feedGO.GetComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0f, 0f);
            fRect.anchorMax = new Vector2(1f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.anchoredPosition = new Vector2(0, 80);
            fRect.sizeDelta = new Vector2(-60, 32);
            var fTmp = feedGO.GetComponent<TextMeshProUGUI>();
            fTmp.text = "";
            fTmp.fontSize = 16;
            fTmp.alignment = TextAlignmentOptions.Center;

            // Confirm Export Button
            GameObject confirmBtnGO = new GameObject("ConfirmBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            confirmBtnGO.transform.SetParent(modalCardGO.transform, false);
            var confRect = confirmBtnGO.GetComponent<RectTransform>();
            confRect.anchorMin = new Vector2(0.5f, 0f);
            confRect.anchorMax = new Vector2(0.5f, 0f);
            confRect.pivot = new Vector2(0.5f, 0f);
            confRect.anchoredPosition = new Vector2(-90, 22);
            confRect.sizeDelta = new Vector2(200, 48);
            var confImg = confirmBtnGO.GetComponent<Image>();
            confImg.sprite = boxSprite;
            confImg.type = Image.Type.Sliced;
            confImg.color = new Color(0.18f, 0.55f, 0.25f, 1f); // Forest green
            var confBtn = confirmBtnGO.GetComponent<Button>();

            GameObject confTextGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            confTextGO.transform.SetParent(confirmBtnGO.transform, false);
            var ctRect = confTextGO.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.sizeDelta = Vector2.zero;
            var ctTmp = confTextGO.GetComponent<TextMeshProUGUI>();
            ctTmp.text = "<b>Confirm Export</b>";
            ctTmp.fontSize = 18;
            ctTmp.alignment = TextAlignmentOptions.Center;
            ctTmp.color = Color.white;

            // Close Button
            GameObject closeBtnGO = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGO.transform.SetParent(modalCardGO.transform, false);
            var clRect = closeBtnGO.GetComponent<RectTransform>();
            clRect.anchorMin = new Vector2(0.5f, 0f);
            clRect.anchorMax = new Vector2(0.5f, 0f);
            clRect.pivot = new Vector2(0.5f, 0f);
            clRect.anchoredPosition = new Vector2(115, 22);
            clRect.sizeDelta = new Vector2(180, 48);
            var clImg = closeBtnGO.GetComponent<Image>();
            clImg.sprite = boxSprite;
            clImg.type = Image.Type.Sliced;
            clImg.color = new Color(0.88f, 0.88f, 0.90f, 1f);
            var clBtn = closeBtnGO.GetComponent<Button>();

            GameObject clTextGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            clTextGO.transform.SetParent(closeBtnGO.transform, false);
            var cltRect = clTextGO.GetComponent<RectTransform>();
            cltRect.anchorMin = Vector2.zero;
            cltRect.anchorMax = Vector2.one;
            cltRect.sizeDelta = Vector2.zero;
            var cltTmp = clTextGO.GetComponent<TextMeshProUGUI>();
            cltTmp.text = "Back to Creator";
            cltTmp.fontSize = 18;
            cltTmp.alignment = TextAlignmentOptions.Center;
            cltTmp.color = new Color(0.24f, 0.26f, 0.30f, 1f);

            SetPrivateField(exportUI, "openExportButton", exportBtn);
            SetPrivateField(exportUI, "modalOverlay", overlayGO);
            SetPrivateField(exportUI, "headerText", mhTmp);
            SetPrivateField(exportUI, "characterDetailsText", dTmp);
            SetPrivateField(exportUI, "ruleHarmonyText", rTmp);
            SetPrivateField(exportUI, "closeButton", clBtn);
            SetPrivateField(exportUI, "confirmExportButton", confBtn);
            SetPrivateField(exportUI, "feedbackText", fTmp);

            overlayGO.SetActive(false);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            var type = target.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"Field '{fieldName}' not found on type {type.Name}");
            }
        }

        #endregion
    }
}
