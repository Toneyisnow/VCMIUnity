using UnityEngine;
using H3Engine.MapObjects;

namespace UnityClient.GUI.Mapping
{
    /// <summary>
    /// Main controller for the adventure map UI.
    ///
    /// Corresponds to VCMI's AdventureMapInterface — it bridges the game state
    /// machine in GameMapScene with the visual overlay managed by MapWidget.
    ///
    /// Current scope (first implementation):
    ///   • Hero selection → show hero info in the info bar
    ///   • Hero deselection → revert info bar to empty state
    ///
    /// Future scope (not yet implemented):
    ///   • Minimap interaction
    ///   • Buttons panel (kingdom overview, spell cast, end turn …)
    ///   • Hero / town list scrolling
    ///   • Status bar text
    ///   • Resource / date bar
    ///   • Map scrolling via screen edges
    ///   • World-view mode
    /// </summary>
    public class MapInterface
    {
        // -----------------------------------------------------------------------
        // State — mirrors VCMI's EAdventureState enum (subset)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Simplified adventure-map state, mirroring VCMI's EAdventureState.
        /// GameMapScene owns the authoritative GameMapState; this enum tracks
        /// the subset that affects the UI overlay.
        /// </summary>
        public enum EAdventureState
        {
            MakingTurn,     // Player's turn — UI fully active
            AiPlayerTurn,   // AI turn — UI mostly disabled
            CastingSpell,   // Spell targeting active
        }

        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------

        private readonly MapWidget widget;
        private EAdventureState    currentState = EAdventureState.MakingTurn;
        private HeroInstance       selectedHero;

        // -----------------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------------

        public MapInterface(MapWidget widget)
        {
            this.widget = widget;
        }

        // -----------------------------------------------------------------------
        // Selection callbacks (called by GameMapScene)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Notify the interface that a hero has been selected.
        /// Updates the info bar to display the hero's details.
        /// Corresponds to VCMI onSelectionChanged() → CInfoBar::showHeroSelection().
        /// </summary>
        public void OnHeroSelected(HeroInstance hero)
        {
            if (hero == null) return;

            selectedHero = hero;
            widget?.ShowHeroInfo(hero);

            Debug.Log(string.Format("[MapInterface] Hero selected: {0} (id={1})",
                hero.Data?.Name ?? "?", hero.Identifier));
        }

        /// <summary>
        /// Notify the interface that the current hero has been deselected.
        /// Reverts the info bar to its empty state.
        /// Corresponds to VCMI onSelectionChanged() with no active hero.
        /// </summary>
        public void OnHeroDeselected()
        {
            selectedHero = null;
            widget?.ShowEmptyInfo();

            Debug.Log("[MapInterface] Hero deselected.");
        }

        // -----------------------------------------------------------------------
        // State management
        // -----------------------------------------------------------------------

        /// <summary>
        /// Set the current adventure state (e.g. player turn vs AI turn).
        /// Widgets can be enabled/disabled based on this state.
        /// Corresponds to VCMI AdventureMapWidget::updateActiveState().
        /// </summary>
        public void SetState(EAdventureState state)
        {
            currentState = state;
            // TODO: enable/disable buttons, lists, etc. based on state
        }

        /// <summary>The currently selected hero, or null.</summary>
        public HeroInstance SelectedHero => selectedHero;

        /// <summary>Current adventure state.</summary>
        public EAdventureState CurrentState => currentState;
    }
}
