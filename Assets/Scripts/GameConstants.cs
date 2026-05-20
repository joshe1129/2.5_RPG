namespace RPG.Core
{
    /// <summary>
    /// Centralized repository for all hardcoded strings and magic numbers.
    /// Prevents typos and makes refactoring easier across the entire project.
    /// </summary>
    public static class GameConstants
    {
        // --- Scene Names ---
        public const string SCENE_MAIN_MENU = "MainMenu";

        // --- Tags ---
        public const string TAG_NPC_JOINABLE = "NPCJoinable";
        public const string TAG_PLAYER = "Player";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_SPAWN_POINT = "SpawnPoint";

        // --- Animator Parameters ---
        public const string ANIM_PARAM_IS_ATTACKING = "isAttacking";
        public const string ANIM_PARAM_IS_DEATH = "isDeath";
        public const string ANIM_PARAM_IS_HIT = "isHit";

        // --- PlayerPrefs Keys ---
        public const string PREFS_MASTER_VOLUME = "masterVolume";
        public const string PREFS_MUSIC_VOLUME = "musicVolume";
        public const string PREFS_UI_VOLUME = "uiVolume";
        public const string PREFS_SFX_VOLUME = "sfxVolume";
        public const string PREFS_AUDIO_MUTED = "audioMuted";
    }
}
