
# Unity Favorites Panel 🎯⭐

A clean, user-friendly, and fully customizable **Favorites Panel** extension for the Unity Editor. Designed to help you quickly access, organize, and manage your most-used assets directly within the editor.

## 🚀 Features

- 🎨 **UI styled to match Unity's Project window**
- 🖱️ **Drag & Drop** support for adding favorites
- 🗂️ **Automatic grouping by asset type** (Scenes, Prefabs, Scripts, Textures, etc.)
- 🧹 **Remove entire groups** with a single click
- ❌ **Per-item remove button** with native icon
- 🖼️ **Alternating row backgrounds** for better visual clarity
- 🔁 **Single click selects the asset**, double click opens it (e.g. opens a scene)
- 🔌 **Modular and extensible** architecture

## 🧰 Installation

1. Clone or download the repository into your Unity project under `Assets/{Your Scripts Folder Path}/Editor/`:
   ```bash
   git clone https://github.com/ibrahimAlbyrk/Favorite-Items.git

2. Open your Unity Editor.
3. Go to **Tools > Favorites** to open the panel.

## 📂 Folder Structure

```
Scripts/
    └── Editor/
        ├── FavoritesWindow.cs
        ├── FavoritesTreeView.cs
        ├── FavoriteTreeViewItem.cs
        └── FavoriteItemsDecorator.cs
```

## ✨ How to Use

* Drag and drop any asset (scene, prefab, texture, etc.) into the Favorites window.
* Remove individual items using the trash icon on the right.
* Remove entire groups via the group header's trash icon.
* Single-click an item to select it in the Project window.
* Double-click an item to open it directly (e.g. a `.unity` scene file).

## 🔧 Developer Notes

* Built on Unity’s **TreeView API** for structured rendering and interaction.
* Theme-aware: adapts to both Light and Dark Editor skins.
* Visual row separation handled via `EditorGUI.DrawRect` with alternating tones.
* Removal actions trigger `Reload()` and `Repaint()` automatically.

## 🤝 Contributing

Feel free to open an issue or submit a pull request if you have ideas, suggestions, or improvements!
