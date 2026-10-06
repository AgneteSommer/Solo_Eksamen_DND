# D&D Beyond: Visual Character Creation Onboarding (Plan & Specification)

> **CORE PROTOTYPING DIRECTIVE: MECHANICS-FIRST / GREYBOXING**  
> For this playable prototype, prioritize functionality, clear layout, and responsive interaction over art polish. Clean colored boxes, standard geometric shapes, and crisp TextMeshPro UI elements are expected. Proof of concept, accurate D&D rule feedback, and an intuitive user flow are the core goals!

---

## 1. Project Overview & Exam Framework
* **Sender:** D&D Beyond (Wizards of the Coast digital companion & toolset).
* **Receiver:** Adults aged 18–34 interested in D&D, particularly beginners and visually-oriented players who find the 300+ page *Player's Handbook* rules intimidating.
* **Core Problem:** Newcomers face cognitive overload from extensive text, tables, and abstract jargon before developing a visual or emotional connection to their character.
* **Solution Concept:** An interactive 2D "paper-doll / miniature" dress-up onboarding experience that serves as an intuitive visual gateway into D&D Beyond's official character creator. It teaches core mechanics through visual cause-and-effect and rule-harmony discovery.

---

## 2. Academic Gamification Framework (For Exam Report & Presentation)
This project uses **three foundational game design mechanics** to transform dry character creation form-filling into a playful experience:

1. **Immediate Feedback Loops & Avatar Customization (Self-Expression / Toy-Play):**
   * *Mechanism:* Equipping gear or picking a species instantly alters the 2D character avatar on the platform.
   * *Learning Theory:* Replaces passive text tables with visual cause-and-effect and player ownership (Karl Kapp).
2. **Curiosity Loops & "Safe Failure" (The Synergy Engine):**
   * *Mechanism:* Unconventional combinations (like a Barbarian or Wizard in Heavy Armor) do not show punitive error messages. Instead, the D20 pulses and reveals a constructive "Synergy" insight card.
   * *Learning Theory:* Encourages active experimentation and safe trial-and-error learning without fear of making a "wrong" character.
3. **Goal Progression & Milestones (The "Adventure Ready" Quest Checklist):**
   * *Mechanism:* A 4-step progress checklist (`[✓] Species`, `[✓] Class`, `[✓] Armor`, `[✓] Weapon`) in the bottom-left mini sheet that culminates in a celebratory **"⭐ Adventure Ready! (4/4)"** status when complete.
   * *Learning Theory:* Breaks a daunting 300-page ruleset into 4 bite-sized, achievable mini-goals (Zeigarnik Effect / Completion Dynamics).

---

## 3. Scene Flow & Platform Integration (Main Menu -> Visual Creator)
To ground the project in the school guidelines (**Sender: D&D Beyond**), the user starts on the official D&D Beyond "Character Creation Method" menu, establishing that this tool is an organic extension of the website:

1. **Scene 0: `MainMenu` (The Entry Gateway):**
   * **Background:** Uses `Assets/Sprites/Main Menu/MainMenu_Background.png` (displays the authentic D&D Beyond methods: *Standard*, *Quickbuilder*, and *Premade*).
   * **Visual Overlay:** Uses `Assets/Sprites/Main Menu/MainMenu_Button.png` stretched full-canvas `(0, 0)` to `(1, 1)` with `raycastTarget = false` so it renders pixel-perfect without distortion and does NOT intercept clicks.
   * **Precise Trigger Zone:** A dedicated invisible button (`RectTransform` + transparent graphic) sits strictly over the 4th card coordinates:
     - `anchorMin = (417/2880, 28/1526) ≈ (0.145f, 0.018f)`
     - `anchorMax = (2430/2880, 455/1526) ≈ (0.844f, 0.298f)`
     - **Hover Isolation:** The button ONLY reacts when hovering over the "Visual Designer" card. Hovering over *Standard*, *Quickbuilder*, or *Premade* does NOT trigger it!
   * **Interaction:** Clicking anywhere inside the 4th card triggers `UI_Click.wav` and transitions smoothly to `SampleScene`.
2. **Scene 1: `SampleScene` (The Visual Character Creator):**
   * Contains a top-left button: `[ < Back to Methods ]` allowing the user to return to the Main Menu anytime (essential for video demonstrations).

---

## 4. Visual Layout & UX Design (16:9 Widescreen Wireframe)
* **Reference Resolution:** `1920 × 1080` (16:9 Landscape).
* **Background Asset:** `Assets/Sprites/CharacterCreater_Background.png` (authentic D&D Beyond web frame with top navigation bar and forest diorama clearing).
* **CRITICAL Header Rule:** **DO NOT render a custom UI `HeaderPanel` or banner!** The official D&D Beyond navbar is already baked into the background image. The custom banner must be removed so it doesn't double-layer.
* **UI Safe-Zone Boundaries:**
  * **Top Margin:** All UI elements must sit below `Y = 970` (so they never overlap the baked-in top navigation bar).
  * **Right Margin:** The right-side category menu must have a `40px` inset (`X = 1370 to 1860`) so it never covers the vertical web browser scrollbar baked into the right edge of `CharacterCreater_Background.png`.
  * **Left Margin:** `X = 40px` to `440px`.
* **Layout Geometry:**
  * **Top-Left (X = 40 to 440, Y = 620 to 960):**
    * **1. Permanent HUD Box (Cannot Minimize):**
      - Title: **"SYNERGY"**
      - Contains: Magical D20 orb indicating current synergy state (e.g. *Harmonious* vs. *Quirk Active*).
      - Behavior: Always visible on screen as a constant status gauge.
    * **2. Pop-up Insight Card (CAN Minimize / Close):**
      - Title: **"RULE QUIRK"**
      - Contains: 1-2 sentence explanation of the specific 5e mechanic (e.g., *"Barbarians lose Rage benefits while wearing Heavy Armor!"*).
      - Behavior: Pops up / slides down when a quirk is triggered, and features a `[X]` minimize/close button so the player can dismiss it at will.
  * **Bottom-Left (X = 40 to 440, Y = 40 to 420):**
    * **Mini Character Sheet Panel (Clean-Slate Start):** Clean, vertically-stacked summary tags using `VerticalLayoutGroup` (no overlapping text).
      - **Initial State:** The player begins with an empty canvas (`0/4 Choices Made`):
        - `[ ] Species: (None chosen)`
        - `[ ] Class: (None chosen)`
        - `[ ] Armor: (None chosen)`
        - `[ ] Weapon: (None chosen)`
        - `Armor Class: 10 (Base)`
      - **Dynamic Progression:** As choices are equipped, slots fill and check off (`[✓]`).
      - **Progress Milestone & Audio Fanfare:**
        - When all 4 slots are chosen, a celebratory gold tag unlocks: **"⭐ Adventure Ready! (4/4)"**!
        - **Sound Trigger:** Instantly plays a celebratory victory fanfare / chime (`Assets/Sound/Quest_Complete.wav`) to reward the player for finishing their build!
  * **Center Stage (X = 440 to 1370):**
    * **Empty Stage Startup (Natural Grounding):**
      - **Initial State:** The center stage starts **COMPLETELY EMPTY** (the character is hidden/inactive until the first selection is made).
      - **NO PLATFORM / PEDESTAL:** The pedestal has been completely removed! The character stands directly on their own feet in the natural forest clearing.
      - **Lowered Framing (Zero Navbar Overlap):**
        - In the previous setup, the head overlapped with the top navigation bar.
        - **Positioning Fix:** Lower the mannequin world position to `Y ≈ -0.7f` with a balanced scale of `~0.42f – 0.44f`.
        - This places the head, hair, and horns at `Y ≈ +2.8` to `+3.2` world space, leaving generous clearance below the top navbar (`Y ≈ +4.0`), with feet resting comfortably on the forest trail near the bottom of the viewport.
  * **Right Panel: Category Menu (X = 1370 to 1860, Width = ~490px):**
    * Inset cleanly away from the right browser scrollbar.
    * Uses a **Two-Level Drill-Down Navigation** (hiding irrelevant categories when viewing sub-options):
      - **Level 1 (Main Menu):** Displays 4 chunky category buttons:
        `[ 1. Species / Race ]`, `[ 2. Class ]`, `[ 3. Armor & Attire ]`, `[ 4. Weapons ]`.
      - **Level 2 (Subcategory View):** When a category is clicked, the main menu is replaced by a dedicated subcategory view:
        - Top Bar: `[ < Back to Categories ]` + Title (e.g. `SELECT CLASS`).
        - Content: Scrollable list of white rounded cards matching the D&D Beyond reference image:
          - Left: Official colored class icon badge.
          - Center: Uppercase bold title (e.g., **BARBARIAN**) + 1-sentence fantasy hook.
          - Right: Subtle right-arrow chevron `>`.
    * **Bottom Action:** `[ Export to D&D Beyond -> ]` button fixed at the bottom right.

```
+----------------------------------------------------------------------------------------------------------------+
|  [D&D BEYOND HEADER BANNER - LIGHT THEME]                                                                      |
|                                                                                                                |
|  (TOP-LEFT)                              (CENTER STAGE: HERO VIEWPORT)               (RIGHT MENU ~460px)       |
|  +-----------------------------+                                                     +-----------------------+ |
|  | [ PERMANENT: SYNERGY ]      |                                                     | [< Back] SELECT CLASS | |
|  | (D20 Orb - Cannot Minimize) |                                                     |-----------------------| |
|  +-----------------------------+                                                     | [ICON] BARBARIAN    > | |
|                 |                                                                    |   "A fierce warrior.."| |
|                 v (Dismissable)                                                      |-----------------------| |
|  +-----------------------------+                     +-----------------------+       | [ICON] BARD         > | |
|  | [ RULE QUIRK [X] ]          |                     |                       |       |   "An inspiring.."    | |
|  | "Barbarians lose Rage       |                     |     HERO MANNEQUIN    |       |-----------------------| |
|  |  benefits when wearing      |                     |    (Stands on feet,   |       | [ICON] CLERIC       > | |
|  |  Heavy Armor!"              |                     |     lowered below     |       |   "A devout champ.."  | |
|  +-----------------------------+                     |     top navbar)       |       +-----------------------+ |
|                                                      |                       |                                 |
|  (BOTTOM-LEFT)                                       |   (Natural Grounding  |                                 |
|  +-----------------------------+                     |    in forest clearing)|                                 |
|  | MINI CHARACTER SHEET        |                     |   - NO PEDESTAL -     |                                 |
|  | [✓] Species: Tiefling       |                     +-----------------------+                                 |
|  | [✓] Class:   Barbarian      |                                                     +-----------------------+ |
|  | [✓] Armor:   Heavy Plate    |                                                     | [ Export Sheet -> ]   | |
|  | [✓] Weapon:  Greataxe       |                                                     +-----------------------+ |
|  | AC: 18                      |                                                                               |
|  | ⭐ ADVENTURE READY! (4/4)    |                                                                               |
|  +-----------------------------+                                                                               |
+----------------------------------------------------------------------------------------------------------------+
```

---

## 4. Complete Class Roster & 1-Sentence Descriptions (Alphabetical)

Each class entry in the selection drawer includes the official D&D Beyond logo badge (with its specific class theme color), uppercase name, and a vibrant 1-sentence fantasy hook focusing on **how the character feels**:

| # | Class Name | 1-Sentence Beginner Fantasy Hook | Key Synergy Insight |
|---|------------|-----------------------------------|---------------------|
| 1 | **Barbarian** | *"A fierce warrior driven by primal fury who charges headfirst into the heat of battle."* | Heavy armor disables Rage benefits. |
| 2 | **Bard** | *"An inspiring performer and charismatic storyteller whose music weaves enchantment and wonder."* | Non-proficient armor blocks spellcasting. |
| 3 | **Cleric** | *"A devout champion of the gods who channels divine light, miracles, and protective magic."* | Proficient with shields & armor. |
| 4 | **Druid** | *"A guardian of the wilderness who commands the forces of nature and transforms into mighty beasts."* | Taboo against metal armor; nature focus. |
| 5 | **Fighter** | *"A master of weapons and battlefield tactics who conquers danger with pure combat skill."* | Peak synergy with all weapon and armor tiers. |
| 6 | **Monk** | *"A disciplined martial artist who channels spiritual inner ki into lightning-fast unarmed strikes."* | Armor disables Martial Arts and Unarmored Defense. |
| 7 | **Paladin** | *"A noble warrior bound by a sacred oath to smite evil and stand as an unyielding beacon of hope."* | High synergy with Heavy Armor and martial weapons. |
| 8 | **Ranger** | *"A master tracker and scout who walks the untamed frontiers with deadly precision and wilderness magic."* | Heavy armor impairs stealth. |
| 9 | **Rogue** | *"A cunning trickster who excels in stealth, agility, and striking lethal blows from the shadows."* | Sneak attack requires finesse/ranged; stealth priority. |
| 10| **Sorcerer** | *"A passionate magic wielder born with wild, raw arcane power coursing through their veins."* | Wearing non-proficient armor blocks all spellcasting. |
| 11| **Warlock** | *"A seeker of occult secrets who draws eerie eldritch power from a pact with an otherworldly patron."* | Medium/heavy armor blocks spellcasting. |
| 12| **Wizard** | *"A scholarly master of the arcane who bends reality to their will through intellect and spellbooks."* | Wearing armor blocks spellcasting; staff serves as focus. |

---

## 5. Class Proficiency & Rule Harmony Matrix (D&D 5e Rules)

### Startup Clean-Slate State (Zero Choices Made):
* When entering the scene, if no choices or no Class has been selected (`Class == None`):
  - **Synergy State:** **Harmonious** (Neutral Balanced).
  - **D20 Indicator:** Soft gold/green harmonious glow (no red alert badge or pulse).
  - **Rule Quirk Pop-up Card:** **COMPLETELY HIDDEN / CLOSED** (`alpha = 0`, inactive).
  - **Status Text:** *"Balanced Synergy — As you select your Class, Armor, and Weapons, your proficiencies and quirks will appear here."*
  - **Rule Rule:** A Rule Quirk CANNOT trigger unless the relevant Class and Equipment are actively selected!

---

### Armor Proficiency & Quirk Matrix:
| Class | Robes / Unarmored | Light Armor | Medium Armor | Heavy Armor |
| :--- | :---: | :---: | :---: | :---: |
| **Barbarian** | ⭐ *Unarmored Defense* (+CON) | Proficient | Proficient | ⚠️ *Quirk: Rage disabled in Heavy Armor* |
| **Bard** | Proficient | Proficient | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* |
| **Cleric** | Proficient | Proficient | Proficient | ⚠️ *Quirk: Non-proficient for standard domains* |
| **Druid** | Proficient | Proficient | ⚠️ *Quirk: Metal taboo (will not wear metal)* | ⚠️ *Quirk: Non-proficient & metal taboo* |
| **Fighter** | Proficient | Proficient | Proficient | ⭐ *Full Mastery (Peak Heavy Plate AC)* |
| **Monk** | ⭐ *Unarmored Defense* (+WIS) | ⚠️ *Quirk: Disables Martial Arts & Unarmored Move* | ⚠️ *Quirk: Disables Martial Arts & Unarmored Move* | ⚠️ *Quirk: Disables Martial Arts & Unarmored Move* |
| **Paladin** | Proficient | Proficient | Proficient | ⭐ *Peak Holy Warrior Synergy* |
| **Ranger** | Proficient | Proficient | Proficient | ⚠️ *Quirk: Non-proficient (disadvantage on stealth)* |
| **Rogue** | Proficient | Proficient | ⚠️ *Quirk: Non-proficient (disadvantage on stealth)* | ⚠️ *Quirk: Non-proficient (blocks stealth & mobility)* |
| **Sorcerer** | Proficient | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* |
| **Warlock** | Proficient | Proficient | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* |
| **Wizard** | ⭐ *Natural Arcane Robes* | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* | ⚠️ *Quirk: Non-proficient (blocks spellcasting)* |

---

### Weapon Proficiency & Synergy Matrix:
| Class | Daggers (Simple, Finesse) | Staff (Simple, Versatile) | Greataxe (Martial Melee) | Longbow (Martial Ranged) |
| :--- | :---: | :---: | :---: | :---: |
| **Barbarian** | Proficient | Proficient | ⭐ *Peak Fury (Reckless Two-Handed Strike)* | Proficient |
| **Bard** | Proficient | Proficient | ⚠️ *Quirk: Non-proficient (no attack bonus)* | ⚠️ *Quirk: Non-proficient (unless Elf)* |
| **Cleric** | Proficient | Proficient | ⚠️ *Quirk: Non-proficient (no attack bonus)* | ⚠️ *Quirk: Non-proficient (unless Elf)* |
| **Druid** | Proficient | ⭐ *Focus / Shillelagh* | ⚠️ *Quirk: Non-proficient (no attack bonus)* | ⚠️ *Quirk: Non-proficient (unless Elf)* |
| **Fighter** | Proficient | Proficient | ⭐ *Peak Martial Mastery* | ⭐ *Peak Archery Synergy* |
| **Monk** | Proficient | ⭐ *Monk Weapon* | ⚠️ *Quirk: Heavy weapon disables Martial Arts* | ⚠️ *Quirk: Heavy weapon disables Martial Arts* |
| **Paladin** | Proficient | Proficient | ⭐ *Peak Divine Smite Weapon* | Proficient |
| **Ranger** | Proficient | Proficient | Proficient | ⭐ *Signature Master Archer Synergy* |
| **Rogue** | ⭐ *Sneak Attack (Finesse)* | Proficient | ⚠️ *Quirk: Heavy weapon cannot Sneak Attack* | ⚠️ *Quirk: Non-proficient (unless Elf)* |
| **Sorcerer** | Proficient | ⭐ *Arcane Focus* | ⚠️ *Quirk: Non-proficient (no attack bonus)* | ⚠️ *Quirk: Non-proficient (unless Elf)* |
| **Warlock** | Proficient | ⭐ *Pact Focus* | ⚠️ *Quirk: Non-proficient (no attack bonus)* | ⚠️ *Quirk: Non-proficient (unless Elf)* |
| **Wizard** | Proficient | ⭐ *Arcane Focus* | ⚠️ *Quirk: Non-proficient (no attack bonus)* | ⚠️ *Quirk: Non-proficient (unless Elf)* |

*Special Racial Synergy Rule:* If the player selects **Elf**, they receive racial proficiency with the **Longbow**, turning it into a Harmonious choice regardless of class!

---

## 6. Final Sprite Asset Mapping (1000×1600 Unified Canvas)
All hand-drawn assets in `Assets/Sprites/Final/` share an identical **1000 × 1600 resolution**. Because they are drawn on the same canvas, setting all `SpriteRenderer` local positions to `(0, 0, 0)` guarantees **100% automatic pixel-perfect alignment** without any offset math!

### Layered Hierarchy & Sorting Orders (No Pedestal):
| Sorting Order | Layer Name | Asset Folder | Available Sprites |
| :---: | :--- | :--- | :--- |
| **10** | `1_Body` | `Assets/Sprites/Final/Races/` | `Elf.png`, `Tiefling.png` |
| **20** | `2_Armor` | `Assets/Sprites/Final/Armor/` | `No Armor.png`, `Light Armor.png`, `Medium Armor.png`, `Heavy Armor.png` |
| **30** | `3_Hair` | `Assets/Sprites/Final/Apperence/`| `Hair1.png`, `Hair2.png` |
| **40** | `4_Horns` | `Assets/Sprites/Final/Apperence/`| `Horn1.png` (Renders on top of Hair!) |
| **50** | `5_Weapon` | `Assets/Sprites/Final/Weapons/` | `Daggers.png`, `Great Sword.png`, `Long Bow.png`, `Staff.png` |

### Customization Categories & Drawer Items:
1. **Species / Race:**
   - Swaps the base body sprite directly to `Elf.png` or `Tiefling.png`.
2. **Armor & Attire:**
   - Options: `No Armor (Clothes)`, `Light Armor`, `Medium Armor`, `Heavy Armor`.
3. **Weapons:**
   - Options: `Daggers` (Finesse), `Great Sword` (Heavy Martial), `Long Bow` (Ranged), `Staff` (Arcane Focus).
4. **Appearance (New Category / Tab in Right Menu):**
   - Sub-options for `Hair` (`None`, `Hair 1`, `Hair 2`) and `Horns` (`None`, `Horn 1`).

---

## 6. Implementation Milestones for Architect Agent

0. **Milestone 0: Main Menu Scene & Trigger Zone Isolation**
   * Background Image with `MainMenu_Background.png` (2880×1526, full stretch).
   * Visual Overlay Image with `MainMenu_Button.png` (2880×1526, full stretch `(0,0)` to `(1,1)`, `raycastTarget = false`) so the card renders at 100% full scale without distortion.
   * **Trigger Zone Isolation:** An invisible button (`RectTransform` + clear graphic) sitting strictly over the bottom card bounds `anchorMin = (0.145, 0.018)`, `anchorMax = (0.844, 0.298)`. Hovering over *Standard*, *Quickbuilder*, or *Premade* will never trigger the button!
   * Clicking the card plays `UI_Click.wav` and loads `SampleScene.unity`.
   * In `SampleScene`, ensure the `[ < Back to Creation Methods ]` button loads `MainMenu.unity`.

1. **Milestone 1: Background Fit, Text Sharpness & Clean-Slate Checklist with Fanfare**
   * Use `Assets/Sprites/CharacterCreater_Background.png` as the background image for `SampleScene`.
   * **REMOVE the custom UI `HeaderPanel` banner!** The official D&D Beyond navigation bar is already baked into the background image.
   * **UI Safe Bounds:** Top UI elements below `Y = 970`, right menu inset by `40px` (`X = 1370 to 1860`).
   * **Text Sharpness:** Configure TextMeshPro font settings / CanvasScaler (`1920x1080`, `dynamicPixelsPerUnit = 10`) and crisp font asset settings for razor-sharp typography.
   * **Mini Character Sheet:** Move to Bottom-Left corner with `VerticalLayoutGroup`. Starts at `0/4 Choices Made`.
   * **Completion Fanfare Audio:** When the player reaches `4/4 Choices Made` (*"⭐ Adventure Ready!"*), play a rewarding victory fanfare / chime (`Assets/Sound/Quest_Complete.wav`).

2. **Milestone 2: Top-Left Synergy HUD, Clean-Slate Balance & Full Proficiency Engine**
   * **Clean-Slate Startup State:**
     - The D20 orb starts in a **Balanced / Harmonious** state (soft green/gold glow, NO alert badge or pulsing).
     - The "RULE QUIRK" card starts **COMPLETELY CLOSED / HIDDEN** on scene launch (`alpha = 0`, inactive).
     - Its initial text defaults to *"Balanced Synergy — Choose your Class, Armor, and Weapons to discover synergies and class quirks."* (NEVER default to a Barbarian quirk before choices are made!).
     - If `Class == None`, no rule quirks can ever trigger.
   * **Full 5e Proficiency & Quirk Rule Set:**
     - Implement the complete Armor & Weapon proficiency matrix (from Section 5):
       - Armor: Barbarian (heavy disables rage), Wizard/Sorcerer/Monk (armor blocks casting/abilities), Bard/Warlock/Rogue (medium/heavy blocks casting/stealth), Druid (metal taboo), Fighter/Paladin (heavy mastery).
       - Weapons: Daggers (proficient all; Rogue sneak attack), Staff (proficient all; caster focus), Greataxe (martial only; Barbarian peak fury; disables Rogue sneak attack), Longbow (martial; Ranger signature; Elf racial proficiency!).
   * Keep the permanent top-left box titled **"SYNERGY"** (houses the D20 orb, cannot be minimized).
   * Pop-up card titled **"RULE QUIRK"** with a visible `[X]` minimize/dismiss button that only slides down when an actual quirk is discovered.

3. **Milestone 3: Empty Center Stage Startup & Natural Grounding (No Pedestal)**
   * **Initial State:** At launch, the center stage is **COMPLETELY EMPTY** (the character mannequin is inactive/hidden until the first selection).
   * **NO PEDESTAL / PLATFORM:** The circular pedestal is removed entirely. The character stands directly on their own feet in the natural forest clearing path.
   * **Lowered Framing (Zero Navbar Overlap):**
     - Adjust the mannequin world position to `Y ≈ -0.7f` with a balanced scale of `~0.42f – 0.44f`.
     - Ensures head, hair, and tall horns remain safely below the baked-in top navbar (`Y <= +3.2f` world space), with feet touching down naturally along the clearing trail.
   * **Materialization:** As soon as the player selects any option (Species, Class, Armor, or Weapon), the character smoothly fades or appears in place!

4. **Milestone 4: 10% Wider Menu & Two-Level Drill-Down**
   * Expand the right panel width by 10% to **~510px** for generous breathing room.
   * Implement drill-down view swapping: Main Categories view (`Race`, `Class`, `Armor & Attire`, `Weapons`) swaps to a dedicated subcategory view with a clean `[ < Back to Categories ]` top button.

5. **Milestone 5: D&D Beyond Light Theme & Official Class Cards**
   * Apply D&D Beyond's light theme palette: parchment/light background (`#F5F5F3`), crisp white rounded cards with subtle borders (`#E0E0E0`).
   * Import the pixel/vector versions of the official D&D Beyond class logos into `Assets/Sprites/Classes`.
   * Structure each class card to match the reference image:
     - Left: Colored class theme badge with white class emblem.
     - Center: Bold uppercase class name (e.g., **BARBARIAN**) + feeling-focused 1-sentence hook.
     - Right: Subtle right chevron `>`.

6. **Milestone 6: Final Hand-Drawn Sprites & Appearance System**
   * Replace greybox placeholder textures with the hand-drawn assets in `Assets/Sprites/Final/`.
   * Configure all `SpriteRenderer` components on the mannequin at local `(0, 0, 0)` with matching sorting orders:
     - `Order 10: Body` (`Elf.png` or `Tiefling.png`)
     - `Order 20: Armor` (`No Armor.png`, `Light Armor.png`, `Medium Armor.png`, `Heavy Armor.png`)
     - `Order 30: Hair` (`Hair1.png`, `Hair2.png`)
     - `Order 40: Horns` (`Horn1.png` - rendered on top of hair!)
     - `Order 50: Weapons` (`Daggers.png`, `Great Sword.png`, `Long Bow.png`, `Staff.png`)
   * Add the **Appearance** category to the right menu allowing the player to customize Hair and Horns!
   * Wire equipment audio triggers to `Assets/Sound/` (`Clothes Apply.wav`, `Metal Armor.mp3`, `Weapon Equip.mp3`).
