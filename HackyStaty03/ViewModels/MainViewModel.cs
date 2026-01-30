using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HackyStaty03.Models;

namespace HackyStaty03.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {

        #region Properties

        #region Data Properties
        [ObservableProperty]
        private bool dataModified;

        [ObservableProperty]
        private string currentDataFile = Properties.HackyStatySetting.Default.LatestJson;

        [ObservableProperty]
        private string currentDataPath = AppDomain.CurrentDomain.BaseDirectory + @"DataStore";

        [ObservableProperty]
        private string logLocation = string.Empty; //Properties.Settings1.Default.LogLocation;

        [ObservableProperty]
        private string statusMessage = "Ready";

        #endregion

        #region Core Properties

        [ObservableProperty]
        ObservableCollection<Team> allTeams = new ObservableCollection<Team>();

        [ObservableProperty]
        ObservableCollection<Team> distinctTeams = new ObservableCollection<Team>();

        //[ObservableProperty]
        //Team? allStatsTeam = null;
        private Team? allStatsTeam = null;
        public Team? AllStatsTeam
        {
            get
            {
                return allStatsTeam;
            }
            set
            {
                if (allStatsTeam != value)
                {
                    allStatsTeam = value;
                    OnPropertyChanged(nameof(AllStatsTeam));
                }
            }
        }


        [ObservableProperty]
        private String newSeasonName = String.Empty;
        [ObservableProperty]
        private int newSeasonId = 0;

        [ObservableProperty]
        private String newLeagueName = String.Empty;
        [ObservableProperty]
        private int newLeagueId = 0;

        [ObservableProperty]
        private String newDivisionName = String.Empty;
        [ObservableProperty]
        private int newDivisionId = 0;

        [ObservableProperty]
        private String newTeamName = String.Empty;
        [ObservableProperty]
        private int newTeamId = 0;

        [ObservableProperty]
        private String modifiedSeasonName = String.Empty;

        [ObservableProperty]
        private String modifiedLeagueName = String.Empty;

        [ObservableProperty]
        private String modifiedDivisionName = String.Empty;

        [ObservableProperty]
        private String modifiedTeamName = String.Empty;


        //[ObservableProperty]
        private OWRoot? mainOWRoot = new();
        public OWRoot? MainOWRoot
        {
            get
            {
                return mainOWRoot;
            }
            set
            {
                if (mainOWRoot != value)
                {
                    mainOWRoot = value;
                    OnPropertyChanged(nameof(MainOWRoot));
                }
            }
        }

        private Season? selectedSeason = new();
        public Season? SelectedSeason
        {
            get
            {
                return selectedSeason;
            }
            set
            {
                if (selectedSeason != value)
                {
                    selectedSeason = value;
                    if (selectedSeason != null)
                    {
                        if (HasChildren(selectedSeason.Children))
                        {
                            SelectedLeague = selectedSeason.Children[0];
                        }
                        ModifiedSeasonName = selectedSeason.Name;
                    }
                    OnPropertyChanged(nameof(SelectedSeason));
                }
            }
        }


        private League? selectedLeague;
        public League? SelectedLeague
        {
            get
            {
                return selectedLeague;
            }
            set
            {
                if (selectedLeague != value)
                {
                    selectedLeague = value;
                    if (selectedLeague != null)
                    {
                        if (HasChildren(selectedLeague.Children))
                        {
                            SelectedDivision = selectedLeague.Children[0];
                        }
                        ModifiedLeagueName = selectedLeague.Name;
                    }
                    OnPropertyChanged(nameof(SelectedLeague));
                }
            }
        }


        private Division? selectedDivision;
        public Division? SelectedDivision
        {
            get
            {
                return selectedDivision;
            }
            set
            {
                if (selectedDivision != value)
                {
                    selectedDivision = value;
                    if (selectedDivision != null)
                    {
                        if (HasChildren(selectedDivision.Children))
                        {
                            SelectedTeam = selectedDivision.Children[0];
                        }
                        ModifiedDivisionName = selectedDivision.Name;
                    }
                    OnPropertyChanged(nameof(SelectedDivision));
                }
            }
        }

        private Team? selectedTeam;
        public Team? SelectedTeam
        {
            get
            {
                return selectedTeam;
            }
            set
            {
                if (selectedTeam != value)
                {
                    selectedTeam = value;
                    if (selectedTeam != null)
                    {
                        ModifiedTeamName = selectedTeam.Name;
                    }
                    OnPropertyChanged(nameof(SelectedTeam));

                }
            }
        }

        #endregion

        #region Random Properties

        private static bool HasChildren<T>(ObservableCollection<T>? children)
        {
            if ((children != null) && (children.Count > 0))
            {
                return true;
            }
            else { return false; }
        }

        private string printLocation = Properties.HackyStatySetting.Default.PrintLocation;
        public string PrintLocation
        {
            get
            {
                return printLocation;
            }
            set
            {
                if (printLocation != value)
                {
                    printLocation = value;
                    Properties.HackyStatySetting.Default.PrintLocation = value;
                    Properties.HackyStatySetting.Default.Save();
                    OnPropertyChanged(nameof(PrintLocation));
                }
            }
        }

        [ObservableProperty]
        private string currentStatisticURL = string.Empty;

        #endregion

        [ObservableProperty]
        private List<PlayerStats>? fullPlayerStats;

        [ObservableProperty]
        private ObservableCollection<PlayerStats>? playerStats = [];

        #endregion

        public MainViewModel()
        {
            //LoadEverything();
        }

        #region Startup/Shutdown

        private void GatherAllTeams()
        {
            //ObservableCollection<Team> distinctTeams = new ObservableCollection<Team>();
            if (MainOWRoot != null)
            {
                foreach (Season season in MainOWRoot.Children)
                {
                    season.ParentRoot = MainOWRoot;
                    foreach (League league in season.Children)
                    {
                        league.ParentSeason = season;
                        foreach (Division division in league.Children)
                        {
                            division.ParentLeague = league;
                            foreach (Team team in division.Children)
                            {
                                team.ParentDivision = division;
                                AllTeams.Add(team);
                            }
                        }
                    }
                }
            }

            DistinctTeams = new ObservableCollection<Team>(AllTeams.DistinctBy(p => p.OWHAId).OrderBy(x => x.Name));
            int teamcount = AllTeams.Count;
            int x = 0;
        }

        [RelayCommand]
        public void LoadEverything()
        {
            CurrentDataFile = Properties.HackyStatySetting.Default.LatestJson;
            if (File.Exists(CurrentDataFile))
            {
                System.Text.Json.JsonSerializerOptions jsonOptions = new()
                {
                    WriteIndented = true
                };

                using var reader = new System.IO.StreamReader(CurrentDataFile);
                while (!reader.EndOfStream)
                {
                    string? jSonString = reader.ReadLine();
                    if (!String.IsNullOrEmpty(jSonString))
                    {
                        MainOWRoot = System.Text.Json.JsonSerializer.Deserialize<OWRoot>(jSonString, jsonOptions);
                    }
                }

                GatherAllTeams();

                //Setup the combobox hierarchy
                if (MainOWRoot != null)
                {
                    if (HasChildren(children: MainOWRoot.Children))
                    {
                        SelectedSeason = MainOWRoot.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedSeasonGuid);
                        SelectedSeason ??= MainOWRoot.Children[0];
                        if (HasChildren(SelectedSeason.Children))
                        {
                            SelectedLeague = SelectedSeason.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedLeagueGuid);
                            SelectedLeague ??= SelectedSeason.Children[0];
                            if (HasChildren(SelectedLeague.Children))
                            {
                                SelectedDivision = SelectedLeague.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedDivisionGuid);
                                SelectedDivision ??= SelectedLeague.Children[0];
                                if (HasChildren(SelectedDivision.Children))
                                {
                                    SelectedTeam = SelectedDivision.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedTeamGuid);
                                    SelectedTeam ??= SelectedDivision.Children[0];
                                }
                                else
                                {
                                    SelectedTeam = null;
                                }
                            }
                            else
                            {
                                SelectedDivision = null;
                                SelectedTeam = null;
                            }
                        }
                        else
                        {
                            SelectedLeague = null;
                            SelectedDivision = null;
                            SelectedTeam = null;
                        }
                    }
                    else
                    {
                        SelectedSeason = null;
                        SelectedLeague = null;
                        SelectedDivision = null;
                        SelectedTeam = null;
                    }
                }
            }
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
            SaveClosingProperties();
            ClearDataFileContent();

            if (DataModified)
            {
                WriteData();
            }
            Properties.HackyStatySetting.Default.Save();
        }

        private void SaveClosingProperties()
        {
            if (SelectedSeason != null)
            {
                Properties.HackyStatySetting.Default.LastSelectedSeasonGuid = SelectedSeason.GuidId;
                if (SelectedLeague != null)
                {
                    Properties.HackyStatySetting.Default.LastSelectedLeagueGuid = SelectedLeague.GuidId;
                    if (SelectedDivision != null)
                    {
                        Properties.HackyStatySetting.Default.LastSelectedDivisionGuid = SelectedDivision.GuidId;
                        if (SelectedTeam != null)
                        {
                            Properties.HackyStatySetting.Default.LastSelectedTeamGuid = SelectedTeam.GuidId;
                        }
                    }
                }
            }
        }
        private static void ClearDataFileContent()
        {
            //FileStream fileStream = File.Open(jsonFileSeason, FileMode.Open);
            ///* 
            // * Set the length of filestream to 0 and flush it to the physical file.
            // *
            // * Flushing the stream is important because this ensures that
            // * the changes to the stream trickle down to the physical file.
            // * 
            // */
            //fileStream.SetLength(0);
            //fileStream.Close(); // This flushes the content, too.
        }

        private OWRoot SortMainOWRoot()
        {
            OWRoot mainOWRootSorted = new();

            if (MainOWRoot != null)
            {
                Season? currentSeason = null;
                League? currentLeague = null;
                Division? currentDivision = null;
                Team? currentTeam = null;

                foreach (Season season in MainOWRoot.Children.OrderBy(x => x.Name))
                {
                    currentSeason = new Season { Name = season.Name, GuidId = season.GuidId, OWHAId = season.OWHAId, Children = [] };
                    foreach (League league in season.Children.OrderByDescending(x => x.Name))
                    {
                        currentLeague = new League { Name = league.Name, GuidId = league.GuidId, OWHAId = league.OWHAId, Children = [] };
                        currentSeason.Children.Add(currentLeague);

                        foreach (Division division in league.Children.OrderByDescending(x => x.Name))
                        {
                            currentDivision = new Division { Name = division.Name, GuidId = division.GuidId, OWHAId = division.OWHAId, Children = [] };
                            currentLeague.Children.Add(currentDivision);

                            foreach (Team team in division.Children.OrderBy(x => x.Name))
                            {
                                currentTeam = new Team { Name = team.Name, GuidId = team.GuidId, OWHAId = team.OWHAId };
                                currentDivision.Children.Add(currentTeam);
                            }
                        }
                    }
                    mainOWRootSorted.Children.Add(currentSeason);
                }
            }
            return mainOWRootSorted;
        }

        public void WriteData()
        {
            OWRoot sortedOWRoot = SortMainOWRoot();
            CurrentDataFile = Path.Combine(CurrentDataPath, $"{DateTime.Now:yyyyMMddHHmmssffff}_seasons.json");
            Properties.HackyStatySetting.Default.LatestJson = CurrentDataFile;

            using System.IO.StreamWriter file5 = new(CurrentDataFile, true);
            string owRoot = System.Text.Json.JsonSerializer.Serialize(sortedOWRoot);
            file5.WriteLine(owRoot);
        }

        [RelayCommand]
        public void Save2DataStore()
        {
            WriteData();
            Properties.HackyStatySetting.Default.Save();
        }

        #endregion

        #region Statistics
        [RelayCommand]
        public void ClearStatistics()
        {
            PlayerStats?.Clear();
        }

        [RelayCommand]
        public async Task PrintStatistics()
        {
            string dateGuid = DateTime.Now.ToString("yyyy.MM.dd");

            StatusMessage = "Printing...";
            await Task.Run(() =>
            {
                //if (IsAllSelected())
                //{
                if ((PlayerStats != null && PlayerStats.Count > 0))
                {
                    using System.IO.StreamWriter file4 = new(path: $"{PrintLocation}{Path.DirectorySeparatorChar}{PlayerStats?.FirstOrDefault()?.TeamName}_{dateGuid}.csv", append: true);
                    file4.WriteLine();
                    file4.WriteLine($"{SelectedSeason?.Name} - {SelectedLeague?.Name} - {SelectedDivision?.Name} - {SelectedTeam?.Name}");
                    file4.WriteLine(value: PlayerStats?.FirstOrDefault()?.TeamName);
                    file4.WriteLine();
                    file4.WriteLine("#" + "," + "Name" + "," + "GP" + "," + "G" + "," + "A" + "," + "PTS" + "," + "PIM");
                    file4.WriteLine();

                    foreach (PlayerStats playerStats in PlayerStats)
                    {
                        file4.WriteLine(playerStats.jersey + "," + playerStats.fname + " " + playerStats.lname + "," + playerStats.GP + "," + playerStats.G + "," + playerStats.A + "," + playerStats.PTS + "," + playerStats.PIMd);
                    }

                }
                else
                {
                    StatusMessage = "Player Stats is null or empty.";
                }
            });
            StatusMessage = "Ready";


        }

        private async Task<ObservableCollection<PlayerStats>>? HitAPI(Season season, League league, Division division, Team team)
        {
            ObservableCollection<PlayerStats>? partialPlayerStats = new ObservableCollection<PlayerStats>();
            partialPlayerStats = [];

            using var httpClient = new System.Net.Http.HttpClient();
            //var json = await httpClient.GetStringAsync(@"https://api.rampinteractive.com/players/getplayerstats/2787/8999/15688/0/193569");

            string daIds = $"{season.OWHAId}/{league.OWHAId}/{division.OWHAId}/0/{team.OWHAId}";

            string link = $"https://api.rampinteractive.com/players/getplayerstats/{daIds}";

            var json = await httpClient.GetStringAsync(link);

            System.Text.Json.Nodes.JsonNode parsedJSon = System.Text.Json.Nodes.JsonNode.Parse(json);
            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            System.Text.Json.Nodes.JsonArray jsonArray = parsedJSon.AsArray();

            if (jsonArray.Count > 0)
            {
                string teamName = jsonArray[1]!["TeamName"].ToString();

                string teamNameNoSpace = teamName.Replace(" ", "");

                int customRank = 1;
                foreach (System.Text.Json.Nodes.JsonNode? jsonItem in jsonArray)
                {
                    PlayerStats newPlayerStats = new()
                    {
                        TeamName = teamNameNoSpace,
                        PID = (int)jsonItem!["PID"],
                        rank = customRank
                    };
                    if (jsonItem!["jersey"] == null)
                    {
                        newPlayerStats.jersey = 0;
                    }
                    else
                    {
                        newPlayerStats.jersey = (int)jsonItem!["jersey"];
                    }
                    newPlayerStats.fname = System.Web.HttpUtility.HtmlDecode(jsonItem!["fname"].ToString());
                    newPlayerStats.lname = System.Web.HttpUtility.HtmlDecode(jsonItem!["lname"].ToString());
                    newPlayerStats.GP = (int)jsonItem!["GP"];
                    newPlayerStats.G = (int)jsonItem!["G"];
                    newPlayerStats.A = (int)jsonItem!["A"];
                    newPlayerStats.PTS = (int)jsonItem!["PTS"];
                    var whatString = jsonItem!["PIM"].ToString();
                    newPlayerStats.PIMs = whatString;
                    newPlayerStats.PIMd = Convert.ToDouble(whatString);

                    partialPlayerStats.Add(newPlayerStats);

                    customRank++;

                    await Task.Delay(100);
                }


            }
            return partialPlayerStats;
        }

        [RelayCommand]
        public async Task FetchStatistics()
        {

            if ((SelectedSeason != null) && (SelectedLeague != null) && (SelectedDivision != null) && (SelectedTeam != null))
            {
                StatusMessage = "Fetching...";
                PlayerStats = [];

                using var httpClient = new System.Net.Http.HttpClient();
                //var json = await httpClient.GetStringAsync(@"https://api.rampinteractive.com/players/getplayerstats/2787/8999/15688/0/193569");

                string daIds = $"{SelectedSeason.OWHAId}/{SelectedLeague.OWHAId}/{SelectedDivision.OWHAId}/0/{SelectedTeam.OWHAId}";

                string link = $"https://api.rampinteractive.com/players/getplayerstats/{daIds}";

                var json = await httpClient.GetStringAsync(link);

                System.Text.Json.Nodes.JsonNode parsedJSon = System.Text.Json.Nodes.JsonNode.Parse(json);
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                System.Text.Json.Nodes.JsonArray jsonArray = parsedJSon.AsArray();

                if (jsonArray.Count > 0)
                {
                    string teamName = jsonArray[1]!["TeamName"].ToString();

                    string teamNameNoSpace = teamName.Replace(" ", "");

                    int customRank = 1;
                    foreach (System.Text.Json.Nodes.JsonNode? jsonItem in jsonArray)
                    {
                        PlayerStats newPlayerStats = new()
                        {
                            TeamName = teamNameNoSpace,
                            PID = (int)jsonItem!["PID"],
                            rank = customRank
                        };
                        if (jsonItem!["jersey"] == null)
                        {
                            newPlayerStats.jersey = 0;
                        }
                        else
                        {
                            newPlayerStats.jersey = (int)jsonItem!["jersey"];
                        }
                        newPlayerStats.fname = System.Web.HttpUtility.HtmlDecode(jsonItem!["fname"].ToString());
                        newPlayerStats.lname = System.Web.HttpUtility.HtmlDecode(jsonItem!["lname"].ToString());
                        newPlayerStats.GP = (int)jsonItem!["GP"];
                        newPlayerStats.G = (int)jsonItem!["G"];
                        newPlayerStats.A = (int)jsonItem!["A"];
                        newPlayerStats.PTS = (int)jsonItem!["PTS"];
                        var whatString = jsonItem!["PIM"].ToString();
                        newPlayerStats.PIMs = whatString;
                        newPlayerStats.PIMd = Convert.ToDouble(whatString);

                        PlayerStats.Add(newPlayerStats);

                        customRank++;

                        await Task.Delay(100);
                    }
                    StatusMessage = "Ready";
                }
                else
                {
                    StatusMessage = "Empty";
                }
                CurrentStatisticURL = link;
            }
            else
            {

                StatusMessage = "Every field needs to be populated.";
            }
        }


        [RelayCommand]
        public async void FetchAllStatistics()
        {
            
            int eye = 0;
            List<Team> listOfSameTeams = AllTeams.Where(x => x.OWHAId == AllStatsTeam.OWHAId).ToList();
            List<ObservableCollection<PlayerStats>> playerStatsCollection = new List<ObservableCollection<PlayerStats>>();
            foreach (Team team in listOfSameTeams)
            {
                Team currentTeam = team;
                Division currentDivision = currentTeam.ParentDivision;
                League currentLeague = currentDivision.ParentLeague;
                Season currentSeason = currentLeague.ParentSeason;

                ObservableCollection<PlayerStats> allstatst = await HitAPI(currentSeason, currentLeague, currentDivision, currentTeam);
                playerStatsCollection.Add(allstatst);

            }

            ObservableCollection<PlayerStats> CumulativePlayerStats = new ObservableCollection<PlayerStats>();
            foreach (ObservableCollection<PlayerStats> ps in playerStatsCollection)
            {
                foreach (PlayerStats player in ps)
                {
                    bool exists = CumulativePlayerStats.Any(x => x.PID == player.PID);
                    if (!exists) //add
                    {
                        CumulativePlayerStats.Add(player);
                    }
                    else
                    {
                        PlayerStats currentPlayersStats = CumulativePlayerStats.Where(x => x.PID == player.PID).FirstOrDefault();
                        currentPlayersStats.GP += player.GP;
                        currentPlayersStats.G += player.G;
                        currentPlayersStats.A += player.A;
                        currentPlayersStats.PTS += player.PTS;
                        currentPlayersStats.PIMd += player.PIMd;
                    }

                }
            }

            StatusMessage = "Fetching...";
            PlayerStats = [];
            int customRank = 1;

            ObservableCollection<PlayerStats> soretedPlayerStats = new ObservableCollection<PlayerStats>(CumulativePlayerStats.OrderByDescending(x => x.PTS));

            //foreach (PlayerStats newPlayerStats in CumulativePlayerStats.OrderByDescending(x=>x.PTS).OrderByDescending(y=>y.G).OrderBy(z=>z.PIMd).OrderBy(t=>t.jersey))
            foreach (PlayerStats newPlayerStats in soretedPlayerStats)
            {
                newPlayerStats.rank = customRank;
                PlayerStats.Add(newPlayerStats);
                customRank++;

                await Task.Delay(100);
            }
            

            

            StatusMessage = "Ready";

            int x = 0;
        }
        #endregion

        #region Random

        [RelayCommand]
        public void CopyStatisticUrl()
        {
            System.Windows.Clipboard.SetText(CurrentStatisticURL);
        }

        private bool IsAllSelected()
        {
            return ((SelectedSeason != null) && (SelectedLeague != null) && (SelectedDivision != null) && (SelectedTeam != null));
        }

        #endregion

        #region Core Functions

        [RelayCommand]
        public void AddSeason()
        {
            if (SeasonAdditionCheck())
            {
                StatusMessage = "Adding Season...";
                Season newSeason = new() { Name = NewSeasonName, OWHAId = NewSeasonId, GuidId = Guid.NewGuid() };
                MainOWRoot.Children.Add(newSeason);
                SelectedSeason = newSeason;
                DataModified = true;
                ClearSeasonFields();
                StatusMessage = "Ready";
            }
            else
            {
                //Status messages will bubble up 
            }
        }

        private bool SeasonAdditionCheck()
        {
            bool good2Go = false;

            if (MainOWRoot != null)
            {
                if (!String.IsNullOrWhiteSpace(NewSeasonName) && (NewSeasonId > 0))
                {
                    if (HasChildren(MainOWRoot.Children))
                    {
                        if (MainOWRoot.Children.Where(x => x.OWHAId == NewSeasonId).Any())
                        {
                            if (System.Windows.MessageBox.Show($"You already have a season with this ID.  Continue?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                good2Go = true;
                            }
                            else { StatusMessage = "Addition cancelled."; }
                        }
                        else
                        {
                            good2Go = true;
                        }
                    }
                    else
                    {
                        good2Go = true;
                    }
                }
                else { StatusMessage = "Invalid parameters."; }
            }
            else { StatusMessage = "A root must exist."; }

            return good2Go;
        }

        [RelayCommand]
        public void AddLeague()
        {
            if (LeagueAdditionCheck())
            {
                StatusMessage = "Adding League...";
                League newLeague = new() { Name = NewLeagueName, OWHAId = NewLeagueId, GuidId = Guid.NewGuid() };
                SelectedSeason.Children.Add(newLeague);
                SelectedLeague = newLeague;
                DataModified = true;
                ClearLeagueFields();
                StatusMessage = "Ready";
            }
            else
            {
                //Status messages will bubble up 
            }
        }
        private bool LeagueAdditionCheck()
        {
            bool good2Go = false;

            if (SelectedSeason != null)
            {
                if (!String.IsNullOrWhiteSpace(NewLeagueName) && (NewLeagueId > 0))
                {
                    if (HasChildren(SelectedSeason.Children))
                    {
                        if (SelectedSeason.Children.Where(x => x.OWHAId == NewLeagueId).Any())
                        {
                            if (System.Windows.MessageBox.Show($"You already have a league with this ID.  Continue?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                good2Go = true;
                            }
                            else { StatusMessage = "Addition cancelled."; }
                        }
                        else
                        {
                            good2Go = true;
                        }
                    }
                    else
                    {
                        good2Go = true;
                    }
                }
                else { StatusMessage = "Invalid parameters."; }
            }
            else { StatusMessage = "A season must be selected."; }

            return good2Go;
        }


        [RelayCommand]
        public void AddDivision()
        {
            if (DivisionAdditionCheck())
            {
                StatusMessage = "Adding Division...";
                Division newDivision = new() { Name = NewDivisionName, OWHAId = NewDivisionId, GuidId = Guid.NewGuid() };
                SelectedLeague.Children.Add(newDivision);
                SelectedDivision = newDivision;
                DataModified = true;
                ClearDivisionFields();
                StatusMessage = "Ready";
            }
            else
            {
                //Status messages will bubble up 
            }

        }

        private bool DivisionAdditionCheck()
        {
            bool good2Go = false;

            if (SelectedLeague != null)
            {
                if (!String.IsNullOrWhiteSpace(NewDivisionName) && (NewDivisionId > 0))
                {
                    if (HasChildren(SelectedLeague.Children))
                    {
                        if (SelectedLeague.Children.Where(x => x.OWHAId == NewDivisionId).Any())
                        {
                            if (System.Windows.MessageBox.Show($"You already have a division with this ID.  Continue?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                good2Go = true;
                            }
                            else { StatusMessage = "Addition cancelled."; }
                        }
                        else
                        {
                            good2Go = true;
                        }
                    }
                    else
                    {
                        good2Go = true;
                    }
                }
                else { StatusMessage = "Invalid parameters."; }
            }
            else { StatusMessage = "A league must be selected."; }

            return good2Go;
        }


        [RelayCommand]
        public void AddTeam()
        {
            if (TeamAdditionCheck())
            {
                StatusMessage = "Adding Division...";
                Team newTeam = new() { Name = NewTeamName, OWHAId = NewTeamId, GuidId = Guid.NewGuid() };
                SelectedDivision.Children.Add(newTeam);
                SelectedTeam = newTeam;
                DataModified = true;
                ClearTeamFields();
                StatusMessage = "Ready";
            }
            else
            {
                //Status messages will bubble up 
            }
        }

        private bool TeamAdditionCheck()
        {
            bool good2Go = false;

            if (SelectedDivision != null)
            {
                if (!String.IsNullOrWhiteSpace(NewTeamName) && (NewTeamId > 0))
                {
                    if (HasChildren(SelectedDivision.Children))
                    {
                        if (SelectedDivision.Children.Where(x => x.OWHAId == NewTeamId).Any())
                        {
                            if (System.Windows.MessageBox.Show($"You already have a team with this ID.  Continue?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                good2Go = true;
                            }
                            else { StatusMessage = "Addition cancelled."; }
                        }
                        else
                        {
                            good2Go = true;
                        }
                    }
                    else
                    {
                        good2Go = true;
                    }
                }
                else { StatusMessage = "Invalid parameters."; }
            }
            else { StatusMessage = "A division must be selected."; }

            return good2Go;
        }

        [RelayCommand]
        public void RenameSeason()
        {
            if (SelectedSeason != null)
            {
                StatusMessage = "Renaming Season...";
                SelectedSeason.Name = ModifiedSeasonName;
                DataModified = true;
                StatusMessage = "Ready";
            }
        }

        [RelayCommand]
        public void RenameLeague()
        {
            if (SelectedLeague != null)
            {
                StatusMessage = "Renaming League...";
                SelectedLeague.Name = ModifiedLeagueName;
                ModifiedLeagueName = String.Empty;
                DataModified = true;
                StatusMessage = "Ready";
            }
        }

        [RelayCommand]
        public void RenameDivision()
        {
            if (SelectedDivision != null)
            {
                StatusMessage = "Renaming Division...";
                SelectedDivision.Name = ModifiedDivisionName;
                DataModified = true;
                StatusMessage = "Ready";
            }
        }

        [RelayCommand]
        public void RenameTeam()
        {
            if (SelectedTeam != null)
            {
                StatusMessage = "Renaming Team...";
                SelectedTeam.Name = ModifiedTeamName;
                DataModified = true;
                StatusMessage = "Ready";
            }
        }


        [RelayCommand]
        public void DeleteSeason()
        {
            if (SelectedSeason != null)
            {
                Season? season2Rmove = MainOWRoot.Children.FirstOrDefault(x => x.GuidId == SelectedSeason.GuidId);
                if (season2Rmove != null)
                {
                    if (System.Windows.MessageBox.Show($"Are you sure? This action will remove all children.", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        StatusMessage = "Deleting Season...";
                        MainOWRoot.Children.Remove(season2Rmove);
                        SelectedLeague = null;
                        SelectedDivision = null;
                        SelectedTeam = null;
                        DataModified = true;
                        StatusMessage = "Ready";
                    }
                }
                else { StatusMessage = "Nothing to delete."; }
            }
            else { StatusMessage = "Nothing to delete."; }
        }

        [RelayCommand]
        public void DeleteLeague()
        {
            if (SelectedLeague != null)
            {
                if (System.Windows.MessageBox.Show($"Are you sure? This action will remove all children.", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting League...";
                    League league2Remove = SelectedSeason.Children.FirstOrDefault(x => x.GuidId == SelectedLeague.GuidId);
                    SelectedSeason.Children.Remove(league2Remove);
                    SelectedDivision = null;
                    SelectedTeam = null;
                    DataModified = true;
                    StatusMessage = "Ready";
                }
            }
            else { StatusMessage = "Nothing to delete."; }
        }

        [RelayCommand]
        public void DeleteDivision()
        {
            if (SelectedDivision != null)
            {
                if (System.Windows.MessageBox.Show($"Are you sure? This action will remove all children.", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting Division...";
                    Division division2Remove = SelectedLeague.Children.Where(x => x.GuidId == SelectedDivision.GuidId).FirstOrDefault();
                    SelectedLeague.Children.Remove(division2Remove);
                    SelectedTeam = null;
                    DataModified = true;
                    StatusMessage = "Ready";
                }
            }
            else { StatusMessage = "Nothing to delete."; }
        }

        [RelayCommand]
        public void DeleteTeam()
        {
            if (SelectedTeam != null)
            {
                if (System.Windows.MessageBox.Show($"Are you sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting Team...";
                    Team? team2Remove = SelectedDivision.Children.FirstOrDefault(x => x.GuidId == SelectedTeam.GuidId);
                    SelectedDivision.Children.Remove(team2Remove);
                    DataModified = true;
                    StatusMessage = "Ready";
                }
            }
            else { StatusMessage = "Nothing to delete."; }
        }

        [RelayCommand]
        public void ClearSeasonFields()
        {
            NewSeasonName = String.Empty;
            NewSeasonId = 0;
        }

        [RelayCommand]
        public void ClearLeagueFields()
        {
            NewLeagueName = String.Empty;
            NewLeagueId = 0;
        }

        [RelayCommand]
        public void ClearDivisionFields()
        {
            NewDivisionName = String.Empty;
            NewDivisionId = 0;
        }

        [RelayCommand]
        public void ClearTeamFields()
        {
            NewTeamName = String.Empty;
            NewTeamId = 0;
        }

        #endregion

        #region "OI Functions"

        [RelayCommand]
        public void SetFile(object obj)
        {
            bool exit = false;
            Microsoft.Win32.OpenFileDialog openFD = new();

            switch (obj.ToString())
            {
                case "CurrentDataFile":
                    openFD.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                    if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(CurrentDataFile)))
                    {
                        openFD.InitialDirectory = System.IO.Path.GetDirectoryName(CurrentDataFile);
                    }
                    break;
                case "FileOne":
                    openFD.Filter = "db files (*.db)|*.db|All files (*.*)|*.*";
                    //if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(FileOne)))
                    //{
                    //    openFD.InitialDirectory = System.IO.Path.GetDirectoryName(FileOne);
                    //}
                    break;
                default:
                    //StatusMessage = "Save Incomplete.";
                    exit = true;
                    break;
            }
            if (!exit)
            {
                if (openFD.ShowDialog() == true)
                {
                    string pathName = openFD.FileName;
                    switch (obj.ToString())
                    {
                        case "CurrentDataFile":
                            CurrentDataFile = pathName;
                            Properties.HackyStatySetting.Default.LatestJson = CurrentDataFile;
                            Properties.HackyStatySetting.Default.Save();
                            break;
                        case "FileOne":
                            //FileOne = pathName;
                            break;
                        default:
                            //StatusMessage = "Save Incomplete.";
                            break;
                    }
                }
            }
        }

        [RelayCommand]
        public void SetFolder(object obj)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new();
            switch (obj.ToString())
            {
                case "PrintLocation":
                    if (LogLocation != string.Empty)
                    {
                        folderBrowserDialog.SelectedPath = LogLocation;
                    }
                    break;
                default:
                    break;
            }
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //string pathName = openFD.FileName;
                string pathName = folderBrowserDialog.SelectedPath;
                switch (obj.ToString())
                {
                    case "PrintLocation":
                        PrintLocation = pathName;
                        break;
                    default:
                        StatusMessage = "Log/Save Incomplete.";
                        break;
                }
            }

        }

        [RelayCommand]
        public void OpenWindowsExplorer(object obj)
        {
            //bool exit = false;
            switch (obj.ToString())
            {
                case "OpenJSon":
                    OpenWE(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"Data"));
                    break;
                case "PrintLocation":
                    OpenWE(PrintLocation);
                    break;
                case "CurrentDataPath":
                    OpenWE(CurrentDataPath);
                    break;
                default:
                    StatusMessage = "Open Unsuccessful.";
                    break;
            }

        }

        private void OpenWE(string location)
        {
            if (Directory.Exists(location))
            {
                Process.Start("explorer.exe", location);
            }
            else
            {
                StatusMessage = location + " does not exists.";
            }
        }

        #endregion
    }
}
