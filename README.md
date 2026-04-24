# CPSC 449 - Library Management System

## Installation Instructions
Install necessary package if needed:

`dotnet tool install --global dotnet-ef`

Update database:

`dotnet ef database update`

## How to run locally
Run the following command:

`dotnet watch run`

## Authentication Help
If using the Swagger UI, follow these steps to authenticate:
1. Make a POST request to /api/auth/login
2. Use email "test@email.com" and password "password" then send the request
3. In the response body, copy the full JWT key under "accessToken"
4. Click on the Authorize button located on the top right of the page
5. Paste in the JWT key and press 'Authorize'
6. All API methods will now be accessible 