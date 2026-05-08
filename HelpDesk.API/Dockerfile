FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src
COPY . .
RUN dotnet restore HelpDesk.slnx
EXPOSE 8080
CMD ["dotnet", "run", "--project", "HelpDesk.API/HelpDesk.API.csproj", "--urls", "http://0.0.0.0:8080"]
