using UnityEngine;

namespace MiniGames
{
    // Actions (body animations) a character can do. Same list as the StoryMaker actions;
    // the inspector shows the names used in the app
    public enum CharacterAction
    {
        [InspectorName("Pararse")] Stand,
        [InspectorName("Jarrito")] Jug,
        [InspectorName("Groovy")] Groovy,
        [InspectorName("Baile loco")] CrazyDance,
        [InspectorName("Twist")] Twist,
        [InspectorName("Volar")] Fly,
        [InspectorName("Andar cool")] CoolWalk,
        [InspectorName("Tener algo")] Hold,
        [InspectorName("Sí")] Yes,
        [InspectorName("No")] No,
        [InspectorName("Androide")] Android,
        [InspectorName("Caminar robot")] RobotWalk,
        [InspectorName("Correr")] Run,
        [InspectorName("Sacudirse")] Shake,
        [InspectorName("Ni idea...")] NoIdea,
        [InspectorName("Trompo")] SpinningTop,
        [InspectorName("Remolino")] Twirl,
        [InspectorName("Caminar")] Walk,
        [InspectorName("Baile inflable")] InflatableDance,
        [InspectorName("¡¡Iupi!!")] Hooray,
        [InspectorName("¡Hola!")] Hello,
    }

    public static class CharacterActions
    {
        // Animator state of each action (the name of its clip)
        static readonly string[] animNames = {
            "idle", "idle_2", "idle_3", "dance_1", "dance_2", "fly", "groovyWalk_right", "hold",
            "yes", "no", "robotIdle", "robotWalk_right", "run_right", "shake", "soWhat", "spin",
            "twirl", "walk_right", "wave", "hurra", "waveHand",
        };

        public static string GetAnimName(CharacterAction action)
        {
            int i = (int)action;
            return i >= 0 && i < animNames.Length ? animNames[i] : animNames[0];
        }
    }
}
