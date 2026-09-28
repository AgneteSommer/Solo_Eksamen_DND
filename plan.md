# D&D Beyond: Visual Character Creation Onboarding (Plan & Specification)

> **CORE PROTOTYPING DIRECTIVE: MECHANICS-FIRST / GREYBOXING**  
> For this initial playable prototype, **do NOT spend time on visual polish, complex art assets, or matching exact D&D Beyond styling**. The priority is to test the interaction loop and prove the mechanics work. Colored boxes, simple geometric shapes, primitive placeholders, and basic UI elements are 100% acceptable and encouraged. Proof of concept and playability come first!

## 1. Project Overview & Exam Framework
* **Sender:** D&D Beyond (Wizards of the Coast digital companion & toolset).
* **Receiver:** Adults aged 18–34 interested in D&D, particularly beginners and visually-oriented players who find the 300+ page *Player's Handbook* rules intimidating.
* **Core Problem:** Newcomers face cognitive overload from extensive text, tables, and abstract jargon before developing a visual or emotional connection to their character.
* **Solution Concept:** An interactive 2D "paper-doll / miniature" dress-up onboarding experience that serves as an intuitive visual gateway into D&D Beyond's official character creator. It teaches core mechanics through visual cause-and-effect and rule-harmony discovery.

---

## 2. Visual Layout & UX Design (Greybox Wireframe)
The screen layout directly translates the paper prototyping mockups into a responsive 2D canvas. Placeholders (simple tinted panels, boxes, and standard UI buttons) should be used:.

```
+-----------------------------------------------------------------------------------+
|  [D&D Beyond Logo / Header]                                                       |
|                                                                                   |
|  (TOP-LEFT)                                   (TOP-RIGHT)                         |
|  +-----------------------+                    +--------------------------------+  |
|  |  [ Magical D20 ]      |                    | Mini Character Sheet           |  |
|  |  Harmony State Glow   |                    | [Tiefling] [Barbarian] [Heavy] |  |
|  +-----------------------+                    +--------------------------------+  |
|         |                                                                         |
|         v (Slides out on click/change)                                            |
|  +-----------------------------------+                                            |
|  | [Note / Rule Insight Card]        |                                            |
|  | "Barbarians lose Rage benefits    |                                            |
|  |  when wearing Heavy Armor!"       |                                            |
|  +-----------------------------------+                                            |
|                                                                                   |
|                        (CENTER)                                                   |
|                 +--------------------+                                            |
|                 |    Character       |                                            |
|                 |    Paper Doll      |                                            |
|                 |   (Layered 2D)     |                                            |
|                 |                    |                                            |
|                 |  ================  |                                            |
|                 |  [Diorama Base]    |                                            |
|                 +--------------------+                                            |
|                                                                                   |
|  (BOTTOM / DRAWER AREA)                                                           |
|  +-----------------------------------------------------------------------------+  |
|  | [Race]   [Class]   [Armor & Attire]   [Weapon]                              |  |
|  |-----------------------------------------------------------------------------|  |
|  | [Subcategory drawer: Thumbnails for Drag & Drop / Click Selection]          |  |
|  +-----------------------------------------------------------------------------+  |
|                                                                                   |
|                                                    [ Export to D&D Beyond -> ]    |
+-----------------------------------------------------------------------------------+
```

---

## 3. Core Mechanics & Architecture

### A. Modular Paper-Doll Layering (Universal Mannequin Pose)
To eliminate sprite multiplication, a single consistent neutral pose is used. Features attach across dedicated Unity 2D Sorting Layers:
1. `Layer 0: Pedestal / Diorama Base` (Includes glowing class rune/aura).
2. `Layer 1: Body Base` (Neutral silhouette, skin tone tinting).
3. `Layer 2: Race Features` (Elf ears / Tiefling horns & tail).
4. `Layer 3: Clothes / Undergarments` (Basic pants/tunic).
5. `Layer 4: Armor Overlay` (Light Leather / Medium Scale / Heavy Plate).
6. `Layer 5: Hair / Headwear`.
7. `Layer 6: Handheld Weapon` (Greataxe, Arcane Staff, Dagger).

### B. Two-Pronged Interaction Model
1. **Concept Cards (Race & Class):**
   * Instant click selection.
   * Selecting **Race** immediately updates physical attachments (horns/ears/skin).
   * Selecting **Class** updates the pedestal rune/aura, mini sheet tag, and default proficiencies.
2. **Tactile Drag-and-Drop + Click-to-Equip (Armor & Weapons):**
   * Hovering over an item enlarges its thumbnail (preview).
   * Dragging an item highlights the drop target on the character; releasing snaps it into place.
   * Double-clicking or clicking the thumbnail also equips it immediately for ease of use.

### C. Rule Harmony & Discovery Engine (The Magical D20)
Instead of an MMO-style "DPS tier list" or punitive "Score", the D20 acts as an **insight indicator**:
* **Harmonious State (Gold/Arcane Glow):**
  * Choices complement each other under 5e rules.
  * *Example:* Barbarian + Medium Armor / Unarmored + Greataxe -> *"Peak Harmony: Full Rage and Unarmored Defense active!"*
* **Discovery / Quirk State (Curious Pulsing / Inspection Icon):**
  * Highlights unusual or conflicting rule interactions without stopping the player.
  * Clicking the D20 or triggering the choice slides out the **"Note:" Card**:
    * *Barbarian + Heavy Armor:* "Barbarians are free to wear heavy armor, but their signature feature — Rage — does not grant damage resistance while wearing it!"
    * *Wizard + Heavy Armor:* "In D&D 5e, wearing armor you aren't proficient with prevents you from casting any spells!"
    * *Wizard + Arcane Staff:* "Your quarterstaff doubles as an Arcane Focus, channeling your magical energy."

### D. Mini Character Sheet Header
* Displays clean visual badge chips at the top right:
  * `[ Species: Tiefling ]`
  * `[ Class: Barbarian ]`
  * `[ Armor: Heavy (AC 16) ]`
  * `[ Weapon: Greataxe (1d12 Slashing) ]`
* Keeps cognitive load low by hiding complex math while showing concrete identity.

---

## 4. Prototype Content Scope (Curated for Solo Exam)

| Category | Options | Visual & Mechanical Role |
| :--- | :--- | :--- |
| **Race / Species** | **Elf**, **Tiefling** | High visual contrast: Graceful ears vs. dramatic curved horns and tail. |
| **Class** | **Barbarian**, **Wizard** | Archetype contrast: Primal martial brawler vs. scholarly spellcaster. |
| **Armor & Attire** | **Unarmored** (Robes/Clothes)<br>**Light** (Leather)<br>**Medium** (Hide/Scale)<br>**Heavy** (Plate) | Demonstrates AC trade-offs, stealth disadvantage, and class restrictions. |
| **Weapons** | **Greataxe** (Two-Handed Heavy)<br>**Arcane Staff** (Focus)<br>**Dagger** (Light Finesse) | Shows weapon size, martial vs. caster focus, and visual flair. |

---

## 5. Technical Architecture for Unity Implementation

### Data Architecture (ScriptableObjects)
* `CharacterOptionSO`: Base ScriptableObject holding ID, Display Name, Category, Icon, SpriteLayer, and Flavor Tagline.
* `EquipmentSO : CharacterOptionSO`: Extends with Armor Type / Weapon Category, Base AC, and Proficiency tags.
* `HarmonyRuleSO`: Contains conditions (`RequiredClass`, `EquippedArmorType`, `EquippedWeapon`) and output status (`HarmonyState`, `InsightNoteText`).

### Manager Scripts
* `CharacterCustomizerManager`: Central state manager storing current choices and broadcasting events (`OnCharacterUpdated`).
* `PaperDollView`: Listens to state changes and updates SpriteRenderers on the modular character layers.
* `RuleHarmonyEvaluator`: Evaluates current build against active `HarmonyRuleSO` assets and updates the D20 controller.
* `D20HarmonyUI`: Controls the D20 magic VFX / animations and drives the slide-in/slide-out "Note:" Card.
* `ItemDragHandler`: Implements `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`, and `IPointerClickHandler` for smooth item interactions.

---

## 6. Implementation Milestones for Architect Agent

1. **Milestone 1: Project Setup & UI Mockup (Canvas & Layout)**
   * Set up URP 2D Camera, resolution scaling (1920x1080 reference).
   * Construct Canvas hierarchy: Header, Top-Left (D20 & Note Card), Top-Right (Mini Sheet), Center (Diorama), Bottom (Category Drawers).
2. **Milestone 2: Paper-Doll Rigging & Sprite Swapping**
   * Create the modular mannequin GameObject with properly sorted `SpriteRenderer` layers.
   * Hook up basic button clicks to swap Sprites on the paper-doll.
3. **Milestone 3: Drag-and-Drop Equipment Interaction**
   * Implement UI drag-and-drop from drawers onto the character viewport.
   * Add snappy feedback (pickup scale, drop snap, reset on cancel).
4. **Milestone 4: Rule Harmony Engine & Note Card Animation**
   * Build rule evaluation logic for Barbarian and Wizard permutations.
   * Implement D20 visual states (glowing gold vs. curiosity pulse).
   * Animate the "Note:" card slide-out when conflicts/insights occur.
5. **Milestone 5: Playable Greybox Verification & Export Summary**
   * Keep visuals greybox (colored boxes, simple labels, basic shapes).
   * Verify all permutations play cleanly in Unity Editor.
   * Add a simple "Export Summary" popup showing the final build choices.
   * (Visual D&D Beyond styling & art polish can be added in a future phase once mechanics are proven).
