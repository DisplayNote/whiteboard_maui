# WhiteboardMaui NuGet Package Publishing Pipeline

## Overview

This document explains the Azure DevOps pipeline (`azure-pipelines-publish-nuget.yml`) that automatically builds, packages, and publishes the WhiteboardMaui.Core NuGet package whenever changes are committed to the `main` branch.

## Pipeline Triggers

The pipeline is automatically triggered when:

- **Branch**: Commits are pushed to the `main` branch
- **Path filters**: Changes are made to any of the following directories:
  - `WhiteboardMaui.Core/*` - Core library source code
  - `WhiteboardMaui/*` - Main application source code

## Versioning Strategy

The pipeline uses an automatic versioning scheme:

- **Major Version**: `1` (fixed)
- **Minor Version**: `0` (fixed)
- **Patch Version**: Auto-incrementing counter based on the source branch
- **Final Version Format**: `1.0.{patch}` (e.g., `1.0.15`)

The version number is automatically incremented with each build, ensuring unique package versions.

## Pipeline Stages

### Stage 1: Build and Package

This stage performs the following operations:

#### 1. Environment Setup
- Checks out the source code with full git history
- Installs .NET 9 SDK

#### 2. MAUI Workload Installation
- Installs the required MAUI workloads for Android development

#### 3. .NET Core Project Build
- Restores NuGet packages for `WhiteboardMaui.Core.csproj`
- Builds the project targeting multiple MAUI platforms including `net9.0-windows10.0.26100.0` framework
- Applies the auto-generated version number during build

#### 4. Code Signing
- Signs the built assemblies using Azure Trusted Signing service
- Applies digital signatures to all DLL files for security and authenticity

#### 5. NuGet Package Creation
- Creates a NuGet package from the built project
- Includes debug symbols for better debugging experience
- Stores all artifacts in the build staging directory

#### 5. GitHub Release Creation
- Creates a new GitHub release with tag format: `nuget_{version}` (e.g., `nuget_1.0.15`)
- Release title: "NuGet Package {version} Release"
- Attaches build artifacts to the release

### Stage 2: Publish to Private Feed

This stage executes only after successful completion of the Build stage and when the source branch is `main`:

#### 1. Artifact Download
- Downloads the NuGet packages created in the previous stage

#### 2. Authentication
- Authenticates with the Azure DevOps NuGet feed

#### 3. Package Publishing
- Pushes the generated `.nupkg` files to the private feed named `displaynote-net-feeds`

## Build Infrastructure

- **Agent Pool**: `agent-pool-windows2025-vmss`
- **Target Platform**: Multi-platform MAUI (Android, iOS, macOS, Windows)
- **Primary Framework**: .NET 9.0 Windows (version 10.0.26100.0)

## Automated Outputs

For each successful pipeline run, the following artifacts are created:

1. **NuGet Package**: Published to the private `displaynote-net-feeds` repository
2. **GitHub Tag**: Created with format `nuget_{version}`
3. **GitHub Release**: Contains the package files
4. **Build Artifacts**: Stored in Azure DevOps for reference

## Benefits

- **Automated Versioning**: No manual version management required
- **Consistent Builds**: Same environment and process for every build
- **Traceability**: Each package version is linked to a specific commit and GitHub release
- **Quality Assurance**: Only changes to relevant paths trigger builds
- **Code Signing**: All assemblies are digitally signed for security
- **Easy Distribution**: Packages are automatically available in the private feed

## Usage

Once published, the package can be consumed in other projects by:

1. Adding the `displaynote-net-feeds` as a package source
2. Installing the package: `WhiteboardMaui.Core` with the latest version
3. The package will include the MAUI controls and components for whiteboard functionality

## Monitoring

To monitor the pipeline:

- Check the Azure DevOps pipeline runs for build status
- Verify new releases appear in the GitHub repository
- Confirm packages are available in the `displaynote-net-feeds` NuGet feed