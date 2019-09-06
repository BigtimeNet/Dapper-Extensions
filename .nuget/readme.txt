This project uses dotnet pack on build to create the nuget package based on the csproj file.

To publish a new nuget package:

1) Install nuget.exe
2) Run the authentication script in this directory (will enable you to push to our private feed)
3) Navigate to our Azure Devops page > Artifacts > Bigtime Feed > Overview > Click Connect to Feed. 
4) Enter the two commands underneath Push Packages using Nuget.exe (not included here for privacy).