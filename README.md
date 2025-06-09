# Selenium C# Test Automation Framework

This repository contains a robust and scalable test automation framework built with modern C# and .NET 9. It is designed for testing web applications using Selenium WebDriver and incorporates a wide range of industry best practices like the Page Object Model (POM), Dependency Injection, and comprehensive reporting.

The framework comes pre-configured with tests for the [Sauce Labs Demo](https://www.saucedemo.com/) website, showcasing its capabilities, including functional, visual, and error-case testing.

## ✨ Key Features

This framework is built to be easy to maintain. Here are some of its core features:

* **Page Object Model (POM)**: Tests are cleanly separated from page-specific logic. The framework uses a combination of Pages, reusable Components, and Element Maps for maximum organization and maintainability, following SOLID principles.
* **Dependency Injection (DI)**: Utilizes `Microsoft.Extensions.DependencyInjection` to manage the lifecycle of services, making the framework modular and easy to extend.
* **Modern Configuration**: Leverages `appsettings.json`, environment-specific configurations (`appsettings.Development.json`, `appsettings.Docker.json`), and User Secrets for flexible and secure management of settings.
* **Parallel, Cross-Browser Execution**: NUnit is configured to run test fixtures in parallel across multiple browsers (Chrome, Firefox) to significantly speed up test runs.
* **Docker Integration**: Comes with a `docker-compose.yml` file to easily spin up a complete testing environment, including a Selenium Grid (Hub and Nodes) and a live Allure report server.
* **Comprehensive Reporting**: Integrates with **Allure Framework** to generate beautiful, detailed, and interactive test reports, including steps, attachments on failure (screenshots), performance metrics, and environment details.
* **Visual Regression Testing**: Includes a `VisualTestService` that compares screenshots against baseline images to catch unintended UI changes. Baselines are automatically created and managed per browser.
* **Robust Interaction & Synchronization**:
  * **Smart Locators**: A `SmartLocators` utility encourages using stable `data-test` attributes over brittle selectors.
  * **Custom Waits**: Extends Selenium's `ExpectedConditions` with custom wait conditions for more complex synchronization scenarios.
  * **Retry Logic**: Implements a `RetryService` using **Polly** to automatically retry failed actions (e.g., finding a stale element), making tests more resilient to transient failures.
* **Performance Monitoring**: A `PerformanceTimer` utility is integrated into the `BaseTest` to measure and log the duration and memory usage of test steps, which can be attached to Allure reports.

## 🛠️ Technology Stack

* **Language & Platform**: C# on .NET 9
* **Test Runner**: NUnit 3
* **Web Automation**: Selenium WebDriver
* **Reporting**: Allure Framework
* **Containerization**: Docker & Docker Compose
* **Resilience Policies**: Polly
* **Assertions**: Shouldly
* **Image Comparison**: ImageSharp, ImageSharp.Compare

## 🚀 Getting Started

Follow these instructions to get the test framework up and running on your local machine.

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Configuration

The framework uses `appsettings.json` for general configuration and `appsettings.Development.json` for local overrides. For sensitive data, such as passwords, you must use .NET's User Secrets.

#### Set Up User Secrets

The SauceDemo password is read from the configuration. To set it securely for your local development environment, open a terminal in the `SeleniumTraining` project directory and run the following command:

```bash
dotnet user-secrets set "SauceDemo:LoginPassword" "the_password_from_the_sauce_demo_website"
```

This command stores the password in a secure location on your machine, outside the project directory, and the framework will automatically load it during initialization.

### 2. Running Tests with Docker (Recommended)

Using Docker is the recommended way to run the tests, as it provides a consistent, isolated environment with the Selenium Grid and all necessary dependencies.

#### Docker Environment File

The `docker-compose.yml` file is configured to use an `.env` file to pass the SauceDemo password to the test runner container.

1.  **Create an `.env` file** in the root directory of the project (the same directory as `docker-compose.yml`).
2.  **Add the following line** to the `.env` file:

    ```
    SAUCE_DEMO_PASSWORD="the_password_from_the_sauce_demo_website"
    ```

#### Docker Commands

All commands should be run from the root directory of the project.

1.  **Build the Test Runner Image:**
    This command builds the Docker image for the .NET test project. You only need to run this again if you change the project's code or dependencies.

    ```bash
    docker compose build test-runner
    ```

2.  **Run All Tests (Chrome & Firefox) and View Live Report:**
    This command starts the Selenium Hub, Chrome node, Firefox node, and the live Allure report server. It then runs the test-runner service, which executes all tests.

    ```bash
    docker compose up --exit-code-from test-runner
    ```
    * `--exit-code-from test-runner`: This ensures that Docker Compose will exit with the status code of the test runner, which is useful for CI environments.

3.  **Run Tests on a Specific Browser:**
    You can run tests on a single browser by providing it as an argument to the `test-runner` service. This is handled by the `entrypoint.sh` script.

    * **Run on Chrome only:**
        ```bash
        docker compose run --rm test-runner Chrome
        ```
    * **Run on Firefox only:**
        ```bash
        docker compose run --rm test-runner Firefox
        ```

4.  **Run Allure Report Server:**
    For the allure report you need to  run the allure-reporter-service. 
    ```bash
    docker compose up -d allure-reporter-service  
    ```

5.  **Cleaning Up:**
    After the test run, if you used `docker compose up`, the containers will stop but not be removed. To clean up all containers, networks, and volumes created by `docker-compose`, run:
    ```bash
    docker compose down -v
    ```

### 3. Running Tests Locally

You can also run tests directly on your local machine using the .NET CLI. This requires you to have Chrome and/or Firefox installed.

1.  **Navigate to the test project directory:**
    ```bash
    cd SeleniumTraining
    ```

2.  **Run all tests:**
    ```bash
    dotnet test
    ```

3.  **Run tests for a specific browser category:**
    * **Run Chrome tests:**
        ```bash
        dotnet test --filter "TestCategory=Chrome"
        ```
    * **Run Firefox tests:**
        ```bash
        dotnet test --filter "TestCategory=Firefox"
        ```

## 📊 Test Reports

The framework is integrated with Allure for rich and interactive test reporting.

### Live Allure Report (Docker)

When you run tests using `docker compose up`, a live Allure report server is started.

* **Access the report**: Open your web browser and navigate to **`http://localhost:5050`**.
* The report will automatically update as the tests run and generate results.

### Generating a Static Allure Report

If you run tests locally or want to generate a shareable HTML report from the results produced by a Docker run, you need the [Allure Commandline tool](https://allurereport.org/docs/gettingstarted-installation/).

1.  **Locate the results directory**: Test results are generated in the `SeleniumTraining/bin/Release/net9.0/allure-results` directory.
2.  **Generate the report**: From the root of the project, run the following command:
    ```bash
    allure serve SeleniumTraining/bin/Release/net9.0/allure-results
    ```
    This command will generate the report and open it in your default web browser.

## 📂 Framework Structure

The project is organized into logical directories to maintain separation of concerns.

* `Core/`: Contains the framework's core engine, including the `BaseTest`, service interfaces (`Contracts/`), service implementations (`Services/`), DI setup, and configuration extensions.
* `Pages/`: This is where the Page Object Model is implemented.
    * `Components/`: Reusable components that can appear on multiple pages (e.g., a product card).
    * `ElementMap/`: Static classes that hold `By` locators for pages and components, keeping selectors separate from page logic.
    * `Enums/`: Enumerations used for parameterizing actions (e.g., `LoginMode`, `SortByType`).
* `Tests/`: Contains all the NUnit test fixtures. Tests are organized by the application feature they cover (e.g., `SauceDemoTests/`).
* `Utils/`: A collection of utility classes, including custom waits, smart locators, performance timers, and extension methods.
    * `Settings/`: Strongly-typed classes that map to sections in `appsettings.json`.
* `allure-results/`: (Generated) Directory where Allure test result files are stored.
* `TestOutput/`: (Generated) Directory for other test artifacts like logs and screenshots.

## ⚖️ License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
