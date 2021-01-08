# Pseudo Markets Portfolio Performance Service API

A .NET Core Web API for calculating Pseudo Markets user portfolio performance

# Requirements
* Pseudo Markets instance 
* MS SQL Server 2017+
* Mongo DB 4.x

# Performance Data
The service currently performs the following calculations:
* Account level unrealized gain or loss (percentage and numerical)
* Position level unrealized gain or loss (percentage and numerical)
* Total current invested value

Support for realized gain loss, time weighted return, and more will be coming soon.

# Usage
This API is designed to be used internally, and has two primary endpoints:

GET: /api/DataLoader/LoadPerformanceData - Run portfolio performance calculations on all accounts

GET: /api/PerformanceReport/GetPerformanceReport/{accountId}/{reportDate} - Fetch performance data for an account on a regular market day
 
NTLM or Basic Auth through IIS is recommended to secure the API from external use. 

(c) 2019 - 2020 Pseudo Markets
