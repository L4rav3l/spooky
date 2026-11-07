using System.Collections.Generic;

namespace spooky;

public class SceneManager
{
    private Dictionary<string, IScene> scenes;
    private IScene CurrentScene;

    public SceneManager()
    {
        scene = new();
    }

    public AddScene(string name, IScene scene)
    {
        scene.LoadContent();

        scenes[name] = scene;
    }

    public RemoveScene(string name)
    {
        scenes[name] = null;
    }

    public void ChangeScene(string name)
    {
        CurrentScene = scenes[name];
    }

    public IScene GetCurrentScene()
    {
        return CurrentScene;
    }
}