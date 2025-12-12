using MunicipalityApp.Models;
using MunicipalityApp.DataStructures;
using MunicipalityApp.Helpers;
using System.Text.Json;

namespace MunicipalityApp.Services
{
    public class IssueService
    {
        private readonly string _filePath;
        private readonly IssueDoublyLinkedList issueList = new IssueDoublyLinkedList();

        public IssueService()
        {
            _filePath = AppDataHelper.GetFilePath("issues.json");
            AppDataHelper.EnsureJsonFile("issues.json");
            LoadIssuesFromFile();
        }

        private void LoadIssuesFromFile()
        {
            if (!File.Exists(_filePath)) return;

            var json = File.ReadAllText(_filePath);
            var issues = JsonSerializer.Deserialize<List<Issue>>(json);
            if (issues == null) return;

            foreach (var issue in issues)
                issueList.AddIssue(issue);
        }

        private void SaveIssuesToFile()
        {
            var issues = issueList.GetAllIssues();
            var json = JsonSerializer.Serialize(issues, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public void AddIssue(Issue issue)
        {
            issueList.AddIssue(issue);
            SaveIssuesToFile();
        }

        public List<Issue> GetAllIssues() => issueList.GetAllIssues();

        public bool UpdateIssueProgress(Guid issueId, string newProgress)
        {
            var issue = issueList.GetAllIssues().FirstOrDefault(i => i.Id == issueId);
            if (issue != null)
            {
                issue.Progress = newProgress;
                SaveIssuesToFile();
                return true;
            }
            return false;
        }

        public List<(string Username, int IssueCount, List<Issue> Issues)> GetLeaderboard()
        {
            return issueList.GetAllIssues()
                .GroupBy(i => i.Username)
                .Select(g => (
                    Username: g.Key,
                    IssueCount: g.Count(),
                    Issues: g.OrderByDescending(i => i.Timestamp).ToList()
                ))
                .OrderByDescending(x => x.IssueCount)
                .ToList();
        }
    }
}
