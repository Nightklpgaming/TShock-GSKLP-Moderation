//Microsoft
using IL.Terraria.Graphics;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
//System
using System.ComponentModel;
using System.Data;
using System.Reflection.PortableExecutable;

//Terraria
using Terraria;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.Hooks;

namespace MKLP.Modules
{
    public static class SurvivalManager
    {
        public static Dictionary<int, string> GetIllegalItem()
        {
            #region [ get Illegal Items ]
            Dictionary<int, string> getillegalitems = new();

            //bool bothevilworld = (Main.drunkWorld || Main.remixWorld || Main.zenithWorld);

            bool allowvanity = true;
            bool allowmusicbox = true;

            bool allowdungeonrush = true;
            bool allowtemplerush = true;

            #region | unobtainable |

            getillegalitems.Add(58, "Unobtainable");//heart
            getillegalitems.Add(1734, "Unobtainable");
            getillegalitems.Add(1867, "Unobtainable");

            getillegalitems.Add(184, "Unobtainable");//star
            getillegalitems.Add(1735, "Unobtainable");
            getillegalitems.Add(1868, "Unobtainable");

            getillegalitems.Add(603, "Unobtainable");
            getillegalitems.Add(766, "Unobtainable");

            int[] unobtainableids = { 2772, 2773, 2775, 2776, 2777, 2778, 2780, 2782, 2783,
                2785, 2880, 3453, 3454, 3455, 3462, 3463, 3465, 3705, 3706, 3847, 3848,
                3849, 3850, 3851, 3853, 3861, 3862, 3978, 4010, 4058, 4143, 5013, 5437 };

            foreach (int add in unobtainableids)
            {
                getillegalitems.Add(add, "Unobtainable");
            }

            if (!Main.drunkWorld && !Main.getGoodWorld && !Main.zenithWorld && !Main.tenthAnniversaryWorld)
            {
                getillegalitems.Add(678, "Unobtainable");
            }
            if (!Main.drunkWorld && !Main.remixWorld && !Main.zenithWorld)
            {
                getillegalitems.Add(5001, "Unobtainbale");
            }
            if (!Main.zenithWorld)
            {
                getillegalitems.Add(5334, "Unobtainable");
                getillegalitems.Add(5382, "Unobtainable");
            }

            if ((NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3) && (Main.zenithWorld || Main.remixWorld))
            {
                getillegalitems.Add(779, "Unobtainable");
                getillegalitems.Add(780, "Unobtainable");
                getillegalitems.Add(781, "Unobtainable");
                getillegalitems.Add(782, "Unobtainable");
                getillegalitems.Add(783, "Unobtainable");
                getillegalitems.Add(784, "Unobtainable");
            }
            if ((NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3) && (!Main.zenithWorld && !Main.remixWorld))
            {
                if (WorldGen.crimson)
                {
                    getillegalitems.Add(782, "Unobtainable");
                } else
                {
                    getillegalitems.Add(784, "Unobtainable");
                }
            }

            #endregion

            #region ( Banners )

            bool allowbanners = false;
            
            if (!allowbanners)
            {
                if (!NPC.downedMechBoss2)
                {
                    int[] addin = { 1669 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Evil Boss");
                    }
                }

                if (!Main.hardMode)
                {
                    int[] addin = { 1689, 2908, 2976, 2910, 1651, 2938, 2966, 2962, 4977, 1699, 1695,
                        1700, 1620, 2923, 1630, 4965, 2969, 2973, 1615, 3448, 1623, 1650, 2898, 1662,
                        3452, 1663, 1674, 1696, 2897, 1640, 2925, 1688, 1666, 4976, 1619, 3418, 3417,
                        3412, 3415, 3416, 1671, 3419, 2905, 4973, 2909, 1637, 3449, 1697, 1624, 1625,
                        4966, 1636, 4974, 1645, 1660, 2936, 1629, 1642, 4975, 2937, 3450, 1677, 1691,
                        1616, 2934, 1631, 4545, 4544, 4546, 4602, 3390, 3780, 3789, 3792, 3790, 3791,
                        2932, 1673, 3441, 3443, 3444, 3442, 1676, 3840, 3842, 3841, 3843};
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3) // mech
                {
                    int[] addin = { 2943, 3446, 1670, 3395, 2920, 1648, 3394, 1687, 3399, 1692 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Mechanical Boss");
                    }
                }

                if (!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss3) // post mech
                {
                    int[] addin = { 1679 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Post Mechanical Boss");
                    }
                }

                if (!NPC.downedPlantBoss)
                {
                    int[] addin = { 2900, 2970, 2930, 2914, 2956, 2965, 2904, 2974, 2975, 2984, 2917,
                        2924, 2958, 3402, 3396, 3397, 3403, 3401, 3400, 2929, 2931, 2961, 2971, 2982,
                        2918, 2919, 2921, 2926, 2941, 2957, 2963, 2993, 2994 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Plantera");
                    }
                    if (!allowtemplerush)
                    {
                        int[] templeids = { 1647, 1667 };
                        foreach (int add in templeids)
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }

                if (!NPC.downedGolemBoss)
                {
                    int[] addin = { 2945, 2946, 2947, 2948, 2949, 2950, 2951, 2952, 2953, 3445, 2972,
                        2901, 2902, 3846 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Golem");
                    }
                }

                if (!NPC.downedAncientCultist)
                {
                    int[] addin = { 3438, 3436, 3437, 3440, 3439, 3433, 3435, 3434, 3432, 3422, 3421, 
                        3424, 3425, 3420, 3423, 3426, 3428, 3430, 3429, 3431, 3427 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Lunatic Cultist");
                    }
                }

                getillegalitems.Add(2989, "Unobtainable");
                getillegalitems.Add(2990, "Unobtainable");
                getillegalitems.Add(2991, "Unobtainable");
                getillegalitems.Add(3404, "Unobtainable");
                getillegalitems.Add(3398, "Unobtainable");
            }

            #endregion

            //2223 

            //118

            #region ( Boss )

            #region [ king Slime ]
            if (!NPC.downedSlimeKing) //king slime
            {
                int[] addin = { 256, 257, 258, 762, 767, 769, 815, 998, 2430, 2489, 2493,
                    2585, 2610, 3090, 3215, 3318, 4120, 4797, 4929, 5131 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "King Slime");
                    }
                }
            }
            #endregion

            #region [ Eye of Cthulhu ]
            if (!NPC.downedBoss1)
            {
                for (int i = 4041; i <= 4048; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Eye of Cthulhu");
                    }
                }

                int[] addin = { 114, 1299, 1360, 1853, 1854, 2112, 3097, 3215, 3216,
                    3217, 3218, 3219, 3220, 3221, 3222, 3262, 3319, 4241, 4347,
                    4798, 4924, 5004, 5276, 5323, 5454, 5455 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Eye of Cthulhu");
                    }
                }

                if (!allowvanity)
                {
                    if (!getillegalitems.ContainsKey(3763))
                    {
                        getillegalitems.Add(3763, "Eye of Cthulhu");
                    }
                }
            }
            #endregion

            #region [ Evil Boss ]
            if (!NPC.downedBoss2)
            {
                for (int i = 3797; i <= 3883; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Evil Boss");
                    }
                }

                int[] addin = { 100, 101, 102, 103, 104, 123, 124, 125, 127, 197, 198, 199,
                    200, 201, 202, 203, 204, 792, 793, 794, 797, 798, 994, 1361, 1362, 2104,
                    2111, 2457, 3060, 3100, 3101, 3126, 3129, 3132, 3135, 3138, 3141, 3144,
                    3147, 3150, 3153, 3156, 3159, 3162, 3165, 3168, 3171, 3174, 3177, 3180,
                    3217, 3218, 3223, 3224, 3266, 3267, 3268, 3320, 3321, 4076, 4131, 4141,
                    4796, 4799, 4800, 4925, 4926, 4946, 4947, 4948, 5325 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Evil Boss");
                    }
                }

                if (!Main.hardMode)
                {
                    int[] hellstoneid = { 116, 117, 119, 120, 121, 122, 174, 175,
                        217, 219, 221, 231, 232, 233, 273, 2365, 4533, 4534, 4535, 4536, 4821 };

                    foreach (int add in hellstoneid) // hellstone are breakable from bombs on hm
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Evil Boss");
                        }
                    }
                }
                if (allowmusicbox)
                {
                    if (getillegalitems.ContainsKey(3869))
                    {
                        getillegalitems.Remove(3869);
                    }
                }
            }
            #endregion

            #region ( Evil Boss Enabled )

            if (MKLP.Config.BossManager.AllowEaterOfWorlds || MKLP.Config.BossManager.AllowBrainOfCthulhu) //modify this
            { //     true if its eow/boc enabled    |    false if its not enabled
                int[] hellstoneid = { 116, 117, 118, 119, 120, 121, 122, 174, 175,
                        217, 219, 221, 231, 232, 233, 273, 2365, 4533, 4534, 4535, 4536, 4821 };

                foreach (int remove in hellstoneid) // hellstone are breakable from bombs on hm
                {
                    if (!getillegalitems.ContainsKey(remove))
                    {
                        getillegalitems.Remove(remove);
                    }
                }
            } else
            {
                if (!getillegalitems.ContainsKey(86))
                {
                    getillegalitems.Add(86, "Evil Boss");
                }
                if (!getillegalitems.ContainsKey(1329))
                {
                    getillegalitems.Add(1329, "Evil Boss");
                }
            }
            //86
            #endregion

            #region [ Skeletron ]
            if (!NPC.downedBoss3)
            {
                for (int i = 3611; i <= 3616; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Skeletron");
                    }
                }

                int[] addin = { 346, 509, 510, 513, 541, 849, 850, 851, 852, 853, 1263, 1273,
                    1313, 1363, 2295, 2296, 2739, 2799, 3085, 3205, 3220, 3221, 3245, 3282,
                    3323, 3619, 3620, 3625, 3629, 3707, 3725, 4264, 4484, 4485, 4703, 4801,
                    4818, 4927, 4993 };

                int[] dungeonrushids = { 112, 113, 151, 152, 153, 155, 156, 157, 163, 164,
                    218, 220, 273, 274, 328, 329, 1613, 3019, 3317, 5010, 5126 };

                int[] vanityids = { 254, 260, 269, 270, 271, 322, 325, 326, 978, 979, 980,
                    981, 1275, 1276, 1281, 1288, 1289, 1429, 1740, 3242, 3243, 3244, 3246,
                    3247, 3362, 3363, 3627, 3730, 3731, 3733, 3734, 3735, 4128, 4129,
                    4130, 4132, 4133, 4134, 4685, 4686, 4704, 4705, 4706, 4707, 4708,
                    4709 };
                //981

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Skeletron");
                        }
                    }
                }
                

                foreach (int add in dungeonrushids)
                {
                    if (!allowdungeonrush)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Skeletron");
                        }
                    }
                    
                }

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Skeletron");
                    }
                }

                if (!Main.drunkWorld && !Main.remixWorld)
                {
                    int[] skeleids = { 154, 327, 768, 808, 811, 820, 827, 890, 891, 932, 959,
                        1307, 3095, 3122, 3124, 4076, 4131, 5074, 5325, 5328, 5358, 5359, 5360,
                        5361, 5438 };
                    foreach (int add in skeleids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Skeletron");
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Bee ]

            if (!NPC.downedQueenBee)
            {
                for (int i = 940; i <= 945; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Queen Bee");
                    }
                }

                int[] addin = { 842, 843, 844, 909, 910, 1123, 1129, 1130, 1132, 1167, 1170, 1249,
                    1339, 1340, 1341, 1342, 1353, 1354, 1355, 1356, 1357, 1359, 1364, 1430, 1578, 
                    1791, 2108, 2361, 2362, 2363, 2364, 2431, 2502, 2888, 3216, 3251, 3322, 3333,
                    4417, 4802, 4922, 4928 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Queen Bee");
                    }
                }
            }

            #endregion

            #region [ Deerclops ]

            if (!NPC.downedDeerclops)
            {
                int[] addin = { 5090, 5095, 5098, 5100, 5108, 5110, 5111, 5117, 5118, 5119, 5385 };

                int[] vanityids = { 5101, 5109, 5113 };

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Deerclops");
                        }
                    }
                }
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Deerclops");
                    }
                }
            }

            #endregion


            #region [ HardMode | Wall of Flesh ]
            if (!Main.hardMode)
            {
                for (int i = 364; i <= 391; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 487; i <= 497; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 514; i <= 528; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 1184; i <= 1224; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 1528; i <= 1537; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3006; i <= 3016; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3797; i <= 3808; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3884; i <= 3898; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3931; i <= 3950; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3955; i <= 3976; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3979; i <= 3987; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }

                if (!allowmusicbox) 
                {
                    for (int i = 562; i <= 574; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "HardMode | Wall of Flesh");
                        }
                    }
                    for (int i = 1596; i <= 1610; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "HardMode | Wall of Flesh");
                        }
                    }
                    for (int i = 5014; i <= 5040; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "HardMode | Wall of Flesh");
                        }
                    }
                    int[] musicboxids = { 576, 2742, 3236, 3237, 3371, 3796, 4077, 4078,
                        4079, 4080, 4081, 4082, 4356, 4357, 4358, 4606, 4979, 4990,
                        4991, 4992, 5006, 5112, 5362 };
                    
                    foreach (int add in musicboxids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                    
                }

                int[] addin = { 400, 401, 402, 403, 404, 406, 408, 409, 412, 415, 416, 417, 420,
                    421, 422, 425, 426, 434, 435, 436, 437, 481, 482, 483, 484, 485, 499, 500,
                    501, 502, 507, 508, 531, 532, 534, 535, 536, 537, 544, 545, 546, 556, 557, 575,
                    604, 605, 672, 682, 683, 684, 685, 686, 723, 725, 726, 761, 763, 770, 776, 777,
                    778, 785, 822, 828, 854, 855, 856, 860, 861, 862, 886, 888, 889, 892, 893, 897,
                    901, 902, 903, 904, 905, 970, 971, 972, 973, 991, 992, 993, 995, 996, 1104,
                    1105, 1106, 1171, 1181, 1244, 1247, 1253, 1264, 1265, 1306, 1308, 1312, 1314,
                    1315, 1321, 1324, 1326, 1328, 1332, 1333, 1334, 1335, 1336, 1347, 1348, 1351,
                    1352, 1353, 1355, 1356, 1365, 1519, 1589, 1590, 1591, 1592, 1593, 1594, 1612,
                    1613, 1704, 1705, 1710, 1716, 1720, 1967, 1968, 2105, 2133, 2137, 2143, 2147,
                    2151, 2155, 2161, 2169, 2170, 2177, 2209, 2238, 2270, 2331, 2370, 2371, 2372,
                    2379, 2389, 2405, 2429, 2454, 2462, 2463, 2465, 2468, 2471, 2473, 2480, 2483,
                    2484, 2494, 2551, 2584, 2607, 2673, 2693, 2694, 2695, 2696, 2697, 2698, 2701,
                    2750, 2751, 2752, 2753, 2754, 2755, 2787, 2788, 2801, 2802, 2998, 3020, 3022,
                    3023, 3029, 3030, 3033, 3034, 3035, 3046, 3049, 3051, 3052, 3053, 3054, 3064,
                    3091, 3092, 3103, 3104, 3182, 3184, 3185, 3186, 3209, 3210, 3211, 3214, 3222,
                    3234, 3238, 3258, 3260, 3269, 3283, 3289, 3290, 3315, 3316, 3324, 3334, 3335,
                    3338, 3339, 3343, 3346, 3351, 3359, 3366, 3385, 3386, 3387, 3388, 3752, 3753,
                    3754, 3755, 3764, 3765, 3766, 3767, 3768, 3769, 3770, 3771, 3776, 3777, 3778,
                    3779, 3781, 3782, 3783, 3784, 3785, 3786, 3787 ,3788, 3794, 3795, 3903, 3904,
                    3907, 3910, 3911, 3915, 3917, 3918, 3920, 3991, 3992, 4001, 4002, 4006, 4017,
                    4023, 4027, 4054, 4091, 4124, 4125, 4139, 4140, 4269, 4270, 4272, 4277, 4278,
                    4279, 4280, 4317, 4348, 4406, 4408, 4488, 4505, 4525, 4526, 4527, 4528, 4611,
                    4613, 4615, 4669, 4675, 4683, 4684, 4695, 4696, 4697, 4712, 4713, 4714, 4735,
                    4736, 4760, 4761, 4788, 4792, 4795, 4878, 4911, 4912, 4930, 4940, 4963, 4988,
                    5003, 5004, 5089, 5096, 5130, 5135, 5231, 5240, 5274, 5324, 5329, 5330, 5334,
                    5336, 5354, 5355, 5381 };

                int[] vanityids = { 503, 504, 505, 754, 755, 870, 871, 872, 1277, 1278, 1279, 1280,
                    1739, 1983, 3025, 3026, 3027, 3028, 3038, 3039, 3040, 3041, 3042, 3190, 3242,
                    3243, 3244, 3259, 3263, 3264, 3265, 3533, 3534, 3535, 3553, 3554, 3555, 3560,
                    3561, 3562, 3597, 3598, 3600, 3773, 3774, 3775, 4994, 4995, 4996, 4997, 4998,
                    4999 };

                if (!allowvanity)
                {
                    if (!NPC.downedBoss3)
                    {
                        int[] skeletronvanityids = { 864, 865, 869, 873, 874, 875, 4994, 4995,
                            4996, 4997, 4998, 4999 };
                        foreach (int add in skeletronvanityids)
                        {
                            if (!getillegalitems.ContainsKey(add))
                            {
                                getillegalitems.Add(add, "Skeletron");
                            }
                        }
                    }
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                }

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    int[] hminprehm = { 621, 624, 627, 630, 633, 637, 640, 643, 646,
                        649, 652, 659, 660, 661, 736, 737, 738, 2004, 2005, 2027, 2039,
                        2051, 2061, 2078, 2088, 2099, 2212, 2310, 2317, 2352, 2400, 2602,
                        2830, 3207, 4098, 4297, 4288, 4623, 4693, 4916, 5310 };

                    foreach (int add in hminprehm)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                    if (!Main.zenithWorld)
                    {
                        if (!getillegalitems.ContainsKey(662))
                        {
                            getillegalitems.Add(662, "HardMode | Wall of Flesh");
                        }
                        if (!getillegalitems.ContainsKey(663))
                        {
                            getillegalitems.Add(663, "HardMode | Wall of Flesh");
                        }
                        if (!getillegalitems.ContainsKey(3045))
                        {
                            getillegalitems.Add(3045, "HardMode | Wall of Flesh");
                        }
                        if (!getillegalitems.ContainsKey(3050))
                        {
                            getillegalitems.Add(3050, "HardMode | Wall of Flesh");
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Slime ]

            if (!NPC.downedQueenSlime)
            {
                int[] addin = { 4758, 4950, 4957, 4958, 4959, 4960, 4980, 4981, 4982,
                    4983, 4984, 4986, 4987, 5131 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Queen Slime");
                    }
                }
            }

            #endregion

            #region ( Mechanical Bosses )

            if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3)
            {
                for (int i = 1554; i <= 1568; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Mechanical Bosses");
                    }
                }
                for (int i = 3602; i <= 3610; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Mechanical Bosses");
                    }
                }

                int[] addin = { 550, 551, 552, 553, 558, 559, 578, 665, 748, 749, 756, 775, 779, 780, 781,
                    782, 783, 784, 787, 809, 813, 817, 821, 900, 1225, 1291, 1302, 1311, 1337, 1344, 1515,
                    1518, 1520, 1521, 1583, 1584, 1585, 1586, 1611, 1708, 1712, 1718, 1722, 2022, 2024,
                    2034, 2036, 2047, 2057, 2067, 2074, 2084, 2094, 2096, 2125, 2130, 2193, 2203, 2241,
                    2246, 2250, 2253, 2256, 2412, 2598, 2617, 2627, 2638, 2640, 2649, 2655, 2832, 2845,
                    3226, 3227, 3228, 3284, 3286, 3287, 3288, 3368, 3580, 3582, 3588, 3592, 3618, 3623,
                    3663, 3726, 3727, 3728, 3729, 3751, 3819, 3823, 3825, 3830, 3833, 3835, 3836, 3852,
                    3854, 3856, 3865, 3868, 3924, 3928, 4060, 4102, 4114, 4126, 4142, 4678, 4730, 4746,
                    4750, 4754, 4790, 4816, 4873, 4896, 4897, 4898, 4899, 4900, 4901, 4947, 5239, 5260,
                    5261, 5262, 5338 };

                int[] vanityids = { 666, 667, 668, 839, 840, 841, 1580, 1581, 1582, 1587, 1588,
                    1742, 1986, 2869, 2870, 2873, 2883, 3024, 3578, 3579, 3581, 3583, 3585,
                    3586, 3587, 3589, 3590, 3591, 3599, 3921, 3922, 3923, 3925, 3926, 3927,
                    3929, 4732, 4733, 4734, 4747, 4748, 4749, 4751, 4752, 4753, 4755, 4756,
                    4757 };

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Mechanical Bosses");
                        }
                    }
                } 
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Mechanical Bosses");
                    }
                }
            }


            if (!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss3)
            {
                for (int i = 1226; i <= 1235; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Post Mechanical Bosses");
                    }
                }

                int[] addin = { 579, 674, 675, 935, 936, 947, 990, 1001, 1002, 1003, 1004, 1005, 1006,
                    1165, 1179, 1316, 1317, 1318, 1327, 1343, 2188, 2220, 2223, 2789, 2792, 3353, 5289,
                    5296, 5382 };


                if (!allowvanity)
                {
                    if (!getillegalitems.ContainsKey(1985))
                    {
                        getillegalitems.Add(1985, "Post Mechanical Bosses");
                    }
                }
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Post Mechanical Bosses");
                    }
                }
            }

            #region [ The Destroyer ]

            if (!NPC.downedMechBoss1)
            {
                int[] addin = { 548, 561, 1366, 3325, 3355, 4699, 4803, 4932 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "The Destroyer");
                    }
                }
            }

            #endregion

            #region [ The Twins ]

            if (!NPC.downedMechBoss2)
            {
                int[] addin = { 533, 549, 1368, 1369, 2106, 2535, 3326, 3354,
                    4698, 4804, 4931 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "The Twins");
                    }
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.netID == 125 || npc.netID == 126)
                        {
                            getillegalitems.Remove(1368);
                            getillegalitems.Remove(1369);
                            break;
                        }
                    }
                }
            }

            #endregion

            #region [ Skeletron Prime ]

            if (!NPC.downedMechBoss3)
            {
                int[] addin = { 506, 547, 1367, 2107, 3327, 3356, 4700, 4805, 4933 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Skeletron Prime");
                    }
                }
            }

            #endregion

            #endregion

            #region [ Plantera ]

            if (!NPC.downedPlantBoss)
            {
                for (int i = 1155; i <= 1162; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Plantera");
                    }
                }
                for (int i = 1543; i <= 1552; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Plantera");
                    }
                }
                for (int i = 1829; i <= 1837; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Plantera");
                    }
                }

                if (!allowmusicbox)
                {
                    for (int i = 1963; i <= 1965; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "Plantera");
                        }
                    }
                }

                int[] addin = { 679, 757, 758, 759, 760, 771, 772, 773, 774, 786, 788, 823, 937, 938,
                    963, 977, 1137, 1141, 1167, 1178, 1182, 1183, 1254, 1255, 1259, 1260, 1262, 1266,
                    1300, 1305, 1339, 1340, 1341, 1342, 1346, 1350, 1357, 1370, 1444, 1445, 1446,
                    1503, 1504, 1505, 1506, 1507, 1508, 1513, 1517, 1569, 1570, 1571, 1572, 1573,
                    1729, 1730, 1782, 1783, 1784, 1785, 1797, 1798, 1801, 1802, 1803, 1804, 1805,
                    1806, 1807, 1811, 1814, 1815, 1816, 1817, 1818, 1826, 1844, 1845, 1855, 1856,
                    1857, 1858, 1864, 1866, 1871, 1914, 1916, 1928, 1929, 1930, 1931, 1946, 1947,
                    1958, 1959, 1960, 1961, 1962, 2028, 2043, 2064, 2071, 2081, 2091, 2103, 2109,
                    2176, 2189, 2383, 2393, 2409, 2605, 2620, 2650, 2770, 2791, 2794, 2847, 3018,
                    3021, 3098, 3105, 3106, 3107, 3108, 3249, 3261, 3291, 3292, 3328, 3336, 3997,
                    3998, 4005, 4013, 4116, 4409, 4444, 4445, 4446, 4447, 4448, 4449, 4457, 4458,
                    4459, 4607, 4679, 4680, 4701, 4731, 4789, 4793, 4794, 4806, 4812, 4813, 4814,
                    4934, 4941, 4942, 4943, 4944, 4945, 4961, 4964, 4985, 5065, 5223, 5227, 5236,
                    5237, 5288, 5291, 5292, 5344, 5345, 5451, 5452 };

                int[] vanityids = { 1514, 1743, 1744, 1745, 1788, 1789, 1790, 1943, 1944,
                    1945, 2878, 2879, 2884, 2885, 4738, 4739, 4740, 4741, 4742 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Plantera");
                    }
                }

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }

                if (!allowtemplerush)
                {
                    int[] skipplantids = { 1142, 1143, 1144, 1445, 1151, 1152, 1153, 1154, 1172,
                        1293, 2195, 2595, 2766, 2767, 5230 };
                    foreach (int add in skipplantids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    for (int i = 5076; i <= 5088; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "Plantera");
                        }
                    }

                    int[] princessids = { 5071, 5072, 5073 };
                    foreach (int add in princessids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }
            }

            #endregion

            #region [ Golem ]

            if (!NPC.downedGolemBoss)
            {
                for (int i = 2803; i <= 2826; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Golem");
                    }
                }
                for (int i = 3870; i <= 3883; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Golem");
                    }
                }
                if (!allowmusicbox)
                {
                    if (!getillegalitems.ContainsKey(4237))
                    {
                        getillegalitems.Add(4237, "Golem");
                    }
                }
                //
                int[] addin = { 899, 948, 1101, 1102, 1122, 1146, 1147, 1148, 1149, 1150,
                    1248, 1258, 1261, 1292, 1294, 1295, 1296, 1297, 1301, 1371, 1865, 2030,
                    2041, 2052, 2062, 2069, 2079, 2089, 2101, 2110, 2199, 2200, 2201, 2202,
                    2218, 2280, 2385, 2396, 2416, 2749, 2769, 2771, 2795, 2796, 2797, 2798,
                    2800, 2836, 2855, 2860, 2861, 2862, 2863, 2864, 2865, 2866, 2867, 2880,
                    2882, 3110, 3235, 3329, 3337, 3358, 3546, 3820, 3826, 3827, 3831, 3834,
                    3858, 3859, 3860, 3863, 3866, 3906, 4106, 4121, 4807, 4815, 4817, 4935,
                    4939, 4948 };

                int[] vanityids = { 3556 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Golem");
                    }
                }
                if (!allowvanity)
                {
                    foreach (int add in addin)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Golem");
                        }
                    }
                }
            }

            #endregion

            #region [ Duke Fishron ]

            if (!NPC.downedFishron)
            {
                int[] addin = { 2588, 2589, 2609, 2611, 2621, 2622, 2623, 2624, 3330, 3367,
                    4808, 4936 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Duke Fishron");
                    }
                }
            }

            #endregion
            
            #region [ Empress of Light ]

            if (!NPC.downedEmpressOfLight)
            {
                int[] addin = { 4715, 4778, 4782, 4783, 4784, 4811, 4823, 4914, 4923, 4949,
                    4952, 4953, 4989, 5005 };

                int[] vanityids = { 5075 };

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Empress of Light");
                        }
                    }
                }

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Empress of Light");
                    }
                }
            }

            #endregion

            #region [ Lunatic Cultist ]

            if (!NPC.downedAncientCultist)
            {
                for (int i = 4145; i <= 4236; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Lunatic Cultist");
                    }
                }

                if (!allowmusicbox)
                {
                    if (!getillegalitems.ContainsKey(3370))
                    {
                        getillegalitems.Add(3370, "Lunatic Cultist");
                    }
                }

                int[] addin = { 2856, 2857, 2858, 2859, 3331, 3357, 3372, 3456,
                    3457, 3458, 3459, 3473, 3474, 3475, 3476, 3536, 3537, 3538,
                    3539, 3540, 3542, 3543, 3544, 3549, 3572, 3573, 3574, 3575,
                    3576, 3601, 4809, 4937 };

                int[] vanityids = { 3526, 3527, 3528, 3529 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Lunatic Cultist");
                    }
                }

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Lunatic Cultist");
                        }
                    }
                }
            }

            #endregion

            #region [ Moon Lord ]

            if (!NPC.downedMoonlord)
            {
                for (int i = 2757; i <= 2765; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Moon Lord");
                    }
                }
                for (int i = 5401; i <= 5416; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Moon Lord");
                    }
                }

                int[] addin = { 1553, 2768, 2774, 2776, 2779, 2781, 2784, 2786, 3063,
                    3065, 3332, 3373, 3381, 3382, 3383, 3384, 3389, 3460, 3461, 3464,
                    3466, 3467, 3468, 3469, 3470, 3471, 3472, 3522, 3523, 3524, 3525,
                    3541, 3567, 3568, 3569, 3570, 3571, 3577, 3595, 3596, 3664, 3930,
                    4318, 4810, 4938, 4954, 4956, 5134, 5335, 5364, 5392, 5393, 5394 };

                int[] vanityids = { 3530 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Moon Lord");
                    }
                }

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Moon Lord");
                        }
                    }
                }

                if (!allowmusicbox)
                {
                    if (!getillegalitems.ContainsKey(3044))
                    {
                        getillegalitems.Add(3044, "Moon Lord");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    if (!getillegalitems.ContainsKey(5044))
                    {
                        getillegalitems.Add(5044, "Moon Lord");
                    }
                }
            }

            #endregion


            #region ( Difficulty )
            /*
            if (Main.expertMode)
            {
                if (Main.masterMode)
                {
                    int[] masterids = { };

                    foreach (int add in masterids)
                    {
                        getillegalitems.Remove(add);
                    }
                }
                int[] expertids = { 1131 };

                foreach (int add in expertids)
                {
                    getillegalitems.Remove(add);
                }
            }
            */
            #endregion

            #endregion

            #region ( Invasion )

            #region [ Goblin Army ]
            if (!NPC.downedGoblins) // Goblin Army
            {
                for (int i = 395; i <= 399; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Goblin Army");
                    }
                }
                for (int i = 3990; i <= 4008; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Goblin Army");
                    }
                }

                int[] addin = { 128, 405, 407, 486, 898, 907, 908, 976, 982, 983, 1163,
                    1164, 860, 861, 862, 901, 902, 903, 904, 1247, 935, 936, 984, 1249,
                    1250, 1251, 1252, 1301, 1343, 1578, 1595, 1612, 1613, 1724, 1860,
                    1861, 1862, 1863, 1864, 1865, 2220, 2221, 3110, 3121, 3122, 3123,
                    3124, 3241, 3250, 3251, 3252, 3366, 3721, 4038, 4874, 5000, 5064,
                    5126, 5331, 5358, 5359, 5360, 5361 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Goblin Army");
                    }
                }
                if (Main.hardMode && (Main.tenthAnniversaryWorld || Main.zenithWorld))
                {
                    if (!getillegalitems.ContainsKey(1724))
                    {
                        getillegalitems.Remove(1724);
                    }
                }
            }
            #endregion

            #region [ Pirate Invasion ]
            if (!NPC.downedPirates) // Pirate Invasion
            {
                int[] addin = { 876, 877, 878, 928, 929, 1180, 1337, 3369 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Pirate Invasion");
                    }
                }
            }
            #endregion

            #region [ Frost Legion ]
            if (!NPC.downedFrost)
            {
                int[] addin = { 3055, 3056, 3057, 3058, 3059 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Frost Legion");
                    }
                }
            }
            #endregion

            #region [ Solar Pillar ]
            if (!NPC.downedTowerSolar)
            {
                if (!getillegalitems.ContainsKey(4677))
                {
                    getillegalitems.Add(4677, "Solar Pillar");
                }
            }
            #endregion

            #endregion

            #region ( weapon switch )
            if (Main.remixWorld || Main.zenithWorld)
            {
                if (getillegalitems.ContainsKey(517))
                {
                    getillegalitems.Remove(517);
                }
                if (getillegalitems.ContainsKey(671))
                {
                    getillegalitems.Remove(671);
                }
                if (getillegalitems.ContainsKey(1314))
                {
                    getillegalitems.Remove(1314);
                }
                if (getillegalitems.ContainsKey(725))
                {
                    getillegalitems.Remove(725);
                }
                if (getillegalitems.ContainsKey(2623))
                {
                    if (allowdungeonrush || NPC.downedBoss3)
                    {
                        getillegalitems.Remove(2623);
                    }
                    
                }
                if (getillegalitems.ContainsKey(683))
                {
                    if (allowdungeonrush || NPC.downedBoss3)
                    {
                        getillegalitems.Remove(683);
                    }

                }
                if (!Main.hardMode)
                {
                    if (!getillegalitems.ContainsKey(3069))
                    {
                        getillegalitems.Add(3069, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalitems.ContainsKey(5147))
                    {
                        getillegalitems.Add(5147, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalitems.ContainsKey(1325))
                    {
                        getillegalitems.Add(1325, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalitems.ContainsKey(1319))
                    {
                        getillegalitems.Add(1319, "HardMode | Wall of Flesh");
                    }
                }
                if (!NPC.downedMechBossAny)
                {
                    if (!getillegalitems.ContainsKey(112))
                    {
                        getillegalitems.Add(112, "Mechanical Bosses");
                    }
                }
                if (!NPC.downedPlantBoss)
                {
                    if (!getillegalitems.ContainsKey(2273))
                    {
                        getillegalitems.Add(2273, "Plantera");
                    }

                }
                if (!NPC.downedFishron)
                {
                    if (!getillegalitems.ContainsKey(157))
                    {
                        getillegalitems.Add(157, "Duke Fishron");
                    }
                }
            }
            #endregion

            #endregion

            return getillegalitems;
        }

        public static Dictionary<short, string> GetIllegalProjectile()
        {

            #region [ get Illegal Projectile ]
            
            Dictionary<short, string> getillegalprojectile = new();

            //bool bothevilworld = (Main.drunkWorld || Main.remixWorld || Main.zenithWorld);

            bool allowdungeonrush = true;
            bool allowtemplerush = true;

            #region | unobtainable |

            short[] hostileProj = { 38, 39, 40, 44, 55, 56, 67, 71, 75, 81, 82, 83, 84,
                98, 99, 100, 101, 102, 109, 110, 115, 128, 129, 164, 174, 176, 177,
                179, 180, 184, 185, 186, 187, 188, 240, 241, 258, 259, 262, 264,
                275, 276, 277, 288, 290, 291, 292, 293, 299, 300, 302, 303, 325,
                326, 327, 328, 329, 345, 346, 347, 348, 349, 350, 351, 352, 384,
                385, 386, 435, 436, 437, 438, 447, 448, 449, 450, 452, 453, 454,
                455, 456, 462, 464, 465, 466, 467, 468, 472, 490, 498, 501, 526,
                537, 538, 539, 540, 572, 573, 574, 575, 576, 577, 578, 579, 580,
                581, 592, 593, 594, 596, 605, 607, 608, 622, 654, 657, 658, 670,
                671, 672, 673, 674, 675, 676, 682, 683, 686, 687, 719, 727, 811,
                813, 814, 836, 871, 872, 873, 874, 909, 919, 920, 921, 922, 923,
                926, 961, 962, 965, 980, 1001, 1002, 1005, 1007, 1013, 1014, 1021 };

            short[] unobtainableids = { 39, 111, 132, 156, 157, 427, 428, 429, 430, 431,
                432, 457, 458, 582, 583, 584, 585, 586, 589, 590, 609, 610, 624, 857,
                880, 924, 925, 929 };

            foreach (short add in unobtainableids)
            {
                if (!getillegalprojectile.ContainsKey(add))
                {
                    getillegalprojectile.Add(add, "Unobtainable");
                }
            }

            foreach (short add in unobtainableids)
            {
                if (!getillegalprojectile.ContainsKey(add))
                {
                    getillegalprojectile.Add(add, "Hostile");
                }
            }

            if (!Main.zenithWorld)
            {
                if (!getillegalprojectile.ContainsKey(1012))
                {
                    getillegalprojectile.Add(1012, "Unobtainable");
                }
            }

            #endregion

            //2223 

            #region ( Boss )

            #region [ king Slime ]
            if (!NPC.downedSlimeKing) //king slime
            {
                short[] addin = { 406, 881 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "King Slime");
                    }
                }
            }
            #endregion

            #region [ Eye of Cthulhu ]
            if (!NPC.downedBoss1)
            {
                short[] addin = { 17, 882, 994, 995 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Eye of Cthulhu");
                    }
                }

            }
            #endregion

            #region [ Evil Boss ]
            if (!NPC.downedBoss2)
            {
                for (short i = 663; i <= 669; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Evil Boss");
                    }
                }
                for (short i = 688; i <= 712; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Evil Boss");
                    }
                }

                short[] addin = { 20, 175, 534, 564, 677, 678, 679, 680, 883, 884, 955 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Evil Boss");
                    }
                }

                if (!Main.hardMode)
                {
                    short[] hellstoneid = { 19, 36, 41, 154, 375, 376, 734, 972, 973, 978 };

                    foreach (short add in hellstoneid) // hellstone are breakable from bombs on hm
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Evil Boss");
                        }
                    }
                }
            }
            #endregion

            #region ( Evil Boss Enabled )

            if (MKLP.Config.BossManager.AllowEaterOfWorlds || MKLP.Config.BossManager.AllowBrainOfCthulhu) //modify this
            { //     true if its eow/boc enabled    |    false if its not enabled
                short[] hellstoneid = { 19, 36, 41, 154, 375, 376, 734, 972, 973, 978 };

                foreach (short remove in hellstoneid) // hellstone are breakable from bombs on hm
                {
                    if (!getillegalprojectile.ContainsKey(remove))
                    {
                        getillegalprojectile.Remove(remove);
                    }
                }
            }
            else
            {

            }
            //86
            #endregion

            #region [ Skeletron ]
            if (!NPC.downedBoss3)
            {
                short[] addin = { 256, 270, 532, 545, 837, 885, 902,};

                short[] dungeonrushids = { 15, 16, 22, 26, 34, 35, 46, 485, 565, 972, 973, 977 };



                foreach (short add in dungeonrushids)
                {
                    if (!allowdungeonrush)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Skeletron");
                        }
                    }

                }

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Skeletron");
                    }
                }

                if (!Main.drunkWorld && !Main.remixWorld)
                {
                    short[] skeleids = { 734, 1019 };
                    foreach (short add in skeleids)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Skeletron");
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Bee ]

            if (!NPC.downedQueenBee)
            {
                short[] addin = { 181, 183, 198, 373, 374, 469, 566, 886, 999 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Queen Bee");
                    }
                }
            }

            #endregion

            #region [ Deerclops ]

            if (!NPC.downedDeerclops)
            {
                short[] addin = { 958, 960, 964, 966, 967, 968, 969 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Deerclops");
                    }
                }
            }

            #endregion


            #region [ HardMode | Wall of Flesh ]
            if (!Main.hardMode)
            {
                for (short i = 57; i <= 64; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (short i = 88; i <= 97; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (short i = 212; i <= 221; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (short i = 476; i <= 482; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }

                short[] addin = { 66, 68, 69, 73, 74, 76, 79, 80, 103, 104, 116, 117, 120, 130, 131,
                    158, 159, 160, 161, 208, 209, 237, 238, 239, 242, 253, 263, 265, 269, 271, 272,
                    278, 279, 280, 286, 287, 359, 367, 377, 378, 379, 382, 393, 390, 391, 392, 393,
                    394, 395, 424, 425, 426, 486, 488, 489, 491, 493, 494, 495, 496, 497, 521, 522,
                    523, 524, 535, 536, 546, 552, 553, 562, 563, 656, 659, 660, 661, 723, 724, 725,
                    726, 755, 756, 757, 823, 826, 838, 852, 853, 858, 859, 866, 877, 908, 912, 913,
                    917, 918, 928, 957 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    short[] hminprehm = { 370 };

                    foreach (short add in hminprehm)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                    if (!Main.zenithWorld)
                    {

                    }
                }
            }
            #endregion

            #region [ Queen Slime ]

            if (!NPC.downedQueenSlime)
            {
                short[] addin = { 864, 934, 935, 936, 937 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Queen Slime");
                    }
                }
            }

            #endregion

            #region ( Mechanical Bosses )

            if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3)
            {
                short[] addin = { 105, 114, 268, 274, 547, 549, 550, 551, 595, 652,
                    665, 666, 678, 689, 692, 695, 697, 698, 699, 700, 702, 704,
                    705, 706, 712, 728, 729, 847, 879, 900, 982 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Mechanical Bosses");
                    }
                }
            }


            if (!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss3)
            {
                for (short i = 222; i <= 229; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Post Mechanical Bosses");
                    }
                }

                short[] addin = { 107, 207, 252, 355, 357, 591, 973, 983 };


                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Post Mechanical Bosses");
                    }
                }
            }

            #region [ The Destroyer ]

            if (!NPC.downedMechBoss1)
            {
                short[] addin = { 106, 887 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "The Destroyer");
                    }
                }
            }

            #endregion

            #region [ The Twins ]

            if (!NPC.downedMechBoss2)
            {
                short[] addin = { 72, 76, 77, 78, 86, 87, 387, 388, 389, 888 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "The Twins");
                    }
                }
            }

            #endregion

            #region [ Skeletron Prime ]

            if (!NPC.downedMechBoss3)
            {
                short[] addin = { 85, 889 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Skeletron Prime");
                    }
                }
            }

            #endregion

            #endregion

            #region [ Plantera ]

            if (!NPC.downedPlantBoss)
            {
                for (short i = 133; i <= 144; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }
                for (short i = 335; i <= 344; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }
                for (short i = 776; i <= 801; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }
                for (short i = 803; i <= 810; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }

                short[] addin = { 150, 151, 152, 189, 190, 191, 192, 193, 194, 195, 199, 206, 210,
                    211, 247, 248, 250, 251, 254, 255, 282, 283, 285, 294, 295, 296, 297, 298,
                    301, 304, 305, 306, 307, 308, 309, 311, 312, 313, 314, 316, 317, 321, 322,
                    323, 324, 353, 356, 483, 484, 487, 504, 509, 510, 511, 512, 513, 514, 533,
                    554, 555, 567, 568, 569, 570, 571, 831, 832, 833, 834, 848, 849, 854, 862,
                    863, 878, 890, 896, 897, 898, 907, 916, 930, 984, 985, 997, 1020 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Plantera");
                    }
                }

                if (!allowtemplerush)
                {
                    short[] templeids = { 200 };

                    foreach (short add in templeids)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Plantera");
                        }
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    if (!getillegalprojectile.ContainsKey(950))
                    {
                        getillegalprojectile.Add(950, "Plantera");
                    }
                    if (!getillegalprojectile.ContainsKey(956))
                    {
                        getillegalprojectile.Add(956, "Plantera");
                    }
                }
            }

            #endregion

            #region [ Golem ]

            if (!NPC.downedGolemBoss)
            {
                for (short i = 439; i <= 446; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Golem");
                    }
                }

                short[] addin = { 182, 246, 249, 260, 261, 423, 433, 434, 451, 459,
                    460, 461, 606, 667, 668, 679, 684, 690, 693, 696, 707, 708,
                    709, 710, 711, 891, 899, 901 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Golem");
                    }
                }
            }

            #endregion

            #region [ Duke Fishron ]

            if (!NPC.downedFishron)
            {
                short[] addin = { 404, 405, 407, 408, 409, 410, 892 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Duke Fishron");
                    }
                }
            }

            #endregion

            #region [ Empress of Light ]

            if (!NPC.downedEmpressOfLight)
            {
                short[] addin = { 856, 895, 915, 927, 931, 932, 946 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Empress of Light");
                    }
                }
            }

            #endregion

            #region [ Lunatic Cultist ]

            if (!NPC.downedAncientCultist)
            {
                short[] addin = { 625, 626, 627, 630, 631, 634, 635, 636, 893, 953 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Lunatic Cultist");
                    }
                }
            }

            #endregion

            #region [ Moon Lord ]

            if (!NPC.downedMoonlord)
            {
                for (short i = 638; i <= 650; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Moon Lord");
                    }
                }
                short[] addin = { 502, 503, 600, 601, 602, 603, 604, 611, 612, 613,
                    614, 615, 616, 617, 618, 619, 620, 623, 632, 633, 714, 715,
                    716, 717, 718, 894, 933, 1015, 1016, 1017 };


                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Moon Lord");
                    }
                }
            }

            #endregion

            #endregion

            #region ( Invasion )

            #region [ Goblin Army ]
            if (!NPC.downedGoblins) // Goblin Army
            {
                /*
                int[] addin = { };

                foreach (int add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Goblin Army");
                    }
                }
                */
            }
            #endregion

            #region [ Pirate Invasion ]
            if (!NPC.downedPirates) // Pirate Invasion
            {
                short[] addin = { 162, 281 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Pirate Invasion");
                    }
                }
            }
            #endregion

            #region [ Frost Legion ]
            if (!NPC.downedFrost)
            {
                /*
                int[] addin = { };
                foreach (int add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Frost Legion");
                    }
                }
                */
            }
            #endregion

            #region [ Solar Pillar ]
            if (!NPC.downedTowerSolar)
            {
                if (!getillegalprojectile.ContainsKey(846))
                {
                    getillegalprojectile.Add(846, "Solar Pillar");
                }
            }
            #endregion

            #endregion

            #region ( weapon switch )
            if (Main.remixWorld || Main.zenithWorld)
            {
                if (getillegalprojectile.ContainsKey(93))
                {
                    getillegalprojectile.Remove(93);
                }
                if (getillegalprojectile.ContainsKey(271))
                {
                    getillegalprojectile.Remove(271);
                }
                if (getillegalprojectile.ContainsKey(120))
                {
                    getillegalprojectile.Remove(120);
                }
                if (getillegalprojectile.ContainsKey(410))
                {
                    if (allowdungeonrush || NPC.downedBoss3)
                    {
                        getillegalprojectile.Remove(410);
                    }

                }
                if (getillegalprojectile.ContainsKey(114))
                {
                    if (allowdungeonrush || NPC.downedBoss3)
                    {
                        getillegalprojectile.Remove(114);
                    }

                }
                if (!Main.hardMode)
                {
                    if (!getillegalprojectile.ContainsKey(954))
                    {
                        getillegalprojectile.Add(954, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalprojectile.ContainsKey(979))
                    {
                        getillegalprojectile.Add(979, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalprojectile.ContainsKey(273))
                    {
                        getillegalprojectile.Add(273, "HardMode | Wall of Flesh");
                    }
                }
                if (!NPC.downedMechBossAny)
                {
                    if (!getillegalprojectile.ContainsKey(15))
                    {
                        getillegalprojectile.Add(15, "Mechanical Bosses");
                    }
                }
                if (!NPC.downedFishron)
                {
                    if (!getillegalprojectile.ContainsKey(22))
                    {
                        getillegalprojectile.Add(22, "Duke Fishron");
                    }
                }
            }
            #endregion

            #endregion

            return getillegalprojectile;
        }

        public static void BossManager()
        {

        }
    }

    
}
