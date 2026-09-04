FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TicketBackend.csproj", "./"]
RUN dotnet restore "TicketBackend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TicketBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TicketBackend.csproj" -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketBackend.dll"]
