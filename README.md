# orleans-asteroid-belt
Example how to use orelans in containerized setup

## What does this project?
The project simulates in a very simple way the movement of asteroids around a gravity pool. 
A asteroid is represented as grain in the orleans cluster. The asteriods are distributed across the orleans cluster.
If two asteroids are crossing paths, the asteroid with the smaller weight is destroyed and cannot move anymore.
The state information about the asteroids are visible on a status dashboard (default page) and updated in realtime via signalR. 

## What is used?
* Orleans: https://github.com/dotnet/orleans
* Orleans SignalR: https://github.com/OrleansContrib/SignalR.Orleans
* Asp.NetCore6: https://github.com/dotnet/aspnetcore
* Docker-Compose: https://docs.docker.com/compose/
* Redis: used for clustering if configured (based on ASPNETCORE_ENVIRONMENT)
* Azure-Storage: used for clustering if configured (based on ASPNETCORE_ENVIRONMENT)

## Run project locally via DOCKER-COMPOSE and VS-Studio
* open https://github.com/ettenauer/orleans-asteroid-belt
* select docker-compose as start up project and press run 
* docker-compose will start two instances of AsteroidBelt.Silo 
* browse https://localhost:5001/ or https://localhost:5002/ to see status dashboard

