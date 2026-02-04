# 🎨 Guide Détaillé - Création de l'UI Student Filter

## 📐 Vue d'ensemble de la Structure

```
Canvas (Screen Space - Overlay)
│
├─ [1] StudentFilterButton          ← Bouton flottant pour ouvrir le menu
│
└─ [2] StudentFilterPanel           ← Panel principal (caché au départ)
    │
    ├─ [3] HeaderPanel
    │   ├─ TitleText
    │   └─ CloseButton
    │
    ├─ [4] FiltersContainer
    │   ├─ YearFilter
    │   │   ├─ YearLabel
    │   │   └─ YearDropdown
    │   ├─ SpecializationFilter
    │   │   ├─ SpecializationLabel
    │   │   └─ SpecializationDropdown
    │   ├─ RoomFilter
    │   │   ├─ RoomLabel
    │   │   └─ RoomDropdown
    │   ├─ HourFilter
    │   │   ├─ HourLabel
    │   │   └─ HourDropdown
    │   ├─ TransportFilter
    │   │   ├─ TransportLabel
    │   │   └─ TransportDropdown
    │   └─ ObjectFilter
    │       ├─ ObjectLabel
    │       └─ ObjectSearchInput
    │
    ├─ [5] ButtonsContainer
    │   ├─ ApplyFiltersButton
    │   └─ ClearFiltersButton
    │
    └─ [6] ResultsContainer
        ├─ ResultsCountText
        └─ ResultsScrollView
            └─ Viewport
                └─ Content
```

---

## 🚀 ÉTAPE 1 : Créer le Canvas Principal

### 1.1 Créer le Canvas
1. **Hierarchy** → Clic droit → **UI** → **Canvas**
2. Renommer en `StudentFilterCanvas`

### 1.2 Configurer le Canvas
Dans l'**Inspector** du Canvas:
```
Canvas:
├─ Render Mode: Screen Space - Overlay
├─ Pixel Perfect: ✓ (coché)
└─ Sort Order: 10 (pour être au-dessus des autres UI)

Canvas Scaler:
├─ UI Scale Mode: Scale With Screen Size
├─ Reference Resolution: 1920 x 1080
├─ Screen Match Mode: Match Width Or Height
└─ Match: 0.5
```

### 1.3 Ajouter l'EventSystem
- Devrait être créé automatiquement
- Si absent: **Clic droit** → **UI** → **Event System**

---

## 🔘 ÉTAPE 2 : Créer le Bouton d'Ouverture

### 2.1 Créer le bouton
1. **Clic droit sur Canvas** → **UI** → **Button - TextMeshPro**
2. Renommer en `StudentFilterButton`

### 2.2 Position et taille
**RectTransform:**
```
Anchors: Top-Right
  Anchor Min: (1, 1)
  Anchor Max: (1, 1)
  Pivot: (1, 1)

Position:
  Pos X: -20
  Pos Y: -20
  
Size:
  Width: 200
  Height: 60
```

### 2.3 Apparence
**Image (component):**
```
Color: Bleu clair (R: 100, G: 150, B: 255, A: 255)
```

**Text (TMP)** (enfant du bouton):
```
Text: "🔍 Student Filter"
Font Size: 24
Alignment: Center + Middle
Color: Blanc
Font Style: Bold
```

### 2.4 Configuration
- **Pour l'instant, ne rien connecter** - on le fera à la fin

---

## 📋 ÉTAPE 3 : Créer le Panel Principal

### 3.1 Créer le panel
1. **Clic droit sur Canvas** → **UI** → **Panel**
2. Renommer en `StudentFilterPanel`

### 3.2 RectTransform
```
Anchors: Stretch (tout)
  Anchor Min: (0, 0)
  Anchor Max: (1, 1)
  
Offsets: Tout à 0
  Left: 0, Right: 0, Top: 0, Bottom: 0
```

### 3.3 Image (Background)
```
Color: Noir semi-transparent (R: 0, G: 0, B: 0, A: 200)
```

### 3.4 Ajouter Layout
**Ajouter Component** → **Vertical Layout Group**
```
Padding:
  Left: 50, Right: 50, Top: 50, Bottom: 50
  
Spacing: 20

Child Alignment: Upper Center

Child Controls Size:
  Width: ✓ (coché)
  Height: ✗ (décoché)
  
Child Force Expand:
  Width: ✓
  Height: ✗
```

---

## 📌 ÉTAPE 4 : Header (Titre + Bouton Fermer)

### 4.1 Créer HeaderPanel
1. **Clic droit sur StudentFilterPanel** → **UI** → **Panel**
2. Renommer en `HeaderPanel`

**Layout Element** (ajouter ce component):
```
Preferred Height: 80
```

**Horizontal Layout Group:**
```
Padding: 10 partout
Spacing: 20
Child Alignment: Middle Left
Child Controls Size: Height ✓
Child Force Expand: Width ✓, Height ✗
```

### 4.2 Créer le titre
1. **Clic droit sur HeaderPanel** → **UI** → **Text - TextMeshPro**
2. Renommer en `TitleText`

```
Text: "Student Whereabouts Filter"
Font Size: 36
Color: Blanc
Font Style: Bold
Alignment: Left + Middle
```

**Layout Element:**
```
Flexible Width: 1
```

### 4.3 Créer le bouton Close
1. **Clic droit sur HeaderPanel** → **UI** → **Button - TextMeshPro**
2. Renommer en `CloseButton`

**RectTransform:**
```
Width: 60
Height: 60
```

**Image:**
```
Color: Rouge (R: 255, G: 80, B: 80)
```

**Text enfant:**
```
Text: "✖"
Font Size: 32
Color: Blanc
Alignment: Center
```

---

## 🎛️ ÉTAPE 5 : Container de Filtres

### 5.1 Créer le container
1. **Clic droit sur StudentFilterPanel** → **Create Empty**
2. Renommer en `FiltersContainer`

**RectTransform:**
```
Height: 400 (flexible)
```

**Vertical Layout Group:**
```
Padding: 20 partout
Spacing: 15
Child Alignment: Upper Center
Child Controls Size: Width ✓, Height ✗
Child Force Expand: Width ✓, Height ✗
```

**Image (optionnel, pour background):**
```
Color: Gris foncé (R: 40, G: 40, B: 50, A: 255)
```

**Layout Element:**
```
Preferred Height: 450
```

---

### 5.2 Créer UN filtre type (on va le dupliquer après)

Je vais détailler le premier, puis tu dupliqueras:

#### 5.2.1 YearFilter
1. **Clic droit sur FiltersContainer** → **Create Empty**
2. Renommer en `YearFilter`

**Horizontal Layout Group:**
```
Spacing: 10
Child Alignment: Middle Left
Child Controls Size: Height ✓
Child Force Expand: Width ✗, Height ✗
```

**Layout Element:**
```
Preferred Height: 50
```

#### 5.2.2 Label
1. **Clic droit sur YearFilter** → **UI** → **Text - TextMeshPro**
2. Renommer en `YearLabel`

```
Text: "Year:"
Font Size: 20
Color: Blanc
Alignment: Left + Middle
```

**Layout Element:**
```
Preferred Width: 200
```

#### 5.2.3 Dropdown
1. **Clic droit sur YearFilter** → **UI** → **Dropdown - TextMeshPro**
2. Renommer en `YearDropdown`

**RectTransform:**
```
Width: 300
Height: 50
```

**TMP_Dropdown:**
```
Options: Laisser vide (sera rempli par code)
```

**Layout Element:**
```
Preferred Width: 300
Preferred Height: 50
```

---

### 5.3 Dupliquer pour les autres filtres

**Maintenant, DUPLIQUE** `YearFilter` 4 fois (Ctrl+D):

1. **Dupliquer** → Renommer en `SpecializationFilter`
   - Label text: "Specialization:"
   - Dropdown: `SpecializationDropdown`

2. **Dupliquer** → Renommer en `RoomFilter`
   - Label text: "Room:"
   - Dropdown: `RoomDropdown`

3. **Dupliquer** → Renommer en `HourFilter`
   - Label text: "Hour:"
   - Dropdown: `HourDropdown`

4. **Dupliquer** → Renommer en `TransportFilter`
   - Label text: "Transport:"
   - Dropdown: `TransportDropdown`

---

### 5.4 Créer le filtre "Object Search" (différent)

1. **Clic droit sur FiltersContainer** → **Create Empty**
2. Renommer en `ObjectFilter`

**Horizontal Layout Group:** (même config que les autres)

#### Label:
```
Text: "Search (hair/clothing):"
Width: 200
```

#### InputField:
1. **Clic droit sur ObjectFilter** → **UI** → **Input Field - TextMeshPro**
2. Renommer en `ObjectSearchInput`

**TMP_InputField:**
```
Text Input:
  ├─ Placeholder: "e.g., red, hoodie..."
  ├─ Character Limit: 50
  └─ Content Type: Standard
```

**Layout Element:**
```
Preferred Width: 300
Preferred Height: 50
```

---

## 🔲 ÉTAPE 6 : Boutons Apply/Clear

### 6.1 Créer le container
1. **Clic droit sur StudentFilterPanel** → **Create Empty**
2. Renommer en `ButtonsContainer`

**Horizontal Layout Group:**
```
Padding: 10 partout
Spacing: 20
Child Alignment: Middle Center
Child Controls Size: Width ✗, Height ✗
Child Force Expand: Width ✗, Height ✗
```

**Layout Element:**
```
Preferred Height: 80
```

### 6.2 Bouton Apply
1. **Clic droit sur ButtonsContainer** → **UI** → **Button - TextMeshPro**
2. Renommer en `ApplyFiltersButton`

**RectTransform:**
```
Width: 200
Height: 60
```

**Image:**
```
Color: Vert (R: 80, G: 200, B: 120)
```

**Text:**
```
Text: "Apply Filters"
Font Size: 22
Font Style: Bold
Color: Blanc
```

### 6.3 Bouton Clear
1. **Dupliquer ApplyFiltersButton**
2. Renommer en `ClearFiltersButton`

**Image:**
```
Color: Orange (R: 255, G: 150, B: 50)
```

**Text:**
```
Text: "Clear All"
```

---

## 📊 ÉTAPE 7 : Zone de Résultats

### 7.1 Créer le container
1. **Clic droit sur StudentFilterPanel** → **Create Empty**
2. Renommer en `ResultsContainer`

**Vertical Layout Group:**
```
Spacing: 10
Child Alignment: Upper Center
Child Controls Size: Width ✓, Height ✗
Child Force Expand: Width ✓, Height ✗
```

**Layout Element:**
```
Flexible Height: 1
```

### 7.2 Compteur de résultats
1. **Clic droit sur ResultsContainer** → **UI** → **Text - TextMeshPro**
2. Renommer en `ResultsCountText`

```
Text: "Students found: 0"
Font Size: 24
Color: Jaune clair (R: 255, G: 255, B: 150)
Alignment: Center
```

**Layout Element:**
```
Preferred Height: 40
```

### 7.3 Scroll View
1. **Clic droit sur ResultsContainer** → **UI** → **Scroll View**
2. Renommer en `ResultsScrollView`

**Scroll Rect:**
```
Horizontal: ✗ (décoché)
Vertical: ✓ (coché)
Movement Type: Elastic
```

**Layout Element:**
```
Flexible Height: 1
```

### 7.4 Configurer le Content
Sélectionner **Content** (enfant de Viewport):

**RectTransform:**
```
Anchor: Top stretch
  Anchor Min: (0, 1)
  Anchor Max: (1, 1)
  Pivot: (0.5, 1)
```

**Vertical Layout Group:**
```
Padding: 10 partout
Spacing: 10
Child Alignment: Upper Center
Child Controls Size: Width ✓, Height ✗
Child Force Expand: Width ✓, Height ✗
```

**Content Size Fitter:**
```
Horizontal Fit: Unconstrained
Vertical Fit: Preferred Size
```

---

## 🎴 ÉTAPE 8 : Créer le Prefab StudentResultItem

### 8.1 Créer le prefab (en dehors du Canvas d'abord)
1. **Hierarchy** → **Create Empty**
2. Renommer en `StudentResultItem`

**RectTransform:**
```
Width: 800
Height: 120
```

### 8.2 Ajouter le composant Script
**Add Component** → `StudentResultItem`

### 8.3 Ajouter l'image de fond
**Add Component** → **Image**
```
Color: Blanc (sera changé par code selon spécialisation)
```

**Ajouter aussi:** **Layout Element**
```
Preferred Height: 120
```

### 8.4 Créer les textes enfants

#### 8.4.1 NameText
1. **Clic droit sur StudentResultItem** → **UI** → **Text - TextMeshPro**
2. Renommer en `NameText`

**RectTransform:**
```
Anchor: Top stretch
  Left: 10, Right: -10, Top: -10, Bottom: -10
Height: 30
```

```
Text: "Student Name"
Font Size: 22
Font Style: Bold
Color: Noir
Alignment: Left + Top
```

#### 8.4.2 InfoText
1. **Clic droit sur StudentResultItem** → **UI** → **Text - TextMeshPro**
2. Renommer en `InfoText`

**RectTransform:**
```
Anchor: Middle stretch
  Left: 10, Right: -10
Pos Y: 0
Height: 40
```

```
Text: "Year: 2024 | Spec: IHM..."
Font Size: 16
Color: Gris foncé (R: 50, G: 50, B: 50)
Alignment: Left + Middle
```

#### 8.4.3 ScheduleText
1. **Clic droit sur StudentResultItem** → **UI** → **Text - TextMeshPro**
2. Renommer en `ScheduleText`

**RectTransform:**
```
Anchor: Bottom stretch
  Left: 10, Right: -10, Bottom: 10
Height: 30
```

```
Text: "13h: Room01 | 14h: Room02..."
Font Size: 14
Color: Gris moyen
Alignment: Left + Bottom
```

### 8.5 Connecter dans le Script Component

Sélectionner `StudentResultItem` (root), dans l'**Inspector**:

**StudentResultItem (Script):**
```
Name Text: [Glisser NameText ici]
Info Text: [Glisser InfoText ici]
Schedule Text: [Glisser ScheduleText ici]
Background Image: [Glisser l'Image component du root ici]
```

### 8.6 Créer le Prefab
1. Créer un dossier: `Assets/QRCUBE/Prefabs/`
2. **Glisser** `StudentResultItem` de la Hierarchy → dans le dossier Prefabs
3. **Supprimer** l'instance de la Hierarchy (on n'en a pas besoin)

---

## 🔌 ÉTAPE 9 : Créer le GameObject Database

### 9.1 Créer l'objet
1. **Hierarchy** → **Create Empty**
2. Renommer en `StudentDatabase`

### 9.2 Ajouter le script
**Add Component** → `StudentDatabase`

**StudentDatabase (Script):**
```
Csv Resource Path: "students"
```

⚠️ **Important:** Assure-toi que `students.csv` est dans `Assets/Resources/`

---

## 🔗 ÉTAPE 10 : Connecter Tout Ensemble !

### 10.1 Ajouter le script StudentFilterUI

Sélectionner `StudentFilterPanel` dans la Hierarchy.

**Add Component** → `StudentFilterUI`

### 10.2 Remplir TOUTES les références

Voici **EXACTEMENT** ce que tu dois glisser-déposer:

```
┌─ StudentFilterUI (Script) ──────────────────────────────────┐
│                                                              │
│ UI Root:                                                     │
│  └─ Filter Panel: [StudentFilterPanel] ← L'objet lui-même   │
│  └─ Open Filter Button: [StudentFilterButton] ← Du Canvas   │
│  └─ Close Button: [CloseButton] ← De HeaderPanel            │
│                                                              │
│ Filter Controls:                                             │
│  └─ Year Dropdown: [YearDropdown]                           │
│  └─ Specialization Dropdown: [SpecializationDropdown]       │
│  └─ Room Dropdown: [RoomDropdown]                           │
│  └─ Hour Dropdown: [HourDropdown]                           │
│  └─ Transport Dropdown: [TransportDropdown]                 │
│  └─ Object Search Input: [ObjectSearchInput]                │
│  └─ Apply Filters Button: [ApplyFiltersButton]              │
│  └─ Clear Filters Button: [ClearFiltersButton]              │
│                                                              │
│ Results:                                                     │
│  └─ Results Scroll View: [ResultsScrollView]                │
│  └─ Results Content: [Content] ← Enfant de Viewport         │
│  └─ Student Item Prefab: [StudentResultItem] ← Du dossier   │
│  └─ Results Count Text: [ResultsCountText]                  │
│                                                              │
│ Database:                                                    │
│  └─ Student Database: [StudentDatabase] ← L'objet créé      │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### 10.3 Vérification finale

✅ Checklist:
- [ ] Toutes les références sont assignées (aucune n'est "None")
- [ ] StudentFilterPanel est **désactivé** au départ (décocher en haut de l'Inspector)
- [ ] StudentResultItem prefab existe dans le dossier Prefabs
- [ ] StudentDatabase existe dans la scène
- [ ] students.csv existe dans Assets/Resources/

---

## 🎮 ÉTAPE 11 : Test Final

### 11.1 Préparer le test
1. Sauvegarder la scène
2. Désactiver `StudentFilterPanel` (décocher en haut de l'Inspector)
3. Mode Play

### 11.2 Tester
1. **Clic sur le bouton "Student Filter"** → Le panel doit s'ouvrir
2. **Les dropdowns doivent être remplis** automatiquement avec les données
3. **Sélectionner des filtres** → Clic "Apply Filters"
4. **Des résultats doivent apparaître** dans la scroll view
5. **Clic "Clear All"** → Affiche tous les étudiants
6. **Clic sur X** → Le panel se ferme

---

## 🐛 Résolution de Problèmes

### "Les dropdowns sont vides"
➡️ Vérifier que:
- `students.csv` est dans `Assets/Resources/`
- `StudentDatabase` est dans la scène
- La Console ne montre pas d'erreur de chargement CSV

### "Aucun résultat n'apparaît"
➡️ Vérifier que:
- `Student Item Prefab` est bien assigné
- `Results Content` est le bon objet (enfant de Viewport)
- Le prefab a bien le script `StudentResultItem`

### "Le panel ne s'ouvre pas"
➡️ Vérifier que:
- `Open Filter Button` est bien connecté
- `Filter Panel` est assigné
- Pas d'erreur dans la Console

### "NullReferenceException"
➡️ Une référence n'est pas assignée dans l'Inspector
- Vérifie **chaque champ** du script StudentFilterUI
- Aucun ne doit être "None"

---

## 📸 Aperçu Visuel Final

```
┌──────────────────────────────────────────────────┐
│                         [🔍 Student Filter]  ← Bouton│
├──────────────────────────────────────────────────┤
│ ┌──────────────────────────────────────────────┐ │
│ │ Student Whereabouts Filter            [✖]   │ │ ← Header
│ ├──────────────────────────────────────────────┤ │
│ │ ┌──────────────────────────────────────────┐ │ │
│ │ │ Year:           [Dropdown ▼]             │ │ │
│ │ │ Specialization: [Dropdown ▼]             │ │ │
│ │ │ Room:           [Dropdown ▼]             │ │ │ ← Filtres
│ │ │ Hour:           [Dropdown ▼]             │ │ │
│ │ │ Transport:      [Dropdown ▼]             │ │ │
│ │ │ Search:         [___________]            │ │ │
│ │ └──────────────────────────────────────────┘ │ │
│ │     [Apply Filters]  [Clear All]            │ │ ← Boutons
│ │                                              │ │
│ │ Students found: 15                           │ │ ← Compteur
│ │ ┌──────────────────────────────────────────┐ │ │
│ │ │ ┌──────────────────────────────────────┐ │ │ │
│ │ │ │ Flossie Wittig                       │ │ │ │
│ │ │ │ Year: 2021 | Spec: MAM | ...         │ │ │ │ ← Résultats
│ │ │ │ 13h: Room25 | 14h: Room25 | ...      │ │ │ │
│ │ │ └──────────────────────────────────────┘ │ │ │
│ │ │ ┌──────────────────────────────────────┐ │ │ │
│ │ │ │ Brittani Caufield                    │ │ │ │
│ │ │ │ ...                                  │ │ │ │
│ │ └──────────────────────────────────────────┘ │ │
│ └──────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────┘
```

---

## ✅ C'est terminé !

Tu as maintenant un système complet de filtrage d'étudiants avec:
- ✨ Interface utilisateur complète
- 🎯 Filtres multiples
- 📊 Affichage des résultats
- 🎨 Design propre et organisé

Bon courage pour l'implémentation ! 🚀
