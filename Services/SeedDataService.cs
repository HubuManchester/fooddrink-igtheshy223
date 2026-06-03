using ssk.Models;

namespace ssk.Services;

public class SeedDataService
{
    private readonly DatabaseService _db;
    private readonly RecipeRepository _recipeRepo;
    private readonly IngredientRepository _ingredientRepo;
    private readonly MealPlanRepository _mealPlanRepo;
    private readonly ShoppingItemRepository _shoppingItemRepo;
    private readonly NutritionRepository _nutritionRepo;
    private readonly UserProfileRepository _userProfileRepo;

    public SeedDataService(
        DatabaseService db,
        RecipeRepository recipeRepo,
        IngredientRepository ingredientRepo,
        MealPlanRepository mealPlanRepo,
        ShoppingItemRepository shoppingItemRepo,
        NutritionRepository nutritionRepo,
        UserProfileRepository userProfileRepo)
    {
        _db = db;
        _recipeRepo = recipeRepo;
        _ingredientRepo = ingredientRepo;
        _mealPlanRepo = mealPlanRepo;
        _shoppingItemRepo = shoppingItemRepo;
        _nutritionRepo = nutritionRepo;
        _userProfileRepo = userProfileRepo;
    }

    private const int CurrentSeedVersion = 9;

    public async Task InitializeAsync()
    {
        try
        {
            await _db.Init();

            var version = await _userProfileRepo.GetSeedVersionAsync();
            if (version >= CurrentSeedVersion)
            {
                System.Diagnostics.Debug.WriteLine($"[SeedDataService] Seed version {version} is up to date, skipping.");
                return;
            }

            // Version upgrade or first fill: clear all old data first
            System.Diagnostics.Debug.WriteLine($"[SeedDataService] Seed version {version} -> {CurrentSeedVersion}, clearing old data...");
            await _db.Database.ExecuteAsync("DELETE FROM recipes");
            await _db.Database.ExecuteAsync("DELETE FROM ingredients");
            await _db.Database.ExecuteAsync("DELETE FROM meal_plans");
            await _db.Database.ExecuteAsync("DELETE FROM shopping_items");
            await _db.Database.ExecuteAsync("DELETE FROM nutrition_targets");
            await _db.Database.ExecuteAsync("DELETE FROM food_recognition_logs");

            await SeedRecipesAsync();
            await SeedIngredientsAsync();
            System.Diagnostics.Debug.WriteLine("[SeedDataService] Recipes & Ingredients done, starting meal plans...");
            await SeedMealPlansAsync();
            System.Diagnostics.Debug.WriteLine("[SeedDataService] Meal plans done.");
            await SeedShoppingItemsAsync();
            await SeedNutritionTargetAsync();

            await _userProfileRepo.SetSeedVersionAsync(CurrentSeedVersion);
            System.Diagnostics.Debug.WriteLine($"[SeedDataService] Seed data initialized successfully. Version={CurrentSeedVersion}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SeedDataService] InitializeAsync error: {ex.Message}");
        }
    }

    private async Task SeedRecipesAsync()
    {
        var recipes = new List<Recipe>
        {
            // Chinese
            new Recipe("Kung Pao Chicken", "Classic Sichuan dish, tender chicken with peanuts, spicy and savory",
                "gongbao.jpg",
                "Chinese", 15, 20, 4, "Easy",
                "[{\"Index\":0,\"Text\":\"Dice chicken breast, marinate with cooking wine and starch for 15 minutes\"},{\"Index\":1,\"Text\":\"Mix sauce: vinegar, soy sauce, sugar, salt, starch water\"},{\"Index\":2,\"Text\":\"Heat wok with cold oil, fry dried chili and Sichuan pepper\"},{\"Index\":3,\"Text\":\"Add chicken dice, stir-fry until color changes\"},{\"Index\":4,\"Text\":\"Add scallion segments and fried peanuts\"},{\"Index\":5,\"Text\":\"Pour in sauce, stir-fry quickly until evenly mixed\"}]",
                "[\"Sichuan\",\"Rice Pairing\",\"Chicken\",\"Spicy\"]", true, false),

            new Recipe("Braised Pork Belly", "Rich but not greasy classic home dish, melts in mouth",
                "hongshaorou.jpg",
                "Chinese", 10, 90, 6, "Medium",
                "[{\"Index\":0,\"Text\":\"Cut pork belly into chunks, blanch to remove blood foam\"},{\"Index\":1,\"Text\":\"Add a little oil in wok, fry rock sugar for caramel color\"},{\"Index\":2,\"Text\":\"Add pork belly, stir-fry to coat with color\"},{\"Index\":3,\"Text\":\"Add scallion segments, ginger slices, star anise, cinnamon\"},{\"Index\":4,\"Text\":\"Add cooking wine, soy sauce, boiling water to cover meat\"},{\"Index\":5,\"Text\":\"Bring to boil on high heat, then simmer on low heat for 60-90 minutes\"},{\"Index\":6,\"Text\":\"Finally reduce sauce on high heat until thick\"}]",
                "[\"Home Style\",\"Pork\",\"Rice Pairing\"]", false, false),

            new Recipe("Tomato Scrambled Egg", "Most classic home dish, sweet and sour appetizing",
                "fanqiechaodan.jpg",
                "Chinese", 5, 10, 3, "Easy",
                "[{\"Index\":0,\"Text\":\"Beat eggs with a little salt\"},{\"Index\":1,\"Text\":\"Cut tomato into chunks and set aside\"},{\"Index\":2,\"Text\":\"Heat wok with oil, scramble eggs until set, remove\"},{\"Index\":3,\"Text\":\"Add more oil in wok, stir-fry tomato until juicy\"},{\"Index\":4,\"Text\":\"Pour in eggs, stir-fry evenly, season with salt and sugar\"}]",
                "[\"Home Style\",\"Quick\",\"Egg\",\"Tomato\"]", true, false),

            new Recipe("Mapo Tofu", "Spicy and savory classic Sichuan dish, silky tofu with rich flavor",
                "mapodoufu.jpg",
                "Chinese", 10, 15, 3, "Easy",
                "[{\"Index\":0,\"Text\":\"Cut tofu into small cubes, blanch in boiling water then remove\"},{\"Index\":1,\"Text\":\"Heat wok with oil, fry doubanjiang and minced garlic until fragrant\"},{\"Index\":2,\"Text\":\"Add minced meat, stir-fry until scattered and colored\"},{\"Index\":3,\"Text\":\"Add water and bring to boil, add tofu and simmer on low for 5 minutes\"},{\"Index\":4,\"Text\":\"Thicken with starch water, sprinkle Sichuan pepper powder and scallion\"}]",
                "[\"Sichuan\",\"Tofu\",\"Spicy\"]", false, false),

            // Western
            new Recipe("Spaghetti Bolognese", "Rich meat sauce paired with pasta, classic Western staple",
                "yijiangmian.jpg",
                "Western", 10, 30, 3, "Easy",
                "[{\"Index\":0,\"Text\":\"Finely chop onion, carrot and celery\"},{\"Index\":1,\"Text\":\"Heat oil in pan, sauté chopped vegetables\"},{\"Index\":2,\"Text\":\"Add ground beef, stir-fry until scattered\"},{\"Index\":3,\"Text\":\"Pour in tomato sauce and water, add thyme and bay leaf\"},{\"Index\":4,\"Text\":\"Simmer on low heat for 20 minutes, season with salt and pepper\"},{\"Index\":5,\"Text\":\"Cook spaghetti until done, drain, top with meat sauce\"}]",
                "[\"Pasta\",\"Western\",\"Beef\"]", false, false),

            new Recipe("Caesar Salad", "Refreshing classic salad with homemade Caesar dressing",
                "kaishaoshala.jpg",
                "Western", 15, 0, 2, "Easy",
                "[{\"Index\":0,\"Text\":\"Wash romaine lettuce, tear into large pieces\"},{\"Index\":1,\"Text\":\"Make Caesar dressing: mix egg yolk, lemon juice, garlic paste, olive oil\"},{\"Index\":2,\"Text\":\"Add Parmesan cheese and anchovy paste\"},{\"Index\":3,\"Text\":\"Cut bread into cubes, toast until golden for croutons\"},{\"Index\":4,\"Text\":\"Drizzle dressing over lettuce, scatter croutons and cheese powder\"}]",
                "[\"Salad\",\"Light Meal\",\"Healthy\"]", false, false),

            new Recipe("French Onion Soup", "Rich and mellow classic French soup",
                "yangcongtang.jpg",
                "Western", 15, 45, 4, "Medium",
                "[{\"Index\":0,\"Text\":\"Slice onion, slowly caramelize on low heat for 30 minutes\"},{\"Index\":1,\"Text\":\"Add white wine and cook until reduced\"},{\"Index\":2,\"Text\":\"Pour in beef broth, add thyme and bay leaf\"},{\"Index\":3,\"Text\":\"Simmer for 15 minutes, season with salt and pepper\"},{\"Index\":4,\"Text\":\"Pour into oven-safe bowl, place bread slice and cheese on top\"},{\"Index\":5,\"Text\":\"Bake in oven until cheese melts and turns golden\"}]",
                "[\"Soup\",\"French\",\"Onion\"]", false, false),

            // Asian
            new Recipe("Japanese Miso Ramen", "Rich miso broth with chewy noodles",
                "lamian.jpg",
                "Asian", 15, 20, 2, "Medium",
                "[{\"Index\":0,\"Text\":\"Bring pork bone broth to boil, add miso and stir well\"},{\"Index\":1,\"Text\":\"Add soy sauce and mirin for seasoning\"},{\"Index\":2,\"Text\":\"Cook noodles until chewy, drain and place in bowl\"},{\"Index\":3,\"Text\":\"Top with chashu pork, soft-boiled egg, corn kernels\"},{\"Index\":4,\"Text\":\"Pour in miso broth, sprinkle scallion and nori\"}]",
                "[\"Ramen\",\"Japanese\",\"Noodles\"]", false, false),

            new Recipe("Korean Stone Pot Bibimbap", "Rich vegetables, crispy rice crust classic Korean dish",
                "banfan.jpg",
                "Asian", 20, 10, 2, "Easy",
                "[{\"Index\":0,\"Text\":\"Stir-fry various vegetables separately: carrot strips, spinach, bean sprouts, mushroom\"},{\"Index\":1,\"Text\":\"Brush sesame oil on inner wall of stone pot\"},{\"Index\":2,\"Text\":\"Put in rice, arrange vegetables and fried egg on top\"},{\"Index\":3,\"Text\":\"Heat stone pot until bottom rice forms crispy crust\"},{\"Index\":4,\"Text\":\"Add Korean chili sauce, mix well and eat\"}]",
                "[\"Bibimbap\",\"Korean\",\"Rice\"]", false, false),

            new Recipe("Japanese Teriyaki Chicken Thigh", "Sweet and savory teriyaki sauce with tender chicken thigh",
                "zhaoshaoji.jpg",
                "Asian", 10, 20, 2, "Easy",
                "[{\"Index\":0,\"Text\":\"Debone chicken thigh, make a few cuts for flavor\"},{\"Index\":1,\"Text\":\"Marinate with salt and pepper for 10 minutes\"},{\"Index\":2,\"Text\":\"Mix teriyaki sauce: soy sauce, mirin, sake, sugar\"},{\"Index\":3,\"Text\":\"Pan-fry skin side down until golden, flip and continue cooking\"},{\"Index\":4,\"Text\":\"Pour in teriyaki sauce, reduce on low heat until thick\"}]",
                "[\"Teriyaki\",\"Japanese\",\"Chicken\"]", false, false),

            // Dessert
            new Recipe("Tiramisu", "Classic Italian dessert, perfect combination of coffee and cheese",
                "tilamisu.jpg",
                "Dessert", 30, 240, 6, "Medium",
                "[{\"Index\":0,\"Text\":\"Beat egg yolk with sugar until pale, add mascarpone cheese and mix\"},{\"Index\":1,\"Text\":\"Whip cream to 60%, gently fold into cheese mixture\"},{\"Index\":2,\"Text\":\"Mix espresso with rum\"},{\"Index\":3,\"Text\":\"Quickly dip ladyfinger biscuits in coffee, layer at container bottom\"},{\"Index\":4,\"Text\":\"Spread a layer of cheese mixture, repeat layers\"},{\"Index\":5,\"Text\":\"Refrigerate at least 4 hours, dust cocoa powder before serving\"}]",
                "[\"Dessert\",\"Cheese\",\"Coffee\",\"Italian\"]", false, false),

            new Recipe("Double Skin Milk", "Silky smooth Cantonese classic dessert",
                "shuangpinai.jpg",
                "Dessert", 10, 20, 4, "Easy",
                "[{\"Index\":0,\"Text\":\"Heat milk in pot until edges bubble, pour into bowl\"},{\"Index\":1,\"Text\":\"Let milk cool to form skin, pour out milk along bowl edge, keep skin in bowl\"},{\"Index\":2,\"Text\":\"Beat egg white with sugar, mix into milk and strain\"},{\"Index\":3,\"Text\":\"Slowly pour back along bowl edge, let milk skin float up\"},{\"Index\":4,\"Text\":\"Cover with plastic wrap, steam for 15 minutes\"}]",
                "[\"Dessert\",\"Cantonese\",\"Milk\"]", false, false),

            new Recipe("Matcha Cake Roll", "Fresh matcha and soft cake perfect combination",
                "mochagaojuan.jpg",
                "Dessert", 20, 15, 6, "Medium",
                "[{\"Index\":0,\"Text\":\"Beat egg yolk with sugar, add milk and vegetable oil, mix well\"},{\"Index\":1,\"Text\":\"Sift in cake flour and matcha powder, mix well\"},{\"Index\":2,\"Text\":\"Whip egg white to soft peaks, fold into batter in batches\"},{\"Index\":3,\"Text\":\"Pour into baking pan, smooth surface, bake at 180C for 15 minutes\"},{\"Index\":4,\"Text\":\"Flip out after baking, spread cream filling, roll up and refrigerate\"}]",
                "[\"Dessert\",\"Matcha\",\"Cake\"]", false, false),

            // Drink
            new Recipe("Mango Pomelo Sago", "Refreshing and sweet classic Hong Kong style dessert drink",
                "yangzhiganlu.jpg",
                "Drink", 10, 0, 4, "Easy",
                "[{\"Index\":0,\"Text\":\"Peel and dice mango, save a few pieces for garnish\"},{\"Index\":1,\"Text\":\"Cook sago until transparent, drain with cold water\"},{\"Index\":2,\"Text\":\"Blend mango flesh with coconut milk in blender\"},{\"Index\":3,\"Text\":\"Place sago and mango puree in glass\"},{\"Index\":4,\"Text\":\"Pour in coconut milk and milk, add pomelo segments for garnish\"}]",
                "[\"Drink\",\"Hong Kong Style\",\"Mango\"]", false, false),

            new Recipe("Osmanthus Sour Plum Drink", "Heat-relieving traditional Chinese drink",
                "suanmeitang.jpg",
                "Drink", 10, 30, 8, "Easy",
                "[{\"Index\":0,\"Text\":\"Wash dried plum, hawthorn, licorice, dried tangerine peel\"},{\"Index\":1,\"Text\":\"Put in pot, add water and bring to boil on high heat\"},{\"Index\":2,\"Text\":\"Turn to low heat and simmer for 30 minutes\"},{\"Index\":3,\"Text\":\"Add rock sugar, stir until dissolved\"},{\"Index\":4,\"Text\":\"Strain and sprinkle dried osmanthus, better chilled\"}]",
                "[\"Drink\",\"Chinese\",\"Cooling\"]", false, false),
        };

        foreach (var recipe in recipes)
        {
            await _recipeRepo.SaveAsync(recipe);
        }
    }

    private async Task SeedIngredientsAsync()
    {
        var ingredients = new List<Ingredient>
        {
            new Ingredient("Tomato", "Vegetable", null, "gram", 500,
                DateTime.Today, DateTime.Today.AddDays(7), "Fridge",
                "{\"Calories\":18,\"Protein\":0.9,\"Carbs\":3.9,\"Fat\":0.2,\"Fiber\":1.2}", "tomato"),
            new Ingredient("Cucumber", "Vegetable", null, "piece", 3,
                DateTime.Today, DateTime.Today.AddDays(5), "Fridge",
                "{\"Calories\":15,\"Protein\":0.7,\"Carbs\":3.6,\"Fat\":0.1,\"Fiber\":0.5}", null),
            new Ingredient("Carrot", "Vegetable", null, "gram", 300,
                DateTime.Today, DateTime.Today.AddDays(14), "Fridge",
                "{\"Calories\":41,\"Protein\":0.9,\"Carbs\":9.6,\"Fat\":0.2,\"Fiber\":2.8}", "carrot"),
            new Ingredient("Chicken Breast", "Meat", null, "gram", 400,
                DateTime.Today, DateTime.Today.AddDays(3), "Freezer",
                "{\"Calories\":165,\"Protein\":31,\"Carbs\":0,\"Fat\":3.6,\"Fiber\":0}", "chicken"),
            new Ingredient("Pork Belly", "Meat", null, "gram", 500,
                DateTime.Today, DateTime.Today.AddDays(5), "Freezer",
                "{\"Calories\":349,\"Protein\":14.5,\"Carbs\":0,\"Fat\":30.8,\"Fiber\":0}", "pork"),
            new Ingredient("Salmon", "Seafood", null, "gram", 200,
                DateTime.Today, DateTime.Today.AddDays(2), "Freezer",
                "{\"Calories\":208,\"Protein\":20,\"Carbs\":0,\"Fat\":13,\"Fiber\":0}", null),
            new Ingredient("Shrimp", "Seafood", null, "gram", 250,
                DateTime.Today, DateTime.Today.AddDays(3), "Freezer",
                "{\"Calories\":99,\"Protein\":24,\"Carbs\":0.2,\"Fat\":0.3,\"Fiber\":0}", "shrimp"),
            new Ingredient("Egg", "Dairy", null, "piece", 10,
                DateTime.Today, DateTime.Today.AddDays(21), "Fridge",
                "{\"Calories\":155,\"Protein\":13,\"Carbs\":1.1,\"Fat\":11,\"Fiber\":0}", "egg"),
            new Ingredient("Apple", "Fruit", null, "piece", 4,
                DateTime.Today, DateTime.Today.AddDays(14), "Fridge",
                "{\"Calories\":52,\"Protein\":0.3,\"Carbs\":14,\"Fat\":0.2,\"Fiber\":2.4}", "apple"),
            new Ingredient("Banana", "Fruit", null, "piece", 3,
                DateTime.Today, DateTime.Today.AddDays(5), "Fridge",
                "{\"Calories\":89,\"Protein\":1.1,\"Carbs\":23,\"Fat\":0.3,\"Fiber\":2.6}", "banana"),
            new Ingredient("Orange", "Fruit", null, "piece", 4,
                DateTime.Today, DateTime.Today.AddDays(10), "Fridge",
                "{\"Calories\":47,\"Protein\":0.9,\"Carbs\":12,\"Fat\":0.1,\"Fiber\":2.4}", "orange"),
            new Ingredient("Rice", "Staple", null, "gram", 2000,
                DateTime.Today, null, "Cabinet",
                "{\"Calories\":346,\"Protein\":6.7,\"Carbs\":77.9,\"Fat\":0.7,\"Fiber\":0.7}", "rice"),
        };

        foreach (var ingredient in ingredients)
        {
            await _ingredientRepo.SaveAsync(ingredient);
        }
    }

    private async Task SeedMealPlansAsync()
    {
        var today = DateTime.Today;
        var startDate = new DateTime(2026, 5, 25);
        var endDate = new DateTime(2026, 7, 1);
        var plans = new List<MealPlan>();

        // From May 25 to July 1, total 38 days
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var dow = date.DayOfWeek;
            var isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
            var isToday = date.Date == today;
            var isFuture = date.Date > today;

            // Past dates all completed; today breakfast lunch completed, dinner snack not completed; future all not completed
            bool donePast = !isToday && !isFuture;
            bool doneTodayMorning = isToday;

            if (isWeekend)
            {
                // ===== Weekend Diet =====
                // Breakfast: fried egg + toast + milk
                plans.Add(new MealPlan(dateStr, "Breakfast", null, "Fried Sunny Egg", 1, 110, 7, 1, 8.5, 0, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "Breakfast", null, "Butter Toast", 1, 180, 5, 24, 7, 1.5, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "Breakfast", null, "Hot Milk", 1, 120, 5, 10, 4, 0, null, donePast || doneTodayMorning));

                // Lunch: three dishes one soup
                plans.Add(new MealPlan(dateStr, "Lunch", null, "Braised Spare Ribs", 1, 420, 22, 12, 28, 0.8, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "Lunch", null, "Garlic Broccoli", 1, 90, 4, 6, 5, 2.5, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "Lunch", null, "Stir-fried Snow Peas", 1, 80, 3, 7, 3.5, 2.8, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "Lunch", null, "Seaweed Egg Drop Soup", 1, 60, 4, 5, 2.5, 0.3, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "Lunch", null, "Rice", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));

                // Dinner: hotpot/BBQ style
                if (dow == DayOfWeek.Saturday)
                {
                    // Saturday hotpot
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Beef Slices", 1, 280, 18, 2, 22, 0, null, donePast));
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Fish Tofu + Shrimp Ball", 1, 160, 8, 10, 8, 0.5, null, donePast));
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Hotpot Vegetable Platter", 1, 85, 3, 8, 3, 3.5, null, donePast));
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Hotpot Broth Base", 1, 90, 2, 6, 6, 0.5, null, donePast));
                }
                else
                {
                    // Sunday BBQ
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Grilled Chicken Wings", 1, 240, 18, 4, 16, 0, null, donePast));
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Grilled Eggplant", 1, 100, 2, 8, 6, 2.5, null, donePast));
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Grilled Enoki Mushroom", 1, 65, 2.5, 5, 3.5, 2, null, donePast));
                    plans.Add(new MealPlan(dateStr, "Dinner", null, "Grilled Corn", 1, 120, 3, 22, 3, 2.5, null, donePast));
                }

                // Snack: dessert/drink
                plans.Add(new MealPlan(dateStr, "Snack", null, "Mango Sago Dessert", 1, 180, 2, 32, 5, 0.8, null, donePast));
                plans.Add(new MealPlan(dateStr, "Snack", null, "Honey Lemon Water", 1, 60, 0.2, 15, 0, 0.2, null, donePast));
            }
            else
            {
                // ===== Weekday Diet =====
                // Simple breakfast: congee + egg or milk + bread
                if (dow == DayOfWeek.Monday || dow == DayOfWeek.Wednesday || dow == DayOfWeek.Friday)
                {
                    plans.Add(new MealPlan(dateStr, "Breakfast", null, "Millet Congee", 1, 80, 2, 15, 1, 0.5, null, donePast || doneTodayMorning));
                    plans.Add(new MealPlan(dateStr, "Breakfast", null, "Boiled Egg", 1, 78, 6.5, 0.6, 5.3, 0, null, donePast || doneTodayMorning));
                    plans.Add(new MealPlan(dateStr, "Breakfast", null, "Cold Cucumber Salad", 1, 30, 1, 4, 0.5, 0.8, null, donePast || doneTodayMorning));
                }
                else
                {
                    plans.Add(new MealPlan(dateStr, "Breakfast", null, "Milk", 1, 120, 5, 10, 4, 0, null, donePast || doneTodayMorning));
                    plans.Add(new MealPlan(dateStr, "Breakfast", null, "Whole Wheat Bread", 1, 160, 6, 28, 2.5, 2, null, donePast || doneTodayMorning));
                }

                // Lunch: two dishes one rice
                switch (dow)
                {
                    case DayOfWeek.Monday:
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Tomato Scrambled Egg", 1, 150, 8, 10, 8, 2, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Shredded Pork with Green Pepper", 1, 210, 15, 6, 13, 1.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Rice", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Tuesday:
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Kung Pao Chicken", 1, 280, 20, 12, 15, 1.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Stir-fried Spinach", 1, 65, 3, 4, 3.5, 2.2, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Rice", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Wednesday:
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Mapo Tofu", 1, 180, 10, 8, 11, 1.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Twice-cooked Pork", 1, 320, 16, 8, 24, 1, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Rice", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Thursday:
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Sweet and Sour Pork Tenderloin", 1, 350, 18, 20, 18, 0.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Garlic Lettuce", 1, 55, 2, 4, 2.5, 1.8, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Rice", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Friday:
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Braised Chicken Thigh", 1, 300, 22, 10, 18, 0.8, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Vinegar Cabbage", 1, 75, 2, 7, 4, 2, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "Lunch", null, "Rice", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                }

                // Dinner: one meat one vegetable one soup
                switch (dow)
                {
                    case DayOfWeek.Monday:
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Steamed Sea Bass", 1, 180, 28, 2, 6, 0, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Stir-fried Bean Sprouts", 1, 60, 3, 6, 2, 1.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Tomato Egg Drop Soup", 1, 70, 4, 6, 3, 0.5, null, donePast));
                        break;
                    case DayOfWeek.Tuesday:
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Braised Tofu", 1, 160, 8, 8, 9, 1, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Stir-fried Broccoli", 1, 75, 3.5, 5, 3, 2.8, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Seaweed Shrimp Skin Soup", 1, 45, 3, 3, 1.5, 0.3, null, donePast));
                        break;
                    case DayOfWeek.Wednesday:
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Cola Chicken Wings", 1, 260, 18, 14, 14, 0.2, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Oyster Sauce Lettuce", 1, 65, 2, 5, 3, 1.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Winter Melon Spare Rib Soup", 1, 110, 8, 6, 6, 0.5, null, donePast));
                        break;
                    case DayOfWeek.Thursday:
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Stir-fried Pork with Garlic Scapes", 1, 220, 14, 8, 14, 1.8, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Cold Wood Ear Mushroom", 1, 55, 1.5, 6, 2, 2.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Corn Spare Rib Soup", 1, 130, 9, 8, 6, 1, null, donePast));
                        break;
                    case DayOfWeek.Friday:
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Braised Fish Chunks", 1, 210, 22, 6, 10, 0.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Shredded Potato Stir-fry", 1, 120, 3, 18, 4, 1.8, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Dinner", null, "Lotus Root Spare Rib Soup", 1, 120, 7, 8, 5, 1.2, null, donePast));
                        break;
                }

                // Snack: fruit/yogurt
                switch (dow)
                {
                    case DayOfWeek.Monday:
                        plans.Add(new MealPlan(dateStr, "Snack", null, "Apple", 1, 95, 0.5, 22, 0.3, 2.4, null, donePast));
                        break;
                    case DayOfWeek.Tuesday:
                        plans.Add(new MealPlan(dateStr, "Snack", null, "Plain Yogurt", 1, 130, 5, 16, 4, 0, null, donePast));
                        break;
                    case DayOfWeek.Wednesday:
                        plans.Add(new MealPlan(dateStr, "Snack", null, "Banana", 1, 105, 1.3, 24, 0.4, 2.6, null, donePast));
                        plans.Add(new MealPlan(dateStr, "Snack", null, "A Handful of Nuts", 1, 120, 3.5, 4, 10, 1.2, null, donePast));
                        break;
                    case DayOfWeek.Thursday:
                        plans.Add(new MealPlan(dateStr, "Snack", null, "Orange", 1, 70, 1.2, 16, 0.2, 3.1, null, donePast));
                        break;
                    case DayOfWeek.Friday:
                        plans.Add(new MealPlan(dateStr, "Snack", null, "Blueberry Yogurt Cup", 1, 150, 4.5, 22, 4, 1.5, null, donePast));
                        break;
                }
            }
        }

        foreach (var plan in plans)
        {
            await _mealPlanRepo.SaveAsync(plan);
        }

        System.Diagnostics.Debug.WriteLine($"[SeedDataService] Seeded {plans.Count} meal plans. Today={today:yyyy-MM-dd}");
    }

    private async Task SeedShoppingItemsAsync()
    {
        var items = new List<ShoppingItem>
        {
            // Vegetable
            new ShoppingItem("Broccoli", "Vegetable", 1, "head", false, null, null),
            new ShoppingItem("Bell Pepper", "Vegetable", 2, "piece", true, null, null),
            new ShoppingItem("Lettuce", "Vegetable", 1, "head", false, null, null),
            new ShoppingItem("Spinach", "Vegetable", 1, "bunch", false, null, null),
            new ShoppingItem("Bean Sprouts", "Vegetable", 1, "bag", false, null, null),
            new ShoppingItem("Lotus Root", "Vegetable", 1, "segment", false, null, null),

            // Meat
            new ShoppingItem("Chicken Thigh", "Meat", 500, "gram", false, null, null),
            new ShoppingItem("Lean Pork", "Meat", 300, "gram", true, null, null),
            new ShoppingItem("Spare Ribs", "Meat", 500, "gram", false, null, null),
            new ShoppingItem("Beef Slices", "Meat", 400, "gram", false, null, null),

            // Seafood
            new ShoppingItem("Sea Bass", "Seafood", 1, "fish", false, null, null),
            new ShoppingItem("Shrimp", "Seafood", 250, "gram", true, null, null),

            // Dairy
            new ShoppingItem("Yogurt", "Dairy", 4, "cup", false, null, null),
            new ShoppingItem("Fresh Milk", "Dairy", 1, "liter", false, null, null),

            // Staple
            new ShoppingItem("Toast Bread", "Staple", 1, "bag", false, null, null),
            new ShoppingItem("Millet", "Staple", 500, "gram", true, null, null),

            // Seasoning
            new ShoppingItem("Oyster Sauce", "Seasoning", 1, "bottle", false, null, null),
            new ShoppingItem("Sichuan Pepper Powder", "Seasoning", 1, "bag", true, null, null),

            // Fruit
            new ShoppingItem("Apple", "Fruit", 6, "piece", false, null, null),
            new ShoppingItem("Orange", "Fruit", 4, "piece", false, null, null),
        };

        foreach (var item in items)
        {
            await _shoppingItemRepo.SaveAsync(item);
        }
    }

    private async Task SeedNutritionTargetAsync()
    {
        var today = DateTime.Today;
        var startDate = new DateTime(2026, 5, 25);
        var endDate = new DateTime(2026, 7, 1);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var dow = date.DayOfWeek;
            var isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;

            double cal, pro, carb, fat, fib;
            if (isWeekend)
            {
                // Weekend: diet more rich, target slightly higher
                cal = 2200; pro = 65; carb = 270; fat = 72; fib = 28;
            }
            else
            {
                // Weekday: normal target
                cal = 2000; pro = 60; carb = 250; fat = 65; fib = 25;
            }

            var target = new NutritionTarget(dateStr, cal, pro, carb, fat, fib);
            await _nutritionRepo.SaveAsync(target);
        }
    }
}
