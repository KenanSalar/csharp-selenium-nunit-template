# Stage 1: Build the test project
# Use the official .NET SDK image as a base. This image contains all the tools needed to restore, build, and publish a .NET application.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install 'jq', a command-line JSON processor. This is needed by the entrypoint script to check the Selenium Hub status.
RUN apt-get update && apt-get install -y jq

# Set the working directory inside the container.
WORKDIR /src

# Copy only the project and solution files first. This is a Docker caching optimization.
# If these files don't change, Docker can reuse the cached layer from the next step, speeding up subsequent builds.
COPY ["SeleniumTraining/SeleniumTraining.csproj", "SeleniumTraining/"]
COPY ["SeleniumTraining.sln", "."]

# Restore all NuGet packages for the solution.
RUN dotnet restore "SeleniumTraining.sln"

# Copy the rest of the source code into the container.
COPY . .

# Set the working directory to the specific project folder.
WORKDIR "/src/SeleniumTraining"

# Build the project in Release configuration. The output will be placed in /app/build.
RUN dotnet build "SeleniumTraining.csproj" -c Release -o /app/build

# Stage 2: Create the final, clean image for running the tests
FROM build AS final
WORKDIR /app

# Copy the build output (the compiled DLLs and dependencies) from the 'build' stage into the final image.
COPY --from=build /app/build .

# Copy the visual testing baseline images from the 'build' stage into the final image.
COPY --from=build /src/SeleniumTraining/ProjectVisualBaselines ./ProjectVisualBaselines

# Copy the entrypoint script into the final image.
COPY entrypoint.sh .

# Make the entrypoint script executable.
RUN chmod +x entrypoint.sh

# Set the entrypoint for the container. This script will be executed when the container starts.
ENTRYPOINT ["./entrypoint.sh"]