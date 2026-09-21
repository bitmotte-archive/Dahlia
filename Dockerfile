FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS prod
WORKDIR /app
COPY --from=builder /app/out .
COPY --from=builder /app/data ./data
ENTRYPOINT ["dotnet", "Dahlia.dll"]