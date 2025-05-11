using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

public class FavoritesWindow : EditorWindow
{
    private readonly List<Object> _favorites = new();
    
    private string searchFilter = "";
    
    private Vector2 scrollPosition;
    
    private static FavoritesWindow window;
    
    private FavoritesTreeView treeView;
    private TreeViewState treeViewState;
    
    [MenuItem("Tools/Favorites")]
    public static void ShowWindow()
    {
        window = GetWindow<FavoritesWindow>("Favorites");
        window.Show();
    }

    private void OnEnable()
    {
        treeViewState ??= new TreeViewState();
        EditorApplication.delayCall += () =>
        {
            LoadFavorites();
            treeView = new FavoritesTreeView(treeViewState, this);
            treeView.Reload();
            Repaint();
        };
        
        wantsMouseMove = true;
        wantsMouseEnterLeaveWindow = true;
    }

    private void OnGUI()
    {
        if (treeView == null) return;
        
        var dropAreaRect = new Rect(0, EditorGUIUtility.singleLineHeight * 2, position.width, position.height - EditorGUIUtility.singleLineHeight * 2);
        var currentEvent = Event.current;
        
        switch (currentEvent.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropAreaRect.Contains(currentEvent.mousePosition))
                    break;
                    
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    
                    foreach (var draggedObject in DragAndDrop.objectReferences)
                    {
                        if (!_favorites.Contains(draggedObject))
                            _favorites.Add(draggedObject);
                    }
                    
                    SaveFavorites();
                    
                    treeView.RememberExpandedGroups();
                    treeView.Reload();
                    Repaint();
                }
                Event.current.Use();
                break;
        }
        
        DrawToolbar();
        
        var treeViewRect = new Rect(0, EditorGUIUtility.singleLineHeight * 2, position.width, position.height - EditorGUIUtility.singleLineHeight * 2);
        treeView.OnGUI(treeViewRect);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        var newSearch = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
        if (newSearch != searchFilter)
        {
            searchFilter = newSearch;
            treeView.searchString = searchFilter;
            
            treeView.RememberExpandedGroups();
            treeView.Reload();
        }
        
        if (GUILayout.Button("Clear All", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Clear Favorites", "Are you sure you want to clear all favorites?", "Yes", "No"))
            {
                _favorites.Clear();
                SaveFavorites();
                
                treeView.RememberExpandedGroups();
                treeView.Reload();
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }

    public void RemoveFromFavorites(Object obj)
    {
        _favorites.Remove(obj);
        SaveFavorites();
        
        treeView.RememberExpandedGroups();
        treeView.Reload();
    }
    
    public List<Object> GetFavorites()
    {
        return _favorites;
    }
    
    private void SaveFavorites()
    {
        var favoritesData = string.Join(",", _favorites.Select(obj => obj.GetInstanceID().ToString()).ToArray());
        EditorPrefs.SetString("FavoritesWindowData", favoritesData);

        EditorApplication.RepaintProjectWindow();
    }
    
    private void LoadFavorites()
    {
        _favorites.Clear();
        var favoritesData = EditorPrefs.GetString("FavoritesWindowData", "");
        
        if (!string.IsNullOrEmpty(favoritesData))
        {
            var ids = favoritesData.Split(',');
            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                if (int.TryParse(id, out var instanceId))
                {
                    var obj = EditorUtility.InstanceIDToObject(instanceId);
                    if (obj)
                    {
                        _favorites.Add(obj);
                    }
                }
            }
        }

        EditorApplication.RepaintProjectWindow();
    }
}