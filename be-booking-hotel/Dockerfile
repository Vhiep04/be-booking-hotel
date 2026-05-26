# ─────────────────────────────────────────
# Stage 1: Build
# ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY be-booking-hotel.csproj ./
RUN dotnet restore be-booking-hotel.csproj

COPY . .
RUN dotnet publish be-booking-hotel.csproj -c Release -o /app/publish --no-restore

# ─────────────────────────────────────────
# Stage 2: Runtime
# ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*
ENV TZ=Asia/Ho_Chi_Minh

ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "be-booking-hotel.dll"]
