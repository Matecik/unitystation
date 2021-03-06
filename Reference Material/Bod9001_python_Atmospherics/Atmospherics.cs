using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atmospherics
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			Console.WriteLine("Press any key to continue . . . ");
			Console.ReadKey();
			//Initialization.JsonImportInitialization();
			Initialization.AtmosphericsInitialization();
			//AtmosphericsTime.Atmospherics();

			Console.WriteLine("Press any key to continue . . . ");
			Console.ReadKey();
		}


	}
	public static class Globals
	{

		public static List<int> TileRange = new List<int>();
		public static Dictionary<List<int>, Dictionary<string, float>> Air = new Dictionary<List<int>, Dictionary<string, float>>();
		public static Dictionary<List<int>, Dictionary<string, float>> AirMixes = new Dictionary<List<int>, Dictionary<string, float>>();
		public static Dictionary<List<int>, Dictionary<string, bool>> Airbools = new Dictionary<List<int>, Dictionary<string, bool>>();

		public static Dictionary<string, float> SpaceAir = new Dictionary<string, float>();
		public static Dictionary<string, float> SpaceMix = new Dictionary<string, float>();

		public static float GasConstant = new float();

		public static Dictionary<List<int>, List<List<int>>> DictionaryOfAdjacents = new Dictionary<List<int>, List<List<int>>>();

		public static HashSet<List<int>> odd_set = new HashSet<List<int>>();
		public static HashSet<List<int>> Even_set = new HashSet<List<int>>();
		public static HashSet<List<int>> TilesWithPlasmaSet = new HashSet<List<int>>();
		public static HashSet<List<int>> UpdateTileSet = new HashSet<List<int>>();
		public static HashSet<List<int>> EdgeTiles = new HashSet<List<int>>();


		public static Dictionary<List<int>, int> CheckCountDictionary = new Dictionary<List<int>, int>();
		public static Dictionary<List<int>, int> CheckCountDictionaryMoving = new Dictionary<List<int>, int>();

		public static Dictionary<string,float> HeatCapacityOfGases = new Dictionary<string,float>();
		public static Dictionary<string,float> MolarMassesOfGases = new Dictionary<string,float>();
		
		public static bool Lag = new bool();
		public static bool OddEven = new bool();
		

	}
	public static class Initialization
	{


		public static void AtmosphericsInitialization()
		{ 
            HeatCapacityInitialization();
			AirInitialization();
			JsonImportInitialization();
			WorseCaseUpdateSet();
			PitchPatch();
			MakingDictionaryOfAdjacents();
			SpaceInitialization();
		}
		static void HeatCapacityInitialization()
		{
			Globals.TileRange.Add(360);
			Globals.TileRange.Add(210);

			Globals.OddEven = false;
			Globals.Lag = false;
			Globals.HeatCapacityOfGases.Add("Oxygen", 0.659f);
			Globals.HeatCapacityOfGases.Add("Nitrogen", 0.743f);
			Globals.HeatCapacityOfGases.Add("Plasma", 0.8f);
			Globals.HeatCapacityOfGases.Add("Carbon Dioxide", 0.655f);
			
			Globals.MolarMassesOfGases.Add("Oxygen", 31.9988f);
			Globals.MolarMassesOfGases.Add("Nitrogen", 28.0134f);
			Globals.MolarMassesOfGases.Add("Plasma", 40f);
			Globals.MolarMassesOfGases.Add("Carbon Dioxide", 44.01f);



			Globals.GasConstant = 8.3144598f;
		}
		
		
		
		static void AirInitialization()
		{

            IEnumerable<int> NumberList1 = Enumerable.Range(0, (Globals.TileRange[0] + 1));
			IEnumerable<int> NumberList2 = Enumerable.Range(0, (Globals.TileRange[1] + 1));
			foreach (int Number1 in NumberList1)
			{
				foreach (int Number2 in NumberList2)
				{
					List<int> CoordinatesList = new List<int>()
					{Number1,Number2};
					Dictionary<string, float> ToApplyAir = new Dictionary<string, float>();					
					ToApplyAir.Add("Temperature", 293.15f);
					ToApplyAir.Add("Pressure", 101.325f);
					ToApplyAir.Add("Moles", 83.142422004453842459076923779184f);
					Globals.Air.Add(CoordinatesList, ToApplyAir);

            
					Dictionary<string, float> ToApplyToMixes = new Dictionary<string, float>();
					ToApplyToMixes.Add("Oxygen", 16.628484400890768491815384755837f);
					ToApplyToMixes.Add("Nitrogen", 66.513937603563073967261539023347f);
					Globals.AirMixes.Add(CoordinatesList, ToApplyToMixes);

					Dictionary<string, bool> ToApplyToStructuralbools = new Dictionary<string, bool>();
					ToApplyToStructuralbools["Obstructed"] = false;
					ToApplyToStructuralbools["Space"] = true;
					Globals.Airbools[CoordinatesList] = ToApplyToStructuralbools;
					if (Number1 > 2 & Number2 > 2)
					{
						List<int> erterf = new List<int>()
						{Number1,Number2};
						Console.WriteLine(Globals.Airbools[erterf]["Space"]);   //  As marked by this

					}

					//Console.WriteLine(Number1 + "yes" + Number2);
					//Console.WriteLine(Globals.Airbools[]);

				}

			}
			Globals.SpaceAir.Add("Temperature", 2.7f);
			Globals.SpaceAir.Add("Pressure", 0.000000316f);
			Globals.SpaceAir.Add("Moles", 0.000000000000281f);

			Globals.SpaceMix.Add("Oxygen", 0.000000000000281f);
		}
		static void JsonImportInitialization()
		{
			var json = System.IO.File.ReadAllText(@"I:\my_stuff\ss13 map\ss15\c#\Atmospherics\Atmospherics\BoxStationStripped.json");
			var wallsFloors = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(json);

			foreach (var walls in wallsFloors["Walls"])
			{
				Globals.Airbools[walls]["Obstructed"] = true;
			}
			foreach (var Floor in wallsFloors["Floor"])
			{
				Globals.Airbools[Floor]["Obstructed"] = true;
			}
		}
		
		
		
		
        static List<List<int>> GenerateAdjacentTiles(List<int> Tile)
		{
			

			List<List<int>> AdjacentTilesRelativeCoordinatesList = new List<List<int>>();

			List<int> temporaryList = new List<int>()
			{0,0};
			AdjacentTilesRelativeCoordinatesList.Add(temporaryList);

			List<int> temporaryList1 = new List<int>()
			{1,0};
			AdjacentTilesRelativeCoordinatesList.Add(temporaryList1);

			List<int> temporaryList2 = new List<int>()
			{0,1};
			AdjacentTilesRelativeCoordinatesList.Add(temporaryList2);

			List<int> temporaryList3 = new List<int>()
			{-1,0};
			AdjacentTilesRelativeCoordinatesList.Add(temporaryList3);

			List<int> temporaryList4 = new List<int>()
			{0,-1};
			AdjacentTilesRelativeCoordinatesList.Add(temporaryList4);

			List<List<int>> WorkedOutList = new List<List<int>>();
			foreach (List<int> TileOffset in AdjacentTilesRelativeCoordinatesList)
			{
				List<int> subList = new List<int>();

				int WorkedOutOffset1 = TileOffset[0] + Tile[0];
				subList.Add(WorkedOutOffset1);
				int WorkedOutOffset2 = TileOffset[1] + Tile[1];
				subList.Add(WorkedOutOffset2);



				if (!(subList[0] > Globals.TileRange[0] || subList[0] < 0 || subList[1] > Globals.TileRange[1] || subList[1] < 0))
				{
					WorkedOutList.Add(subList);
				}
			}

			//foreach (var sublist in WorkedOutList)
			//{
			//	foreach (var obj in sublist)
			//	{
			//		Console.WriteLine(obj);
			//	}
			//}
			return WorkedOutList;
		}
		
		
		
        static void MakingDictionaryOfAdjacents()
        {

            IEnumerable<int> NumberList1 = Enumerable.Range(1, (Globals.TileRange[0] + 1));
            IEnumerable<int> NumberList2 = Enumerable.Range(1, (Globals.TileRange[1] + 1));
            foreach (int Number1 in NumberList1)
            {
                foreach (int Number2 in NumberList2)
                {
                    List<int> CoordinatesList = new List<int>
                    {Number1,Number2};
					List<List<int>> Adjacents = GenerateAdjacentTiles(CoordinatesList);
					Globals.DictionaryOfAdjacents.Add(CoordinatesList, Adjacents);
                }
            }
			
			Console.WriteLine("MakingDictionaryOfAdjacents Done!");
        }
		
		
		
		static void WorseCaseUpdateSet()
		{
			IEnumerable<int> NumberList1 = Enumerable.Range(1, (Globals.TileRange[0] + 1));
            IEnumerable<int> NumberList2 = Enumerable.Range(1, (Globals.TileRange[1] + 1));
            foreach (int Number1 in NumberList1)
            {
				Console.WriteLine(Number1);
                foreach (int Number2 in NumberList2)
				{
					Console.WriteLine(Number2);
					List<int> CoordinatesList = new List<int>()
					{Number1,Number2};
					if (Globals.Airbools[CoordinatesList]["Obstructed"] == false)
					{
						Globals.UpdateTileSet.Add(CoordinatesList);
					}
				}
			}
			Console.WriteLine("WorseCaseUpdateSet Done!");
		}
		
		
		static void PitchPatch()
		{
			

			IEnumerable<int> NumberList1 = Enumerable.Range(1, (Globals.TileRange[0] + 1));
			IEnumerable<int> NumberList2 = Enumerable.Range(1, (Globals.TileRange[1] + 1));
            foreach (int Number1 in NumberList1)
            {
                foreach (int Number2 in NumberList2)
				{
					List<int> CoordinatesList = new List<int>
					{Number1,Number2};
					if (Number1 % 1 == 0)
					{
						if (Number2 % 1 == 0)
						{
							Globals.odd_set.Add(CoordinatesList);
						}
						else
						{
							Globals.Even_set.Add(CoordinatesList);
						}
					}
					else
					{
						if (Number2 % 1 == 0)
						{
							
							Globals.Even_set.Add(CoordinatesList);
						}
						else
						{
							Globals.odd_set.Add(CoordinatesList);
						}
					}

				}
			}
			Console.WriteLine("PitchPatch Done!");
		}
		
		
		
		static void MakingCheckCountDictionarys()
		{
			
			

            IEnumerable<int> NumberList1 = Enumerable.Range(1, (Globals.TileRange[0] + 1));
            IEnumerable<int> NumberList2 = Enumerable.Range(1, (Globals.TileRange[1] + 1));
            foreach (int Number1 in NumberList1)
            {
                foreach (int Number2 in NumberList2)
				{
					List<int> CoordinatesList = new List<int>
					{Number1,Number2};
					Globals.CheckCountDictionary.Add(CoordinatesList,0);
					Globals.CheckCountDictionaryMoving.Add(CoordinatesList,0);
				}
			}
			Console.WriteLine("MakingCheckCountDictionarys Done!");
		}
		
		
		
		static void SpaceInitialization()
		{
            IEnumerable<int> NumberList1 = Enumerable.Range(1, (Globals.TileRange[0] + 1));
			IEnumerable<int> NumberList2 = Enumerable.Range(1, (Globals.TileRange[1] + 1));
			foreach (int Number1 in NumberList1)
			{
				foreach (int Number2 in NumberList2)
				{
					List<int> CoordinatesList = new List<int>()
					{Number1,Number2};
					if (Globals.Airbools[CoordinatesList]["Space"] == true)
					{
						if (Globals.Airbools[CoordinatesList]["Obstructed"] == false)
						{
							Globals.Air[CoordinatesList] = new Dictionary<string, float>(Globals.SpaceAir);
							Globals.AirMixes[CoordinatesList] = new Dictionary<string, float>(Globals.SpaceMix);
						}
						else
						{
							Globals.Airbools[CoordinatesList]["Space"] = false;
						}
					}

				}
			
			}
		}
			
	}
	public static class AtmosphericsTime
	{
		
		
		
		
		static bool GasMoving(List<int> Tile)
		{
			List<List<int>> AdjacentTilesAndItself = Globals.DictionaryOfAdjacents[Tile];
			Dictionary<string, float> MixCalculationDictionary = new Dictionary<string, float>();
			Dictionary<string, float> JMCalculationDictionary = new Dictionary<string, float>();
			bool RemoveTile = new bool();
			RemoveTile = false;
			List<List<int>> TileWorkingOn = new List<List<int>>();
			List<string> keyToDelete = new List<string>();
			bool Decay = new bool();
			Decay = true;
			bool IsSpace = new bool();
			IsSpace = false;
			float MolesAll = new float();
			MolesAll = 0f;
			float Temperature = new float();
			Temperature = 0f;
			int Count = new int();
			Count = 0;
			float KeyMixWorkedOut = new float();
			float Pressure = new float();




			


			foreach (List<int> TileInWorkList in AdjacentTilesAndItself)
			{
				if (Globals.Airbools[TileInWorkList]["Space"] == true)
				{
					IsSpace = true;
					break;
				}
				if (Globals.Airbools[TileInWorkList]["Obstructed"] == false)
				{
					foreach (KeyValuePair<string, float> KeyValue in Globals.AirMixes[TileInWorkList])
					{
						float MixCalculationDictionarykey = new float();
						float JMCalculationDictionarykey = new float();
						if (MixCalculationDictionary.TryGetValue(KeyValue.Key, out MixCalculationDictionarykey))
						{}
						else
						{
							MixCalculationDictionarykey = 0;
						}
						if (JMCalculationDictionary.TryGetValue(KeyValue.Key, out JMCalculationDictionarykey))
						{}
						else
						{
							JMCalculationDictionarykey = 0;
						}
						
						MixCalculationDictionary[KeyValue.Key] = (KeyValue.Value + MixCalculationDictionarykey); //*************** 
						JMCalculationDictionary[KeyValue.Key] = ((Globals.Air[TileInWorkList]["Temperature"] * KeyValue.Value) * Globals.HeatCapacityOfGases[KeyValue.Key]) + JMCalculationDictionarykey; //***************
						if (KeyValue.Value < 0.000000000000001)
						{
							keyToDelete.Add(KeyValue.Key);
						}
					}
					if (0.01f < (Math.Abs(Globals.Air[Tile]["Pressure"] - Globals.Air[TileInWorkList]["Pressure"])))
					{
						Decay = false;
					}
					Count += 1;
					TileWorkingOn.Add(TileInWorkList);
				}
			}
			if (IsSpace == false)
			{
				foreach (KeyValuePair<string, float> KeyValue in MixCalculationDictionary)
				{
					KeyMixWorkedOut = (MixCalculationDictionary[KeyValue.Key] / Count);
					MolesAll += KeyMixWorkedOut;

					JMCalculationDictionary[KeyValue.Key] = (((JMCalculationDictionary[KeyValue.Key] / Globals.HeatCapacityOfGases[KeyValue.Key]) / KeyMixWorkedOut) / JMCalculationDictionary.Count);
					Globals.AirMixes[Tile][KeyValue.Key] = KeyMixWorkedOut;
					Temperature += (JMCalculationDictionary[KeyValue.Key] / Count);

					if (KeyValue.Key == "Plasma")
					{
						if (KeyValue.Value > 0.0f) // This needs tweaking to find what the minimum amount of plasma is needed For a reaction is
						{
							Globals.TilesWithPlasmaSet.Add(Tile);
						}
						//Globals.TilesWithPlasmaSet.Add(Tile)
					}
				}
				Pressure = (((MolesAll * Globals.GasConstant * Temperature) / 2) / 1000);

				foreach (string Key in keyToDelete)
				{
					try
					{
						Globals.AirMixes[Tile].Remove(Key);

					}
					catch (KeyNotFoundException){}
				}
				foreach (List<int> TileApplyingList in TileWorkingOn)
				{
					if (Globals.Airbools[TileApplyingList]["Space"] == false)
					{
						Globals.AirMixes[TileApplyingList] = new Dictionary<string, float>(Globals.AirMixes[Tile]);
						Globals.Air[TileApplyingList]["Temperature"] = Temperature;
						Globals.Air[TileApplyingList]["Moles"] = MolesAll;
						Globals.Air[TileApplyingList]["Pressure"] = Pressure;
					}
				}
			}
			else
			{
				foreach (List<int> TileApplyingList in TileWorkingOn)
				{
					Globals.AirMixes[TileApplyingList] = new Dictionary<string, float>(Globals.SpaceMix);

					Globals.Air[TileApplyingList]["Temperature"] = 2.7f;
					Globals.Air[TileApplyingList]["Moles"] = 0.000000000000281f;
					Globals.Air[TileApplyingList]["Pressure"] = 0.000000316f;
				}
			}
			if (Decay == true)
			{
				if (Globals.CheckCountDictionaryMoving[Tile] >= 3)
				{
					RemoveTile = true;
					Globals.CheckCountDictionaryMoving[Tile] = 0;
				}
				else
				{
					Globals.CheckCountDictionaryMoving[Tile] += 1;
				}
			}
			return RemoveTile;
		}
		
		
		
		
		static bool LagOvereLay(bool OddEven)
		{
			List<List<int>> TilesRemoveFromUpdate = new List<List<int>>();
			
			bool RemoveTile = new bool();
			foreach (List<int> TileCalculating in Globals.UpdateTileSet)
			{
				RemoveTile = false;
				if (Globals.Lag == true)
				{
					if (OddEven == true)
					{
						if (Globals.odd_set.Contains(TileCalculating))
						{
							RemoveTile = GasMoving(TileCalculating);
							if (RemoveTile == true)
							{
								TilesRemoveFromUpdate.Add(TileCalculating);
							}
						}
					}
					else
					{
						if (Globals.Even_set.Contains(TileCalculating))
						{
							RemoveTile = GasMoving(TileCalculating);
							if (RemoveTile == true)
							{
								TilesRemoveFromUpdate.Add(TileCalculating);
							}
						}
					}
				}
				else
				{
					RemoveTile = GasMoving(TileCalculating);
					if (RemoveTile == true)
					{
						TilesRemoveFromUpdate.Add(TileCalculating);
					}
				}
			}
			foreach (List<int> TileRemoveing in TilesRemoveFromUpdate)
			{
				Globals.EdgeTiles.Add(TileRemoveing);
				Globals.UpdateTileSet.Remove(TileRemoveing);
			}
			if (OddEven == true)
			{
				OddEven = false;
			}
			else
			{
				OddEven = true;
			}
		
			return OddEven;
		}
		
		
		
		static void DoTheEdge()
		{
			if (!Globals.EdgeTiles.Any())
			{
				return;
			}
			bool Decay = new bool();
			HashSet<List<int>> NewEdgeTiles = new HashSet<List<int>>();
			int CountForUpdateSet =  new int();
			foreach (List<int> TileCheckingList in Globals.EdgeTiles)
			{
				Tuple<int, int> TileChecking = Tuple.Create<int, int>(TileCheckingList[0], TileCheckingList[1]);
				List<List<int>> AdjacentTilesAndItself = new List<List<int>>(Globals.DictionaryOfAdjacents[TileCheckingList]);
				AdjacentTilesAndItself.RemoveAt(0);
				Decay = true;
				CountForUpdateSet = 0;
				foreach (List<int> AdjacentTileList in AdjacentTilesAndItself)
				{
					if (Globals.Airbools[AdjacentTileList]["Obstructed"] == false)
					{
						if (0.00001f < (Math.Abs(Globals.Air[AdjacentTileList]["Pressure"] - Globals.Air[TileCheckingList]["Pressure"])))
						{
							Decay = false;
								if (! Globals.UpdateTileSet.Contains(AdjacentTileList))
							{
								if  (! NewEdgeTiles.Contains(AdjacentTileList))
								{
									Globals.UpdateTileSet.Add(TileCheckingList);
									NewEdgeTiles.Add(AdjacentTileList);
								}
								else
								{
									Globals.UpdateTileSet.Add(TileCheckingList);
								}
							
							}
							else
							{
								CountForUpdateSet += 1;
							}
						}
					}
				}
				if (Decay == true)
				{
					if (Globals.CheckCountDictionary[TileCheckingList] >= 2)
					{
						bool Decayfnleall = new bool();
						Decayfnleall = true;
						foreach (List<int> AdjacentTile in AdjacentTilesAndItself)
						{
							if (Globals.UpdateTileSet.Contains(AdjacentTile))
							{
								Decayfnleall = false;
							}
						}
						if (Decayfnleall == true)
						{

							if (Globals.Airbools[TileCheckingList]["Obstructed"] == false)
							{
								NewEdgeTiles.Add(TileCheckingList);
							}
						}
						Globals.CheckCountDictionary[TileCheckingList] = 0;
						
					}
					else
					{
						Globals.CheckCountDictionary[TileCheckingList] += 1;
						NewEdgeTiles.Add(TileCheckingList);
					}
				}
				else
				{
					if(CountForUpdateSet > 1)
					{
						Globals.UpdateTileSet.Add(TileCheckingList);
					}
				}
			}
			Globals.EdgeTiles = NewEdgeTiles;
		}
		static void AirReactions()
		{
			HashSet<List<int>> TilesWithPlasmaSetCopy = new HashSet<List<int>>(Globals.TilesWithPlasmaSet);

			foreach (List<int> TilePlasma in Globals.TilesWithPlasmaSet)
			{
				float AirMixesyPlasmakey = new float();
			if (Globals.AirMixes[TilePlasma].TryGetValue("Plasma", out AirMixesyPlasmakey))
			{}
			else
			{
				AirMixesyPlasmakey = 0;
			}		
				if (AirMixesyPlasmakey > 0) 
				{
					if (Globals.Air[TilePlasma]["Temperature"] > 1643.15)
					{
						
						float AirMixesyOxygenkey = new float();
						if (Globals.AirMixes[TilePlasma].TryGetValue("Oxygen", out AirMixesyOxygenkey))
						{}
						else
						{
							AirMixesyOxygenkey = 0;
						}	
						float AirMixesyCarbonDioxidekey = new float();
						if (Globals.AirMixes[TilePlasma].TryGetValue("Carbon Dioxide", out AirMixesyCarbonDioxidekey))
						{}
						else
						{
							AirMixesyCarbonDioxidekey = 0;
						}	
						float TemperatureScale = new float(); 
						float TheOxygenMoles = AirMixesyOxygenkey;
						float TheCarbonDioxideMoles = new float();
						TheCarbonDioxideMoles = AirMixesyCarbonDioxidekey;
						if (TheOxygenMoles > 1)
						{
							TemperatureScale = 1;
						}
						else
						{
							TemperatureScale = (Globals.Air[TilePlasma]["Temperature"]-373)/(1643.15f-373);
						}
						if (TemperatureScale > 0)
						{
							float PlasmaBurnRate = new float();
							float ThePlasmaMoles = Globals.AirMixes[TilePlasma]["Plasma"];
							float OxygenBurnRate = (1.4f - TemperatureScale);
							if (TheOxygenMoles > ThePlasmaMoles * 10)
							{
								PlasmaBurnRate = ((ThePlasmaMoles * TemperatureScale) / 4);
							}
							else
							{
								PlasmaBurnRate = (TemperatureScale * (TheOxygenMoles / 10)) / 4;
							}
							if (PlasmaBurnRate > 0.03f)
							{
								float EnergyReleased = new float();
								float FuelBurnt = new float();
								float JM = new float();
								float J = new float();
								
								ThePlasmaMoles -= PlasmaBurnRate;
								TheOxygenMoles -= (PlasmaBurnRate * OxygenBurnRate);
								TheCarbonDioxideMoles += PlasmaBurnRate;
								
								EnergyReleased = (3000000 * PlasmaBurnRate);
								FuelBurnt = (PlasmaBurnRate)*(1+OxygenBurnRate);
								
								Globals.AirMixes[TilePlasma]["Oxygen"] = TheOxygenMoles;
								Globals.AirMixes[TilePlasma]["Plasma"] = ThePlasmaMoles;
								Globals.AirMixes[TilePlasma]["Carbon Dioxide"] = TheCarbonDioxideMoles;
								
								JM = ((Globals.Air[TilePlasma]["Temperature"] * TheCarbonDioxideMoles) * Globals.HeatCapacityOfGases["Carbon Dioxide"]);
								J = (Globals.MolarMassesOfGases["Carbon Dioxide"] * JM);
								J += EnergyReleased;
								JM = (J / Globals.MolarMassesOfGases["Carbon Dioxide"]);
								Globals.Air[TilePlasma]["Temperature"] =((JM / Globals.HeatCapacityOfGases["Carbon Dioxide"]) / TheCarbonDioxideMoles);
							}
						}
					}
				}
			}
		}
		
		
		public static void Atmospherics()
		{
			int count = 1;
			Stopwatch sw = new Stopwatch();
			Stopwatch swTick = new Stopwatch();
			sw.Start();
			bool OddEven = new bool();
			OddEven = true;
			while (count++ < 101)
			{
				swTick.Start(); 
				OddEven = LagOvereLay(OddEven);
				//Air_Reactions()
				DoTheEdge();
				swTick.Stop();
				float timeTaken = 0.001f * swTick.ElapsedMilliseconds;
				if (timeTaken  > 0.2f)
				{
					if (timeTaken  < 0.1f)
					{
						Globals.Lag = false;
					}
				}
				else
				{
					Globals.Lag = true;
				}
			}		
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
		}
		
	}
}