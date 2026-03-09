# Items Catalog

## 🎣 Equipment (Rods)

### Old Rod
* **ID:** `item_rod_old`
* **Name:** Old Rod
* **Description:** A simple wooden stick with a string. Better than using your bare hands.
* **Type:** `rod`
* **Effect Target:** `catch_difficulty`
* **Effect Value:** `0`
* **Base Price:** 50

### Good Rod
* **ID:** `item_rod_good`
* **Name:** Good Rod
* **Description:** A decent fiberglass rod. Makes reeling in fish a bit smoother.
* **Type:** `rod`
* **Effect Target:** `catch_difficulty`
* **Effect Value:** `15`
* **Base Price:** 500

### Super Rod
* **ID:** `item_rod_super`
* **Name:** Super Rod
* **Description:** A high-tech carbon fiber rod. The fish practically catch themselves.
* **Type:** `rod`
* **Effect Target:** `catch_difficulty`
* **Effect Value:** `35`
* **Base Price:** 2000

### Legendary Cat-Rod
* **ID:** `item_rod_legendary`
* **Name:** Legendary Cat-Rod
* **Description:** Forged from pure gold and cat hair. The ultimate fishing tool.
* **Type:** `rod`
* **Effect Target:** `catch_difficulty`
* **Effect Value:** `60`
* **Base Price:** 10000


## 🪱 Consumables (Baits)

### Worm Bait
* **ID:** `item_bait_worm`
* **Name:** Worm Bait
* **Description:** A basic juicy worm. Slightly increases the chance of finding rare fish.
* **Type:** `bait`
* **Effect Target:** `spawn_rate_rare`
* **Effect Value:** `10`
* **Base Price:** 20

### Glow-Worm Bait
* **ID:** `item_bait_glow`
* **Name:** Glow-Worm Bait
* **Description:** A radioactive-looking worm. Greatly attracts Special rarity fish.
* **Type:** `bait`
* **Effect Target:** `spawn_rate_special`
* **Effect Value:** `25`
* **Base Price:** 60

### Magic Chum
* **ID:** `item_bait_magic`
* **Name:** Magic Chum
* **Description:** A sparkly fish food mix. Epic fish can't resist this scent.
* **Type:** `bait`
* **Effect Target:** `spawn_rate_epic`
* **Effect Value:** `40`
* **Base Price:** 150

### Star Fragment Bait
* **ID:** `item_bait_star`
* **Name:** Star Fragment Bait
* **Description:** A piece of a fallen star. Legend says it summons the rarest creatures of the sea.
* **Type:** `bait`
* **Effect Target:** `spawn_rate_legendary`
* **Effect Value:** `50`
* **Base Price:** 500


## 🗑️ Junk (Pond Trash)

### Old Boot
* **ID:** `item_junk_boot`
* **Name:** Old Boot
* **Description:** A smelly, wet boot. Who keeps throwing these in the pond?
* **Type:** `trash`
* **Effect Target:** `none`
* **Effect Value:** `0`
* **Base Price:** 1

### Tangled Seaweed
* **ID:** `item_junk_weed`
* **Name:** Tangled Seaweed
* **Description:** Just a clump of useless, slimy seaweed.
* **Type:** `trash`
* **Effect Target:** `none`
* **Effect Value:** `0`
* **Base Price:** 1

### Empty Can
* **ID:** `item_junk_can`
* **Name:** Empty Can
* **Description:** An empty can of tuna. The cats nearby look disappointed.
* **Type:** `trash`
* **Effect Target:** `none`
* **Effect Value:** `0`
* **Base Price:** 2


## 🎁 Gifts (Affinity Items)

### Yarn Ball
* **ID:** `item_gift_yarn`
* **Name:** Yarn Ball
* **Description:** A soft red yarn ball. A nice gift for any feline friend.
* **Type:** `gift`
* **Effect Target:** `affinity`
* **Effect Value:** `5`
* **Base Price:** 30

### Catnip Pouch
* **ID:** `item_gift_catnip`
* **Name:** Catnip Pouch
* **Description:** Premium quality catnip. Guaranteed to make a cat very happy (and a bit crazy).
* **Type:** `gift`
* **Effect Target:** `affinity`
* **Effect Value:** `15`
* **Base Price:** 100

### Plush Mouse
* **ID:** `item_gift_plush`
* **Name:** Plush Mouse
* **Description:** A highly realistic toy mouse. The ultimate gift to win a cat's heart.
* **Type:** `gift`
* **Effect Target:** `affinity`
* **Effect Value:** `35`
* **Base Price:** 300


## 🎟️ Utility & Gacha

### Mystery Cat Box
* **ID:** `item_ticket_box`
* **Name:** Mystery Cat Box
* **Description:** A strange cardboard box. You can hear purring inside. Open it to find a new friend!
* **Type:** `gacha`
* **Effect Target:** `summon_cat`
* **Effect Value:** `1`
* **Base Price:** 1000

### Strong Coffee
* **ID:** `item_cure_coffee`
* **Name:** Strong Coffee
* **Description:** Extremely bitter and dark. Instantly cures any dizziness or 'tavern side effects'.
* **Type:** `cure`
* **Effect Target:** `tavern_lvl`
* **Effect Value:** `-10`
* **Base Price:** 40

---

# Services Catalog

## 🍻 Tavern Buffs (Purchasable with Coins)
*Note: Consuming these might secretly increase the player's `tavern_lvl` (drunkenness).*

### Sailor's Rum
* **ID:** `srv_tavern_rum`
* **Name:** Sailor's Rum
* **Description:** A strong drink that makes you brave. Doubles the chance of finding rare fish, but makes you very dizzy.
* **Type:** `buff`
* **Effect Target:** `rare_spawn_multiplier`
* **Effect Value:** `2.0`
* **Coin Price:** 150

### Fish & Chips Meal
* **ID:** `srv_tavern_meal`
* **Name:** Fish & Chips Meal
* **Description:** A hearty meal that fills you with energy. Grants 50% more XP from all activities for 1 hour.
* **Type:** `buff`
* **Effect Target:** `xp_gain_multiplier`
* **Effect Value:** `1.5`
* **Coin Price:** 100

### Catnip Tea
* **ID:** `srv_tavern_tea`
* **Name:** Catnip Tea
* **Description:** A relaxing brew. Makes you smell incredibly good to other cats. Increases affinity gained from gifts by 50%.
* **Type:** `buff`
* **Effect Target:** `affinity_gain_multiplier`
* **Effect Value:** `1.5`
* **Coin Price:** 80


## ⛪ Church Streaks (Free / Granted by Praying)
*Note: These are blessings given by Deus. The `coin_price` is 0 because they are earned via the `church_streak` system.*

### Deus's Blessing
* **ID:** `srv_church_blessing`
* **Name:** Deus's Blessing
* **Description:** The market smiles upon the faithful. Increases the selling price of all fish by 10% today.
* **Type:** `streak`
* **Effect Target:** `sell_price_multiplier`
* **Effect Value:** `1.1`
* **Coin Price:** 0

### Holy Light
* **ID:** `srv_church_holy_light`
* **Name:** Holy Light
* **Description:** A calming light guides your hands. Decreases the speed and difficulty of the fishing minigame.
* **Type:** `streak`
* **Effect Target:** `minigame_difficulty`
* **Effect Value:** `-0.2`
* **Coin Price:** 0

### Miracle Catch
* **ID:** `srv_church_miracle`
* **Name:** Miracle Catch
* **Description:** A true miracle! Grants a 10% chance to catch two identical fish at the same time.
* **Type:** `streak`
* **Effect Target:** `double_catch_chance`
* **Effect Value:** `0.1`
* **Coin Price:** 0


## 🐾 Helper Cats (Auto-Farming & Utilities)
*Note: Players can hire these cats for a day after unlocking them via quests.*

### Apprentice Fisher
* **ID:** `srv_helper_fisher`
* **Name:** Apprentice Fisher
* **Description:** Hires a small cat to fish for you while you are offline. Generates 1 random Common or Rare fish every hour.
* **Type:** `helper`
* **Effect Target:** `auto_farm_fish_per_hour`
* **Effect Value:** `1.0`
* **Coin Price:** 500

### Scavenger Cat
* **ID:** `srv_helper_scavenger`
* **Name:** Scavenger Cat
* **Description:** Hires a sneaky cat to dive into the pond and collect junk and lost items while you rest. Great for cleaning quests.
* **Type:** `helper`
* **Effect Target:** `auto_farm_junk_per_hour`
* **Effect Value:** `2.0`
* **Coin Price:** 300

### Charismatic Promoter
* **ID:** `srv_helper_promoter`
* **Name:** Charismatic Promoter
* **Description:** A loud cat that yells about how great you are at the market. Grants a 15% discount on all shop items today.
* **Type:** `helper`
* **Effect Target:** `shop_discount_multiplier`
* **Effect Value:** `0.85`
* **Coin Price:** 800


## 🏪 Market Upgrades (Permanent or Long-term Services)

### Bag Expansion I
* **ID:** `srv_market_bag_1`
* **Name:** Bag Expansion I
* **Description:** Bao sews some extra pockets into your backpack. Increases maximum inventory slots by 10.
* **Type:** `upgrade`
* **Effect Target:** `max_inventory_bonus`
* **Effect Value:** `10.0`
* **Coin Price:** 1000

### Pier Wooden Bucket
* **ID:** `srv_market_bucket_1`
* **Name:** Pier Wooden Bucket
* **Description:** Bao sets up a sturdy wooden bucket next to the pier. Adds 20 slots of external storage for your fish.
* **Type:** `upgrade`
* **Effect Target:** `external_storage_bonus`
* **Effect Value:** `20.0`
* **Coin Price:** 1500

### Pier Ice Cooler
* **ID:** `srv_market_bucket_2`
* **Name:** Pier Ice Cooler
* **Description:** Upgrades your wooden bucket to a high-quality ice cooler. Adds another 30 slots of external storage to keep fish fresh.
* **Type:** `upgrade`
* **Effect Target:** `external_storage_bonus`
* **Effect Value:** `30.0`
* **Coin Price:** 3500
