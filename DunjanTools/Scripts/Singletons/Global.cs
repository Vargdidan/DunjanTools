using Godot;
using System;

public class Global : Node
{
    public Node CurrentScene { get; set; }
    [Signal]
    delegate void ChangedMap();

    public override void _Ready()
    {
        OS.WindowMaximized = true;

        //Do directory stuff
        ClientVariables clientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        Directory ImportantFolders = new Directory();

        if (ImportantFolders.Open(clientVariables.TokenFolder) != Godot.Error.Ok)
        {
            ImportantFolders.MakeDir(clientVariables.TokenFolder);
        }

        if (ImportantFolders.Open(clientVariables.MapFolder) != Godot.Error.Ok)
        {
            ImportantFolders.MakeDir(clientVariables.MapFolder);
        }

        if (ImportantFolders.Open(clientVariables.DataFolder) != Godot.Error.Ok)
        {
            ImportantFolders.MakeDir(clientVariables.DataFolder);
        }

        Viewport root = GetTree().Root;
        CurrentScene = root.GetChild(root.GetChildCount() - 1);
    }

    public void GotoScene(string path)
    {
        // This function will usually be called from a signal callback,
        // or some other function from the current scene.
        // Deleting the current scene at this point is
        // a bad idea, because it may still be executing code.
        // This will result in a crash or unexpected behavior.

        // The solution is to defer the load to a later time, when
        // we can be sure that no code from the current scene is running:

        CallDeferred(nameof(DeferredGotoScene), path);
    }

    public void DeferredGotoScene(string path)
    {
        // It is now safe to remove the current scene
        CurrentScene.Free();

        // Load a new scene.
        var nextScene = (PackedScene)GD.Load(path);

        // Instance the new scene.
        CurrentScene = nextScene.Instance();

        // Add it to the active scene, as child of root.
        GetTree().Root.AddChild(CurrentScene);

        // Optionally, to make it compatible with the SceneTree.change_scene() API.
        GetTree().CurrentScene = CurrentScene;
    }

    public void ChangeMap()
    {
        EmitSignal(nameof(ChangedMap));
    }
}
