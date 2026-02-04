# 🎓 Student Whereabouts Filter System - Setup Guide

## 📋 Overview
This system allows filtering and displaying student locations based on multiple criteria:
- **Student year** (2016-2025)
- **Specialization** (IHM, MAM, AL, WD, SAID)
- **Room proximity** (any room in the building)
- **Time of day** (13h-17h)
- **Transportation mode** (bus, bike, walking, drive)
- **Physical attributes** (hair color, clothing)

---

## 📁 Files Created

### C# Scripts (in `Assets/SCRIPTS/QRCUBE/`)
1. **StudentData.cs** - Data structure for individual students
2. **StudentDatabase.cs** - CSV loader and filtering engine
3. **StudentFilterUI.cs** - Main UI controller
4. **StudentResultItem.cs** - Individual result display component

### Data Files
- **Assets/Resources/students.csv** - Student database (you need to create this manually)

---

## 🛠️ Unity Setup Instructions

### Step 1: Create the CSV File
1. Create folder: `Assets/Resources/` (if it doesn't exist)
2. Create file: `Assets/Resources/students.csv`
3. Copy your student data into it (provided in the request)

### Step 2: Create UI Hierarchy

#### Main Canvas Structure:
```
Canvas
├─ StudentFilterButton (Button) ← Opens the filter menu
└─ StudentFilterPanel (Panel) ← Main filter UI
   ├─ Header
   │  ├─ Title (Text - TMP) "Student Filter"
   │  └─ CloseButton (Button) "X"
   │
   ├─ FiltersSection (Vertical Layout Group)
   │  ├─ YearDropdown (TMP_Dropdown)
   │  ├─ SpecializationDropdown (TMP_Dropdown)
   │  ├─ RoomDropdown (TMP_Dropdown)
   │  ├─ HourDropdown (TMP_Dropdown)
   │  ├─ TransportDropdown (TMP_Dropdown)
   │  └─ ObjectSearchInput (TMP_InputField)
   │
   ├─ ButtonsSection (Horizontal Layout Group)
   │  ├─ ApplyFiltersButton (Button) "Apply Filters"
   │  └─ ClearFiltersButton (Button) "Clear All"
   │
   └─ ResultsSection
      ├─ ResultsCountText (Text - TMP) "Students found: 0"
      └─ ResultsScrollView (Scroll View)
         └─ Content (Transform)
            └─ [StudentResultItems spawned here]
```

### Step 3: Create Student Result Item Prefab

Create a prefab: `Assets/QRCUBE/StudentResultItem.prefab`

**Structure:**
```
StudentResultItem (with StudentResultItem.cs)
├─ BackgroundImage (Image)
├─ NameText (Text - TMP)
├─ InfoText (Text - TMP)
└─ ScheduleText (Text - TMP)
```

**Recommended Settings:**
- Add `LayoutElement` component (min height: 120)
- Add `Image` component for background
- Use different colors per specialization (handled in code)

### Step 4: Setup Database GameObject

1. Create empty GameObject: `StudentDatabase`
2. Add component: `StudentDatabase.cs`
3. Set `csvResourcePath` to `"students"` (without .csv extension)

### Step 5: Setup UI Controller

1. Select `StudentFilterPanel` GameObject
2. Add component: `StudentFilterUI.cs`
3. Assign all references in the Inspector:
   - **Filter Panel**: The main panel GameObject
   - **Open Filter Button**: Button to open the menu
   - **Close Button**: X button
   - **Dropdowns**: All 5 dropdown fields
   - **Object Search Input**: The input field
   - **Apply/Clear Buttons**: The filter action buttons
   - **Results Scroll View**: The scroll view container
   - **Results Content**: The Content transform inside scroll view
   - **Student Item Prefab**: Your StudentResultItem prefab
   - **Results Count Text**: The counter text
   - **Student Database**: Reference to the StudentDatabase object

---

## 🎮 Usage

### From Unity Editor:
1. Play the scene
2. Click the "Student Filter" button
3. Select filter criteria from dropdowns
4. Click "Apply Filters"
5. Results appear in the scroll view

### Filter Options:
- **Year**: Filter by graduation year (All, 2016-2025)
- **Specialization**: IHM, MAM, AL, WD, SAID
- **Room**: Any room in the building
- **Hour**: Show students at specific time (13h-17h) or All
- **Transport**: bus, bike, walking, drive
- **Object Search**: Free text search for hair/clothing

### Programmatic Access:
```csharp
// Get the UI controller
StudentFilterUI filterUI = FindObjectOfType<StudentFilterUI>();

// Open menu with specific room pre-selected
filterUI.OpenWithRoomFilter("Room507");

// Or access database directly
StudentDatabase db = FindObjectOfType<StudentDatabase>();
List<StudentData> students = db.FilterStudents(
    year: 2024,
    specialization: "IHM",
    transport: "bike"
);
```

---

## 🎨 UI Customization

### Color Coding by Specialization:
The `StudentResultItem` automatically colors backgrounds:
- **IHM**: Light blue
- **MAM**: Light orange
- **AL**: Light green
- **WD**: Light pink
- **SAID**: Light purple

### Adding More Filters:
Edit `StudentFilterUI.ApplyFilters()` and `StudentDatabase.FilterStudents()` to add new criteria.

---

## 🔗 Integration with Existing Room System

You can link this with your room selection system:

```csharp
// In RoomCube.cs or similar:
void OnRoomSelected(string roomId)
{
    StudentFilterUI filterUI = FindObjectOfType<StudentFilterUI>();
    if (filterUI != null)
    {
        filterUI.OpenWithRoomFilter(roomId);
    }
}
```

---

## 📊 CSV Format Requirements

The CSV must have these columns in order:
```
name,gender,13h,14h,15h,16h,17h,hair,height,transport,clothing,year,specialization
```

- **Columns 3-7** (13h-17h): Room IDs where student is located
- **year**: Integer (graduation year)
- **height**: Integer (cm)
- **All other fields**: Text strings

---

## 🐛 Troubleshooting

### "CSV not found" error:
- Ensure `students.csv` is in `Assets/Resources/` folder
- The Resources folder name must be exact
- No subfolder inside Resources (unless you update the path)

### Dropdowns are empty:
- Database loads in `Awake()`, UI initializes in `Start()`
- Make sure `StudentDatabase` GameObject exists in scene
- Check Console for CSV loading errors

### Results don't appear:
- Verify `studentItemPrefab` is assigned
- Check `resultsContent` transform is assigned
- Ensure prefab has `StudentResultItem` component

### Filters not working:
- "All" option should be index 0 in all dropdowns
- Hour dropdown: All=0, 13h=1, 14h=2, etc.
- Check Console for any null reference errors

---

## ✨ Features

✅ Multi-criteria filtering
✅ Real-time results display  
✅ Color-coded by specialization
✅ Room proximity detection
✅ Time-based location tracking
✅ Transportation mode filter
✅ Physical attribute search
✅ Clean, expandable architecture

---

## 📝 Notes

- The system automatically detects CSV separator (`,` or `;`)
- All filters are optional - leaving "All" shows all students
- Combining filters uses AND logic (all conditions must match)
- Room proximity can check specific hour or any time in the day
- Object search looks in both hair and clothing fields

---

## 🚀 Future Enhancements

Possible additions:
- Distance calculation to current player position
- Map visualization of student locations
- Real-time clock integration (auto-update by hour)
- Export filtered results to CSV
- Student detail popup with full schedule
- Navigation to student location in AR
