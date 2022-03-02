#!/bin/bash

dotnet publish -c release -o pub/win --os win --self-contained true
dotnet publish -c release -o pub/linux --os linux --self-contained true