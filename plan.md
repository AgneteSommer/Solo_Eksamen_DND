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

## 3. Visual Layout & UX Design (16:9 Widescreen Wireframe)
* **Reference Resolution:** `1920 × 1080` (16:9 Landscape).
* **Visual Theme & Palette (D&D Beyond Official Light Theme):**
  * Inspired directly by D&D Beyond's actual web character builder (see reference screenshot):
    - **Background:** Light off-white parchment / clean neutral paper (`#F5F5F3` / `#F8F7F5`).
    - **UI Panels & Cards:** Crisp white rounded cards (`#FFFFFF` with `#E0E0E0` border, `radius: 8px`).
    - **Typography:** Bold dark charcoal/black headers (`#242527`), soft grey subtitles/captions (`#666666`), and classic D&D Beyond red accents (`#B71C1C`).
    - **Chevrons:** Subtle blue/grey navigation arrows (`#2576B3` / `#8091A5`).
* **Layout Geometry:**
  * **Top Header (Y = 1020 to 1080):** Compact D&D Beyond banner (`height: 60px`).
  * **Top-Left (X = 40 to 440, Y = 620 to 1000):**
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
      - **Progress Milestone:** When all 4 slots are chosen, a celebratory gold tag unlocks: **"⭐ Adventure Ready! (4/4)"**!
  * **Center Stage (X = 460 to 1380):**
    * **Hero-Scale Character & Platform:**
      - The character mannequin stands **tall and heroic** (~750px tall).
      - **Initial Visual State:** Starts as a clean, neutral mannequin silhouette on the pedestal (no pre-equipped armor, horns, or weapon).
      - **Platform Alignment:** The circular diorama platform is positioned **directly at the character's feet** and grounded near the bottom of the screen (`Y ≈ -2.8` to `-3.0` world space). The character stands *on* the platform, not floating or clipped at the waist!
  * **Right Panel: Category Menu (X = 1390 to 1900, Width = ~510px — 10% Wider for Breathing Room):**
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
|  |  benefits when wearing      |                     |    (750px tall mini)  |       | [ICON] CLERIC       > | |
|  |  Heavy Armor!"              |                     |                       |       |   "A devout champ.."  | |
|  +-----------------------------+                     |                       |       +-----------------------+ |
|                                                      |                       |                                 |
|  (BOTTOM-LEFT)                                       |                       |                                 |
|  +-----------------------------+                     |                       |                                 |
|  | MINI CHARACTER SHEET        |                     |                       |                                 |
|  | [✓] Species: Tiefling       |                     |                       |                                 |
|  | [✓] Class:   Barbarian      |                     |                       |                                 |
|  | [✓] Armor:   Heavy Plate    |                     |   =================   |       +-----------------------+ |
|  | [✓] Weapon:  Greataxe       |                     |   [ Platform/Feet ]   |       | [ Export Sheet -> ]   | |
|  | AC: 18                      |                     +-----------------------+       +-----------------------+ |
|  | ⭐ ADVENTURE READY! (4/4)    |                     (Grounded at bottom)                                      |
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

## 5. Implementation Milestones for Architect Agent

1. **Milestone 1: Mini Sheet Relocation & Clean-Slate Checklist**
   * Move Mini Character Sheet to the **Bottom-Left corner**.
   * Use `VerticalLayoutGroup` so `Armor Class` never overlaps text.
   * **Clean-Slate Startup:** The player starts with `0/4` choices made. The mannequin starts as a neutral blank base on the platform, and the checklist shows `[ ] (None chosen)`.
   * Add dynamic checklist checks (`[✓]`) that unlock the celebratory golden **"⭐ Adventure Ready! (4/4)"** status when all 4 slots are filled.

2. **Milestone 2: Top-Left Synergy HUD vs. Dismissable Rule Quirk Card**
   * Keep the permanent top-left box titled **"SYNERGY"** (houses the D20 orb, cannot be minimized).
   * Create the pop-up/slide-down card below it titled **"RULE QUIRK"** with a visible `[X]` minimize/dismiss button that lets the player close it.

3. **Milestone 3: Grounded Platform Alignment**
   * Position the circular platform sprite directly under the soles of the mannequin's feet.
   * Ground the character + platform near the bottom of the screen (`Y ≈ -2.8` to `-3.0` world space).

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
