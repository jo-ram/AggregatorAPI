using Aggregator.Service.Models;
using AggregatorAPI.Models;

namespace AggregatorAPI.Interfaces;

public interface IGithubService
{
    Task<Result<List<GithubRepoInfo>>> GetGithubReposAsync(string githubOrg);
}