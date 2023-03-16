
Create Docker based console app

```
dotnet new console -o AsbApp -n DotNet.Docker
```

Add ASB support

```
dotnet add package Azure.Messaging.ServiceBus
```

Run app

```
ASB_CONN="Endpoint=sb://....." dotnet run
```

Build docker image

```
docker build -t asb-app .
```

Run docker 

```
docker run --rm -e ASB_CONN="Endpoint=sb://... " asb-app
```