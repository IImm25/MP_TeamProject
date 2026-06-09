FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Backend.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y libglpk40 && \
    ln -s /usr/lib/x86_64-linux-gnu/libglpk.so.40 /usr/lib/x86_64-linux-gnu/libglpk.so
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "Backend.dll"]