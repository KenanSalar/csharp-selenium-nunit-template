# Stage 1: Build the test project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
RUN apt-get update && apt-get install -y jq
WORKDIR /src

COPY ["SeleniumTraining/SeleniumTraining.csproj", "SeleniumTraining/"]
COPY ["SeleniumTraining.sln", "."]
RUN dotnet restore "SeleniumTraining.sln"

COPY . .
WORKDIR "/src/SeleniumTraining"
RUN dotnet build "SeleniumTraining.csproj" -c Release -o /app/build

# Stage 2: Create the final image with the test runner
FROM build AS final
WORKDIR /app
COPY --from=build /app/build .

COPY --from=build /src/SeleniumTraining/ProjectVisualBaselines ./ProjectVisualBaselines

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]