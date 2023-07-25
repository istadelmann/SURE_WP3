/**
	Sustainable Energy Development game modeling the Swiss energy Grid.
	Copyright (C) 2023 Università della Svizzera Italiana

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Godot;
using System;

// ============================================================
// ==================== RESOURCE DATATYPES ====================
// ============================================================

// ==================== Energy DataType ====================

// Models the resource managed by the EnergyManager
public struct Energy {
	public float SupplySummer; // Total Supply for the next turn for the summer months
	public float SupplyWinter; // Total Supply for the next turn for the winter months
	public float DemandSummer; // Total Demand for the next turn for the summer months
	public float DemandWinter; // Total Demand for the next turn for the winter months
	public float SurplusSummer; // Amount of excess energy produced in the summer months (represents an underproduction if negative)
	public float SurplusWinter; // Amount of excess energy produced in the winter months (represents an underproduction if negative)


	// Basic constructor for the Energy Ressource
	public Energy(float SS=0, float SW=0, float DS=0, float DW=0, float SurS=0, float SurW=0) {
		SupplySummer = SS;
		SupplyWinter = SW;
		DemandSummer = DS;
		DemandWinter = DW;
		SurplusSummer = SurS;
		SurplusWinter = SurW;
	}
}

// ==================== Environment DataType ====================

// Models the environment resource
public struct Environment {
	public int Pollution; // Aggregate amount of pollution generated by the power plants
	public float LandUse; // Aggregate space taken up by all of the power plants
	public float Biodiversity; // Level of biodiversity remaining on the map

	// Computes the value used to update the environment bar
	public float EnvBarValue() =>
		2.0f * (0.5f * Biodiversity - 0.5f * LandUse);

	// Basic constructor for the environment struct
	public Environment(int p=0, float lu=0.0f, float bd=0.0f) {
		Pollution = p;

		// Make sure that the following two metrics are percentages
		LandUse = Math.Max(0.0f, Math.Min(lu, 1.0f)); 
		Biodiversity = Math.Max(0.0f, Math.Min(bd, 1.0f));
	}
}

// ==================== Money DataType ====================

// Contains all of the fields related to money
public struct MoneyData {
	public int Money; // Total amount of available money
	public int Budget; // Total budget for this round
	public int Production; // Money spent on production costs this round
	public int Build; // Money spent on build costs this round
	public int Imports; // Money spent on imports this turn

	// Default constructor for the MoneyData
	public MoneyData(int start_money) {
		Money = start_money;
		Budget = start_money;
		Production = 0;
		Build = 0;
		Imports = 0;
	}

	// Resets the spending statistics at the end of each round
	public void NextTurn(int new_budget, int production, int ImportCost) {
		Money += new_budget - production - ImportCost;
		Budget = Money;
		Production = production;
		Build = 0;
		Imports = ImportCost;
	}

	// Spends money by updating the data correctly
	public void SpendMoney(int amountBuild) {
		Build += amountBuild;
		Money -= amountBuild;
	}
}

// ============================================================
// ======================= FANCY ENUMS ========================
// ============================================================

// ==================== Config Enum ====================

// Models a config type
public readonly struct Config {
	// Represents the different types of configs
	public enum Type { POWER_PLANT, NONE };

	// Internal storage of a config
	public readonly Type type;

	// Basic constructor for the Config type
	public Config(Type ct)  {
		type = ct;
	}

	// Override equality and inequality operators
	public static bool operator ==(Config l, Config other) => l.type == other.type;
	public static bool operator !=(Config l, Config other) => l.type != other.type;

	// Override the incrementation and decrementation operators
	public static Config operator ++(Config l) => new Config((Type)((int)(l.type + 1) % (int)(Type.NONE + 1)));
	public static Config operator --(Config l) => new Config((Type)((int)(l.type - 1) % (int)(Type.NONE + 1)));

	// Implicit conversion from the enum to the struct
	public static implicit operator Config(Type lt) => new Config(lt);

	// Implicit conversion from the struct to the enum
	public static implicit operator Type(Config l) => l.type;

	// Implicit conversion from a string to a config type
	public static implicit operator Config(string s) {
		// Make it as easy to parse as possible
		string s_ = s.ToLower().StripEdges();
		if(s == "powerplants") {
			return new Config(Type.POWER_PLANT);
		} 
		return new Config(Type.NONE);
	}
	
	// Implicit conversion to a string
	public override string ToString() => type == Type.POWER_PLANT ? "powerplants.xml" : "";

	// Performs the same check as the == operator, but with a run-time check on the type
	public override bool Equals(object obj) {
		// Check for null and compare run-time types.
		if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
			return false;
		}
		// Perform actual equality check
		return type == ((Config)obj).type;
	}

	// Override of the get hashcode method (needed to overload == and !=)
	public override int GetHashCode() => HashCode.Combine(type);
}

// ==================== Language Enum ====================

// Models a language
public readonly struct Language {
	// Represents the different types of languages
	public enum Type { EN, FR, DE, IT };

	// Internal storage of a language
	public readonly Type lang;

	// Basic constructor for the language
	public Language(Type l)  {
		lang = l;
	}

	// Override equality and inequality operators
	public static bool operator ==(Language l, Language other) => l.lang == other.lang;
	public static bool operator !=(Language l, Language other) => l.lang != other.lang;

	// Override the incrementation and decrementation operators
	public static Language operator ++(Language l) => new Language((Type)((int)(l.lang + 1) % (int)(Type.IT + 1)));
	public static Language operator --(Language l) => new Language((Type)((int)(l.lang - 1) % (int)(Type.IT + 1)));

	// Implicit conversion from the enum to the struct
	public static implicit operator Language(Type lt) => new Language(lt);

	// Implicit conversion from the struct to the enum
	public static implicit operator Type(Language l) => l.lang;

	// Implicit conversion from a string to a language
	public static implicit operator Language(string s) {
		// Make it as easy to parse as possible
		string s_ = s.ToLower().StripEdges();
		if(s == "en" || s == "english") {
			return new Language(Type.EN);
		} 
		if (s == "fr" || s == "french" || s == "français") {
			return new Language(Type.FR);
		} 
		if(s == "de" || s == "german" || s == "deutsch") {
			return new Language(Type.DE);
		} 
		return new Language(Type.IT);
	}
	
	// Implicit conversion to a string
	public override string ToString() => lang == Type.EN ? "en" : 
										 lang == Type.FR ? "fr" :
										 lang == Type.DE ? "de" :
										 "it";

	public string ToName() => lang == Type.EN ? "Language: English" : 
							  lang == Type.FR ? "Langue: Français" :
							  lang == Type.DE ? "Sprache: Deutsch" :
							  "Lingua: Italiano";

	// Performs the same check as the == operator, but with a run-time check on the type
	public override bool Equals(object obj) {
		// Check for null and compare run-time types.
		if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
			return false;
		}
		// Perform actual equality check
		return lang == ((Language)obj).lang;
	}

	// Override of the get hashcode method (needed to overload == and !=)
	public override int GetHashCode() => HashCode.Combine(lang);
}

// ==========================================================
// ==================== UTILITY DATATYPES ===================
// ==========================================================

// ==================== Config Datatype ===================

// Abstract notion of config data
public interface ConfigData {
    // Copies the data into a powerplant
    public abstract void _CopyTo(ref PowerPlant nd);
}

// Represents the data retrieved from a powerplant config file
public readonly struct PowerPlantConfigData : ConfigData {
    
    // Metadata fields
    public readonly int BuildCost;
    public readonly int BuildTime;
    public readonly int LifeCycle;

    // Energy fields
    public readonly int ProductionCost;
    public readonly int Capacity;
    public readonly float Availability;

    // Environment fields
    public readonly int Pollution;
    public readonly float LandUse;
    public readonly float Biodiversity;

    // Basic constructor for the datatype
    public PowerPlantConfigData(
        int bc=0, int bt=0, int lc=0,
        int pc=0, int cap=0, float av=0,
        int pol=0, float lu=0, float bd=0
    ) {
        // Simply fill in the fields
        BuildCost = bc;
        BuildTime = bt;
        LifeCycle = lc;
        ProductionCost = pc;
        Capacity = cap;
        Availability = Math.Max(0.0f, Math.Min(av, 1.0f));
        Pollution = pol;
        LandUse = lu;
        Biodiversity = bd;
    }

    // Copies the datatype fields into a PowerPlant Object
    public void _CopyTo(ref PowerPlant PP) {
        // Sanity check 
        if(PP == null) {
            throw new ArgumentException("Invalid PowerPlant was given!");
        }

        // Copy in the public fields
        PP.BuildCost = BuildCost;
        PP.BuildTime = BuildTime;
        PP.LifeCycle = LifeCycle;
        PP.LandUse = LandUse;
        PP.BiodiversityImpact = Biodiversity;

        // Copy in the private fields
        PP._UdpatePowerPlantFields(
            true, 
            Pollution,
            ProductionCost,
            Capacity,
            Availability
        );
    }
}

// ==================== UI Info Datatype ===================

// Data structure for the information displayed in the info boxes
public struct InfoData {
	// === Field Numbers for each type ===
	public const int N_W_ENERGY_FIELDS = 2;
	public const int N_S_ENERGY_FIELDS = 2;
	public const int N_ENV_FIELDS = 4;
	public const int N_SUPPORT_FIELDS = 2;
	public const int N_MONEY_FIELDS = 5;

	// === Energy metrics ===
	public int W_EnergyDemand; // Energy demand for the winter season
	public int W_EnergySupply; // Energy supply for the winter season
	public int S_EnergyDemand; // Energy demand for the summer season
	public int S_EnergySupply; // Energy supply for the summer season

	// === Support metrics ===
	public int EnergyAffordability; // Used in the support bar
	public int EnvAesthetic; // Also used in the support bar

	// === Environment metrics ===
	public int LandUse; // Used in the environment bar
	public int Pollution; // Also for the environment bar
	public int Biodiversity; // For the environment bar

	// === Money Metrics ===
	public int Budget; // The amount of money you are generating this turn
	public int Production; // The amount of money used for production this turn
	public int Building; // The amount of money spent on building this turn
	public int Money; // The total amount of money you have
	public int Imports; // The total amount spent on imports last turn

	// Constructor for the Data
	public InfoData() {
		W_EnergyDemand = 0; 
		W_EnergySupply = 0; 
		S_EnergyDemand = 0; 
		S_EnergySupply = 0; 

		EnergyAffordability = 0; 
		EnvAesthetic = 0; 

		LandUse = 0;
		Pollution = 0;
		Biodiversity = 0; 

		Budget = 0;
		Production = 0;
		Building = 0;
		Money = 0; 
		Imports = 0;
	}
}