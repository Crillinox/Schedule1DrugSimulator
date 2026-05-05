// ================================================================
//  Schedule 1 (by TVGS) - Distinct Product Simulator v2
//  Features:
//   - User input: pick one drug, multiple (comma-separated), or "all"
//   - Per-drug distinct product counts (separate seen sets)
//   - Average addictiveness across all reachable products
//   - % of products at 100% addictiveness
//   - Most-used ingredient ranking
// ================================================================
using System;
using System.Collections.Generic;
using System.Linq;

enum E {
    Energizing=0,Gingeritis,Sneaky,CalorieDense,TropicThunder,
    Balding,Sedating,Toxic,Athletic,Slippery,Foggy,Spicy,
    BrightEyed,Jennerising,ThoughtProvoking,LongFaced,
    Euphoric,Focused,Refreshing,Munchies,Calming,
    Disorienting,Explosive,Laxative,Paranoia,Schizophrenic,
    SeizureInducing,Smelly,AntiGravity,Cyclopean,Glowing,
    Zombifying,Shrinking,Electrifying
}
enum I { Cuke=0,Banana,Paracetamol,Donut,Viagra,MouthWash,FluMedicine,Gasoline,EnergyDrink,MotorOil,MegaBean,Chili,Battery,Iodine,Addy,HorseSemen }

class Sim {
    // Base effect each ingredient adds (if no transform fires)
    static readonly E[] Base = {
        E.Energizing,E.Gingeritis,E.Sneaky,E.CalorieDense,E.TropicThunder,E.Balding,
        E.Sedating,E.Toxic,E.Athletic,E.Slippery,E.Foggy,E.Spicy,
        E.BrightEyed,E.Jennerising,E.ThoughtProvoking,E.LongFaced
    };

    // Addictiveness per effect (0.0–1.0 scale, from game data / Peepwood guide)
    // Product addictiveness = min(1, sum of all effect addictiveness values) shown as %
    static readonly double[] EffAddictiveness = new double[34];
    static void InitAddictiveness(){
        EffAddictiveness[(int)E.Energizing]       = 0.340;
        EffAddictiveness[(int)E.Gingeritis]       = 0.000;
        EffAddictiveness[(int)E.Sneaky]           = 0.327;
        EffAddictiveness[(int)E.CalorieDense]     = 0.100;
        EffAddictiveness[(int)E.TropicThunder]    = 0.456; // Tier 3 community value
        EffAddictiveness[(int)E.Balding]          = 0.000;
        EffAddictiveness[(int)E.Sedating]         = 0.000;
        EffAddictiveness[(int)E.Toxic]            = 0.000;
        EffAddictiveness[(int)E.Athletic]         = 0.315;
        EffAddictiveness[(int)E.Slippery]         = 0.231;
        EffAddictiveness[(int)E.Foggy]            = 0.363;
        EffAddictiveness[(int)E.Spicy]            = 0.352;
        EffAddictiveness[(int)E.BrightEyed]       = 0.400;
        EffAddictiveness[(int)E.Jennerising]      = 0.420;
        EffAddictiveness[(int)E.ThoughtProvoking] = 0.442;
        EffAddictiveness[(int)E.LongFaced]        = 0.525;
        EffAddictiveness[(int)E.Euphoric]         = 0.188;
        EffAddictiveness[(int)E.Focused]          = 0.160; 
        EffAddictiveness[(int)E.Refreshing]       = 0.140;
        EffAddictiveness[(int)E.Munchies]         = 0.120;
        EffAddictiveness[(int)E.Calming]          = 0.100;
        EffAddictiveness[(int)E.Disorienting]     = 0.000;
        EffAddictiveness[(int)E.Explosive]        = 0.000;
        EffAddictiveness[(int)E.Laxative]         = 0.100;
        EffAddictiveness[(int)E.Paranoia]         = 0.000;
        EffAddictiveness[(int)E.Schizophrenic]    = 0.000;
        EffAddictiveness[(int)E.SeizureInducing]  = 0.000;
        EffAddictiveness[(int)E.Smelly]           = 0.000;
        EffAddictiveness[(int)E.AntiGravity]      = 0.540;
        EffAddictiveness[(int)E.Cyclopean]        = 0.560;
        EffAddictiveness[(int)E.Glowing]          = 0.480;
        EffAddictiveness[(int)E.Zombifying]       = 0.580;
        EffAddictiveness[(int)E.Shrinking]        = 0.600;
        EffAddictiveness[(int)E.Electrifying]     = 0.235;
    }

    static readonly Dictionary<(E,I),E> T = new Dictionary<(E,I),E>{
        {(E.Toxic,I.Cuke),E.Euphoric},{(E.Slippery,I.Cuke),E.Munchies},{(E.Sneaky,I.Cuke),E.Paranoia},
        {(E.Foggy,I.Cuke),E.Cyclopean},{(E.Gingeritis,I.Cuke),E.ThoughtProvoking},{(E.Munchies,I.Cuke),E.Athletic},
        {(E.Euphoric,I.Cuke),E.Laxative},
        {(E.Energizing,I.Banana),E.ThoughtProvoking},{(E.Toxic,I.Banana),E.Smelly},{(E.Smelly,I.Banana),E.AntiGravity},
        {(E.Calming,I.Banana),E.Sneaky},{(E.Paranoia,I.Banana),E.Jennerising},{(E.Disorienting,I.Banana),E.Focused},
        {(E.Munchies,I.Paracetamol),E.AntiGravity},{(E.Slippery,I.Paracetamol),E.Sneaky},{(E.CalorieDense,I.Paracetamol),E.Sneaky},
        {(E.Gingeritis,I.Paracetamol),E.Refreshing},{(E.Foggy,I.Paracetamol),E.Calming},{(E.Energizing,I.Paracetamol),E.Paranoia},
        {(E.Euphoric,I.Paracetamol),E.Sneaky},
        {(E.CalorieDense,I.Donut),E.Explosive},{(E.Balding,I.Donut),E.Sneaky},{(E.Paranoia,I.Donut),E.Focused},
        {(E.Athletic,I.Viagra),E.Sneaky},{(E.Sedating,I.Viagra),E.TropicThunder},{(E.Calming,I.Viagra),E.TropicThunder},
        {(E.Calming,I.MouthWash),E.AntiGravity},{(E.Foggy,I.MouthWash),E.Calming},{(E.Sedating,I.MouthWash),E.Munchies},
        {(E.Athletic,I.FluMedicine),E.Munchies},{(E.Energizing,I.FluMedicine),E.Sedating},{(E.Sedating,I.FluMedicine),E.Refreshing},
        {(E.Calming,I.FluMedicine),E.Munchies},
        {(E.Energizing,I.Gasoline),E.Euphoric},{(E.Gingeritis,I.Gasoline),E.Smelly},{(E.Sneaky,I.Gasoline),E.TropicThunder},
        {(E.Munchies,I.Gasoline),E.Sedating},{(E.Paranoia,I.Gasoline),E.Calming},{(E.Disorienting,I.Gasoline),E.Glowing},
        {(E.Focused,I.EnergyDrink),E.Shrinking},{(E.Sedating,I.EnergyDrink),E.Munchies},{(E.Euphoric,I.EnergyDrink),E.Energizing},
        {(E.Paranoia,I.MotorOil),E.AntiGravity},{(E.Energizing,I.MotorOil),E.Munchies},{(E.Slippery,I.MotorOil),E.Toxic},
        {(E.Calming,I.MegaBean),E.Glowing},{(E.Energizing,I.MegaBean),E.Cyclopean},{(E.Sneaky,I.MegaBean),E.Calming},
        {(E.ThoughtProvoking,I.MegaBean),E.Cyclopean},{(E.Foggy,I.MegaBean),E.Disorienting},{(E.SeizureInducing,I.MegaBean),E.Focused},
        {(E.Laxative,I.Chili),E.LongFaced},{(E.Athletic,I.Chili),E.Euphoric},{(E.Munchies,I.Chili),E.Toxic},
        {(E.Sneaky,I.Chili),E.BrightEyed},{(E.Energizing,I.Chili),E.Euphoric},
        {(E.Euphoric,I.Battery),E.Zombifying},{(E.Cyclopean,I.Battery),E.Glowing},{(E.Munchies,I.Battery),E.TropicThunder},
        {(E.BrightEyed,I.Battery),E.Zombifying},
        {(E.Calming,I.Iodine),E.Jennerising},{(E.Foggy,I.Iodine),E.SeizureInducing},{(E.Gingeritis,I.Iodine),E.SeizureInducing},
        {(E.Sedating,I.Addy),E.Gingeritis},{(E.Foggy,I.Addy),E.Focused},{(E.Disorienting,I.Addy),E.Electrifying},
        {(E.Explosive,I.Addy),E.Euphoric},
        {(E.Gingeritis,I.HorseSemen),E.Refreshing},{(E.Sedating,I.HorseSemen),E.Electrifying},
        {(E.Athletic,I.HorseSemen),E.Gingeritis},{(E.Energizing,I.HorseSemen),E.Electrifying},
    };

    const int MaxE=8, MaxS=8;
    static readonly string[] INames = Enum.GetNames(typeof(I));

    // Drug definitions: name, starting effect set
    static readonly (string Name, long StartKey)[] Drugs = {
        ("Meth/Cocaine/Shrooms", 0L),
        ("OG Kush",              1L<<(int)E.Calming),
        ("Sour Diesel",          1L<<(int)E.Energizing),
        ("Green Crack",          1L<<(int)E.Refreshing),
        ("Granddaddy Purple",    1L<<(int)E.Sedating),
    };

    static void Main(){
        InitAddictiveness();

        Console.WriteLine("================================================");
        Console.WriteLine(" Schedule 1 (TVGS) - Product Simulator v2");
        Console.WriteLine("================================================");
        Console.WriteLine(" Available drugs:");
        Console.WriteLine("   meth        - Meth / Cocaine / Shrooms (same transforms)");
        Console.WriteLine("   ogkush      - OG Kush");
        Console.WriteLine("   sourdiesel  - Sour Diesel");
        Console.WriteLine("   greencrack  - Green Crack");
        Console.WriteLine("   gdp         - Granddaddy Purple");
        Console.WriteLine("   all         - Run all drugs");
        Console.WriteLine();
        Console.Write(" Enter selection (comma-separated or 'all'): ");
        string input = Console.ReadLine()?.Trim().ToLower() ?? "all";

        var selected = ParseSelection(input);
        if(selected.Count == 0){ Console.WriteLine("No valid selection. Defaulting to all."); selected = Drugs.Select((_,i)=>i).ToList(); }

        Console.WriteLine();
        Console.WriteLine($" Running BFS for {selected.Count} drug(s)...");
        Console.WriteLine();

        long[] ingUsage = new long[16];
        var results = new List<(string Name, int Unique, double AvgAddictive, double Pct100)>();

        foreach(int di in selected){
            var (drugName, startKey) = Drugs[di];
            var seen = new HashSet<long>();
            seen.Add(startKey);
            var frontier = new HashSet<long>{ startKey };
            double totalAdd = Addictiveness(startKey);
            int total100 = Addictiveness(startKey) >= 1.0 ? 1 : 0;

            for(int step=0; step<MaxS; step++){
                var next = new HashSet<long>();
                foreach(var sk in frontier){
                    var efx = Decode(sk);
                    for(int ing=0;ing<16;ing++){
                        var nfx = Apply(efx,(I)ing);
                        long nk = Encode(nfx);
                        if(seen.Add(nk)){
                            next.Add(nk);
                            ingUsage[ing]++;
                            double add = Addictiveness(nk);
                            totalAdd += add;
                            if(add >= 1.0) total100++;
                        }
                    }
                }
                if(next.Count==0) break;
                frontier = next;
            }

            double avg = seen.Count > 0 ? totalAdd / seen.Count * 100.0 : 0;
            double pct = seen.Count > 0 ? (double)total100 / seen.Count * 100.0 : 0;
            results.Add((drugName, seen.Count, avg, pct));
        }

        // Print per-drug results
        Console.WriteLine("================================================");
        Console.WriteLine(" RESULTS PER DRUG");
        Console.WriteLine("================================================");
        int grandTotal = 0;
        foreach(var (name, unique, avg, pct100) in results){
            Console.WriteLine();
            Console.WriteLine($" ▶  {name}");
            Console.WriteLine($"    Distinct products   : {unique,10:N0}");
            Console.WriteLine($"    Avg addictiveness   : {avg,9:F1}%");
            Console.WriteLine($"    Products at 100%    : {pct100,9:F1}% of all products");
            grandTotal += unique;
        }

        if(selected.Count > 1){
            Console.WriteLine();
            Console.WriteLine($" Grand total (with overlap): {grandTotal:N0}");
        }

        // Ingredient ranking (across all selected drugs)
        Console.WriteLine();
        Console.WriteLine("================================================");
        Console.WriteLine(" INGREDIENT RANKING (new unique products unlocked)");
        Console.WriteLine("================================================");
        var ranked = ingUsage.Select((c,i)=>(Name:INames[i],Count:c)).OrderByDescending(x=>x.Count).ToList();
        for(int r=0;r<ranked.Count;r++){
            string tag = r==0?" ← ★ MOST USED":"";
            Console.WriteLine($"  #{r+1,-2} {ranked[r].Name,-14}  {ranked[r].Count,8:N0} unique products{tag}");
        }
        Console.WriteLine("================================================");
    }

    static List<int> ParseSelection(string input){
        var map = new Dictionary<string,int>{
            {"meth",0},{"cocaine",0},{"shrooms",0},{"crystal",0},
            {"ogkush",1},{"og",1},{"og kush",1},
            {"sourdiesel",2},{"sour",2},{"sour diesel",2},
            {"greencrack",3},{"green",3},{"green crack",3},
            {"gdp",4},{"granddaddy",4},{"grandaddypurple",4},
        };
        if(input=="all") return Drugs.Select((_,i)=>i).ToList();
        var result = new HashSet<int>();
        foreach(var part in input.Split(',')){
            var key = part.Trim().ToLower().Replace(" ","");
            // try exact
            if(map.TryGetValue(key, out int idx)) result.Add(idx);
            else { // try partial match on drug names
                foreach(var kv in map) if(kv.Key.Contains(key)||key.Contains(kv.Key)) result.Add(kv.Value);
            }
        }
        return result.OrderBy(x=>x).ToList();
    }

    static double Addictiveness(long key){
        double sum=0;
        for(int i=0;i<34;i++) if((key>>i&1)==1) sum+=EffAddictiveness[i];
        return Math.Min(1.0, sum);
    }

    static long Encode(HashSet<E> fx){ long k=0; foreach(var e in fx) k|=1L<<(int)e; return k; }
    static HashSet<E> Decode(long k){ var r=new HashSet<E>(); for(int i=0;i<34;i++) if((k>>i&1)==1) r.Add((E)i); return r; }

    static HashSet<E> Apply(HashSet<E> cur, I ing){
        var r=new HashSet<E>(cur);
        var rem=new List<E>(); var add=new List<E>();
        foreach(var e in cur){ E ne; if(T.TryGetValue((e,ing),out ne)){rem.Add(e);add.Add(ne);} }
        foreach(var e in rem) r.Remove(e);
        foreach(var e in add) r.Add(e);
        if(r.Count<MaxE) r.Add(Base[(int)ing]);
        return r;
    }
}
