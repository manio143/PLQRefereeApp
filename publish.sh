#!/bin/bash

# This script allows me to publish the project to my private repository
# from which it is pulled on the production server.

# Get SHA of current commit
SHA=$(git rev-parse HEAD)

if [[ "$1" = "stable" ]]; then
    dotnet build --configuration Release && \
    dotnet publish --configuration Release && \
    cp Dockerfile bin/Release/netcoreapp2.1/publish && \
    cd bin/Release/netcoreapp2.1/publish && \
    docker build . -t manio143/plqref:stable && \
    docker push manio143/plqref:stable
else
    dotnet build && \
    dotnet publish && \
    cp Dockerfile bin/Debug/netcoreapp2.1/publish && \
    cd bin/Debug/netcoreapp2.1/publish && \
    docker build . -t manio143/plqref:latest -t manio143/plqref:sha${SHA:0:6} && \
    docker push manio143/plqref:latest && \
    docker push manio143/plqref:sha${SHA:0:6}
fi
