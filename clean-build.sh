#!/bin/bash
# Script to clean and rebuild the solution

echo "Cleaning build artifacts..."

# Clean all bin and obj folders
find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null

echo "Running dotnet clean..."
dotnet clean

echo "Restoring NuGet packages..."
dotnet restore

echo "Building solution..."
dotnet build

echo "Done! If errors persist, try rebuilding in your IDE."
