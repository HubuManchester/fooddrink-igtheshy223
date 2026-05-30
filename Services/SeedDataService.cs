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

    private const int CurrentSeedVersion = 8;

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

            // 版本升级或首次填充：先清除所有旧数据
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
            // 中式
            new Recipe("宫保鸡丁", "经典川菜，鸡肉鲜嫩配花生米，麻辣鲜香",
                "gongbao.jpg",
                "中式", 15, 20, 4, "简单",
                "[{\"Index\":0,\"Text\":\"鸡胸肉切丁，加料酒、淀粉腌制15分钟\"},{\"Index\":1,\"Text\":\"调碗汁：醋、酱油、糖、盐、淀粉水混合\"},{\"Index\":2,\"Text\":\"热锅冷油，爆香干辣椒和花椒\"},{\"Index\":3,\"Text\":\"下鸡丁滑炒至变色\"},{\"Index\":4,\"Text\":\"加入葱段和油炸花生米\"},{\"Index\":5,\"Text\":\"倒入碗汁快速翻炒均匀即可\"}]",
                "[\"川菜\",\"下饭\",\"鸡肉\",\"辣\"]", true, false),

            new Recipe("红烧肉", "肥而不腻的经典家常菜，入口即化",
                "hongshaorou.jpg",
                "中式", 10, 90, 6, "中等",
                "[{\"Index\":0,\"Text\":\"五花肉切块焯水去血沫\"},{\"Index\":1,\"Text\":\"锅中放少量油，加冰糖炒糖色\"},{\"Index\":2,\"Text\":\"放入五花肉翻炒上色\"},{\"Index\":3,\"Text\":\"加入葱段、姜片、八角、桂皮\"},{\"Index\":4,\"Text\":\"加入料酒、酱油、开水没过肉\"},{\"Index\":5,\"Text\":\"大火烧开后转小火炖60-90分钟\"},{\"Index\":6,\"Text\":\"最后大火收汁至浓稠\"}]",
                "[\"家常\",\"猪肉\",\"下饭\"]", false, false),

            new Recipe("番茄炒蛋", "最经典的家常菜，酸甜开胃",
                "fanqiechaodan.jpg",
                "中式", 5, 10, 3, "简单",
                "[{\"Index\":0,\"Text\":\"鸡蛋打散加少许盐搅匀\"},{\"Index\":1,\"Text\":\"番茄切块备用\"},{\"Index\":2,\"Text\":\"热锅下油炒鸡蛋至凝固盛出\"},{\"Index\":3,\"Text\":\"锅中再加油炒番茄出汁\"},{\"Index\":4,\"Text\":\"倒入鸡蛋翻炒均匀，加盐和糖调味\"}]",
                "[\"家常\",\"快手\",\"鸡蛋\",\"番茄\"]", true, false),

            new Recipe("麻婆豆腐", "麻辣鲜香的经典川菜，豆腐嫩滑入味",
                "mapodoufu.jpg",
                "中式", 10, 15, 3, "简单",
                "[{\"Index\":0,\"Text\":\"豆腐切小块，开水焯烫后捞出\"},{\"Index\":1,\"Text\":\"热锅下油，炒香豆瓣酱和蒜末\"},{\"Index\":2,\"Text\":\"加入肉末炒散变色\"},{\"Index\":3,\"Text\":\"加水烧开，放入豆腐小火炖5分钟\"},{\"Index\":4,\"Text\":\"水淀粉勾芡，撒花椒粉和葱花\"}]",
                "[\"川菜\",\"豆腐\",\"辣\"]", false, false),

            // 西式
            new Recipe("意大利肉酱面", "浓郁的肉酱搭配意面，经典西式主食",
                "yijiangmian.jpg",
                "西式", 10, 30, 3, "简单",
                "[{\"Index\":0,\"Text\":\"洋葱、胡萝卜、芹菜切碎\"},{\"Index\":1,\"Text\":\"热锅下油炒香蔬菜碎\"},{\"Index\":2,\"Text\":\"加入牛肉末炒散\"},{\"Index\":3,\"Text\":\"倒入番茄酱、水，加百里香和月桂叶\"},{\"Index\":4,\"Text\":\"小火炖煮20分钟，盐胡椒调味\"},{\"Index\":5,\"Text\":\"意面煮熟捞出，浇上肉酱即可\"}]",
                "[\"意面\",\"西式\",\"牛肉\"]", false, false),

            new Recipe("凯撒沙拉", "清爽的经典沙拉，配自制凯撒酱",
                "kaishaoshala.jpg",
                "西式", 15, 0, 2, "简单",
                "[{\"Index\":0,\"Text\":\"罗马生菜洗净撕成大块\"},{\"Index\":1,\"Text\":\"制作凯撒酱：蛋黄、柠檬汁、蒜泥、橄榄油搅匀\"},{\"Index\":2,\"Text\":\"加入帕玛森芝士碎和鳀鱼酱\"},{\"Index\":3,\"Text\":\"面包切丁烤至金黄做面包丁\"},{\"Index\":4,\"Text\":\"生菜淋上酱汁，撒面包丁和芝士粉\"}]",
                "[\"沙拉\",\"轻食\",\"健康\"]", false, false),

            new Recipe("法式洋葱汤", "浓郁醇厚的经典法式汤品",
                "yangcongtang.jpg",
                "西式", 15, 45, 4, "中等",
                "[{\"Index\":0,\"Text\":\"洋葱切丝，小火慢炒30分钟至焦糖色\"},{\"Index\":1,\"Text\":\"加入白葡萄酒煮至收干\"},{\"Index\":2,\"Text\":\"倒入牛肉高汤，加百里香和月桂叶\"},{\"Index\":3,\"Text\":\"小火炖煮15分钟，盐胡椒调味\"},{\"Index\":4,\"Text\":\"盛入烤碗，放上面包片和芝士\"},{\"Index\":5,\"Text\":\"入烤箱烤至芝士融化呈金黄\"}]",
                "[\"汤\",\"法式\",\"洋葱\"]", false, false),

            // 日韩
            new Recipe("日式味噌拉面", "浓郁味噌汤底配上劲道面条",
                "lamian.jpg",
                "日韩", 15, 20, 2, "中等",
                "[{\"Index\":0,\"Text\":\"猪骨汤煮沸，加入味噌搅匀\"},{\"Index\":1,\"Text\":\"加入酱油、味醂调味\"},{\"Index\":2,\"Text\":\"煮面条至劲道，捞出放入碗中\"},{\"Index\":3,\"Text\":\"铺上叉烧肉、溏心蛋、玉米粒\"},{\"Index\":4,\"Text\":\"注入味噌汤，撒葱花和海苔\"}]",
                "[\"拉面\",\"日式\",\"面食\"]", false, false),

            new Recipe("韩式石锅拌饭", "蔬菜丰富，锅巴香脆的经典韩料",
                "banfan.jpg",
                "日韩", 20, 10, 2, "简单",
                "[{\"Index\":0,\"Text\":\"各种蔬菜分别炒熟：胡萝卜丝、菠菜、豆芽、蘑菇\"},{\"Index\":1,\"Text\":\"石锅内壁刷香油\"},{\"Index\":2,\"Text\":\"放入米饭，铺上各种蔬菜和煎蛋\"},{\"Index\":3,\"Text\":\"石锅加热至底部米饭形成锅巴\"},{\"Index\":4,\"Text\":\"加入韩式辣酱，搅拌均匀食用\"}]",
                "[\"拌饭\",\"韩式\",\"米饭\"]", false, false),

            new Recipe("日式照烧鸡腿", "甜咸适口的照烧酱搭配嫩滑鸡腿",
                "zhaoshaoji.jpg",
                "日韩", 10, 20, 2, "简单",
                "[{\"Index\":0,\"Text\":\"鸡腿去骨，划几刀方便入味\"},{\"Index\":1,\"Text\":\"用盐和胡椒腌制10分钟\"},{\"Index\":2,\"Text\":\"调照烧汁：酱油、味醂、清酒、糖混合\"},{\"Index\":3,\"Text\":\"鸡皮朝下煎至金黄，翻面继续煎\"},{\"Index\":4,\"Text\":\"倒入照烧汁，小火收汁至浓稠\"}]",
                "[\"照烧\",\"日式\",\"鸡肉\"]", false, false),

            // 甜品
            new Recipe("提拉米苏", "经典意式甜品，咖啡与芝士的完美结合",
                "tilamisu.jpg",
                "甜品", 30, 240, 6, "中等",
                "[{\"Index\":0,\"Text\":\"蛋黄加糖打至发白，加入马斯卡彭芝士拌匀\"},{\"Index\":1,\"Text\":\"淡奶油打至六分发，轻柔拌入芝士糊\"},{\"Index\":2,\"Text\":\"浓缩咖啡加朗姆酒混合\"},{\"Index\":3,\"Text\":\"手指饼干快速蘸咖啡液，铺一层在容器底\"},{\"Index\":4,\"Text\":\"铺一层芝士糊，重复铺层\"},{\"Index\":5,\"Text\":\"冷藏至少4小时，食用前撒可可粉\"}]",
                "[\"甜品\",\"芝士\",\"咖啡\",\"意式\"]", false, false),

            new Recipe("双皮奶", "顺滑细腻的广式经典甜品",
                "shuangpinai.jpg",
                "甜品", 10, 20, 4, "简单",
                "[{\"Index\":0,\"Text\":\"牛奶倒入锅中加热至边缘冒泡，倒入碗中\"},{\"Index\":1,\"Text\":\"待牛奶冷却形成奶皮，沿碗边倒出牛奶，奶皮留碗中\"},{\"Index\":2,\"Text\":\"蛋清加糖搅匀，倒入牛奶中混合过滤\"},{\"Index\":3,\"Text\":\"沿碗边慢慢倒回，让奶皮浮起\"},{\"Index\":4,\"Text\":\"盖上保鲜膜，上锅蒸15分钟\"}]",
                "[\"甜品\",\"广式\",\"牛奶\"]", false, false),

            new Recipe("抹茶蛋糕卷", "清香抹茶与柔软蛋糕的完美组合",
                "mochagaojuan.jpg",
                "甜品", 20, 15, 6, "中等",
                "[{\"Index\":0,\"Text\":\"蛋黄加糖打发，加入牛奶和植物油搅匀\"},{\"Index\":1,\"Text\":\"筛入低筋面粉和抹茶粉拌匀\"},{\"Index\":2,\"Text\":\"蛋白打至湿性发泡，分次拌入面糊\"},{\"Index\":3,\"Text\":\"倒入烤盘抹平，180度烤15分钟\"},{\"Index\":4,\"Text\":\"出炉后倒扣，抹上奶油馅，卷起冷藏\"}]",
                "[\"甜品\",\"抹茶\",\"蛋糕\"]", false, false),

            // 饮品
            new Recipe("杨枝甘露", "清爽香甜的经典港式甜品饮品",
                "yangzhiganlu.jpg",
                "饮品", 10, 0, 4, "简单",
                "[{\"Index\":0,\"Text\":\"芒果去皮切块，留几粒装饰\"},{\"Index\":1,\"Text\":\"西米煮至透明，过冷水沥干\"},{\"Index\":2,\"Text\":\"芒果肉加椰浆用搅拌机打成泥\"},{\"Index\":3,\"Text\":\"杯中放入西米和芒果泥\"},{\"Index\":4,\"Text\":\"倒入椰浆和牛奶，放入柚子粒装饰\"}]",
                "[\"饮品\",\"港式\",\"芒果\"]", false, false),

            new Recipe("桂花酸梅汤", "消暑解渴的传统中式饮品",
                "suanmeitang.jpg",
                "饮品", 10, 30, 8, "简单",
                "[{\"Index\":0,\"Text\":\"乌梅、山楂、甘草、陈皮洗净\"},{\"Index\":1,\"Text\":\"放入锅中加水大火煮沸\"},{\"Index\":2,\"Text\":\"转小火煮30分钟\"},{\"Index\":3,\"Text\":\"加入冰糖搅拌至融化\"},{\"Index\":4,\"Text\":\"过滤后撒上干桂花，冷藏后饮用更佳\"}]",
                "[\"饮品\",\"中式\",\"解暑\"]", false, false),
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
            new Ingredient("番茄", "蔬菜", null, "克", 500,
                DateTime.Today, DateTime.Today.AddDays(7), "冰箱",
                "{\"Calories\":18,\"Protein\":0.9,\"Carbs\":3.9,\"Fat\":0.2,\"Fiber\":1.2}", "tomato"),
            new Ingredient("黄瓜", "蔬菜", null, "根", 3,
                DateTime.Today, DateTime.Today.AddDays(5), "冰箱",
                "{\"Calories\":15,\"Protein\":0.7,\"Carbs\":3.6,\"Fat\":0.1,\"Fiber\":0.5}", null),
            new Ingredient("胡萝卜", "蔬菜", null, "克", 300,
                DateTime.Today, DateTime.Today.AddDays(14), "冰箱",
                "{\"Calories\":41,\"Protein\":0.9,\"Carbs\":9.6,\"Fat\":0.2,\"Fiber\":2.8}", "carrot"),
            new Ingredient("鸡胸肉", "肉类", null, "克", 400,
                DateTime.Today, DateTime.Today.AddDays(3), "冰箱冷冻",
                "{\"Calories\":165,\"Protein\":31,\"Carbs\":0,\"Fat\":3.6,\"Fiber\":0}", "chicken"),
            new Ingredient("五花肉", "肉类", null, "克", 500,
                DateTime.Today, DateTime.Today.AddDays(5), "冰箱冷冻",
                "{\"Calories\":349,\"Protein\":14.5,\"Carbs\":0,\"Fat\":30.8,\"Fiber\":0}", "pork"),
            new Ingredient("三文鱼", "海鲜", null, "克", 200,
                DateTime.Today, DateTime.Today.AddDays(2), "冰箱冷冻",
                "{\"Calories\":208,\"Protein\":20,\"Carbs\":0,\"Fat\":13,\"Fiber\":0}", null),
            new Ingredient("虾仁", "海鲜", null, "克", 250,
                DateTime.Today, DateTime.Today.AddDays(3), "冰箱冷冻",
                "{\"Calories\":99,\"Protein\":24,\"Carbs\":0.2,\"Fat\":0.3,\"Fiber\":0}", "shrimp"),
            new Ingredient("鸡蛋", "蛋奶", null, "个", 10,
                DateTime.Today, DateTime.Today.AddDays(21), "冰箱",
                "{\"Calories\":155,\"Protein\":13,\"Carbs\":1.1,\"Fat\":11,\"Fiber\":0}", "egg"),
            new Ingredient("苹果", "水果", null, "个", 4,
                DateTime.Today, DateTime.Today.AddDays(14), "冰箱",
                "{\"Calories\":52,\"Protein\":0.3,\"Carbs\":14,\"Fat\":0.2,\"Fiber\":2.4}", "apple"),
            new Ingredient("香蕉", "水果", null, "根", 3,
                DateTime.Today, DateTime.Today.AddDays(5), "冰箱",
                "{\"Calories\":89,\"Protein\":1.1,\"Carbs\":23,\"Fat\":0.3,\"Fiber\":2.6}", "banana"),
            new Ingredient("橙子", "水果", null, "个", 4,
                DateTime.Today, DateTime.Today.AddDays(10), "冰箱",
                "{\"Calories\":47,\"Protein\":0.9,\"Carbs\":12,\"Fat\":0.1,\"Fiber\":2.4}", "orange"),
            new Ingredient("大米", "主食", null, "克", 2000,
                DateTime.Today, null, "橱柜",
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

        // 从5月25日到7月1日，共38天
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var dow = date.DayOfWeek;
            var isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
            var isToday = date.Date == today;
            var isFuture = date.Date > today;

            // 过去的日期全部完成；今天早餐午餐完成，晚餐加餐未完成；未来全部未完成
            bool donePast = !isToday && !isFuture;
            bool doneTodayMorning = isToday;

            if (isWeekend)
            {
                // ===== 周末饮食 =====
                // 早餐：煎蛋+吐司+牛奶
                plans.Add(new MealPlan(dateStr, "早餐", null, "煎荷包蛋", 1, 110, 7, 1, 8.5, 0, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "早餐", null, "黄油吐司", 1, 180, 5, 24, 7, 1.5, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "早餐", null, "热牛奶", 1, 120, 5, 10, 4, 0, null, donePast || doneTodayMorning));

                // 午餐：三菜一汤
                plans.Add(new MealPlan(dateStr, "午餐", null, "红烧排骨", 1, 420, 22, 12, 28, 0.8, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "午餐", null, "蒜蓉西兰花", 1, 90, 4, 6, 5, 2.5, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "午餐", null, "清炒荷兰豆", 1, 80, 3, 7, 3.5, 2.8, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "午餐", null, "紫菜蛋花汤", 1, 60, 4, 5, 2.5, 0.3, null, donePast || doneTodayMorning));
                plans.Add(new MealPlan(dateStr, "午餐", null, "米饭", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));

                // 晚餐：火锅/烧烤类
                if (dow == DayOfWeek.Saturday)
                {
                    // 周六火锅
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "肥牛卷", 1, 280, 18, 2, 22, 0, null, donePast));
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "鱼豆腐+虾滑", 1, 160, 8, 10, 8, 0.5, null, donePast));
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "火锅蔬菜拼盘", 1, 85, 3, 8, 3, 3.5, null, donePast));
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "火锅底料汤底", 1, 90, 2, 6, 6, 0.5, null, donePast));
                }
                else
                {
                    // 周日烧烤
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "烤鸡翅", 1, 240, 18, 4, 16, 0, null, donePast));
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "烤茄子", 1, 100, 2, 8, 6, 2.5, null, donePast));
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "烤金针菇", 1, 65, 2.5, 5, 3.5, 2, null, donePast));
                    plans.Add(new MealPlan(dateStr, "晚餐", null, "烤玉米", 1, 120, 3, 22, 3, 2.5, null, donePast));
                }

                // 加餐：甜品/饮品
                plans.Add(new MealPlan(dateStr, "加餐", null, "芒果西米露", 1, 180, 2, 32, 5, 0.8, null, donePast));
                plans.Add(new MealPlan(dateStr, "加餐", null, "蜂蜜柠檬水", 1, 60, 0.2, 15, 0, 0.2, null, donePast));
            }
            else
            {
                // ===== 工作日饮食 =====
                // 早餐简单：粥+鸡蛋 或 牛奶+面包
                if (dow == DayOfWeek.Monday || dow == DayOfWeek.Wednesday || dow == DayOfWeek.Friday)
                {
                    plans.Add(new MealPlan(dateStr, "早餐", null, "小米粥", 1, 80, 2, 15, 1, 0.5, null, donePast || doneTodayMorning));
                    plans.Add(new MealPlan(dateStr, "早餐", null, "水煮蛋", 1, 78, 6.5, 0.6, 5.3, 0, null, donePast || doneTodayMorning));
                    plans.Add(new MealPlan(dateStr, "早餐", null, "凉拌黄瓜", 1, 30, 1, 4, 0.5, 0.8, null, donePast || doneTodayMorning));
                }
                else
                {
                    plans.Add(new MealPlan(dateStr, "早餐", null, "牛奶", 1, 120, 5, 10, 4, 0, null, donePast || doneTodayMorning));
                    plans.Add(new MealPlan(dateStr, "早餐", null, "全麦面包", 1, 160, 6, 28, 2.5, 2, null, donePast || doneTodayMorning));
                }

                // 午餐：两菜一饭
                switch (dow)
                {
                    case DayOfWeek.Monday:
                        plans.Add(new MealPlan(dateStr, "午餐", null, "番茄炒蛋", 1, 150, 8, 10, 8, 2, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "青椒肉丝", 1, 210, 15, 6, 13, 1.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "米饭", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Tuesday:
                        plans.Add(new MealPlan(dateStr, "午餐", null, "宫保鸡丁", 1, 280, 20, 12, 15, 1.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "清炒菠菜", 1, 65, 3, 4, 3.5, 2.2, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "米饭", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Wednesday:
                        plans.Add(new MealPlan(dateStr, "午餐", null, "麻婆豆腐", 1, 180, 10, 8, 11, 1.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "回锅肉", 1, 320, 16, 8, 24, 1, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "米饭", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Thursday:
                        plans.Add(new MealPlan(dateStr, "午餐", null, "糖醋里脊", 1, 350, 18, 20, 18, 0.5, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "蒜蓉生菜", 1, 55, 2, 4, 2.5, 1.8, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "米饭", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                    case DayOfWeek.Friday:
                        plans.Add(new MealPlan(dateStr, "午餐", null, "红烧鸡腿", 1, 300, 22, 10, 18, 0.8, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "醋溜白菜", 1, 75, 2, 7, 4, 2, null, donePast || doneTodayMorning));
                        plans.Add(new MealPlan(dateStr, "午餐", null, "米饭", 1, 200, 4, 44, 0.5, 0.5, null, donePast || doneTodayMorning));
                        break;
                }

                // 晚餐：一荤一素一汤
                switch (dow)
                {
                    case DayOfWeek.Monday:
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "清蒸鲈鱼", 1, 180, 28, 2, 6, 0, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "炒豆芽", 1, 60, 3, 6, 2, 1.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "番茄蛋花汤", 1, 70, 4, 6, 3, 0.5, null, donePast));
                        break;
                    case DayOfWeek.Tuesday:
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "红烧豆腐", 1, 160, 8, 8, 9, 1, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "清炒西兰花", 1, 75, 3.5, 5, 3, 2.8, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "紫菜虾皮汤", 1, 45, 3, 3, 1.5, 0.3, null, donePast));
                        break;
                    case DayOfWeek.Wednesday:
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "可乐鸡翅", 1, 260, 18, 14, 14, 0.2, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "蚝油生菜", 1, 65, 2, 5, 3, 1.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "冬瓜排骨汤", 1, 110, 8, 6, 6, 0.5, null, donePast));
                        break;
                    case DayOfWeek.Thursday:
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "蒜苔炒肉", 1, 220, 14, 8, 14, 1.8, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "凉拌木耳", 1, 55, 1.5, 6, 2, 2.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "玉米排骨汤", 1, 130, 9, 8, 6, 1, null, donePast));
                        break;
                    case DayOfWeek.Friday:
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "红烧鱼块", 1, 210, 22, 6, 10, 0.5, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "炒土豆丝", 1, 120, 3, 18, 4, 1.8, null, donePast));
                        plans.Add(new MealPlan(dateStr, "晚餐", null, "莲藕排骨汤", 1, 120, 7, 8, 5, 1.2, null, donePast));
                        break;
                }

                // 加餐：水果/酸奶
                switch (dow)
                {
                    case DayOfWeek.Monday:
                        plans.Add(new MealPlan(dateStr, "加餐", null, "苹果", 1, 95, 0.5, 22, 0.3, 2.4, null, donePast));
                        break;
                    case DayOfWeek.Tuesday:
                        plans.Add(new MealPlan(dateStr, "加餐", null, "原味酸奶", 1, 130, 5, 16, 4, 0, null, donePast));
                        break;
                    case DayOfWeek.Wednesday:
                        plans.Add(new MealPlan(dateStr, "加餐", null, "香蕉", 1, 105, 1.3, 24, 0.4, 2.6, null, donePast));
                        plans.Add(new MealPlan(dateStr, "加餐", null, "坚果一小把", 1, 120, 3.5, 4, 10, 1.2, null, donePast));
                        break;
                    case DayOfWeek.Thursday:
                        plans.Add(new MealPlan(dateStr, "加餐", null, "橙子", 1, 70, 1.2, 16, 0.2, 3.1, null, donePast));
                        break;
                    case DayOfWeek.Friday:
                        plans.Add(new MealPlan(dateStr, "加餐", null, "蓝莓酸奶杯", 1, 150, 4.5, 22, 4, 1.5, null, donePast));
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
            // 蔬菜
            new ShoppingItem("西兰花", "蔬菜", 1, "颗", false, null, null),
            new ShoppingItem("彩椒", "蔬菜", 2, "个", true, null, null),
            new ShoppingItem("生菜", "蔬菜", 1, "颗", false, null, null),
            new ShoppingItem("菠菜", "蔬菜", 1, "把", false, null, null),
            new ShoppingItem("豆芽", "蔬菜", 1, "袋", false, null, null),
            new ShoppingItem("莲藕", "蔬菜", 1, "节", false, null, null),

            // 肉类
            new ShoppingItem("鸡腿", "肉类", 500, "克", false, null, null),
            new ShoppingItem("猪瘦肉", "肉类", 300, "克", true, null, null),
            new ShoppingItem("排骨", "肉类", 500, "克", false, null, null),
            new ShoppingItem("肥牛卷", "肉类", 400, "克", false, null, null),

            // 海鲜
            new ShoppingItem("鲈鱼", "海鲜", 1, "条", false, null, null),
            new ShoppingItem("虾仁", "海鲜", 250, "克", true, null, null),

            // 蛋奶
            new ShoppingItem("酸奶", "蛋奶", 4, "盒", false, null, null),
            new ShoppingItem("鲜牛奶", "蛋奶", 1, "升", false, null, null),

            // 主食
            new ShoppingItem("吐司面包", "主食", 1, "袋", false, null, null),
            new ShoppingItem("小米", "主食", 500, "克", true, null, null),

            // 调味料
            new ShoppingItem("蚝油", "调味料", 1, "瓶", false, null, null),
            new ShoppingItem("花椒粉", "调味料", 1, "袋", true, null, null),

            // 水果
            new ShoppingItem("苹果", "水果", 6, "个", false, null, null),
            new ShoppingItem("橙子", "水果", 4, "个", false, null, null),
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
                // 周末：饮食更丰富，目标略高
                cal = 2200; pro = 65; carb = 270; fat = 72; fib = 28;
            }
            else
            {
                // 工作日：正常目标
                cal = 2000; pro = 60; carb = 250; fat = 65; fib = 25;
            }

            var target = new NutritionTarget(dateStr, cal, pro, carb, fat, fib);
            await _nutritionRepo.SaveAsync(target);
        }
    }
}
