FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.sln ./
COPY RabbitMQ.Api/*.csproj ./RabbitMQ.Api/
RUN dotnet restore

COPY RabbitMQ.Api/. ./RabbitMQ.Api/

WORKDIR /app/RabbitMQ.Api
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build /app/RabbitMQ.Api/out .

ENTRYPOINT [ "dotnet", "RabbitMQ.Api.dll" ]