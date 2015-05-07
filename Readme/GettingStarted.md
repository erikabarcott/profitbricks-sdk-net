This guide will show you how to programmatically perform common management tasks using the .NET SDK for the ProfitBricks API. [.NET] (http://www.microsoft.com/net) is a software framework developed by Microsoft that runs primarily on Microsoft Windows. 

## Table of Contents

* [Concepts](#concepts)
* [Getting Started](#getting-started)
* [How to: Create Data Center](#how-to-create-a-data-center)
* [How to: Delete Data Center](#how-to-delete-a-data-center)
* [How to: Create Server](#how-to-create-a-server)
* [How to: Update Cores, Memory, and Disk](#how-to-update-cores-memory-and-disk)
* [How to: Detach and Reattach a Storage Volume](#how-to-detach-and-reattach-a-storage-volume)
* [How to: List Servers, Volumes, and Data Centers](#how-to-list-servers-volumes-and-data-centers)
* [How to: Create Additional Network Interfaces](#how-to-create-additional-network-interfaces)
* [Example](#example)

--------------

## Concepts

This .NET library wraps the ProfitBricks SOAP API. All API operations are performed over SSL and authenticated using your ProfitBricks portal credentials. The API can be accessed within an instance running in ProfitBricks or directly over the Internet from any application that can send an HTTPS request and receive an HTTPS response. 

## Getting Started

Before you begin you will need to have [signed-up](https://www.profitbricks.com/signup) for a ProfitBricks account. The credentials you setup during sign-up will be used to authenticate against the API.
 
### Installation

The official .NET library is available from the ProfitBricks GitHub account found [here](https://github.com/profitbricks). You can install the latest stable version by cloning the repository and then adding the binaries to your project.

### Configuration

Depending on the type of project, you will need to create either an App.config or Web.config file to interact with the service before you begin. This file should contain the following values: 

```sh
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="UserName" value="[Your User Name]"/>
    <add key="Password" value="[Your Password]"/>
  </appSettings>
  <system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name="ProfitBricksProxy.ProfitbricksApiServicePortType">
          <security mode="Transport">
            <transport clientCredentialType="Basic" proxyCredentialType="None" realm="" />
            <message clientCredentialType="UserName" algorithmSuite="Default" />
          </security>
        </binding>
    </bindings>
    <client>
      <endpoint address="https://api.profitbricks.com/1.3" binding="basicHttpBinding"
          bindingConfiguration="ProfitBricksProxy.ProfitbricksApiServicePortType" contract="ProfitBricksProxy.ProfitbricksApiServicePortType"
          name="ProfitBricksProxy.ProfitbricksApiServicePortType" />
    </client>
  </system.serviceModel>
</configuration>
```

### Using the Driver

Here is a simple example on how to use the library.

List all data centers: 

     var dcs = ProfitBricksSoapClient.Instance.DataCenters.Get();
    
This will list all virtual datacenters you have under your account.

### Additional Documentation and Support

You can engage with us in the community and we'll be more than happy to answer any questions you might have. 

## How to: Create a Datacenter

ProfitBricks introduces the concept of Virtual Datacenters. These are logically separated from one and the other and allow you to have a self-contained environment for all servers, volumes, networking, snapshots, and so forth. The goal is to give you the same experience as you would have if you were running your own physical datacenter.

You are required to have a Virtual Datacenter (vDC) created before you can create any other objects. Think of the vDC as a bucket in which all objects, such as servers and volumes, live. 

The following code example shows you how to programmatically create a datacenter: 

	using ProfitBricks.Client;
	using ProfitBricks.POCO.Requests;

	namespace ProfitBricksExample
	{
		class Program
		{
			static void Main(string[] args)
			{
				var dcCreateRequest = new CreateDataCenterRequest
				{
					DataCenterName = "test",
					Location = ProfitBricks.POCO.Enums.Location.uslas
				};

				var dcCreateResponse = ProfitBricksSoapClient.Instance.DataCenters.Create(dcCreateRequest);

			}
		}
	}

## How to: Delete a Datacenter

You will want to exercise a bit of caution here. Removing a datacenter will **destroy** all objects contained within that datacenter -- servers, volumes, snapshots, and so on. The objects -- once removed -- will be unrecoverable. 

The code to remove a datacenter is as follows. This example assumes you want to remove a datacenter with the ID '7a857cab-ec08-445b-9b3a-52d0357326db':

    ProfitBricksSoapClient.Instance.DataCenters.Delete(dcCreateResponse.Id);

## How to: Create a Server

The following example illustrates how you would create a server and assign it an OS, cores, and memory. We urge you to check the [documentation](https://devops.profitbricks.com/api/) to see the complete list attributes available.

	var serverCreateRequest = new CreateServerRequest
	{
		DataCenterId = dcCreateResponse.Id,
		AvailabilityZone = AvailabilityZone.AUTO,
		Cores = 4,
		InternetAccess = true,
		OsType = OsType.WINDOWS,
		Ram = 256
	};

	var serverCreateResponse = ProfitBricksSoapClient.Instance.Servers.Create(serverCreateRequest);

One of the unique features of the ProfitBricks platform when compared with the other providers is that it allows you to define your own settings for cores, memory, and disk size without being tied to a particular size.  

## How to: Update Cores, Memory, and Disk

ProfitBricks allows users to dynamically update cores, memory, and disk independently of each other. This removes the restriction of needing to upgrade to the next size up to receive an increase in memory. You can now simply increase the instances memory keeping your costs in-line with your resource needs. 

The following code illustrates how you can update cores and memory: 

	var updateServerRequest = new UpdateServerRequest 
	{ 
		ServerId = serverCreateResponse.Id, 
		ServerName = "newName", 
		Cores = 3, Ram = 512 
	};
	
	ProfitBricksSoapClient.Instance.Servers.Update(updateServerRequest);

As you can see, in a single line of code we've updated the name, cores, and memory all on the fly.

## How to: Detach and Reattach a Storage Volume

ProfitBricks allows for the creation of multiple storage volumes. You can detach and reattach these on the fly. This allows for various scenarios such as re-attaching a failed OS disk to another server for possible recovery, move a volume to another location and re-attaching to a different server and spinning it up. 

The following illustrates how you would attach a volume and then detach it from a server:

	//First we need to create a volume.
	var createVolumeRequest = new CreateVolumeRequest
	{
		DataCenterId = dcCreateResponse.Id,
		Size = 1
	};

	var createVolumeResponse = ProfitBricksSoapClient.Instance.Volumes.Create(createVolumeRequest);
	
	//Then we are going to attach the newly volume to a server.
	var connectStorageRequest = new ConnectStorageRequest
	{
		ServerId = createServerResponse.Id,
		StorageId = createVolumeResponse.Id,
		BusType = BusType.SCSI,
	};

	ProfitBricksSoapClient.Instance.Volumes.ConnectVolume(connectStorageRequest);

	//Here we are going to detach it from the server.
	ProfitBricksSoapClient.Instance.Volumes.DisconnectVolume(createVolumeResponse.Id, createServerResponse.Id);

## <a name="ListServersVolumesDataCenters"></a>How to: List Servers, Volumes, and Data Centers

You can pull various lists of information from your vDCs using the .NET library. The three most common ones are Datacenters, Servers, and Volumes.

The following code illustrates how to pull these three list types: 

	var dcs = ProfitBricksSoapClient.Instance.DataCenters.Get();
	var servers = ProfitBricksSoapClient.Instance.Servers.Get();
	var volumes = ProfitBricksSoapClient.Instance.Volumes.Get();

## How to: Create Additional Network Interfaces

The ProfitBricks platform supports adding multiple NICs to a server. These NICs can be used to create different, segmented networks on the platform.
The sample below shows you how to add a second NIC to an existing server: 

	var createNicRequest = new CreateNicRequest 
	{ 
		ServerId = createServerResponse.Id, 
		NicName = "new nic name", 
		DhcpActive = true 
	};

	ProfitBricksSoapClient.Instance.Nics.Create(createNicRequest);

One item to note is this function will result in the server being rebooted. 

## Example

	using ProfitBricks.Client;
	using ProfitBricks.POCO.Enums;
	using ProfitBricks.POCO.Requests;

	namespace ProfitBricksExample
	{
		class Program
		{
			static void Main(string[] args)
			{
				// CreateDataCenterRequest. 
				// The only required field is DataCenterName. 
				// If location parameter is left empty data center will be created in the default region of the customer
				var dcCreateRequest = new CreateDataCenterRequest
				{
					DataCenterName = "test",
					Location = ProfitBricks.POCO.Enums.Location.uslas
				};

				// Response will contain Id of a newly created data center.
				var dcCreateResponse = ProfitBricksSoapClient.Instance.DataCenters.Create(dcCreateRequest);

				// CreateServerRequest. 
				// DataCenterId: Defines the data center wherein the server is to be created.
				// AvailabilityZone: Selects the zone in which the server is going to be created (AUTO, ZONE_1, ZONE_2).
				// Cores: Number of cores to be assigned to the specified server. Required field.
				// InternetAccess: Set to TRUE to connect the server to the Internet via the specified LAN ID.
				// OsType: Sets the OS type of the server.
				// Ram: Number of RAM memory (in MiB) to be assigned to the server.
				var serverCreateRequest = new CreateServerRequest
				{
					DataCenterId = dcCreateResponse.Id,
					AvailabilityZone = AvailabilityZone.AUTO,
					Cores = 4,
					InternetAccess = true,
					OsType = OsType.WINDOWS,
					Ram = 256
				};

				// response will contain Id of a newly created server.
				var createServerResponse = ProfitBricksSoapClient.Instance.Servers.Create(serverCreateRequest);

				// UpdateServerRequest
				// ServerId: Id of the server to be updated.
				// ServerName: Renames target virtual server
				// Cores: Updates the amount of cores of the target virtual server
				// Ram: Updates the RAM memory (in MiB) of the target virtual server. The minimum RAM size is 256 MiB
				var updateServerRequest = new UpdateServerRequest
				{
					ServerId = createServerResponse.Id,
					ServerName = "newName",
					Cores = 3,
					Ram = 512
				};

				ProfitBricksSoapClient.Instance.Servers.Update(updateServerRequest);

				// CreateVolumeRequest
				// DataCenterId: Defines the data center wherein the storage is to be created. If left empty, the storage will be created in a new data center
				// Size: Storage size (in GiB). Required Field.
				var createVolumeRequest = new CreateVolumeRequest
				{
					DataCenterId = dcCreateResponse.Id,
					Size = 1
				};

				// Response will contain Id of a newly created volume.
				var createVolumeResponse = ProfitBricksSoapClient.Instance.Volumes.Create(createVolumeRequest);

				// ConnectStorageRequest
				// ServerId: Identifier of the target virtual storage. Required field.
				// StorageId: Identifier of the virtual storage to be connected. Required field.
				// BusType: Bus type to which the storage will be connected
				var connectStorageRequest = new ConnectStorageRequest
				{
					ServerId = createServerResponse.Id,
					StorageId = createVolumeResponse.Id,
					BusType = BusType.SCSI,
				};

				ProfitBricksSoapClient.Instance.Volumes.ConnectVolume(connectStorageRequest);

				ProfitBricksSoapClient.Instance.Volumes.DisconnectVolume(createVolumeResponse.Id, createServerResponse.Id);

				// Fetches list of all DataCenters
				var dcs = ProfitBricksSoapClient.Instance.DataCenters.Get();

				// Fetches list of all Servers
				var servers = ProfitBricksSoapClient.Instance.Servers.Get();

				// Fetches list of all Volumes
				var volumes = ProfitBricksSoapClient.Instance.Volumes.Get();

				// CreateNicRequest
				// Identifier of the target virtual server. Required field.
				// NicName: Names the NIC
				// Toggles usage of ProfitBricks DHCP
				var createNicRequest = new CreateNicRequest
				{
					ServerId = createServerResponse.Id,
					NicName = "new nic name",
					DhcpActive = true
				};

				ProfitBricksSoapClient.Instance.Nics.Create(createNicRequest);
			}
		}
	}