using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public class FavoritesTreeView : TreeView
{
    private readonly FavoritesWindow _window;
    private List<Object> _favorites => _window.GetFavorites() ?? new List<Object>();
    private int _rowCounter;
    
    private HashSet<int> _expandedGroupIds = new();

    public FavoritesTreeView(TreeViewState state, FavoritesWindow favWindow) : base(state)
    {
        _window = favWindow;
        Reload();
    }

    public new void OnGUI(Rect rect)
    {
        _rowCounter = 0;
        if (rootItem?.children == null)
            Reload();
        
        base.OnGUI(rect);
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root", children = new List<TreeViewItem>() };
        var groups = GroupFavoritesByType();

        var id = 1;
        foreach (var kvp in groups)
        {
            var groupItem = new TreeViewItem(id++, 0, kvp.Key);
            root.AddChild(groupItem);

            foreach (var obj in kvp.Value)
            {
                var item = new FavoriteTreeViewItem(id++, 1, obj.name, obj);
                groupItem.AddChild(item);
            }

            if (_expandedGroupIds.Contains(groupItem.id))
            {
                SetExpanded(groupItem.id, true);
            }
        }

        SetupDepthsFromParentsAndChildren(root);
        return root;
    }
    
    protected override bool CanMultiSelect(TreeViewItem item) => false;

    protected override void RowGUI(RowGUIArgs args)
    {
        var rowRect = args.rowRect;

        Color bgColor;
        if (EditorGUIUtility.isProSkin)
        {
            bgColor = (_rowCounter % 2 == 0)
                ? new Color(0.2f, 0.2f, 0.2f)
                : new Color(0.18f, 0.18f, 0.18f);
        }
        else
        {
            bgColor = (_rowCounter % 2 == 0)
                ? new Color(0.90f, 0.90f, 0.90f, 1f)
                : new Color(0.82f, 0.82f, 0.82f, 1f);
        }
        EditorGUI.DrawRect(rowRect, bgColor);
        _rowCounter++;

        var isGroup = args.item.depth == 0;
        var isEntry = args.item.depth == 1;
        var iconSize = rowRect.height - 4f;
        const float padding = 4f;

        if (isEntry)
        {
            var iconRect = new Rect(rowRect.x + rowRect.width - iconSize - padding, rowRect.y + 2, iconSize, iconSize);
            var removeIcon = EditorGUIUtility.IconContent("TreeEditor.Trash");
            if (GUI.Button(iconRect, removeIcon, GUIStyle.none))
            {
                if (args.item is FavoriteTreeViewItem entry)
                    _window.RemoveFromFavorites(entry.favoriteObject);
                Reload();
                _window.Repaint();
                return;
            }
            args.rowRect = new Rect(rowRect.x, rowRect.y, rowRect.width - iconSize - padding, rowRect.height);
        }
        else if (isGroup)
        {
            var iconRect = new Rect(rowRect.x + rowRect.width - iconSize - padding, rowRect.y + 2, iconSize, iconSize);
            var clearIcon = EditorGUIUtility.IconContent("TreeEditor.Trash");
            if (GUI.Button(iconRect, clearIcon, GUIStyle.none))
            {
                var groupName = args.item.displayName;
                var toRemove = _favorites
                    .Where(obj => AssetTypeName(Path.GetExtension(AssetDatabase.GetAssetPath(obj)).ToLowerInvariant()) == groupName)
                    .ToList();
                foreach (var obj in toRemove)
                    _window.RemoveFromFavorites(obj);
                Reload();
                _window.Repaint();
                return;
            }
            args.rowRect = new Rect(rowRect.x, rowRect.y, rowRect.width - iconSize - padding, rowRect.height);
        }

        base.RowGUI(args);
    }

    protected override void SingleClickedItem(int id)
    {
        if (FindItem(id, rootItem) is FavoriteTreeViewItem { depth: 1 } treeItem)
        {
            Selection.activeObject = treeItem.favoriteObject;
            EditorGUIUtility.PingObject(treeItem.favoriteObject);
        }
        base.SingleClickedItem(id);
    }

    protected override void DoubleClickedItem(int id)
    {
        if (FindItem(id, rootItem) is FavoriteTreeViewItem { depth: 1 } treeItem)
        {
            var obj = treeItem.favoriteObject;
            var path = AssetDatabase.GetAssetPath(obj);
            if (path.EndsWith(".unity"))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(path);
            }
            else
            {
                AssetDatabase.OpenAsset(obj);
            }
        }
    }

    protected override bool CanStartDrag(CanStartDragArgs args)
    {
        return args.draggedItem is FavoriteTreeViewItem;
    }

    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
        var dragged = args.draggedItemIDs
            .Select(i => FindItem(i, rootItem) as FavoriteTreeViewItem)
            .Where(x => x != null)
            .Select(x => x.favoriteObject)
            .ToArray();
        
        if (dragged.Length == 0)
            return;
        
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.objectReferences = dragged;
        DragAndDrop.StartDrag("Dragging Favorite Items");
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    }
    
    public void RememberExpandedGroups()
    {
        _expandedGroupIds.Clear();
        foreach (var item in GetExpanded())
        {
            if (FindItem(item, rootItem) is { depth: 0 } treeItem)
            {
                _expandedGroupIds.Add(treeItem.id);
            }
        }
    }
    
    private Dictionary<string, List<Object>> GroupFavoritesByType()
    {
        return _favorites
            .GroupBy(obj => AssetTypeName(Path.GetExtension(AssetDatabase.GetAssetPath(obj)).ToLowerInvariant()))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static string AssetTypeName(string extension)
    {
        switch (extension)
        {
            case ".unity": return "Scenes";
            case ".prefab": return "Prefabs";
            case ".shader":
            case ".cginc": return "Shaders";
            case ".mat": return "Materials";
            case ".fbx":
            case ".obj": return "Models";
            case ".psd":
            case ".png":
            case ".jpg": return "Textures";
            case ".cs": return "Scripts";
            default: return "Others";
        }
    }
}