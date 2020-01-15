



namespace H3Engine.Components.Protocols
{
    public interface IMapCallback
    {
        // Hero Operations
        void changePrimSkill(object hero, object which, object val, bool abs = false);
        void changeSecSkill(object hero, object which, int val, bool abs = false);
        void changeSpells(object hero, bool give, object spells);
	    void giveHeroNewArtifact(object h, object artType, object pos);
	    void giveHeroArtifact(object h, object a, object pos); //pos==-1 - first free slot in backpack=0; pos==-2 - default if available or backpack
        bool moveHero(object hid, object dst, bool teleporting, bool transit = false);
	    void giveHeroBonus(int bonus);
	    void setMovePoints(object points);
	    void setManaPoints(object heroId, int points);
	    void giveHero(object heroId, object playerColor);
	    void heroExchange(object heroId1, object heroId2); //when two heroes meet on adventure map
	    

        /*
        void putArtifact(const ArtifactLocation &al, const CArtifactInstance* a) = 0;
        void removeArtifact(const ArtifactLocation &al) = 0;
	    bool moveArtifact(const ArtifactLocation &al1, const ArtifactLocation &al2) = 0;
	    void synchronizeArtifactHandlerLists() = 0;
        void heroVisitCastle(const CGTownInstance* obj, const CGHeroInstance* hero)=0;
        void stopHeroVisitCastle(const CGTownInstance* obj, const CGHeroInstance* hero)=0;
        void startBattlePrimary(const CArmedInstance* army1, const CArmedInstance* army2, int3 tile, const CGHeroInstance* hero1, const CGHeroInstance* hero2, bool creatureBank = false, const CGTownInstance* town = nullptr)=0; //use hero=nullptr for no hero
        void startBattleI(const CArmedInstance* army1, const CArmedInstance* army2, int3 tile, bool creatureBank = false)=0; //if any of armies is hero, hero will be used
        void startBattleI(const CArmedInstance* army1, const CArmedInstance* army2, bool creatureBank = false)=0; //if any of armies is hero, hero will be used, visitable tile of second obj is place of battle
        */

        // Player Operations
        /*
        virtual void giveResource(PlayerColor player, Res::ERes which, int val)=0;
	virtual void giveResources(PlayerColor player, TResources resources)=0;

	virtual void giveCreatures(const CArmedInstance* objid, const CGHeroInstance* h, const CCreatureSet &creatures, bool remove) =0;
        virtual void takeCreatures(ObjectInstanceID objid, const std::vector<CStackBasicDescriptor> &creatures) =0;
	virtual bool changeStackCount(const StackLocation &sl, TQuantity count, bool absoluteValue = false) =0;
        virtual bool changeStackType(const StackLocation &sl, const CCreature* c) =0;
        virtual bool insertNewStack(const StackLocation &sl, const CCreature* c, TQuantity count = -1) =0; //count -1 => moves whole stack
	virtual bool eraseStack(const StackLocation &sl, bool forceRemoval = false) =0;
        virtual bool swapStacks(const StackLocation &sl1, const StackLocation &sl2) =0;
	virtual bool addToSlot(const StackLocation &sl, const CCreature* c, TQuantity count) =0; //makes new stack or increases count of already existing
	virtual void tryJoiningArmy(const CArmedInstance* src, const CArmedInstance* dst, bool removeObjWhenFinished, bool allowMerging) =0; //merges army from src do dst or opens a garrison window
        virtual bool moveStack(const StackLocation &src, const StackLocation &dst, TQuantity count) = 0;
        */


        // Dialog Operations
        /*
        void showBlockingDialog(BlockingDialog* iw) =0;
	    void showGarrisonDialog(ObjectInstanceID upobj, ObjectInstanceID hid, bool removableUnits) =0; //cb will be called when player closes garrison window
	    void showTeleportDialog(TeleportDialog* iw) =0;
	    void showThievesGuildWindow(PlayerColor player, ObjectInstanceID requestingObjId) =0;


        bool removeObject(const CGObjectInstance* obj)=0;
        void setBlockVis(ObjectInstanceID objid, bool bv)=0;
	    void setOwner(const CGObjectInstance* objid, PlayerColor owner)=0;
        */

        /*
        virtual void removeAfterVisit(const CGObjectInstance*object) = 0; //object will be destroyed when interaction is over. Do not call when interaction is not ongoing!

	
	    virtual void showCompInfo(ShowInInfobox* comp)=0;
	    virtual void setAmount(ObjectInstanceID objid, ui32 val)=0;
	    virtual void changeObjPos(ObjectInstanceID objid, int3 newPos, ui8 flags)=0;
	    virtual void sendAndApply(CPackForClient* pack) = 0;
	    virtual void changeFogOfWar(int3 center, ui32 radius, PlayerColor player, bool hide) = 0;
	    virtual void changeFogOfWar(std::unordered_set<int3, ShashInt3> &tiles, PlayerColor player, bool hide) = 0;
        */
    }
}
