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
        private String newSeasonName;
        [ObservableProperty]
        private int newSeasonId;

        [ObservableProperty]
        private String newLeagueName;
        [ObservableProperty]
        private int newLeagueId;

        [ObservableProperty]
        private String newDivisionName;
        [ObservableProperty]
        private int newDivisionId;

        [ObservableProperty]
        private String newTeamName;
        [ObservableProperty]
        private int newTeamId;

        [ObservableProperty]
        private String modifiedSeasonName;

        [ObservableProperty]
        private String modifiedLeagueName;

        [ObservableProperty]
        private String modifiedDivisionName;

        [ObservableProperty]
        private String modifiedTeamName;


        [ObservableProperty]
        private OWRoot mainOWRoot = new OWRoot();
        //public OWRoot MainOWRoot
        //{
        //    get
        //    {
        //        return mainOWRoot;
        //    }
        //    set
        //    {
        //        if (mainOWRoot != value)
        //        {
        //            mainOWRoot = value;
        //            OnPropertyChanged(nameof(MainOWRoot));
        //        }
        //    }
        //}

        private Season selectedSeason;
        public Season SelectedSeason
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


        private League selectedLeague;
        public League SelectedLeague
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


        private Division selectedDivision;
        public Division SelectedDivision
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

        private Team selectedTeam;
        public Team SelectedTeam
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


        private bool HasChildren<T>(ObservableCollection<T>? children)
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
        private string currentStatisticURL;

        #endregion

        [ObservableProperty]
        private List<PlayerStats> fullPlayerStats;

        [ObservableProperty]
        private ObservableCollection<PlayerStats> playerStats;

        #endregion

        public MainViewModel()
        {
            //LoadEverything();
        }

        #region Startup/Shutdown
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

        [RelayCommand]
        public void LoadEverything()
        {
            CurrentDataFile = Properties.HackyStatySetting.Default.LatestJson;
            if (File.Exists(CurrentDataFile))
            {
                System.Text.Json.JsonSerializerOptions jsonOptions = new System.Text.Json.JsonSerializerOptions();
                jsonOptions.WriteIndented = true;

                using (var reader = new System.IO.StreamReader(CurrentDataFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string? jSonString = reader.ReadLine();
                        MainOWRoot = System.Text.Json.JsonSerializer.Deserialize<OWRoot>(jSonString, jsonOptions);
                    }

                    if (HasChildren(MainOWRoot.Children))
                    {
                        SelectedSeason = MainOWRoot.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedSeasonGuid);
                        if (SelectedSeason == null)
                        {
                            SelectedSeason = MainOWRoot.Children[0];
                        }
                        if (HasChildren(SelectedSeason.Children))
                        {
                            SelectedLeague = SelectedSeason.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedLeagueGuid);
                            if (SelectedLeague == null)
                            {
                                SelectedLeague = SelectedSeason.Children[0];
                            }
                            if (HasChildren(SelectedLeague.Children))
                            {
                                SelectedDivision = SelectedLeague.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedDivisionGuid);
                                if (SelectedDivision == null)
                                {
                                    SelectedDivision = SelectedLeague.Children[0];
                                }
                                if (HasChildren(SelectedDivision.Children))
                                {
                                    SelectedTeam = SelectedDivision.Children.FirstOrDefault(x => x.GuidId == Properties.HackyStatySetting.Default.LastSelectedTeamGuid);
                                    if (SelectedTeam == null)
                                    {
                                        SelectedTeam = SelectedDivision.Children[0];
                                    }
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
        private void ClearDataFileContent()
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
        public void WriteData()
        {
            CurrentDataFile = Path.Combine(CurrentDataPath, $"{DateTime.Now.ToString("yyyyMMddHHmmssffff")}_seasons.json");
            Properties.HackyStatySetting.Default.LatestJson = CurrentDataFile;

            using (System.IO.StreamWriter file5 = new System.IO.StreamWriter(CurrentDataFile, true))
            {
                string owRoot = System.Text.Json.JsonSerializer.Serialize(MainOWRoot);
                file5.WriteLine(owRoot);
            }
        }

        #endregion

        #region Statistics
        [RelayCommand]
        public void ClearStatistics()
        {
            if (PlayerStats != null)
            {
                PlayerStats.Clear();
            }
        }

        [RelayCommand]
        public async void PrintStatistics()
        {
            string dateGuid = DateTime.Now.ToString("yyyy.MM.dd");
            if ((PlayerStats != null && PlayerStats.Count > 0))
            {
                StatusMessage = "Printing...";
                await Task.Run(() =>
                {
                    using (System.IO.StreamWriter file4 = new System.IO.StreamWriter(PrintLocation + Path.DirectorySeparatorChar + PlayerStats.FirstOrDefault().TeamName + "_" + dateGuid + ".csv", true))
                    {
                        file4.WriteLine();
                        file4.WriteLine($"{SelectedSeason.Name} - {SelectedLeague.Name} - {SelectedDivision.Name} - {SelectedTeam.Name}");
                        file4.WriteLine(PlayerStats.FirstOrDefault().TeamName);
                        file4.WriteLine();
                        file4.WriteLine("#" + "," + "Name" + "," + "GP" + "," + "G" + "," + "A" + "," + "PTS" + "," + "PIM");
                        file4.WriteLine();

                        foreach (Models.PlayerStats playerStats in this.PlayerStats)
                        {
                            file4.WriteLine(playerStats.jersey + "," + playerStats.fname + " " + playerStats.lname + "," + playerStats.GP + "," + playerStats.G + "," + playerStats.A + "," + playerStats.PTS + "," + playerStats.PIMd);
                        }
                    }
                });
                StatusMessage = "Ready";
            }
            else
            {
                StatusMessage = "Player Stats is null or empty.";
            }
        }

        [RelayCommand]
        public async Task FetchStatistics()
        {
            if ((SelectedSeason != null) && (SelectedLeague != null) && (SelectedDivision != null) && (SelectedTeam != null))
            {
                StatusMessage = "Fetching...";
                PlayerStats = new ObservableCollection<PlayerStats>();

                using (var httpClient = new System.Net.Http.HttpClient())
                {
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
                        foreach (System.Text.Json.Nodes.JsonNode jsonItem in jsonArray)
                        {
                            PlayerStats newPlayerStats = new PlayerStats();

                            newPlayerStats.TeamName = teamNameNoSpace;

                            newPlayerStats.PID = (int)jsonItem!["PID"];

                            newPlayerStats.rank = customRank;
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
                        }
                        StatusMessage = "Ready";
                    }
                    else
                    {
                        StatusMessage = "Empty";
                    }
                    CurrentStatisticURL = link;

                }
            }
            else
            {

                StatusMessage = "Every field needs to be populated.";
            }
        }

        #endregion

        #region Random

        public void CopyStatisticUrl()
        {
            System.Windows.Clipboard.SetText(currentStatisticURL);
        }

        #endregion

        #region Core Functions

        [RelayCommand]
        public void AddSeason()
        {
            if (SeasonAdditionCheck())
            {
                StatusMessage = "Adding Season...";
                Season newSeason = new Season { Name = NewSeasonName, OWHAId = NewSeasonId, GuidId = Guid.NewGuid() };
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
                League newLeague = new League { Name = NewLeagueName, OWHAId = NewLeagueId, GuidId = Guid.NewGuid() };
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
                Division newDivision = new Division { Name = NewDivisionName, OWHAId = NewDivisionId, GuidId = Guid.NewGuid() };
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
                Team newTeam = new Team { Name = NewTeamName, OWHAId = NewTeamId, GuidId = Guid.NewGuid() };
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
                if (System.Windows.MessageBox.Show($"Are you sure? This action will remove all children.", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting Season...";
                    Season season2Rmove = MainOWRoot.Children.Where(x => x.GuidId == SelectedSeason.GuidId).FirstOrDefault();
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

        [RelayCommand]
        public void DeleteLeague()
        {
            if (SelectedLeague != null)
            {
                if (System.Windows.MessageBox.Show($"Are you sure? This action will remove all children.", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting League...";
                    League league2Remove = SelectedSeason.Children.Where(x => x.GuidId == SelectedLeague.GuidId).FirstOrDefault();
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
                    Team team2Remove = SelectedDivision.Children.Where(x => x.GuidId == SelectedTeam.GuidId).FirstOrDefault();
                    SelectedDivision.Children.Remove(team2Remove);
                    DataModified = true;
                    StatusMessage = "Ready";
                }
            }
            else { StatusMessage = "Nothing to delete."; }
        }


        public void ClearSeasonFields()
        {
            NewSeasonName = String.Empty;
            NewSeasonId = 0;
        }
        public void ClearLeagueFields()
        {
            NewLeagueName = String.Empty;
            NewLeagueId = 0;
        }
        public void ClearDivisionFields()
        {
            NewDivisionName = String.Empty;
            NewDivisionId = 0;
        }
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
            Microsoft.Win32.OpenFileDialog openFD = new Microsoft.Win32.OpenFileDialog();

            switch (obj.ToString())
            {
                case "CurrentDataFile":
                    openFD.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
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
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
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
