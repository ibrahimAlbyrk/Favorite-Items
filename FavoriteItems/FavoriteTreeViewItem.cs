using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

public sealed class FavoriteTreeViewItem : TreeViewItem
{
    public readonly Object favoriteObject;
    
    public FavoriteTreeViewItem(int id, int depth, string displayName, Object obj) : base(id, depth, displayName)
    {
        favoriteObject = obj;
        icon = AssetPreview.GetMiniThumbnail(obj);
    }
}