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

            Debug.Log($"<color=#2EA3FF><b>D&D Beyond:</b> Generated {options.Count} Options and {rules.Count} Harmony Rules!</color>");
        }

        [MenuItem("DND Beyond/2. Build Complete Greybox Scene")]
        public static void BuildCompleteScene()
        {
            GenerateAllAssets();

            // Setup Camera
            SetupCamera();

            // Setup Mannequin in World Space
            var mannequinObj = SetupMannequin();

            // Setup Event System
            SetupEventSystem();

            // Setup Canvas and UI
            SetupCanvas(mannequinObj);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#44FF88><b>D&D Beyond:</b> Complete Greybox Scene Successfully Built and Saved!</color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(SPRITES_DIR)) Directory.CreateDirectory(SPRITES_DIR);
            if (!Directory.Exists(DATA_DIR)) Directory.CreateDirectory(DATA_DIR);
            if (!Directory.Exists(RULES_DIR)) Directory.CreateDirectory(RULES_DIR);
            if (!Directory.Exists(PREFABS_DIR)) Directory.CreateDirectory(PREFABS_DIR);
        }

        #region Sprite Generation

        private static void GenerateAllSprites()
        {
            // Canvas sizes: 128x128 or 256x256
            CreateAndSaveSprite("spr_pedestal", 256, 128, DrawPedestal);
            CreateAndSaveSprite("spr_pedestal_aura", 256, 128, DrawPedestalAura);
            CreateAndSaveSprite("spr_body", 256, 256, DrawBodySilhouette);
            CreateAndSaveSprite("spr_hair", 256, 256, DrawHair);
            CreateAndSaveSprite("spr_clothes", 256, 256, DrawClothes);
            CreateAndSaveSprite("spr_elf_ears", 256, 256, DrawElfEars);
            CreateAndSaveSprite("spr_tiefling_horns", 256, 256, DrawTieflingHornsAndTail);
            CreateAndSaveSprite("spr_armor_robes", 256, 256, DrawRobesArmor);
            CreateAndSaveSprite("spr_armor_light", 256, 256, DrawLightLeatherArmor);
            CreateAndSaveSprite("spr_armor_medium", 256, 256, DrawMediumScaleArmor);
            CreateAndSaveSprite("spr_armor_heavy", 256, 256, DrawHeavyPlateArmor);
            CreateAndSaveSprite("spr_weapon_greataxe", 256, 256, DrawGreataxe);
            CreateAndSaveSprite("spr_weapon_staff", 256, 256, DrawArcaneStaff);
            CreateAndSaveSprite("spr_weapon_dagger", 256, 256, DrawDagger);
            CreateAndSaveSprite("spr_d20", 128, 128, DrawD20);
            CreateAndSaveSprite("spr_card_frame", 128, 128, DrawCardFrame);
            CreateAndSaveSprite("spr_box_white", 32, 32, DrawSolidBox);
        }

        private static void CreateAndSaveSprite(string name, int width, int height, System.Action<Texture2D> drawAction)
        {
            string path = $"{SPRITES_DIR}/{name}.png";
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Fill clear
            Color[] clear = new Color[width * height];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            tex.SetPixels(clear);

            drawAction(tex);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void DrawSolidBox(Texture2D tex)
        {
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
        }

        private static void DrawPedestal(Texture2D tex)
        {
            int cx = tex.width / 2;
            int cy = 45;
            int rx = 105;
            int ry = 35;

            // Draw base cylinder
            for (int y = 15; y < 55; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    if (dx * dx <= 1f)
                    {
                        float shade = 0.35f + 0.15f * (1f - dx * dx);
                        tex.SetPixel(x, y, new Color(shade, shade, shade + 0.05f, 1f));
                    }
                }
            }

            // Draw top ellipse
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        float rim = 0.55f + 0.1f * dy;
                        tex.SetPixel(x, y, new Color(rim, rim, rim + 0.05f, 1f));
                    }
                }
            }
        }

        private static void DrawPedestalAura(Texture2D tex)
        {
            int cx = tex.width / 2;
            int cy = 45;
            int rx = 100;
            int ry = 32;

            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    float distSq = dx * dx + dy * dy;
                    if (distSq <= 1f)
                    {
                        // Glowing ring
                        float ring = Mathf.Abs(Mathf.Sqrt(distSq) - 0.75f);
                        if (ring < 0.15f)
                        {
                            float alpha = (1f - ring / 0.15f) * 0.85f;
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                        }
                    }
                }
            }
        }

        private static void DrawBodySilhouette(Texture2D tex)
        {
            // Humanoid neutral mannequin pose
            int cx = tex.width / 2;

            // Head (circle at cx, 205, r=22)
            DrawFilledCircle(tex, cx, 205, 22, Color.white);
            // Neck
            DrawFilledRect(tex, cx - 6, 178, 12, 10, Color.white);
            // Torso (rect cx-24 to cx+24, y=105 to 180)
            DrawFilledRect(tex, cx - 22, 105, 44, 75, Color.white);
            // Arms
            DrawFilledRect(tex, cx - 36, 110, 12, 65, Color.white); // Left arm
            DrawFilledRect(tex, cx + 24, 110, 12, 65, Color.white); // Right arm
            // Legs
            DrawFilledRect(tex, cx - 18, 30, 14, 75, Color.white); // Left leg
            DrawFilledRect(tex, cx + 4, 30, 14, 75, Color.white);  // Right leg
            // Feet
            DrawFilledRect(tex, cx - 22, 22, 18, 10, Color.white);
            DrawFilledRect(tex, cx + 4, 22, 18, 10, Color.white);
        }

        private static void DrawHair(Texture2D tex)
        {
            int cx = tex.width / 2;
            // Stylish hair silhouette on top of head
            DrawFilledCircle(tex, cx, 218, 22, new Color(0.2f, 0.15f, 0.1f, 1f));
            DrawFilledRect(tex, cx - 24, 195, 8, 22, new Color(0.2f, 0.15f, 0.1f, 1f));
            DrawFilledRect(tex, cx + 16, 195, 8, 22, new Color(0.2f, 0.15f, 0.1f, 1f));
        }

        private static void DrawClothes(Texture2D tex)
        {
            int cx = tex.width / 2;
            // Undergarment tunic & pants
            Color underColor = new Color(0.45f, 0.40f, 0.35f, 1f);
            DrawFilledRect(tex, cx - 20, 105, 40, 60, underColor);
            DrawFilledRect(tex, cx - 17, 45, 12, 60, underColor * 0.9f);
            DrawFilledRect(tex, cx + 5, 45, 12, 60, underColor * 0.9f);
        }

        private static void DrawElfEars(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color earColor = new Color(0.96f, 0.85f, 0.78f, 1f);
            // Left pointed ear
            for (int i = 0; i < 20; i++)
            {
                int x = cx - 22 - i;
                int y = 205 + (i / 2);
                DrawFilledCircle(tex, x, y, 4, earColor);
            }
            // Right pointed ear
            for (int i = 0; i < 20; i++)
            {
                int x = cx + 22 + i;
                int y = 205 + (i / 2);
                DrawFilledCircle(tex, x, y, 4, earColor);
            }
        }

        private static void DrawTieflingHornsAndTail(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color hornColor = new Color(0.25f, 0.12f, 0.15f, 1f);

            // Left curved horn
            for (int t = 0; t <= 30; t++)
            {
                float angle = Mathf.PI * 0.65f + (t / 30f) * 0.7f;
                int hx = cx - 12 - (int)(Mathf.Cos(angle) * (20 + t));
                int hy = 215 + (int)(Mathf.Sin(angle) * (30 + t * 0.6f));
                DrawFilledCircle(tex, hx, hy, 5 - (t / 8), hornColor);
            }

            // Right curved horn
            for (int t = 0; t <= 30; t++)
            {
                float angle = Mathf.PI * 0.35f - (t / 30f) * 0.7f;
                int hx = cx + 12 + (int)(Mathf.Cos(angle) * (20 + t));
                int hy = 215 + (int)(Mathf.Sin(angle) * (30 + t * 0.6f));
                DrawFilledCircle(tex, hx, hy, 5 - (t / 8), hornColor);
            }

            // Tail curling to side
            Color tailColor = new Color(0.85f, 0.35f, 0.38f, 1f);
            for (int t = 0; t <= 40; t++)
            {
                float rad = (t / 40f) * Mathf.PI;
                int tx = cx - 18 - (int)(Mathf.Sin(rad) * 35);
                int ty = 95 - t + (int)(Mathf.Cos(rad) * 15);
                DrawFilledCircle(tex, tx, ty, 4 - (t / 14), tailColor);
            }
        }

        private static void DrawRobesArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color robeColor = new Color(0.35f, 0.28f, 0.55f, 1f);
            Color trimColor = new Color(0.9f, 0.75f, 0.3f, 1f);

            // Robe body down past knees
            DrawFilledRect(tex, cx - 24, 60, 48, 115, robeColor);
            // Sleeve folds
            DrawFilledRect(tex, cx - 38, 120, 16, 50, robeColor);
            DrawFilledRect(tex, cx + 22, 120, 16, 50, robeColor);
            // Golden sash trim
            DrawFilledRect(tex, cx - 4, 60, 8, 115, trimColor);
            DrawFilledRect(tex, cx - 22, 115, 44, 8, trimColor);
        }

        private static void DrawLightLeatherArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color leather = new Color(0.55f, 0.35f, 0.20f, 1f);
            Color darkLeather = new Color(0.40f, 0.22f, 0.12f, 1f);

            // Leather cuirass
            DrawFilledRect(tex, cx - 23, 105, 46, 75, leather);
            // Shoulder straps
            DrawFilledRect(tex, cx - 26, 160, 10, 20, darkLeather);
            DrawFilledRect(tex, cx + 16, 160, 10, 20, darkLeather);
            // Cross belt
            for (int i = 0; i < 40; i++)
            {
                DrawFilledCircle(tex, cx - 20 + i, 170 - i, 3, darkLeather);
            }
        }

        private static void DrawMediumScaleArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color bronze = new Color(0.70f, 0.52f, 0.28f, 1f);
            Color iron = new Color(0.45f, 0.48f, 0.52f, 1f);

            // Scale breastplate
            DrawFilledRect(tex, cx - 25, 105, 50, 75, iron);
            // Segmented bronze scales
            for (int row = 0; row < 5; row++)
            {
                int y = 115 + row * 12;
                for (int col = -2; col <= 2; col++)
                {
                    DrawFilledCircle(tex, cx + col * 9, y, 5, bronze);
                }
            }
            // Pauldrons
            DrawFilledCircle(tex, cx - 28, 170, 10, bronze);
            DrawFilledCircle(tex, cx + 28, 170, 10, bronze);
        }

        private static void DrawHeavyPlateArmor(Texture2D tex)
        {
            int cx = tex.width / 2;
            Color steel = new Color(0.82f, 0.85f, 0.90f, 1f);
            Color steelDark = new Color(0.55f, 0.58f, 0.65f, 1f);
            Color goldTrim = new Color(0.92f, 0.75f, 0.25f, 1f);

            // Full steel cuirass
            DrawFilledRect(tex, cx - 26, 105, 52, 75, steel);
            // Center ridge
            DrawFilledRect(tex, cx - 3, 110, 6, 70, steelDark);
            // Huge heavy pauldrons
            DrawFilledRect(tex, cx - 42, 155, 18, 26, steel);
            DrawFilledRect(tex, cx + 24, 155, 18, 26, steel);
            DrawFilledRect(tex, cx - 42, 178, 18, 4, goldTrim);
            DrawFilledRect(tex, cx + 24, 178, 18, 4, goldTrim);
            // Plated faulds / tassets
            DrawFilledRect(tex, cx - 24, 85, 48, 22, steelDark);
            // Heavy greaves
            DrawFilledRect(tex, cx - 19, 32, 16, 50, steel);
            DrawFilledRect(tex, cx + 3, 32, 16, 50, steel);
        }

        private static void DrawGreataxe(Texture2D tex)
        {
            int cx = tex.width / 2 + 55;
            Color wood = new Color(0.45f, 0.28f, 0.15f, 1f);
            Color steel = new Color(0.85f, 0.88f, 0.92f, 1f);
            Color steelEdge = new Color(0.95f, 0.98f, 1f, 1f);

            // Long haft
            DrawFilledRect(tex, cx - 4, 30, 8, 180, wood);

            // Axe head at top (y=160 to 200)
            int hy = 175;
            // Left crescent blade
            for (int r = 0; r <= 32; r++)
            {
                int x = cx - r;
                int h = 15 + (int)(r * 0.9f);
                DrawFilledRect(tex, x - 2, hy - h / 2, 4, h, r > 26 ? steelEdge : steel);
            }
            // Right crescent blade
            for (int r = 0; r <= 32; r++)
            {
                int x = cx + r;
                int h = 15 + (int)(r * 0.9f);
                DrawFilledRect(tex, x - 2, hy - h / 2, 4, h, r > 26 ? steelEdge : steel);
            }
            // Center ring socket
            DrawFilledCircle(tex, cx, hy, 10, new Color(0.3f, 0.3f, 0.35f, 1f));
        }

        private static void DrawArcaneStaff(Texture2D tex)
        {
            int cx = tex.width / 2 + 55;
            Color wood = new Color(0.52f, 0.35f, 0.22f, 1f);
            Color crystal = new Color(0.35f, 0.75f, 1.0f, 1f);
            Color glow = new Color(0.6f, 0.9f, 1.0f, 0.7f);

            // Long twisting staff
            DrawFilledRect(tex, cx - 4, 25, 8, 185, wood);

            // Crown prongs holding crystal
            DrawFilledRect(tex, cx - 12, 195, 6, 25, wood);
            DrawFilledRect(tex, cx + 6, 195, 6, 25, wood);

            // Glowing arcane sphere
            DrawFilledCircle(tex, cx, 210, 16, glow);
            DrawFilledCircle(tex, cx, 210, 11, crystal);
            DrawFilledCircle(tex, cx - 3, 213, 4, Color.white);
        }

        private static void DrawDagger(Texture2D tex)
        {
            int cx = tex.width / 2 + 45;
            int cy = 110;
            Color steel = new Color(0.85f, 0.88f, 0.95f, 1f);
            Color gold = new Color(0.92f, 0.75f, 0.25f, 1f);
            Color grip = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Grip / Handle
            DrawFilledRect(tex, cx - 3, cy - 35, 6, 30, grip);
            // Pommel
            DrawFilledCircle(tex, cx, cy - 36, 6, gold);
            // Crossguard
            DrawFilledRect(tex, cx - 16, cy - 5, 32, 6, gold);

            // Sharp pointed blade
            for (int y = 0; y < 55; y++)
            {
                int halfW = (int)(8f * (1f - (float)y / 55f));
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
            int r = 48;

            Color gold = new Color(0.95f, 0.78f, 0.2f, 1f);
            Color goldDark = new Color(0.65f, 0.48f, 0.1f, 1f);

            // Hexagonal icosahedron outline
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, cy), new Vector2(cx, cy));
                    if (dist <= r)
                    {
                        float innerRatio = dist / r;
                        tex.SetPixel(x, y, Color.Lerp(gold, goldDark, innerRatio));
                    }
                }
            }

            // Facet division lines
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                int px = cx + (int)(Mathf.Cos(a) * r);
                int py = cy + (int)(Mathf.Sin(a) * r);
                DrawLine(tex, cx, cy, px, py, Color.white);
            }

            // Central "20"
            DrawFilledCircle(tex, cx, cy, 14, new Color(0.2f, 0.15f, 0.05f, 0.9f));
        }

        private static void DrawCardFrame(Texture2D tex)
        {
            Color border = new Color(0.4f, 0.45f, 0.5f, 1f);
            Color fill = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    bool isBorder = x <= 3 || x >= tex.width - 4 || y <= 3 || y >= tex.height - 4;
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

        #region ScriptableObjects Creation

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITES_DIR}/{name}.png");
        }

        private static List<CharacterOptionSO> CreateScriptableObjects()
        {
            List<CharacterOptionSO> list = new List<CharacterOptionSO>();

            // Races
            var elf = CreateOrUpdateOption<CharacterOptionSO>("race_elf", "Elf", OptionCategory.Race, opt =>
            {
                opt.raceType = CharacterRace.Elf;
                opt.icon = LoadSprite("spr_elf_ears");
                opt.mannequinSprite = LoadSprite("spr_elf_ears");
                opt.primaryColor = new Color(0.96f, 0.88f, 0.82f);
                opt.flavorTagline = "Fey Ancestry & Keen Senses";
                opt.description = "Graceful humanoid with pointed ears and natural affinity for arcane dexterity.";
            });
            list.Add(elf);

            var tiefling = CreateOrUpdateOption<CharacterOptionSO>("race_tiefling", "Tiefling", OptionCategory.Race, opt =>
            {
                opt.raceType = CharacterRace.Tiefling;
                opt.icon = LoadSprite("spr_tiefling_horns");
                opt.mannequinSprite = LoadSprite("spr_tiefling_horns");
                opt.primaryColor = new Color(0.85f, 0.35f, 0.38f);
                opt.flavorTagline = "Hellish Resistance & Darkvision";
                opt.description = "Horned humanoid bearing the infernal heritage of the Lower Planes.";
            });
            list.Add(tiefling);

            // Classes
            var barbarian = CreateOrUpdateOption<CharacterOptionSO>("class_barbarian", "Barbarian", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Barbarian;
                opt.icon = LoadSprite("spr_weapon_greataxe");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.95f, 0.35f, 0.15f);
                opt.flavorTagline = "Primal Rage & Unarmored Defense";
                opt.description = "Fierce warrior who channels primal fury into unmatched resilience and martial prowess.";
            });
            list.Add(barbarian);

            var wizard = CreateOrUpdateOption<CharacterOptionSO>("class_wizard", "Wizard", OptionCategory.Class, opt =>
            {
                opt.classType = CharacterClass.Wizard;
                opt.icon = LoadSprite("spr_weapon_staff");
                opt.mannequinSprite = LoadSprite("spr_pedestal_aura");
                opt.primaryColor = new Color(0.25f, 0.65f, 1.0f);
                opt.flavorTagline = "Arcane Spellcasting & Spellbook";
                opt.description = "Scholarly magic-user capable of manipulating reality through study and arcane focus.";
            });
            list.Add(wizard);

            // Armor (EquipmentSO)
            var robes = CreateOrUpdateOption<EquipmentSO>("armor_robes", "Cloth Robes", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.None;
                eq.baseAC = 10;
                eq.stealthDisadvantage = false;
                eq.icon = LoadSprite("spr_armor_robes");
                eq.mannequinSprite = LoadSprite("spr_armor_robes");
                eq.primaryColor = new Color(0.45f, 0.38f, 0.65f);
                eq.flavorTagline = "Unarmored Attire (No Restriction)";
                eq.description = "Comfortable, unrestrictive scholar robes allowing complete somatic freedom.";
            });
            list.Add(robes);

            var lightArmor = CreateOrUpdateOption<EquipmentSO>("armor_light", "Leather Armor", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.Light;
                eq.baseAC = 11;
                eq.stealthDisadvantage = false;
                eq.icon = LoadSprite("spr_armor_light");
                eq.mannequinSprite = LoadSprite("spr_armor_light");
                eq.primaryColor = new Color(0.55f, 0.35f, 0.20f);
                eq.flavorTagline = "Light Armor (AC 11 + Full DEX)";
                eq.description = "Supple molded leather offering protection without impeding agility or stealth.";
            });
            list.Add(lightArmor);

            var medArmor = CreateOrUpdateOption<EquipmentSO>("armor_medium", "Scale Mail", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.Medium;
                eq.baseAC = 14;
                eq.stealthDisadvantage = true;
                eq.icon = LoadSprite("spr_armor_medium");
                eq.mannequinSprite = LoadSprite("spr_armor_medium");
                eq.primaryColor = new Color(0.70f, 0.52f, 0.28f);
                eq.flavorTagline = "Medium Armor (AC 14 + DEX max 2)";
                eq.description = "Overlapping bronze and iron scales. Sturdy protection, but causes disadvantage on stealth.";
            });
            list.Add(medArmor);

            var heavyArmor = CreateOrUpdateOption<EquipmentSO>("armor_heavy", "Plate Armor", OptionCategory.Armor, eq =>
            {
                eq.armorType = ArmorType.Heavy;
                eq.baseAC = 18;
                eq.stealthDisadvantage = true;
                eq.icon = LoadSprite("spr_armor_heavy");
                eq.mannequinSprite = LoadSprite("spr_armor_heavy");
                eq.primaryColor = new Color(0.85f, 0.88f, 0.92f);
                eq.flavorTagline = "Heavy Armor (AC 18 Flat, Disadv Stealth)";
                eq.description = "Interlocking steel plates covering the entire body. Maximum AC, but requires heavy armor proficiency.";
            });
            list.Add(heavyArmor);

            // Weapons (EquipmentSO)
            var greataxe = CreateOrUpdateOption<EquipmentSO>("weapon_greataxe", "Greataxe", OptionCategory.Weapon, eq =>
            {
                eq.weaponType = WeaponType.Greataxe;
                eq.damage = "1d12";
                eq.damageType = "Slashing";
                eq.weaponProperties = "Heavy, Two-Handed";
                eq.isArcaneFocus = false;
                eq.icon = LoadSprite("spr_weapon_greataxe");
                eq.mannequinSprite = LoadSprite("spr_weapon_greataxe");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "1d12 Slashing (Martial Heavy)";
                eq.description = "Massive bearded axe capable of cleaving through enemies in frenzy.";
            });
            list.Add(greataxe);

            var staff = CreateOrUpdateOption<EquipmentSO>("weapon_staff", "Arcane Staff", OptionCategory.Weapon, eq =>
            {
                eq.weaponType = WeaponType.ArcaneStaff;
                eq.damage = "1d6";
                eq.damageType = "Bludgeoning";
                eq.weaponProperties = "Versatile (Focus)";
                eq.isArcaneFocus = true;
                eq.icon = LoadSprite("spr_weapon_staff");
                eq.mannequinSprite = LoadSprite("spr_weapon_staff");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "1d6 Bludgeoning (Arcane Focus)";
                eq.description = "Quarterstaff crowned with an attuned crystal orb, channeling magical spells.";
            });
            list.Add(staff);

            var dagger = CreateOrUpdateOption<EquipmentSO>("weapon_dagger", "Dagger", OptionCategory.Weapon, eq =>
            {
                eq.weaponType = WeaponType.Dagger;
                eq.damage = "1d4";
                eq.damageType = "Piercing";
                eq.weaponProperties = "Finesse, Light, Thrown";
                eq.isArcaneFocus = false;
                eq.icon = LoadSprite("spr_weapon_dagger");
                eq.mannequinSprite = LoadSprite("spr_weapon_dagger");
                eq.primaryColor = Color.white;
                eq.flavorTagline = "1d4 Piercing (Finesse, Light)";
                eq.description = "Swift and easily concealed blade suitable for precision strikes.";
            });
            list.Add(dagger);

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

            // Rule 1: Barbarian + Heavy Armor (Quirk)
            rules.Add(CreateOrUpdateRule("rule_barbarian_heavy_armor", "Barbarians & Heavy Armor", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Barbarian;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Rage Restriction";
                r.insightNoteText = "Barbarians are free to wear heavy armor, but their signature feature — Rage — does not grant damage resistance or bonus damage while wearing it!";
            }));

            // Rule 2: Wizard + Heavy Armor (Quirk)
            rules.Add(CreateOrUpdateRule("rule_wizard_heavy_armor", "Spellcasting Blocked!", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 100;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Heavy;
                r.shortStatus = "Non-Proficient Armor";
                r.insightNoteText = "In D&D 5e, wearing armor you aren't proficient with prevents you from casting any spells! Wizards lack heavy armor proficiency.";
            }));

            // Rule 3: Wizard + Medium Armor (Quirk)
            rules.Add(CreateOrUpdateRule("rule_wizard_medium_armor", "Spellcasting Blocked!", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 90;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Medium;
                r.shortStatus = "Non-Proficient Armor";
                r.insightNoteText = "Wizards lack proficiency with medium armor. Wearing armor without proficiency completely halts all spellcasting gestures!";
            }));

            // Rule 4: Wizard + Light Armor (Quirk)
            rules.Add(CreateOrUpdateRule("rule_wizard_light_armor", "Armor Non-Proficiency", r =>
            {
                r.harmonyState = HarmonyState.DiscoveryQuirk;
                r.priority = 85;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.Light;
                r.shortStatus = "Non-Proficient Armor";
                r.insightNoteText = "Standard Wizards lack Light Armor proficiency. You cannot cast spells while wearing armor you are not proficient with!";
            }));

            // Rule 5: Barbarian + Greataxe + Medium/Robes (Peak Synergy)
            rules.Add(CreateOrUpdateRule("rule_barbarian_peak_fury", "Peak Primal Fury!", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 60;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Barbarian;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.Greataxe;
                r.shortStatus = "Peak Martial Harmony";
                r.insightNoteText = "Peak Harmony: Full Rage benefits active! Wielding a two-handed Greataxe empowers Reckless Attack for massive 1d12 slashing damage.";
            }));

            // Rule 6: Wizard + Arcane Staff (Synergy)
            rules.Add(CreateOrUpdateRule("rule_wizard_arcane_staff", "Arcane Focus Attuned", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 50;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkWeapon = true;
                r.requiredWeapon = WeaponType.ArcaneStaff;
                r.shortStatus = "Spellcasting Focus";
                r.insightNoteText = "Your quarterstaff doubles as an Arcane Focus, channeling spells cleanly without needing a material component pouch.";
            }));

            // Rule 7: Barbarian + Unarmored (Synergy)
            rules.Add(CreateOrUpdateRule("rule_barbarian_unarmored", "Unarmored Defense", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 45;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Barbarian;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.None;
                r.shortStatus = "Unarmored Defense (AC 15)";
                r.insightNoteText = "While not wearing armor, your Armor Class equals 10 + Dexterity modifier + Constitution modifier (Base AC 15)!";
            }));

            // Rule 8: Wizard + Robes (Synergy)
            rules.Add(CreateOrUpdateRule("rule_wizard_robes", "Unrestricted Casting", r =>
            {
                r.harmonyState = HarmonyState.Harmonious;
                r.priority = 40;
                r.checkClass = true;
                r.requiredClass = CharacterClass.Wizard;
                r.checkArmor = true;
                r.requiredArmor = ArmorType.None;
                r.shortStatus = "Full Spellcasting Freedom";
                r.insightNoteText = "Flowing cloth robes allow free somatic spellcasting gestures and synergize seamlessly with Mage Armor!";
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
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
            cam.transform.position = new Vector3(0, 0, -10f);
        }

        private static GameObject SetupMannequin()
        {
            GameObject mannequin = GameObject.Find("ModularMannequin");
            if (mannequin == null)
            {
                mannequin = new GameObject("ModularMannequin");
            }
            mannequin.transform.position = new Vector3(0f, 0.35f, 0f);
            mannequin.transform.localScale = Vector3.one * 1.55f;

            var paperDoll = mannequin.GetComponent<PaperDollView>();
            if (paperDoll == null) paperDoll = mannequin.AddComponent<PaperDollView>();

            // Layers
            var pedestal = GetOrCreateLayerChild(mannequin, "0_Pedestal", 0, LoadSprite("spr_pedestal"), Color.white);
            var aura = GetOrCreateLayerChild(mannequin, "1_PedestalAura", 1, LoadSprite("spr_pedestal_aura"), new Color(0.95f, 0.35f, 0.15f, 0.6f));
            var body = GetOrCreateLayerChild(mannequin, "2_BodyBase", 2, LoadSprite("spr_body"), new Color(0.96f, 0.88f, 0.82f));
            var raceFeatures = GetOrCreateLayerChild(mannequin, "3_RaceFeatures", 3, LoadSprite("spr_tiefling_horns"), new Color(0.85f, 0.35f, 0.38f));
            var clothes = GetOrCreateLayerChild(mannequin, "4_Clothes", 4, LoadSprite("spr_clothes"), Color.white);
            var armor = GetOrCreateLayerChild(mannequin, "5_ArmorOverlay", 5, LoadSprite("spr_armor_heavy"), Color.white);
            var hair = GetOrCreateLayerChild(mannequin, "6_Hair", 6, LoadSprite("spr_hair"), Color.white);
            var weapon = GetOrCreateLayerChild(mannequin, "7_Weapon", 7, LoadSprite("spr_weapon_greataxe"), Color.white);

            paperDoll.AssignRenderers(pedestal, aura, body, raceFeatures, clothes, armor, hair, weapon);

            return mannequin;
        }

        private static SpriteRenderer GetOrCreateLayerChild(GameObject parent, string name, int order, Sprite sprite, Color color)
        {
            Transform childTr = parent.transform.Find(name);
            GameObject go = childTr != null ? childTr.gameObject : new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = Vector3.zero;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            sr.sprite = sprite;
            sr.color = color;
            return sr;
        }

        private static void SetupEventSystem()
        {
            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGO = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
            }
            else
            {
                // Ensure InputSystemUIInputModule
                var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (oldModule != null) Object.DestroyImmediate(oldModule);
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
        }

        private static void SetupCanvas(GameObject mannequinObj)
        {
            GameObject canvasGO = GameObject.Find("MainCanvas");
            if (canvasGO != null)
            {
                Object.DestroyImmediate(canvasGO);
            }

            canvasGO = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Central Customizer Manager on Canvas or separate GameObject
            var manager = canvasGO.AddComponent<CharacterCustomizerManager>();
            var evaluator = canvasGO.AddComponent<RuleHarmonyEvaluator>();

            // Load options and rules into manager
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

            // Set initial build choices: Tiefling Barbarian HeavyArmor Greataxe (triggers the classic rule quirk!)
            var tieflingOpt = optionList.Find(o => o.id == "race_tiefling");
            var barbarianOpt = optionList.Find(o => o.id == "class_barbarian");
            var heavyOpt = optionList.Find(o => o.id == "armor_heavy") as EquipmentSO;
            var axeOpt = optionList.Find(o => o.id == "weapon_greataxe") as EquipmentSO;

            manager.CurrentBuild.currentRace = tieflingOpt;
            manager.CurrentBuild.currentClass = barbarianOpt;
            manager.CurrentBuild.currentArmor = heavyOpt;
            manager.CurrentBuild.currentWeapon = axeOpt;

            Sprite boxSprite = LoadSprite("spr_box_white");
            Sprite d20Sprite = LoadSprite("spr_d20");
            Sprite cardSprite = LoadSprite("spr_card_frame");

            // 1. Header
            CreateHeader(canvasGO.transform, boxSprite);

            // 2. Top-Left: D20 + Sliding Note Panel
            CreateTopLeftD20AndNote(canvasGO.transform, boxSprite, d20Sprite, cardSprite);

            // 3. Top-Right: Mini Sheet Chips
            CreateTopRightMiniSheet(canvasGO.transform, boxSprite);

            // 4. Center Viewport Drop Zone (Over the Mannequin)
            CreateCenterDropZone(canvasGO.transform, boxSprite);

            // 5. Drawer Item Prefab
            GameObject drawerItemPrefab = CreateDrawerItemPrefab(cardSprite);

            // 6. Bottom Category Drawer
            CreateBottomDrawer(canvasGO.transform, boxSprite, drawerItemPrefab);

            // 7. Export Button & Modal
            CreateExportModal(canvasGO.transform, boxSprite, cardSprite);
        }

        private static void CreateHeader(Transform canvasTr, Sprite boxSprite)
        {
            GameObject header = new GameObject("HeaderPanel", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(canvasTr, false);
            var rect = header.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 60);

            var img = header.GetComponent<Image>();
            img.sprite = boxSprite;
            img.color = new Color(0.09f, 0.11f, 0.15f, 0.95f);

            // Title
            GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(header.transform, false);
            var tr = titleGO.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f);
            tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = new Vector2(800, 45);

            var tmp = titleGO.GetComponent<TextMeshProUGUI>();
            tmp.text = "<b><color=#E03B3B>D&D</color> BEYOND</b>  |  VISUAL CHARACTER CREATOR (GREYBOX PROTOTYPE)";
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static void CreateTopLeftD20AndNote(Transform canvasTr, Sprite boxSprite, Sprite d20Sprite, Sprite cardSprite)
        {
            GameObject d20Container = new GameObject("D20HarmonyPanel", typeof(RectTransform));
            d20Container.transform.SetParent(canvasTr, false);
            var rect = d20Container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(30, -75);
            rect.sizeDelta = new Vector2(380, 400);

            var harmonyUI = d20Container.AddComponent<D20HarmonyUI>();

            // D20 Button Box
            GameObject btnGO = new GameObject("D20Button", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(d20Container.transform, false);
            var bRect = btnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 1f);
            bRect.anchorMax = new Vector2(0f, 1f);
            bRect.pivot = new Vector2(0f, 1f);
            bRect.anchoredPosition = Vector2.zero;
            bRect.sizeDelta = new Vector2(260, 75);

            var btnImg = btnGO.GetComponent<Image>();
            btnImg.sprite = boxSprite;
            btnImg.color = new Color(0.14f, 0.16f, 0.22f, 0.95f);
            var btn = btnGO.GetComponent<Button>();

            // D20 Glow Circle
            GameObject glowGO = new GameObject("D20Glow", typeof(RectTransform), typeof(Image));
            glowGO.transform.SetParent(btnGO.transform, false);
            var gRect = glowGO.GetComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0.5f);
            gRect.anchorMax = new Vector2(0f, 0.5f);
            gRect.pivot = new Vector2(0.5f, 0.5f);
            gRect.anchoredPosition = new Vector2(40, 0);
            gRect.sizeDelta = new Vector2(65, 65);
            var glowImg = glowGO.GetComponent<Image>();
            glowImg.sprite = d20Sprite;
            glowImg.color = new Color(1f, 0.5f, 0.2f, 0.8f);

            // D20 Icon
            GameObject iconGO = new GameObject("D20Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(btnGO.transform, false);
            var iRect = iconGO.GetComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0f, 0.5f);
            iRect.anchorMax = new Vector2(0f, 0.5f);
            iRect.pivot = new Vector2(0.5f, 0.5f);
            iRect.anchoredPosition = new Vector2(40, 0);
            iRect.sizeDelta = new Vector2(50, 50);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.sprite = d20Sprite;

            // Status label
            GameObject statusGO = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusGO.transform.SetParent(btnGO.transform, false);
            var sRect = statusGO.GetComponent<TextMeshProUGUI>().rectTransform;
            sRect.anchorMin = new Vector2(0f, 0.5f);
            sRect.anchorMax = new Vector2(1f, 0.5f);
            sRect.pivot = new Vector2(0f, 0.5f);
            sRect.anchoredPosition = new Vector2(80, 0);
            sRect.sizeDelta = new Vector2(-85, 50);
            var sTmp = statusGO.GetComponent<TextMeshProUGUI>();
            sTmp.text = "<b>RULE QUIRK</b>\n<size=12><color=#AAAAAA>Click to Toggle Note</size></color>";
            sTmp.fontSize = 17;
            sTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Quirk Badge (Exclamation mark icon)
            GameObject badgeGO = new GameObject("QuirkBadge", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(btnGO.transform, false);
            var badgeRect = badgeGO.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(1f, 1f);
            badgeRect.anchoredPosition = new Vector2(8, 8);
            badgeRect.sizeDelta = new Vector2(24, 24);
            var badgeImg = badgeGO.GetComponent<Image>();
            badgeImg.sprite = boxSprite;
            badgeImg.color = new Color(0.95f, 0.25f, 0.2f, 1f);

            // Note Card Panel (Below D20)
            GameObject noteCard = new GameObject("NoteCardPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            noteCard.transform.SetParent(d20Container.transform, false);
            var nRect = noteCard.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0f, 1f);
            nRect.anchorMax = new Vector2(0f, 1f);
            nRect.pivot = new Vector2(0f, 1f);
            nRect.anchoredPosition = new Vector2(0, -90);
            nRect.sizeDelta = new Vector2(360, 220);

            var nImg = noteCard.GetComponent<Image>();
            nImg.sprite = cardSprite;
            nImg.type = Image.Type.Sliced;
            nImg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            var cg = noteCard.GetComponent<CanvasGroup>();

            // Note Tag
            GameObject tagGO = new GameObject("TagText", typeof(RectTransform), typeof(TextMeshProUGUI));
            tagGO.transform.SetParent(noteCard.transform, false);
            var tagRect = tagGO.GetComponent<RectTransform>();
            tagRect.anchorMin = new Vector2(0f, 1f);
            tagRect.anchorMax = new Vector2(1f, 1f);
            tagRect.pivot = new Vector2(0f, 1f);
            tagRect.anchoredPosition = new Vector2(16, -14);
            tagRect.sizeDelta = new Vector2(-32, 24);
            var tagTmp = tagGO.GetComponent<TextMeshProUGUI>();
            tagTmp.text = "[ 5e RULE RESTRICTION ]";
            tagTmp.fontSize = 13;
            tagTmp.color = new Color(1f, 0.5f, 0.2f);
            tagTmp.fontStyle = FontStyles.Bold;

            // Note Title
            GameObject noteTitleGO = new GameObject("NoteTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            noteTitleGO.transform.SetParent(noteCard.transform, false);
            var ntRect = noteTitleGO.GetComponent<RectTransform>();
            ntRect.anchorMin = new Vector2(0f, 1f);
            ntRect.anchorMax = new Vector2(1f, 1f);
            ntRect.pivot = new Vector2(0f, 1f);
            ntRect.anchoredPosition = new Vector2(16, -38);
            ntRect.sizeDelta = new Vector2(-32, 28);
            var ntTmp = noteTitleGO.GetComponent<TextMeshProUGUI>();
            ntTmp.text = "Barbarians & Heavy Armor";
            ntTmp.fontSize = 17;
            ntTmp.fontStyle = FontStyles.Bold;
            ntTmp.color = Color.white;

            // Note Body
            GameObject noteBodyGO = new GameObject("NoteBody", typeof(RectTransform), typeof(TextMeshProUGUI));
            noteBodyGO.transform.SetParent(noteCard.transform, false);
            var nbRect = noteBodyGO.GetComponent<RectTransform>();
            nbRect.anchorMin = new Vector2(0f, 0f);
            nbRect.anchorMax = new Vector2(1f, 1f);
            nbRect.pivot = new Vector2(0.5f, 0.5f);
            nbRect.anchoredPosition = new Vector2(0, -25);
            nbRect.sizeDelta = new Vector2(-32, -80);
            var nbTmp = noteBodyGO.GetComponent<TextMeshProUGUI>();
            nbTmp.text = "Barbarians are free to wear heavy armor, but their signature feature — Rage — does not grant damage resistance while wearing it!";
            nbTmp.fontSize = 14;
            nbTmp.color = new Color(0.85f, 0.88f, 0.92f);
            // word wrapping enabled by default

            // Close Button
            GameObject closeBtnGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGO.transform.SetParent(noteCard.transform, false);
            var cRect = closeBtnGO.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(1f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(1f, 1f);
            cRect.anchoredPosition = new Vector2(-10, -10);
            cRect.sizeDelta = new Vector2(24, 24);
            var cImg = closeBtnGO.GetComponent<Image>();
            cImg.sprite = boxSprite;
            cImg.color = new Color(0.3f, 0.35f, 0.4f, 0.8f);
            var cBtn = closeBtnGO.GetComponent<Button>();

            // Wire D20HarmonyUI serialized fields via reflection or helper
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

        private static void CreateTopRightMiniSheet(Transform canvasTr, Sprite boxSprite)
        {
            GameObject sheetPanel = new GameObject("MiniSheetPanel", typeof(RectTransform), typeof(Image));
            sheetPanel.transform.SetParent(canvasTr, false);
            var rect = sheetPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-30, -75);
            rect.sizeDelta = new Vector2(480, 130);

            var img = sheetPanel.GetComponent<Image>();
            img.sprite = boxSprite;
            img.color = new Color(0.11f, 0.13f, 0.18f, 0.95f);

            var miniUI = sheetPanel.AddComponent<MiniSheetUI>();

            // Header label
            GameObject headerGO = new GameObject("HeaderLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            headerGO.transform.SetParent(sheetPanel.transform, false);
            var hRect = headerGO.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0f, 1f);
            hRect.anchoredPosition = new Vector2(14, -10);
            hRect.sizeDelta = new Vector2(-28, 24);
            var hTmp = headerGO.GetComponent<TextMeshProUGUI>();
            hTmp.text = "<b>MINI CHARACTER SHEET</b>";
            hTmp.fontSize = 13;
            hTmp.color = new Color(0.7f, 0.75f, 0.85f);

            // Total AC banner
            GameObject acGO = new GameObject("TotalAC", typeof(RectTransform), typeof(TextMeshProUGUI));
            acGO.transform.SetParent(sheetPanel.transform, false);
            var acRect = acGO.GetComponent<RectTransform>();
            acRect.anchorMin = new Vector2(1f, 1f);
            acRect.anchorMax = new Vector2(1f, 1f);
            acRect.pivot = new Vector2(1f, 1f);
            acRect.anchoredPosition = new Vector2(-14, -10);
            acRect.sizeDelta = new Vector2(200, 24);
            var acTmp = acGO.GetComponent<TextMeshProUGUI>();
            acTmp.text = "ARMOR CLASS: 18";
            acTmp.fontSize = 14;
            acTmp.fontStyle = FontStyles.Bold;
            acTmp.alignment = TextAlignmentOptions.TopRight;
            acTmp.color = new Color(0.95f, 0.8f, 0.3f);

            // 4 Chip Containers
            var speciesChip = CreateChip(sheetPanel.transform, boxSprite, new Vector2(14, -40), new Vector2(215, 36), "[ Species: Tiefling ]");
            var classChip = CreateChip(sheetPanel.transform, boxSprite, new Vector2(245, -40), new Vector2(215, 36), "[ Class: Barbarian ]");
            var armorChip = CreateChip(sheetPanel.transform, boxSprite, new Vector2(14, -82), new Vector2(215, 36), "[ Armor: Heavy (AC 18) ]");
            var weaponChip = CreateChip(sheetPanel.transform, boxSprite, new Vector2(245, -82), new Vector2(215, 36), "[ Weapon: Greataxe ]");

            SetPrivateField(miniUI, "speciesChipText", speciesChip);
            SetPrivateField(miniUI, "classChipText", classChip);
            SetPrivateField(miniUI, "armorChipText", armorChip);
            SetPrivateField(miniUI, "weaponChipText", weaponChip);
            SetPrivateField(miniUI, "totalAcText", acTmp);
        }

        private static TextMeshProUGUI CreateChip(Transform parent, Sprite boxSprite, Vector2 pos, Vector2 size, string text)
        {
            GameObject chipGO = new GameObject("Chip", typeof(RectTransform), typeof(Image));
            chipGO.transform.SetParent(parent, false);
            var rect = chipGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = chipGO.GetComponent<Image>();
            img.sprite = boxSprite;
            img.color = new Color(0.18f, 0.22f, 0.30f, 0.95f);

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(chipGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
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
            rect.anchoredPosition = new Vector2(0, 40);
            rect.sizeDelta = new Vector2(360, 480);

            var dropZone = dropZoneGO.GetComponent<CharacterDropZone>();
            var img = dropZoneGO.GetComponent<Image>();
            img.sprite = boxSprite;
            img.color = new Color(1f, 1f, 1f, 0.04f); // Subtle drop region

            SetPrivateField(dropZone, "highlightBorder", img);
        }

        private static GameObject CreateDrawerItemPrefab(Sprite cardSprite)
        {
            string path = $"{PREFABS_DIR}/DrawerItemView.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = new GameObject("DrawerItemView", typeof(RectTransform), typeof(Image), typeof(Button), typeof(ItemDragHandler), typeof(DrawerItemView));
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(170, 190);

            var img = go.GetComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.13f, 0.16f, 0.22f, 1f);

            var btn = go.GetComponent<Button>();
            var dragHandler = go.GetComponent<ItemDragHandler>();
            var view = go.GetComponent<DrawerItemView>();

            // Selection Border
            GameObject borderGO = new GameObject("SelectionBorder", typeof(RectTransform), typeof(Image));
            borderGO.transform.SetParent(go.transform, false);
            var bRect = borderGO.GetComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.sizeDelta = new Vector2(4, 4);
            var bImg = borderGO.GetComponent<Image>();
            bImg.sprite = cardSprite;
            bImg.type = Image.Type.Sliced;
            bImg.color = new Color(0.95f, 0.75f, 0.2f, 1f);
            borderGO.transform.SetAsFirstSibling();

            // Icon
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(go.transform, false);
            var iRect = iconGO.GetComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0.5f, 1f);
            iRect.anchorMax = new Vector2(0.5f, 1f);
            iRect.pivot = new Vector2(0.5f, 1f);
            iRect.anchoredPosition = new Vector2(0, -12);
            iRect.sizeDelta = new Vector2(90, 90);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.preserveAspect = true;

            // Title
            GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(go.transform, false);
            var tRect = titleGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0f);
            tRect.anchorMax = new Vector2(1f, 0f);
            tRect.pivot = new Vector2(0.5f, 0f);
            tRect.anchoredPosition = new Vector2(0, 48);
            tRect.sizeDelta = new Vector2(-16, 24);
            var tTmp = titleGO.GetComponent<TextMeshProUGUI>();
            tTmp.text = "Item Name";
            tTmp.fontSize = 15;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = Color.white;

            // Subtitle
            GameObject subGO = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGO.transform.SetParent(go.transform, false);
            var sRect = subGO.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0f);
            sRect.anchorMax = new Vector2(1f, 0f);
            sRect.pivot = new Vector2(0.5f, 0f);
            sRect.anchoredPosition = new Vector2(0, 8);
            sRect.sizeDelta = new Vector2(-16, 38);
            var sTmp = subGO.GetComponent<TextMeshProUGUI>();
            sTmp.text = "Details / Stats";
            sTmp.fontSize = 11;
            sTmp.alignment = TextAlignmentOptions.Top;
            sTmp.color = new Color(0.7f, 0.75f, 0.85f);
            // word wrapping enabled by default

            SetPrivateField(dragHandler, "cardTransform", rect);
            SetPrivateField(dragHandler, "iconImage", iconImg);

            SetPrivateField(view, "iconImage", iconImg);
            SetPrivateField(view, "titleText", tTmp);
            SetPrivateField(view, "subtitleText", sTmp);
            SetPrivateField(view, "selectionBorder", bImg);
            SetPrivateField(view, "dragHandler", dragHandler);
            SetPrivateField(view, "clickButton", btn);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateBottomDrawer(Transform canvasTr, Sprite boxSprite, GameObject drawerItemPrefab)
        {
            GameObject drawerGO = new GameObject("CategoryDrawerPanel", typeof(RectTransform), typeof(Image), typeof(CategoryDrawerUI));
            drawerGO.transform.SetParent(canvasTr, false);
            var rect = drawerGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 270);

            var img = drawerGO.GetComponent<Image>();
            img.sprite = boxSprite;
            img.color = new Color(0.09f, 0.11f, 0.15f, 0.98f);

            var drawerUI = drawerGO.GetComponent<CategoryDrawerUI>();

            // Category Tab Buttons Container
            GameObject tabsGO = new GameObject("TabsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabsGO.transform.SetParent(drawerGO.transform, false);
            var tRect = tabsGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(0f, 1f);
            tRect.pivot = new Vector2(0f, 1f);
            tRect.anchoredPosition = new Vector2(30, -10);
            tRect.sizeDelta = new Vector2(800, 42);

            var hlg = tabsGO.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            var raceBtn = CreateTabButton(tabsGO.transform, boxSprite, "1. Species / Race");
            var classBtn = CreateTabButton(tabsGO.transform, boxSprite, "2. Class");
            var armorBtn = CreateTabButton(tabsGO.transform, boxSprite, "3. Armor & Attire");
            var weaponBtn = CreateTabButton(tabsGO.transform, boxSprite, "4. Weapons");

            // Items Container (Horizontal scroll/layout)
            GameObject itemsContainerGO = new GameObject("ItemsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            itemsContainerGO.transform.SetParent(drawerGO.transform, false);
            var icRect = itemsContainerGO.GetComponent<RectTransform>();
            icRect.anchorMin = new Vector2(0f, 0f);
            icRect.anchorMax = new Vector2(1f, 1f);
            icRect.anchoredPosition = new Vector2(30, -30);
            icRect.sizeDelta = new Vector2(-420, -70);

            var ihlg = itemsContainerGO.GetComponent<HorizontalLayoutGroup>();
            ihlg.spacing = 16;
            ihlg.childControlWidth = false;
            ihlg.childControlHeight = false;
            ihlg.childForceExpandWidth = false;
            ihlg.childForceExpandHeight = false;

            SetPrivateField(drawerUI, "raceTabButton", raceBtn);
            SetPrivateField(drawerUI, "classTabButton", classBtn);
            SetPrivateField(drawerUI, "armorTabButton", armorBtn);
            SetPrivateField(drawerUI, "weaponTabButton", weaponBtn);
            SetPrivateField(drawerUI, "itemsContainer", itemsContainerGO.transform);
            SetPrivateField(drawerUI, "drawerItemPrefab", drawerItemPrefab);
        }

        private static Button CreateTabButton(Transform parent, Sprite boxSprite, string title)
        {
            GameObject btnGO = new GameObject("Tab_" + title, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(175, 42);

            var img = btnGO.GetComponent<Image>();
            img.sprite = boxSprite;
            img.color = new Color(0.16f, 0.19f, 0.25f, 1f);

            var btn = btnGO.GetComponent<Button>();

            GameObject textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(btnGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = title;
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        private static void CreateExportModal(Transform canvasTr, Sprite boxSprite, Sprite cardSprite)
        {
            // Bottom Right Export Button
            GameObject exportBtnGO = new GameObject("ExportButton", typeof(RectTransform), typeof(Image), typeof(Button));
            exportBtnGO.transform.SetParent(canvasTr, false);
            var bRect = exportBtnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(1f, 0f);
            bRect.anchorMax = new Vector2(1f, 0f);
            bRect.pivot = new Vector2(1f, 0f);
            bRect.anchoredPosition = new Vector2(-30, 20);
            bRect.sizeDelta = new Vector2(280, 54);

            var bImg = exportBtnGO.GetComponent<Image>();
            bImg.sprite = boxSprite;
            bImg.color = new Color(0.85f, 0.22f, 0.22f, 1f); // D&D Beyond Red
            var exportBtn = exportBtnGO.GetComponent<Button>();

            GameObject btnTextGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextGO.transform.SetParent(exportBtnGO.transform, false);
            var btRect = btnTextGO.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.sizeDelta = Vector2.zero;
            var btTmp = btnTextGO.GetComponent<TextMeshProUGUI>();
            btTmp.text = "<b>Export to D&D Beyond -></b>";
            btTmp.fontSize = 16;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;

            // Modal Overlay
            GameObject overlayGO = new GameObject("ExportModalOverlay", typeof(RectTransform), typeof(Image));
            overlayGO.transform.SetParent(canvasTr, false);
            var oRect = overlayGO.GetComponent<RectTransform>();
            oRect.anchorMin = Vector2.zero;
            oRect.anchorMax = Vector2.one;
            oRect.sizeDelta = Vector2.zero;
            var oImg = overlayGO.GetComponent<Image>();
            oImg.sprite = boxSprite;
            oImg.color = new Color(0f, 0f, 0f, 0.75f);

            // Modal Card
            GameObject modalCardGO = new GameObject("ModalCard", typeof(RectTransform), typeof(Image));
            modalCardGO.transform.SetParent(overlayGO.transform, false);
            var mRect = modalCardGO.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f);
            mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.pivot = new Vector2(0.5f, 0.5f);
            mRect.sizeDelta = new Vector2(560, 480);

            var mImg = modalCardGO.GetComponent<Image>();
            mImg.sprite = cardSprite;
            mImg.type = Image.Type.Sliced;
            mImg.color = new Color(0.12f, 0.14f, 0.19f, 1f);

            var exportUI = exportBtnGO.AddComponent<ExportSummaryUI>();

            // Modal Header
            GameObject mHeadGO = new GameObject("ModalHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
            mHeadGO.transform.SetParent(modalCardGO.transform, false);
            var mhRect = mHeadGO.GetComponent<RectTransform>();
            mhRect.anchorMin = new Vector2(0f, 1f);
            mhRect.anchorMax = new Vector2(1f, 1f);
            mhRect.pivot = new Vector2(0.5f, 1f);
            mhRect.anchoredPosition = new Vector2(0, -20);
            mhRect.sizeDelta = new Vector2(-40, 36);
            var mhTmp = mHeadGO.GetComponent<TextMeshProUGUI>();
            mhTmp.text = "<b>D&D BEYOND EXPORT SUMMARY</b>";
            mhTmp.fontSize = 20;
            mhTmp.alignment = TextAlignmentOptions.Center;
            mhTmp.color = Color.white;

            // Details Text
            GameObject detailsGO = new GameObject("DetailsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            detailsGO.transform.SetParent(modalCardGO.transform, false);
            var dRect = detailsGO.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 1f);
            dRect.anchorMax = new Vector2(1f, 1f);
            dRect.pivot = new Vector2(0f, 1f);
            dRect.anchoredPosition = new Vector2(30, -70);
            dRect.sizeDelta = new Vector2(-60, 140);
            var dTmp = detailsGO.GetComponent<TextMeshProUGUI>();
            dTmp.text = "Species: Tiefling\nClass: Barbarian\nArmor: Plate Armor (AC 18)\nWeapon: Greataxe (1d12 Slashing)";
            dTmp.fontSize = 16;
            dTmp.lineSpacing = 10;
            dTmp.color = new Color(0.9f, 0.92f, 0.95f);

            // Rule Harmony Text
            GameObject ruleGO = new GameObject("RuleHarmonyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            ruleGO.transform.SetParent(modalCardGO.transform, false);
            var rRect = ruleGO.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0f, 0f);
            rRect.anchorMax = new Vector2(1f, 1f);
            rRect.pivot = new Vector2(0.5f, 0.5f);
            rRect.anchoredPosition = new Vector2(0, -35);
            rRect.sizeDelta = new Vector2(-60, -260);
            var rTmp = ruleGO.GetComponent<TextMeshProUGUI>();
            rTmp.text = "<color=#FF8844><b>[Rage Restriction]</b></color>\nBarbarians lose Rage benefits when wearing Heavy Armor!";
            rTmp.fontSize = 14;
            rTmp.color = Color.white;
            // word wrapping enabled by default

            // Feedback Text
            GameObject feedGO = new GameObject("FeedbackText", typeof(RectTransform), typeof(TextMeshProUGUI));
            feedGO.transform.SetParent(modalCardGO.transform, false);
            var fRect = feedGO.GetComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0f, 0f);
            fRect.anchorMax = new Vector2(1f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.anchoredPosition = new Vector2(0, 75);
            fRect.sizeDelta = new Vector2(-60, 30);
            var fTmp = feedGO.GetComponent<TextMeshProUGUI>();
            fTmp.text = "";
            fTmp.fontSize = 14;
            fTmp.alignment = TextAlignmentOptions.Center;

            // Confirm Export Button
            GameObject confirmBtnGO = new GameObject("ConfirmBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            confirmBtnGO.transform.SetParent(modalCardGO.transform, false);
            var confRect = confirmBtnGO.GetComponent<RectTransform>();
            confRect.anchorMin = new Vector2(0.5f, 0f);
            confRect.anchorMax = new Vector2(0.5f, 0f);
            confRect.pivot = new Vector2(0.5f, 0f);
            confRect.anchoredPosition = new Vector2(-80, 20);
            confRect.sizeDelta = new Vector2(180, 44);
            var confImg = confirmBtnGO.GetComponent<Image>();
            confImg.sprite = boxSprite;
            confImg.color = new Color(0.22f, 0.65f, 0.35f, 1f);
            var confBtn = confirmBtnGO.GetComponent<Button>();

            GameObject confTextGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            confTextGO.transform.SetParent(confirmBtnGO.transform, false);
            var ctRect = confTextGO.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.sizeDelta = Vector2.zero;
            var ctTmp = confTextGO.GetComponent<TextMeshProUGUI>();
            ctTmp.text = "Confirm Export";
            ctTmp.fontSize = 14;
            ctTmp.alignment = TextAlignmentOptions.Center;
            ctTmp.color = Color.white;

            // Close Button
            GameObject closeBtnGO = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGO.transform.SetParent(modalCardGO.transform, false);
            var clRect = closeBtnGO.GetComponent<RectTransform>();
            clRect.anchorMin = new Vector2(0.5f, 0f);
            clRect.anchorMax = new Vector2(0.5f, 0f);
            clRect.pivot = new Vector2(0.5f, 0f);
            clRect.anchoredPosition = new Vector2(110, 20);
            clRect.sizeDelta = new Vector2(160, 44);
            var clImg = closeBtnGO.GetComponent<Image>();
            clImg.sprite = boxSprite;
            clImg.color = new Color(0.3f, 0.35f, 0.42f, 1f);
            var clBtn = closeBtnGO.GetComponent<Button>();

            GameObject clTextGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            clTextGO.transform.SetParent(closeBtnGO.transform, false);
            var cltRect = clTextGO.GetComponent<RectTransform>();
            cltRect.anchorMin = Vector2.zero;
            cltRect.anchorMax = Vector2.one;
            cltRect.sizeDelta = Vector2.zero;
            var cltTmp = clTextGO.GetComponent<TextMeshProUGUI>();
            cltTmp.text = "Back to Creator";
            cltTmp.fontSize = 14;
            cltTmp.alignment = TextAlignmentOptions.Center;
            cltTmp.color = Color.white;

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
