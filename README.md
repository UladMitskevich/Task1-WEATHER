## Overview
⚠️ This project were implemented with a STRAIGHTFORWARD approach.

The task description is quite vague and lacks sufficient detail, leaving a lot open to interpretation. The code itself has scattered comments that only address a few aspects. I'd be happy to dive deeper, clarify these uncertainties, and explore all potential implementation options together.

## Goals
* Every minute, fetch data from: https://api.openweathermap.org/data/2.5/weather?q=London&appid=YOUR_API_KEY (you need to sign up to generate a new API Key) and store success/failure attempt log in the table and full payload in the blob.
* Create a GET API call to list all logs for the specific time period (from/to) (implement an Azure Function HTTP Trigger that queries the Azure Table Storage and returns logs within the specified time period
* Create a GET API call to fetch a payload from blob for the specific log entry (implement an Azure Function HTTP Trigger that retrieves the specific payload from Azure Blob Storage based on the log entry ID)

## Setup Instructions
Before you begin, ensure you have the following tools and accounts set up:
* OpenWeatherMap API Key: Sign up on OpenWeatherMap and generate an API key to access weather data.
* Azure Functions Core Tools: Install the Azure Functions Core Tools to run and test Azure Functions locally.
* Azurite: Install Azurite to emulate Azure Storage locally.

### Solution
1. **Clone the repository:**
   
2. **Configure the API Key:**
   - Open the `appsettings.json` file.
   - Add your OpenWeather API key.

3. **Other Configurations**
```
"Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    //Customizable Cron schedule for background WeatherFunction
    "WeatherFunctionCronTimer": "0 */1 * * * *",
    //flat structure for simplicity
    "OpenWeatherMapAPIEndpointWeather": "https://api.openweathermap.org/data/2.5/weather",
    "OpenWeatherMapAPIEndpointQParameter": "London",
    "OpenWeatherMapAPIKey": "{key}",
    "WeatherTableName": "WeatherRequests",
    "WeatherBlobContainerName": "weather-resuest-payload"
  }
```
4. **Run the service**

## Solution Structure
### ScheduledWeatherFetcherFunction
Scheduled Azure Fucntion gets weather each 1 minute
### GetWeatherRequestFunction
Route "v1/WeatherRequests".

Return all logs for the specific time period (from/to).
![image](https://github.com/user-attachments/assets/f32d6519-a694-43c7-924b-588725145edb)

### GetWeatherRequestPayloadFunction
Route "v1/WeatherRequests/{logEntryID}/Payload"

Retrieves the specific payload from Azure Blob Storage based on the log entry ID.

ℹ️ We use CombinedKey {city}_{datetime} London_2025-04-04T16:25:00Z
![image](https://github.com/user-attachments/assets/fa5cc866-f1bb-414b-b406-4f9337672cd9)

### Table
WeatherRequests
![image](https://github.com/user-attachments/assets/6730e232-b84d-42c5-904d-9d4fa277bdf9)

### Blob Storage
Payloads
![image](https://github.com/user-attachments/assets/e98357a9-fe33-446e-b88b-a5dc10752c74)

