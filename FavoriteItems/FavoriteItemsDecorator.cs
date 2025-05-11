using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public static class FavoriteItemsDecorator
{
    private static readonly List<int> _favoriteIDs = new();
    private static readonly Texture2D _starIcon;
    
    static FavoriteItemsDecorator()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        EditorApplication.update += OnEditorUpdate;
        
        _starIcon = EditorGUIUtility.FindTexture("d_Favorite Icon");
        
        LoadFavoriteIDs();
    }
    
    private static void OnEditorUpdate()
    {
        LoadFavoriteIDs();
    }
    
    private static void LoadFavoriteIDs()
    {
        var favoritesData = EditorPrefs.GetString("FavoritesWindowData", "");
        _favoriteIDs.Clear();
        
        if (!string.IsNullOrEmpty(favoritesData))
        {
            var ids = favoritesData.Split(',');
            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                if (int.TryParse(id, out var instanceId))
                {
                    _favoriteIDs.Add(instanceId);
                }
            }
        }
    }
    
    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(assetPath))
            return;
            
        var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (!obj)
            return;
            
        if (_favoriteIDs.Contains(obj.GetInstanceID()))
        {
            var starRect = new Rect(selectionRect)
            {
                x = selectionRect.x + selectionRect.width - 20,
                width = 16,
                height = 16
            };
            starRect.y += (selectionRect.height - 16) / 2;
            
            if (_starIcon)
                GUI.DrawTexture(starRect, _starIcon);
            else
                GUI.Label(starRect, "★", EditorStyles.boldLabel);
        }
    }
}