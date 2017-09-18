# AtlCodeCampDocker
Straight Forward Docker on Windows

## Cheatsheet
|Task|docker command|
|:----- |:----------- |
|Display Version|`docker version`|
|List installed images|`docker images`|
|Remove an image|`docker rmi <image name>`|
|Remove a container|`docker rm <container>`|
|Show running containers|`docker ps`|
|Show all containers|`docker ps -a`|
|Run a command in a NEW container|`docker run <container> <command>`<br>Interactive mode<br>`docker run -i <container> <command>`|
|Run a command in an EXISTING container|`docker exec <container> <command>`<br>Interactive mode<br>`docker exec -i <container> <command>`|
|Connect to a container's console|`docker attach <container>`|
|Get a Windows container's IP address|`docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' <container>`|
|Build an image|`docker build <folder> -t <image name>`<br>Example:<br>`docker build . -t ormico/helloworld`|
Image names are often prefixed with a namespace. In the example above "ormico" is used as the namespace prefix and "/" is used as a separator.

Images also support tags. Multiple images can share the same name if they are differentiated by a tag.
Tags are specified using the ":". Tags are often used to specify version numbers and operating systems.

Dockerfile reference https://docs.docker.com/engine/reference/builder/