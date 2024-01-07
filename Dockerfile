# Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /JamieBot
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /JamieBot
COPY --from=build /JamieBot/out .
ENV ASPNETCORE_URLS=http://*:80
CMD dotnet JamieBot.dll