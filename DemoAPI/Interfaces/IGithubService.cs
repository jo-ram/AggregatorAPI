using AggregatorAPI.Models;

namespace AggregatorAPI.Interfaces;

public interface IGithubService
{
    Task<List<GithubRepoInfo>> GetGithubReposAsync(string githubOrg);
}