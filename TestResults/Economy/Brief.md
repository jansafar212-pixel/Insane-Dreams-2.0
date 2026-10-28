# AKUMU 2 — SPRINT 0034
## Economy, Shop & Progression Overhaul

**Project:** AKUMU 2 (Unity 2D URP, Android + PC)

**Priority:** High

**Objective:** Turn the existing wallet pickups, weapons, ammunition, armor, medical supplies and consumables into a coherent, balanced purchasing and progression system.

### 1. Preserve existing gameplay and performance

The recent Android optimization sprint successfully resolved major smoothness problems on the actual phone.

**This is non-negotiable: DO NOT REGRESS PERFORMANCE.**

- Preserve the optimized rendering, camera, physics and enemy systems.
- Avoid unnecessary per-frame calculations, allocations and UI rebuilding.
- Keep Android performance targeting stable 60 FPS.
- Do not redesign weapons or change their established handling.
- Do not rewrite functioning systems unnecessarily.
- Keep the game fully playable with mobile controls.

### 2. Proper currency and wallet system

Improve the existing wallet pickup implementation.

- Wallets are collected automatically within the established pickup radius.
- Display brief, readable currency feedback.
- Maintain the total currency collected during the current level.
- Show earnings in the end-of-level results screen.
- Maintain a persistent player currency balance between missions and shop visits.
- Prevent duplicate wallet pickups and unintended duplicate payouts.
- Ensure currency is saved and loaded correctly.
- Never allow negative currency balances.

Add a configurable mission-completion reward alongside wallet earnings.

On failure, retain any previously banked money. Apply a modest, configurable consolation payout for collected wallets if appropriate, without making intentionally failing missions the best farming strategy.

### 3. Weapon purchasing and ownership

Introduce or improve actual weapon ownership and purchasing through the existing shop.

- Weapons must have configurable prices.
- Purchased weapons remain owned between missions.
- Previously owned weapons cannot be purchased again.
- Only owned weapons can be equipped.
- The player must always have access to a usable starter weapon.
- More powerful or specialized weapons should generally require greater investment.
- Keep every weapon's previously established strengths and weaknesses.

Suggested economic tiers:

| Tier | Weapon category | Economic role |
|---|---|---|
| Starter | Basic pistol | Always accessible |
| Affordable | Common SMGs and basic firearms | Early progression |
| Mid-tier | Carbines, assault rifles, shotguns | Regular upgrades |
| Premium | Desert Eagle, sniper rifle, specialized weapons | Significant purchases |

Only use weapons actually implemented in the project. Do not invent new weapon assets or mechanics.

### 4. Ammunition economy

Make ammunition a meaningful but fair expense.

- Each weapon uses its existing ammunition type and capacity.
- Spare ammunition can be purchased in the shop.
- Different ammunition types have different configurable prices.
- Common ammunition should be reasonably affordable.
- Powerful or specialized ammunition should be significantly more expensive.
- Purchases must obey reserve-ammunition capacity.
- Prevent purchasing ammunition that cannot fit into reserves.
- Keep the current/capacity ammo HUD working correctly.
- Preserve existing in-level ammo pickup behavior.

**Critical rule:** The player must never become economically softlocked because they cannot afford ammunition for a usable basic weapon.

Provide a modest guaranteed basic ammunition allowance at mission start if necessary. This should not grant unlimited premium ammunition or undermine the economy.

### 5. Armor and helmet purchases

Integrate the existing light/heavy body armor and helmet systems into the shop.

Preserve the established balancing philosophy:

- Armor must never reduce player movement speed.
- Heavy protection should have meaningful but reasonable trade-offs.
- Balance via spare ammunition capacity, grenade capacity and weapon reload duration.
- Maintain a sensible minimum reserve ammunition floor.
- Helmets have smaller penalties than equivalent body armor.
- Do not stack penalties to absurd levels.
- Preserve established protection rules, including melee attacks bypassing ballistic body armor.

Armor purchases and equipment selection must save correctly.

Do not recreate armor mechanics already working in the project.

### 6. Medical shop category

Fix and finalize the existing **Medical** category.

It must contain:

**Duct Tape**
- Preserve its existing hold-to-heal functionality.
- Preserve the rule that the player cannot fire while healing.
- Display its price and available quantity accurately.

**Faintanyl**
- Shop-only fictional consumable.
- Must appear exclusively under Medical.
- Preserve its existing gameplay effects.
- Preserve its existing satirical description: "relax its not addictive, just habit forming"

Ensure both items are purchasable, correctly inventoried and usable after purchase.

### 7. Grenades and revival injections

Integrate existing consumables into the economy.

Grenades:
- Fragmentation
- Flashbang
- Smoke

Use their existing effects, inventory limits and aiming mechanics.

Revival injections:
- Purchasable only through the shop.
- Maximum two carried.
- Preserve the existing two-second invulnerability after revival.
- No free replacement injections during a level.
- Preserve the no-checkpoints design.

Prevent consumables from exceeding established inventory limits.

### 8. Economy balance

Do not assign arbitrary prices without considering actual mission earnings.

Inspect existing Level 1 wallet placement, enemy rewards if applicable, mission rewards, ammunition consumption and equipment costs.

Establish configurable prices so that:

- A successful first mission earns a meaningful purchasing opportunity.
- Players can afford basic supplies without excessive grinding.
- Premium weapons require saving money.
- Buying powerful equipment involves trade-offs.
- Consumable prices are substantial enough to matter but not punitive.
- Players who spend inefficiently can recover economically.

Keep all prices, rewards and relevant limits centralized in existing configuration/data structures where possible.

Simulate at least three player scenarios:
1. New player with little money.
2. Average player completing Level 1.
3. Experienced player saving for premium equipment.

Report the resulting affordability and progression.

### 9. Shop UI improvements

Improve the current shop rather than replacing it with a totally different system.

Maintain AKUMU 2's established art direction:

- Chunky, handmade, old-school pixel-art style.
- Imperfect but charming visual presentation.
- No polished corporate-style UI.
- Clear categories and item descriptions.
- Readable prices and currency balance.
- Obvious owned/equipped/out-of-stock indicators.
- Disabled purchase actions when insufficient funds.
- Clear confirmation feedback after purchases.
- Touch-friendly controls on Android.

The store should feel like part of AKUMU 2, not a generic mobile game marketplace.

### 10. Persistence and reliability

Audit the full purchase-to-gameplay flow.

Test:

- Buying a weapon, closing the game and reopening it.
- Purchasing ammunition and consuming it during a mission.
- Buying and equipping armor.
- Buying medical supplies and using them.
- Purchasing revival injections and consuming them.
- Returning to the shop after completing a level.
- Running out of money.
- Attempting purchases with insufficient funds.
- Reopening the game after an interrupted purchase or mission.
- Preventing inventory, wallet and currency duplication.

Use reliable local persistence. Avoid unnecessary dependencies or network requirements.

### 11. Scope restrictions

DO NOT:

- Add new enemy archetypes.
- Create Level 2.
- Overhaul the camera.
- Redesign gunplay from Sprint 0033.
- Rework the entire graphics pipeline.
- Introduce advertisements, microtransactions or real-money purchases.
- Add complicated RPG systems.
- Remove the game's intentionally fast-paced arcade identity.

This sprint concerns **the economy and purchasing loop**, not new gameplay systems.

### 12. Acceptance criteria

Sprint 0034 is complete when:

- [ ] Currency collection, rewards and saving function correctly.
- [ ] Players can purchase and equip existing weapons.
- [ ] Ammunition costs and capacity checks work.
- [ ] Armor, helmets and consumables are integrated.
- [ ] Medical contains Duct Tape and Faintanyl correctly.
- [ ] Revival injections remain capped at two.
- [ ] The player cannot become economically softlocked.
- [ ] Purchases and equipment persist across restarts.
- [ ] Shop UI works comfortably on Android.
- [ ] Level 1 can be completed, rewards collected, equipment purchased and the level replayed.
- [ ] Android smoothness and responsiveness remain intact.

### Final instructions

First inspect the existing economy, shop, inventory and persistence code. Reuse working implementations rather than introducing parallel systems.

Implement the sprint directly in the project.

After implementation, report:
1. Files modified.
2. Systems implemented or repaired.
3. Final configurable item prices and rewards.
4. Economy balance simulations.
5. Tests completed.
6. Any remaining limitations or regressions.

**Quality over scope. Keep the game fast, funny, reliable and financially balanced.**