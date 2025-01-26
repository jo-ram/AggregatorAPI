Endpoint

GET https://localhost:7140/api/aggregation/aggregated-data

Description

This endpoint retrieves aggregated data from multiple sources, including:

1.News data filtered by the search query.

2.Weather data for a specified city.

3.GitHub repository data for a specified organization.

The response contains data combined from all three sources.

Query Parameters


searchQueryParam    	The search query for fetching news articles (technology, sports etc)

city	                The city name for retrieving weather data.	New York, London

githubOrgRepo	        The name of the GitHub organization to fetch repository data for (dotnet, adobe, Netflix etc)

shortBy (optional)	  Determines the sorting order for news articles. Use asc for ascending or desc for descending.	asc, desc

filter	(optional)    Filter condition for the news articles	(author eq 'Lloyd Lee' or sourceName eq 'BBC News')


Response Format

The response is an aggregated object containing the following properties:

News	      List<Article>	    A list of news articles filtered and sorted based on the input parameters.

Weather	    WeatherInfo	      Weather information for the specified city.

GithubRepos	List<Repo>	      A list of repositories from the specified GitHub organization.


Nested Object Definitions

Article

Title: string - The title of the news article.
Description: string - A brief description of the article.
PublishedAt: string - The publication date of the article.
Url: string - The URL to the full article.

WeatherInfo
City: string - The city name.
Temperature: double - The current temperature in Celsius.
WeatherDescription: string - A brief description of the current weather.

Repo
Name: string - The name of the repository.
Description: string - A brief description of the repository.
LastUpdatedAt: string - The last updated date of the repository.
CreatedOn: string - The creation date of the repository.
Language: string - The programming language of the repository.


Example Request
URL:

https://localhost:7140/api/aggregation/aggregated-data?searchQueryParam=technology&city=London&githubOrgRepo=dotnet&shortBy=desc&filter=author eq ''


Example Response



-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Endpoint

GET https://localhost:7140/api/statistics/request-statistics

Description

This endpoint provides statistics for API requests across the application. It includes:

Total number of requests for each service.
Average response time for requests to each service.
Performance buckets for response times, categorized as:
Fast: Response time < 100 ms.
Average: Response time between 100 ms and 200 ms.
Slow: Response time > 200 ms.

Example Request
URL:

https://localhost:7140/api/statistics/request-statistics

Example Response
